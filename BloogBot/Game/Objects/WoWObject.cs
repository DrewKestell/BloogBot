using BloogBot.Game.Enums;
using System;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;

namespace BloogBot.Game.Objects
{
    public unsafe abstract class WoWObject
    {
        public readonly IntPtr Pointer;
        public readonly ulong Guid;
        public readonly ObjectType ObjectType;

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        delegate void GetPositionDelegate(IntPtr objectPtr, ref XYZ pos);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        delegate float GetFacingDelegate(IntPtr objectPtr);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        delegate void InteractDelegate(IntPtr objectPtr);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        delegate IntPtr GetNameDelegate(IntPtr objectPtr);

        readonly GetPositionDelegate getPositionFunction;

        readonly GetFacingDelegate getFacingFunction;

        readonly InteractDelegate interactFunction;

        readonly GetNameDelegate getNameFunction;

        internal WoWObject(IntPtr pointer, ulong guid, ObjectType objectType)
        {
            Pointer = pointer;
            Guid = guid;
            ObjectType = objectType;

            var vTableAddr = MemoryManager.ReadIntPtr(pointer);

            var getPositionAddr = IntPtr.Add(vTableAddr, MemoryAddresses.WoWObject_GetPositionFunOffset);
            var getPositionFunPtr = MemoryManager.ReadIntPtr(getPositionAddr);
            getPositionFunction = Marshal.GetDelegateForFunctionPointer<GetPositionDelegate>(getPositionFunPtr);

            var getFacingAddr = IntPtr.Add(vTableAddr, MemoryAddresses.WoWObject_GetFacingFunOffset);
            var getFacingFunPtr = MemoryManager.ReadIntPtr(getFacingAddr);
            getFacingFunction = Marshal.GetDelegateForFunctionPointer<GetFacingDelegate>(getFacingFunPtr);

            var interactAddr = IntPtr.Add(vTableAddr, MemoryAddresses.WoWObject_InteractFunOffset);
            var interactFunPtr = MemoryManager.ReadIntPtr(interactAddr);
            interactFunction = Marshal.GetDelegateForFunctionPointer<InteractDelegate>(interactFunPtr);

            var getNameAddr = IntPtr.Add(vTableAddr, MemoryAddresses.WoWObject_GetNameFunOffset);
            var getNameFunPtr = MemoryManager.ReadIntPtr(getNameAddr);
            getNameFunction = Marshal.GetDelegateForFunctionPointer<GetNameDelegate>(getNameFunPtr);
        }

        public Position Position => GetPosition();

        [HandleProcessCorruptedStateExceptions]
        Position GetPosition()
        {
            try
            {
                var xyz = new XYZ();
                getPositionFunction(Pointer, ref xyz);

                return new Position(xyz);
            }
            catch (AccessViolationException)
            {
                Console.WriteLine("Access violation on WoWObject.Position. Swallowing.");
                return new Position(0, 0, 0);
            }
        }

        public float Facing => GetFacing();

        [HandleProcessCorruptedStateExceptions]
        float GetFacing()
        {
            try
            {
                return getFacingFunction(Pointer);
            }
            catch (AccessViolationException)
            {
                Console.WriteLine("Access violation on WoWObject.Facing. Swallowing.");
                return 0;
            }
        }

        public string Name => GetName();

        [HandleProcessCorruptedStateExceptions]
        string GetName()
        {
            try
            {
                var ptr = getNameFunction(Pointer);

                if (ptr != IntPtr.Zero)
                    return MemoryManager.ReadString(ptr);
                else
                    return "";
            }
            catch (AccessViolationException)
            {
                Console.WriteLine("Access violation on WoWObject.Name. Swallowing.");
                return "";
            }
        }
        
        public void Interact() => interactFunction(Pointer);

        [HandleProcessCorruptedStateExceptions]
        void TryInteract()
        {
            try
            {
                TryInteract();
            }
            catch (AccessViolationException)
            {
                Console.WriteLine("Access violation on WoWObject.Name. Swallowing.");
            }
        }

        protected IntPtr GetDescriptorPtr() => MemoryManager.ReadIntPtr(IntPtr.Add(Pointer, MemoryAddresses.WoWObject_DescriptorOffset));
    }
}
