using WoWActivityMember.Mem;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using static WoWActivityMember.Constants.Enums;

namespace WoWActivityMember.Objects
{
    public unsafe abstract class WoWObject
    {
        public virtual IntPtr Pointer { get; set; }
        public virtual ulong Guid { get; set; }
        public virtual WoWObjectTypes ObjectType { get; set; }

        // used for interacting in vanilla
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void RightClickObjectDelegate(IntPtr unitPtr, int autoLoot);

        private readonly RightClickObjectDelegate rightClickObjectFunction;

        public WoWObject() { }

        public WoWObject(IntPtr pointer, ulong guid, WoWObjectTypes objectType)
        {
            Pointer = pointer;
            Guid = guid;
            ObjectType = objectType;

            rightClickObjectFunction = Marshal.GetDelegateForFunctionPointer<RightClickObjectDelegate>(0x60BEA0);
        }
        public float ScaleX => MemoryManager.ReadFloat(IntPtr.Add(GetDescriptorPtr(), MemoryAddresses.WoWObject_ScaleXOffset));

        public virtual Position Position => GetPosition();

        [HandleProcessCorruptedStateExceptions]
        private Position GetPosition()
        {
            try
            {
                if (ObjectType == WoWObjectTypes.OT_UNIT || ObjectType == WoWObjectTypes.OT_PLAYER)
                {
                    var x = MemoryManager.ReadFloat(IntPtr.Add(Pointer, 0x9B8));
                    var y = MemoryManager.ReadFloat(IntPtr.Add(Pointer, 0x9BC));
                    var z = MemoryManager.ReadFloat(IntPtr.Add(Pointer, 0x9C0));

                    return new Position(x, y, z);
                }
                else
                {
                    float x;
                    float y;
                    float z;
                    if (MemoryManager.ReadInt(GetDescriptorPtr() + 0x54) == 3)
                    {
                        x = MemoryManager.ReadFloat(GetDescriptorPtr() + 0x3C);
                        y = MemoryManager.ReadFloat(GetDescriptorPtr() + (0x3C + 4));
                        z = MemoryManager.ReadFloat(GetDescriptorPtr() + (0x3C + 8));
                        return new Position(x, y, z);
                    }
                    var v2 = MemoryManager.ReadInt(IntPtr.Add(Pointer, 0x210));
                    IntPtr xyzStruct;
                    if (v2 != 0)
                    {
                        var underlyingFuncPtr = MemoryManager.ReadInt(IntPtr.Add(MemoryManager.ReadIntPtr(v2), 0x44));
                        switch (underlyingFuncPtr)
                        {
                            case 0x005F5C10:
                                x = MemoryManager.ReadFloat(v2 + 0x2c);
                                y = MemoryManager.ReadFloat(v2 + 0x2c + 0x4);
                                z = MemoryManager.ReadFloat(v2 + 0x2c + 0x8);
                                return new Position(x, y, z);
                            case 0x005F3690:
                                v2 = (int)IntPtr.Add(MemoryManager.ReadIntPtr(IntPtr.Add(MemoryManager.ReadIntPtr(v2 + 0x4), 0x110)), 0x24);
                                x = MemoryManager.ReadFloat(v2);
                                y = MemoryManager.ReadFloat(v2 + 0x4);
                                z = MemoryManager.ReadFloat(v2 + 0x8);
                                return new Position(x, y, z);
                        }
                        xyzStruct = v2 + 0x44;
                    }
                    else
                    {
                        xyzStruct = IntPtr.Add(MemoryManager.ReadIntPtr(IntPtr.Add(Pointer, 0x110)), 0x24);
                    }
                    x = MemoryManager.ReadFloat(xyzStruct);
                    y = MemoryManager.ReadFloat(IntPtr.Add(xyzStruct, 0x4));
                    z = MemoryManager.ReadFloat(IntPtr.Add(xyzStruct, 0x8));
                    return new Position(x, y, z);
                }
            }
            catch (AccessViolationException)
            {
                Console.WriteLine("Access violation on WoWObject.Position. Swallowing.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"[WOW OBJECT]{e.Message} {e.StackTrace}");
            }
            return new Position(0, 0, 0);
        }

        public Position GetPointBehindUnit(float parDistanceToMove)
        {
            var newX = Position.X + parDistanceToMove * (float)-Math.Cos(Facing);
            var newY = Position.Y + parDistanceToMove * (float)-Math.Sin(Facing);
            var end = new Position(newX, newY, Position.Z);
            return end;
        }
        public float Facing => GetFacing();

        [HandleProcessCorruptedStateExceptions]
        private float GetFacing()
        {
            try
            {
                if (ObjectType == WoWObjectTypes.OT_PLAYER || ObjectType == WoWObjectTypes.OT_UNIT)
                {
                    float facing = MemoryManager.ReadFloat(Pointer + 0x9C4);

                    if (facing < 0)
                    {
                        facing = (float)(Math.PI * 2) + facing;
                    }
                    return facing;
                }
                else
                {
                    return 0;
                }
            }
            catch (AccessViolationException)
            {
                Console.WriteLine("Access violation on WoWObject.Facing. Swallowing.");
                return 0;
            }
        }

        public string Name => GetName();

        [HandleProcessCorruptedStateExceptions]
        private string GetName()
        {
            try
            {
                if (ObjectType == WoWObjectTypes.OT_PLAYER)
                {
                    var namePtr = MemoryManager.ReadIntPtr(0xC0E230);
                    while (true)
                    {
                        var nextGuid = MemoryManager.ReadUlong(IntPtr.Add(namePtr, 0xC));

                        if (nextGuid != Guid)
                            namePtr = MemoryManager.ReadIntPtr(namePtr);
                        else
                            break;
                    }
                    return MemoryManager.ReadString(IntPtr.Add(namePtr, 0x14));
                }
                else if (ObjectType == WoWObjectTypes.OT_UNIT)
                {
                    var ptr1 = MemoryManager.ReadInt(IntPtr.Add(Pointer, 0xB30));
                    var ptr2 = MemoryManager.ReadInt(ptr1);
                    return MemoryManager.ReadString(ptr2);
                }
                else if (ObjectType == WoWObjectTypes.OT_GAMEOBJ)
                {
                    var ptr1 = MemoryManager.ReadIntPtr(IntPtr.Add(Pointer, 0x214));
                    var ptr2 = MemoryManager.ReadIntPtr(IntPtr.Add(ptr1, 0x8));
                    return MemoryManager.ReadString(ptr2);
                } else
                {
                    return null;
                }
            }
            catch (AccessViolationException)
            {
                Console.WriteLine("Access violation on WoWObject.Name. Swallowing.");
                return "";
            }
            catch (Exception e)
            {
                Console.WriteLine($"Catchall exception {e.Message} {e.StackTrace}");
                return "";
            }
        }

        public void Interact()
        {
            rightClickObjectFunction(Pointer, 0);
        }

        protected IntPtr GetDescriptorPtr() => MemoryManager.ReadIntPtr(IntPtr.Add(Pointer, MemoryAddresses.WoWObject_DescriptorOffset));
    }
}
