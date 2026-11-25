using BloogBot.Game.Enums;
using System;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;

namespace BloogBot.Game.Objects
{
    public unsafe abstract class WoWObject
    {
        public virtual IntPtr Pointer { get; set; }
        public virtual ulong Guid { get; set; }
        public virtual ObjectType ObjectType { get; set; }

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        delegate void GetPositionDelegate(IntPtr objectPtr, ref XYZ pos);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        delegate float GetFacingDelegate(IntPtr objectPtr);

        // used for interacting in vanilla
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        delegate void RightClickObjectDelegate(IntPtr unitPtr, int autoLoot);
        
        // used for interacting in others
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        delegate void InteractDelegate(IntPtr objectPtr);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        delegate IntPtr GetNameDelegate(IntPtr objectPtr);

        GetPositionDelegate getPositionFunction;

        GetFacingDelegate getFacingFunction;

        RightClickObjectDelegate rightClickObjectFunction;

        InteractDelegate interactFunction;

        GetNameDelegate getNameFunction;

        public WoWObject() { }

        public WoWObject(IntPtr pointer, ulong guid, ObjectType objectType)
        {
            Pointer = pointer;
            Guid = guid;
            ObjectType = objectType;

            InitializeFunctions(pointer);
        }

        void InitializeFunctions(IntPtr pointer)
        {
            if (pointer == IntPtr.Zero)
                return;

            // TODO: I can't figure out how to get the vtable addresses for the Vanilla client (or if they even exist) so we do it this way for now
            if (ClientHelper.ClientVersion != ClientVersion.Vanilla)
            {
                var vTableAddr = MemoryManager.ReadIntPtr(pointer);
                if (vTableAddr == IntPtr.Zero)
                    return;

                var getPositionAddr = IntPtr.Add(vTableAddr, MemoryAddresses.WoWObject_GetPositionFunOffset);
                var getPositionFunPtr = MemoryManager.ReadIntPtr(getPositionAddr);
                if (getPositionFunPtr != IntPtr.Zero)
                    getPositionFunction = Marshal.GetDelegateForFunctionPointer<GetPositionDelegate>(getPositionFunPtr);

                var getFacingAddr = IntPtr.Add(vTableAddr, MemoryAddresses.WoWObject_GetFacingFunOffset);
                var getFacingFunPtr = MemoryManager.ReadIntPtr(getFacingAddr);
                if (getFacingFunPtr != IntPtr.Zero)
                    getFacingFunction = Marshal.GetDelegateForFunctionPointer<GetFacingDelegate>(getFacingFunPtr);

                var interactAddr = IntPtr.Add(vTableAddr, MemoryAddresses.WoWObject_InteractFunOffset);
                var interactFunPtr = MemoryManager.ReadIntPtr(interactAddr);
                if (interactFunPtr != IntPtr.Zero)
                    interactFunction = Marshal.GetDelegateForFunctionPointer<InteractDelegate>(interactFunPtr);

                var getNameAddr = IntPtr.Add(vTableAddr, MemoryAddresses.WoWObject_GetNameFunOffset);
                var getNameFunPtr = MemoryManager.ReadIntPtr(getNameAddr);
                if (getNameFunPtr != IntPtr.Zero)
                    getNameFunction = Marshal.GetDelegateForFunctionPointer<GetNameDelegate>(getNameFunPtr);
            }
            else if (rightClickObjectFunction == null)
            {
                rightClickObjectFunction = Marshal.GetDelegateForFunctionPointer<RightClickObjectDelegate>((IntPtr)0x60BEA0);
            }
        }

        internal void UpdatePointer(IntPtr pointer)
        {
            if (pointer == IntPtr.Zero || pointer == Pointer)
                return;

            Pointer = pointer;
            InitializeFunctions(pointer);
            OnPointerChanged();
        }

        internal void Invalidate()
        {
            if (Pointer == IntPtr.Zero)
                return;

            Pointer = IntPtr.Zero;
            OnPointerChanged();
        }

        protected virtual void OnPointerChanged()
        {
        }

        IntPtr ReadDescriptorPtr() =>
            Pointer == IntPtr.Zero
                ? IntPtr.Zero
                : MemoryManager.ReadIntPtr(IntPtr.Add(Pointer, MemoryAddresses.WoWObject_DescriptorOffset));

        internal bool IsValid
        {
            get
            {
                if (Pointer == IntPtr.Zero || Guid == 0)
                    return false;

                var descriptorPtr = ReadDescriptorPtr();
                return descriptorPtr != IntPtr.Zero && MemoryManager.ReadUlong(descriptorPtr) == Guid;
            }
        }

        public virtual Position Position => GetPosition();

        [HandleProcessCorruptedStateExceptions]
        Position GetPosition()
        {
            try
            {
                if (!IsValid)
                    return new Position(0, 0, 0);

                if (ClientHelper.ClientVersion == ClientVersion.Vanilla)
                {
                    if (ObjectType == ObjectType.Unit || ObjectType == ObjectType.Player)
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
                            var underlyingFuncPtr = MemoryManager.ReadInt(IntPtr.Add(MemoryManager.ReadIntPtr((IntPtr)v2), 0x44));
                            switch (underlyingFuncPtr)
                            {
                                case 0x005F5C10:
                                    x = MemoryManager.ReadFloat((IntPtr)(v2 + 0x2c));
                                    y = MemoryManager.ReadFloat((IntPtr)(v2 + 0x2c + 0x4));
                                    z = MemoryManager.ReadFloat((IntPtr)(v2 + 0x2c + 0x8));
                                    return new Position(x, y, z);
                                case 0x005F3690:
                                    v2 = (int)IntPtr.Add(MemoryManager.ReadIntPtr(IntPtr.Add(MemoryManager.ReadIntPtr((IntPtr)(v2 + 0x4)), 0x110)), 0x24);
                                    x = MemoryManager.ReadFloat((IntPtr)v2);
                                    y = MemoryManager.ReadFloat((IntPtr)(v2 + 0x4));
                                    z = MemoryManager.ReadFloat((IntPtr)(v2 + 0x8));
                                    return new Position(x, y, z);
                            }
                            xyzStruct = (IntPtr)(v2 + 0x44);
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
                else
                {
                    if (getPositionFunction == null)
                        return new Position(0, 0, 0);

                    var xyz = new XYZ();
                    getPositionFunction(Pointer, ref xyz);

                    return new Position(xyz);
                }
                
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
                if (!IsValid)
                    return 0;

                if (ClientHelper.ClientVersion == ClientVersion.Vanilla)
                {
                    if (ObjectType == ObjectType.Player || ObjectType == ObjectType.Unit)
                    {
                        return MemoryManager.ReadFloat(Pointer + 0x9C4);
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    if (getFacingFunction == null)
                        return 0;

                    return getFacingFunction(Pointer);
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
        string GetName()
        {
            try
            {
                if (!IsValid)
                    return "";

                if (ClientHelper.ClientVersion == ClientVersion.Vanilla)
                {
                    if (ObjectType == ObjectType.Player)
                    {
                        var namePtr = MemoryManager.ReadIntPtr((IntPtr)0xC0E230);
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
                    else if (ObjectType == ObjectType.Unit)
                    {
                        var ptr1 = MemoryManager.ReadInt(IntPtr.Add(Pointer, 0xB30));
                        var ptr2 = MemoryManager.ReadInt((IntPtr)ptr1);
                        return MemoryManager.ReadString((IntPtr)ptr2);
                    }
                    else
                    {
                        var ptr1 = MemoryManager.ReadIntPtr(IntPtr.Add(Pointer, 0x214));
                        var ptr2 = MemoryManager.ReadIntPtr(IntPtr.Add(ptr1, 0x8));
                        return MemoryManager.ReadString(ptr2);
                    }
                }
                else
                {
                    if (getNameFunction == null)
                        return "";

                    var ptr = getNameFunction(Pointer);

                    if (ptr != IntPtr.Zero)
                        return MemoryManager.ReadString(ptr);
                    else
                        return "";
                }
                
            }
            catch (AccessViolationException)
            {
                Console.WriteLine("Access violation on WoWObject.Name. Swallowing.");
                return "";
            }
        }
        
        [HandleProcessCorruptedStateExceptions]
        public void Interact()
        {
            try
            {
                if (!IsValid)
                    return;

                if (ClientHelper.ClientVersion == ClientVersion.Vanilla)
                {
                    rightClickObjectFunction?.Invoke(Pointer, 0);
                }
                else
                {
                    interactFunction?.Invoke(Pointer);
                }
            }
            catch (AccessViolationException)
            {
                Console.WriteLine("Access violation on WoWObject.Interact. Swallowing.");
            }
        }

        protected IntPtr GetDescriptorPtr()
        {
            var descriptorPtr = ReadDescriptorPtr();
            if (descriptorPtr == IntPtr.Zero)
                return IntPtr.Zero;

            if (Guid != 0 && MemoryManager.ReadUlong(descriptorPtr) != Guid)
                return IntPtr.Zero;

            return descriptorPtr;
        }
    }
}
