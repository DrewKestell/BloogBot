using RaidMemberBot.Constants;
using RaidMemberBot.Mem;
using System;

namespace RaidMemberBot.Objects
{
    /// <summary>
    ///     Represents a basic WoW object
    /// </summary>
    public class WoWObject
    {
        /// <summary>
        ///     Constructor taking guid aswell Ptr to object
        /// </summary>
        internal WoWObject(ulong parGuid, IntPtr parPointer, Enums.WoWObjectTypes parType)
        {
            Guid = parGuid;
            Pointer = parPointer;
            WoWType = parType;
        }

        internal IntPtr Pointer { get; set; }
        internal bool CanRemove { get; set; }

        /// <summary>
        ///     GUID of the object
        /// </summary>
        public ulong Guid { get; private set; }

        /// <summary>
        ///     The type of the object
        /// </summary>
        public Enums.WoWObjectTypes WoWType { get; private set; }

        /// <summary>
        /// Facing of the object
        /// </summary>
        public virtual float Facing { get; set; }

        /// <summary>
        ///     Location
        /// </summary>
        public virtual Location Location { get; internal set; }

        /// <summary>
        ///     Name of object
        /// </summary>
        public virtual string Name { get; internal set; }

        /// <summary>
        ///     Get descriptor function to avoid some code
        /// </summary>
        internal T GetDescriptor<T>(int descriptor) where T : struct
        {
            uint ptr = Memory.Reader.Read<uint>(IntPtr.Add(Pointer, Offsets.ObjectManager.DescriptorOffset));
            return Memory.Reader.Read<T>(new IntPtr(ptr + descriptor));
        }

        internal void SetDescriptor<T>(int descriptor, T parValue) where T : struct
        {
            uint ptr = Memory.Reader.Read<uint>(IntPtr.Add(Pointer, Offsets.ObjectManager.DescriptorOffset));
            Memory.Reader.Write(new IntPtr(ptr + descriptor), parValue);
        }

        /// <summary>
        ///     Read relative to base pointer
        /// </summary>
        internal T ReadRelative<T>(int offset) where T : struct
        {
            return Memory.Reader.Read<T>(IntPtr.Add(Pointer, offset));
        }
    }
}
