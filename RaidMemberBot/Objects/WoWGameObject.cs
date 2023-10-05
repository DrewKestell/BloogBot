using RaidMemberBot.Constants;
using RaidMemberBot.ExtensionMethods;
using RaidMemberBot.Mem;
using SharpDX;
using System;

namespace RaidMemberBot.Objects
{
    /// <summary>
    ///     Represents a WoW game object (herbs, veins etc.)
    /// </summary>
    public class WoWGameObject : WoWObject
    {
        /// <summary>
        ///     Constructor taking guid aswell Ptr to object
        /// </summary>
        internal WoWGameObject(ulong parGuid, IntPtr parPointer, Enums.WoWObjectTypes parType)
            : base(parGuid, parPointer, parType)
        {
        }

        internal int Type => GetDescriptor<int>(0x54);

        internal Matrix TransportMatrix
        {
            get
            {
                var pos = Location;
                var facing = Facing;
                var cosFacing = (float)Math.Cos(facing);
                var sinFacing = (float)Math.Sin(facing);

                var posMatrix2 = new Matrix(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1);
                posMatrix2.M41 = posMatrix2.M31 * pos.Z + posMatrix2.M21 * pos.Y + posMatrix2.M11 * pos.X +
                                 posMatrix2.M41;

                posMatrix2.M42 = posMatrix2.M32 * pos.Z + posMatrix2.M22 * pos.Y + posMatrix2.M12 * pos.X +
                                 posMatrix2.M42;

                posMatrix2.M43 = posMatrix2.M33 * pos.Z + posMatrix2.M23 * pos.Y + posMatrix2.M13 * pos.X +
                                 posMatrix2.M43;

                //var facingMatrix2 = new Matrix4x4(cosFacing, -1 * sinFacing, 0, 0, sinFacing, cosFacing, 0, 0, 0, 0, 1,
                //    0, 0, 0, 0, 1);
                var facingMatrix2 = new Matrix(cosFacing, sinFacing, 0, 0, -1 * sinFacing, cosFacing, 0, 0, 0, 0,
                    1 - cosFacing + cosFacing, 0, 0, 0, 0, 1);
                return Matrix.Multiply(facingMatrix2, posMatrix2);
            }
        }

        /// <summary>
        ///     Location
        /// </summary>
        public override Location Location
        {
            get
            {
                try
                {
                    float x;
                    float y;
                    float z;
                    if (Type == 3)
                    {
                        x = GetDescriptor<float>(0x3C);
                        y = GetDescriptor<float>(0x3C + 4);
                        z = GetDescriptor<float>(0x3C + 8);
                        return new Location(x, y, z);
                    }
                    var v2 = Pointer.Add(0x210).ReadAs<int>();
                    IntPtr xyzStruct;
                    if (v2 != 0)
                    {
                        var underlyingFuncPtr = v2.ReadAs<IntPtr>().Add(0x44).ReadAs<int>();
                        switch (underlyingFuncPtr)
                        {
                            case 0x005F5C10:
                                x = (v2 + 0x2c).ReadAs<float>();
                                y = (v2 + 0x2c + 0x4).ReadAs<float>();
                                z = (v2 + 0x2c + 0x8).ReadAs<float>();
                                return new Location(x, y, z);
                            case 0x005F3690:
                                v2 = (int)(v2 + 0x4).ReadAs<IntPtr>().Add(0x110).ReadAs<IntPtr>().Add(0x24);
                                x = v2.ReadAs<float>();
                                y = (v2 + 0x4).ReadAs<float>();
                                z = (v2 + 0x8).ReadAs<float>();
                                return new Location(x, y, z);
                        }
                        xyzStruct = (IntPtr)(v2 + 0x44);
                    }
                    else
                    {
                        xyzStruct = Pointer.Add(0x110).ReadAs<IntPtr>().Add(0x24);
                    }
                    x = xyzStruct.ReadAs<float>();
                    y = xyzStruct.Add(0x4).ReadAs<float>();
                    z = xyzStruct.Add(0x8).ReadAs<float>();
                    return new Location(x, y, z);
                }
                catch
                {
                    return new Location(0, 0, 0);
                }

            }
        }

        /// <inheritdoc />
        public override float Facing
        {
            get
            {
                try
                {
                    var facing = 0f;
                    var v1 = Pointer.Add(0x210).ReadAs<IntPtr>();
                    // ReSharper disable once ConvertIfStatementToReturnStatement
                    if (v1 != IntPtr.Zero)
                    {
                        var v2 = v1.ReadAs<IntPtr>().Add(0x48).ReadAs<IntPtr>();
                        if (v2 == (IntPtr)0x005F36C0)
                        {
                            //return *(*(*(this + 4) + 272) + 48);
                            facing = v1.Add(0x4).ReadAs<IntPtr>().Add(0x110).ReadAs<IntPtr>().Add(0x30).ReadAs<float>();
                        }
                        else
                        {
                            facing = v1.Add(0x50).ReadAs<float>();
                        }
                    }
                    else
                    {
                        facing = Pointer.Add(0x110).ReadAs<IntPtr>().Add(0x30).ReadAs<float>();
                    }
                    //var pi2 = (float)(Math.PI * 2);
                    //if (facing >= pi2)
                    //    facing -= pi2;
                    //else if (facing < 0)
                    //    facing += pi2;

                    return facing;

                }
                catch
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Get gathering info about the gameobject
        /// </summary>
        public GatherInfo GatherInfo
        {
            get
            {
                var gameObjType = GetDescriptor<int>(0x54);
                if (gameObjType != 3)
                {
                    return new GatherInfo { RequiredSkill = 0, Type = Enums.GatherType.None };
                }
                var lockRowId = Pointer.Add(0x214).ReadAs<IntPtr>().Add(0x1c).ReadAs<int>();
                var maxLockRowId = 0xc0dae8.ReadAs<int>();
                if (lockRowId > maxLockRowId)
                {
                    return new GatherInfo { RequiredSkill = 0, Type = Enums.GatherType.None };
                }
                var lockRowPtr = 0xc0dae4.ReadAs<IntPtr>().Add(lockRowId * 4).ReadAs<IntPtr>();
                var isGatherType = lockRowPtr.Add(0x4).ReadAs<int>();
                if (isGatherType != 2)
                {
                    return new GatherInfo { RequiredSkill = 0, Type = Enums.GatherType.None };
                }
                var gatherType = lockRowPtr.Add(0x24).ReadAs<int>();
                var requiredSkill = lockRowPtr.Add(0x11 * 4).ReadAs<int>();
                return new GatherInfo { RequiredSkill = requiredSkill, Type = (Enums.GatherType)gatherType };
            }
        }

        /// <summary>
        ///     Name
        /// </summary>
        public override string Name
        {
            get
            {
                try
                {
                    var ptr1 = ReadRelative<IntPtr>(Offsets.GameObject.NameBase);
                    var ptr2 = Memory.Reader.Read<IntPtr>(IntPtr.Add(ptr1, Offsets.GameObject.NameBasePtr1));
                    return ptr2.ReadString();
                }
                catch (Exception)
                {
                    return "";
                }
            }
        }

        /// <summary>
        ///     The ID of the game object
        /// </summary>
        public int Id => IntPtr.Add(Pointer, 0x294).ReadAs<int>();

        /// <summary>
        ///     Get the owner of the game object
        /// </summary>
        public ulong OwnedBy => GetDescriptor<ulong>(0x18);

        /// <summary>
        ///     Check if the object is bobbing (obviously only working for bobbers)
        /// </summary>
        public bool IsBobbing => ReadRelative<short>(0xE8) == 1;

        /// <summary>
        ///     Interacts with the object
        /// </summary>
        /// <param name="parAutoLoot">shift pressed</param>
        public void Interact(bool parAutoLoot)
        {
            Functions.OnRightClickObject(Pointer, Convert.ToInt32(parAutoLoot));
        }
    }
}
