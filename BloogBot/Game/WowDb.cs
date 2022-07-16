using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace BloogBot.Game
{
    public static class WowDb
    {
        public static readonly Dictionary<ClientDb, DbTable> Tables = new Dictionary<ClientDb, DbTable>();

        static WowDb()
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

        public class DbTable
        {
            readonly IntPtr pointer;
            
            public DbTable(IntPtr pointer)
            {
                this.pointer = pointer;
            }

            // For all DBs, we should use GetRow, except for Spells.db, which should use GetLocalizedRow
            public IntPtr GetRow(int index)
            {
                return Functions.GetRow(pointer, index);
            }

            // For all DBs, we should use GetRow, except for Spells.db, which should use GetLocalizedRow
            public IntPtr GetLocalizedRow(int index)
            {
                var rowPtr = Marshal.AllocHGlobal(4 * 4 * 256);
                return Functions.GetLocalizedRow(IntPtr.Subtract(pointer, 0x18), index, rowPtr);
            }
        }
    }

    public enum ClientDb : uint
    {
        Spell = 0x00000194, // 0x00A751FC
    }
}