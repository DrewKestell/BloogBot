using ForegroundBotRunner.Mem;
using GameData.Core.Enums;
using GameData.Core.Interfaces;
using GameData.Core.Models;
using System.Runtime.InteropServices;

namespace ForegroundBotRunner.Objects
{
    public unsafe abstract class WoWObject(nint pointer, HighGuid guid, WoWObjectType objectType) : IWoWObject
    {
        public nint Pointer { get; } = pointer;
        // used for interacting in vanilla
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void RightClickObjectDelegate(nint unitPtr, int autoLoot);

        private readonly RightClickObjectDelegate rightClickObjectFunction = Marshal.GetDelegateForFunctionPointer<RightClickObjectDelegate>(0x60BEA0);

        public float ScaleX => MemoryManager.ReadFloat(nint.Add(GetDescriptorPtr(), MemoryAddresses.WoWObject_ScaleXOffset));
        public float Height => MemoryManager.ReadFloat(nint.Add(Pointer, MemoryAddresses.WoWObject_HeightOffset));
        public Position Position => GetPosition();

        private Position GetPosition()
        {
            try
            {
                if (ObjectType == WoWObjectType.Unit || ObjectType == WoWObjectType.Player)
                {
                    var x = MemoryManager.ReadFloat(nint.Add(Pointer, 0x9B8));
                    var y = MemoryManager.ReadFloat(nint.Add(Pointer, 0x9BC));
                    var z = MemoryManager.ReadFloat(nint.Add(Pointer, 0x9C0));

                    return new(x, y, z);
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
                        return new(x, y, z);
                    }
                    var v2 = MemoryManager.ReadInt(nint.Add(Pointer, 0x210));
                    nint xyzStruct;
                    if (v2 != 0)
                    {
                        var underlyingFuncPtr = MemoryManager.ReadInt(nint.Add(MemoryManager.ReadIntPtr(v2), 0x44));
                        switch (underlyingFuncPtr)
                        {
                            case 0x005F5C10:
                                x = MemoryManager.ReadFloat(v2 + 0x2c);
                                y = MemoryManager.ReadFloat(v2 + 0x2c + 0x4);
                                z = MemoryManager.ReadFloat(v2 + 0x2c + 0x8);
                                return new(x, y, z);
                            case 0x005F3690:
                                v2 = (int)nint.Add(MemoryManager.ReadIntPtr(nint.Add(MemoryManager.ReadIntPtr(v2 + 0x4), 0x110)), 0x24);
                                x = MemoryManager.ReadFloat(v2);
                                y = MemoryManager.ReadFloat(v2 + 0x4);
                                z = MemoryManager.ReadFloat(v2 + 0x8);
                                return new Position(x, y, z);
                        }
                        xyzStruct = v2 + 0x44;
                    }
                    else
                    {
                        xyzStruct = nint.Add(MemoryManager.ReadIntPtr(nint.Add(Pointer, 0x110)), 0x24);
                    }
                    x = MemoryManager.ReadFloat(xyzStruct);
                    y = MemoryManager.ReadFloat(nint.Add(xyzStruct, 0x4));
                    z = MemoryManager.ReadFloat(nint.Add(xyzStruct, 0x8));
                    return new(x, y, z);
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
            return new(0, 0, 0);
        }

        public float Facing
        {
            get
            {
                try
                {
                    if (ObjectType == WoWObjectType.Unit || ObjectType == WoWObjectType.Player)
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
        }

        public string Name
        {
            get
            {
                try
                {
                    if (ObjectType == WoWObjectType.Player)
                    {
                        var namePtr = MemoryManager.ReadIntPtr(0xC0E230);
                        while (true)
                        {
                            var nextGuid = MemoryManager.ReadUlong(nint.Add(namePtr, 0xC));

                            if (nextGuid != Guid)
                                namePtr = MemoryManager.ReadIntPtr(namePtr);
                            else
                                break;
                        }
                        return MemoryManager.ReadString(nint.Add(namePtr, 0x14));
                    }
                    else if (ObjectType == WoWObjectType.Unit)
                    {
                        var ptr1 = MemoryManager.ReadInt(nint.Add(Pointer, 0xB30));
                        var ptr2 = MemoryManager.ReadInt(ptr1);
                        return MemoryManager.ReadString(ptr2);
                    }
                    else if (ObjectType == WoWObjectType.GameObj)
                    {
                        var ptr1 = MemoryManager.ReadIntPtr(nint.Add(Pointer, 0x214));
                        var ptr2 = MemoryManager.ReadIntPtr(nint.Add(ptr1, 0x8));
                        return MemoryManager.ReadString(ptr2);
                    }
                    else
                    {
                        return string.Empty;
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
        }

        public ulong Guid => throw new NotImplementedException();

        public HighGuid HighGuid => throw new NotImplementedException();

        public WoWObjectType ObjectType => throw new NotImplementedException();

        public uint LastUpated => throw new NotImplementedException();

        public uint Entry => throw new NotImplementedException();

        public bool InWorld => throw new NotImplementedException();

        public uint LastUpdated => throw new NotImplementedException();

        public ulong TransportGuid => throw new NotImplementedException();

        public Position TransportOffset => throw new NotImplementedException();

        public float SwimPitch => throw new NotImplementedException();

        public float JumpVerticalSpeed => throw new NotImplementedException();

        public float JumpSinAngle => throw new NotImplementedException();

        public float JumpCosAngle => throw new NotImplementedException();

        public float JumpHorizontalSpeed => throw new NotImplementedException();

        public float SplineElevation => throw new NotImplementedException();

        public float TransportOrientation => throw new NotImplementedException();

        public uint TransportLastUpdated => throw new NotImplementedException();

        public SplineFlags SplineFlags { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Position SplineFinalPoint { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ulong SplineTargetGuid { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public float SplineFinalOrientation { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int SplineTimePassed { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int SplineDuration { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public uint SplineId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public List<Position> SplineNodes { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Position SplineFinalDestination { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Interact()
        {
            rightClickObjectFunction(Pointer, 0);
        }

        protected nint GetDescriptorPtr() => MemoryManager.ReadIntPtr(nint.Add(Pointer, MemoryAddresses.WoWObject_DescriptorOffset));

        public bool IsFacing(Position position)
        {
            throw new NotImplementedException();
        }

        public bool InLosWith(Position position)
        {
            throw new NotImplementedException();
        }

        public bool IsFacing(IWoWObject objc)
        {
            throw new NotImplementedException();
        }

        public bool InLosWith(IWoWObject objc)
        {
            throw new NotImplementedException();
        }

        public bool IsBehind(IWoWUnit unit)
        {
            throw new NotImplementedException();
        }

        public bool IsBehind(IWoWObject target)
        {
            throw new NotImplementedException();
        }
    }
}
