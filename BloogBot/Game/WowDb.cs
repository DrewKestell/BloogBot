using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace BloogBot.Game
{
    public class WowDb
    {
        private static readonly Dictionary<ClientDb, DbTable> Tables = new Dictionary<ClientDb, DbTable>();

        public WowDb()
        {
            for (var tableBase = (IntPtr)MemoryAddresses.WowDbTableBase;
                MemoryManager.ReadByte(tableBase) != 0xC3;
                tableBase += 0x11)
            {
                var index = MemoryManager.ReadUint(tableBase + 1);
                var tablePtr = new IntPtr(MemoryManager.ReadInt(tableBase + 0xB) + 0x18);
                Tables.Add((ClientDb)index, new DbTable(tablePtr));
            }
        }

        public DbTable this[ClientDb db]
        {
            get { return Tables[db]; }
        }

        public class DbTable
        {
            internal readonly IntPtr Address;
            private ClientDb_GetLocalizedRow _getLocalizedRow;
            private ClientDb_GetRow _getRow;

            public DbTable(IntPtr address)
            {
                Address = address;
                var h = Manager.Memory.Read<DbHeader>(Address);
                MaxIndex = h.MaxIndex;
                MinIndex = h.MinIndex;
            }

            public uint MinIndex { get; private set; }
            public uint MaxIndex { get; private set; }

            // For all DBs, we should use GetRow, except for Spells.db, which should use GetLocalizedRow
            public Row GetRow(int index)
            {
                return GetRowFromDelegate(index);
            }

            // For all DBs, we should use GetRow, except for Spells.db, which should use GetLocalizedRow
            public Row GetLocalizedRow(int index)
            {
                if (_getLocalizedRow == null)
                {
                    _getLocalizedRow =
                        Manager.Memory.RegisterDelegate<ClientDb_GetLocalizedRow>((IntPtr)Pointers.DBC.GetLocalizedRow);
                }
                IntPtr rowPtr = Marshal.AllocHGlobal(4 * 4 * 256);
                int tmp = _getLocalizedRow(new IntPtr(Address.ToInt32() - 0x18), index, rowPtr);
                if (tmp != 0)
                {
                    return new Row(rowPtr, true);
                }
                Marshal.FreeHGlobal(rowPtr);
                return null;
            }

            private Row GetRowFromDelegate(int index)
            {
                if (_getRow == null)
                {
                    _getRow = Manager.Memory.RegisterDelegate<ClientDb_GetRow>((IntPtr)Pointers.DBC.GetRow);
                }
                var ret = new IntPtr(_getRow(new IntPtr(Address.ToInt32()), index));
                return ret == IntPtr.Zero ? null : new Row(ret, false);
            }


            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            private delegate int ClientDb_GetLocalizedRow(IntPtr instance, int index, IntPtr rowPtr);

            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            private delegate int ClientDb_GetRow(IntPtr instance, int idx);

            [StructLayout(LayoutKind.Sequential)]
            private struct DbHeader
            {
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public readonly uint[] Junk;

                public readonly uint MaxIndex;
                public readonly uint MinIndex;
            }

            public class Row : IDisposable
            {
                private readonly bool _isManagedMem;
                private IntPtr _rowPtr;
                private object structedObject;

                private Row(IntPtr rowPtr)
                {
                    _rowPtr = rowPtr;
                }

                internal Row(IntPtr rowPtr, bool isManagedMem)
                    : this(rowPtr)
                {
                    _isManagedMem = isManagedMem;
                }

                public void Dispose()
                {
                    if (_isManagedMem)
                    {
                        Marshal.FreeHGlobal(_rowPtr);
                    }

                    _rowPtr = IntPtr.Zero;
                    GC.SuppressFinalize(this);
                }

                public T GetField<T>(uint index) where T : struct
                {
                    try
                    {
                        if (typeof(T) == typeof(string))
                        {
                            // Sometimes.... generics ****ing suck
                            object s =
                                Marshal.PtrToStringAnsi(
                                    Manager.Memory.Read<IntPtr>(new IntPtr((uint)_rowPtr + (index * 4))));
                            return (T)s;
                        }

                        return Manager.Memory.Read<T>(new IntPtr((uint)_rowPtr + (index * 4)));
                    }
                    catch
                    {
                        return default(T);
                    }
                }

                //public void SetField(uint index, int value)
                //{
                //    byte[] bs = BitConverter.GetBytes(value);
                //    Win32.WriteBytes((IntPtr)(_rowPtr.ToUInt32() + (index * 4)), bs);
                //}

                public T GetStruct<T>() where T : struct
                {
                    try
                    {
                        if (structedObject == null)
                        {
                            IntPtr addr = _rowPtr;
                            structedObject = (T)Marshal.PtrToStructure(addr, typeof(T));
                            Marshal.FreeHGlobal(addr);
                        }
                        return (T)structedObject;
                    }
                    catch
                    {
                        return default(T);
                    }
                }
            }
        }
    }

    public enum ClientDb : uint
    {
        Spell = 0x00000194, // 0x00A751FC
    }
}