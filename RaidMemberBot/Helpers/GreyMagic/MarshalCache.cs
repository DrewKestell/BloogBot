using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Runtime.InteropServices;

namespace RaidMemberBot.Helpers.GreyMagic
{
    internal static class MarshalCache<T>
    {
        /// <summary> The size of the Type </summary>
        internal static int Size;

        /// <summary> The size of the Type </summary>
        internal static uint SizeU;

        /// <summary> The real, underlying type. </summary>
        internal static Type RealType;

        /// <summary> The type code </summary>
        internal static TypeCode TypeCode;

        /// <summary> True if this type requires the Marshaler to map variables. (No direct pointer dereferencing) </summary>
        internal static bool TypeRequiresMarshal;

        internal static bool IsIntPtr;

        internal static readonly GetUnsafePtrDelegate GetUnsafePtr;

        static MarshalCache()
        {
            TypeCode = Type.GetTypeCode(typeof(T));

            // Bools = 1 char.
            if (typeof(T) == typeof(bool))
            {
                Size = 1;
                RealType = typeof(T);
            }
            else if (typeof(T).IsEnum)
            {
                var underlying = typeof(T).GetEnumUnderlyingType();
                Size = Marshal.SizeOf(underlying);
                RealType = underlying;
                TypeCode = Type.GetTypeCode(underlying);
            }
            else
            {
                Size = Marshal.SizeOf(typeof(T));
                RealType = typeof(T);
            }

            IsIntPtr = RealType == typeof(IntPtr);

            SizeU = (uint)Size;

            Debug.Write("[MarshalCache] " + typeof(T) + " Size: " + SizeU);

            // Basically, if any members of the type have a MarshalAs attrib, then we can't just pointer deref. :(
            // This literally means any kind of MarshalAs. Strings, arrays, custom type sizes, etc.
            // Ideally, we want to avoid the Marshaler as much as possible. It causes a lot of overhead, and for a memory reading
            // lib where we need the best speed possible, we do things manually when possible!
            TypeRequiresMarshal =
                RealType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Any(
                    m => m.GetCustomAttributes(typeof(MarshalAsAttribute), true).Any());

            // Generate a method to get the address of a generic type. We'll be using this for RtlMoveMemory later for much faster structure reads.
            var method = new DynamicMethod(string.Format("GetPinnedPtr<{0}>", typeof(T).FullName.Replace(".", "<>")),
                typeof(void*), new[] { typeof(T).MakeByRefType() }, typeof(MarshalCache<>).Module);
            var generator = method.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Conv_U);
            generator.Emit(OpCodes.Ret);
            GetUnsafePtr = (GetUnsafePtrDelegate)method.CreateDelegate(typeof(GetUnsafePtrDelegate));
        }

        #region Nested type: GetUnsafePtrDelegate

        internal unsafe delegate void* GetUnsafePtrDelegate(ref T value);

        #endregion
    }
}
