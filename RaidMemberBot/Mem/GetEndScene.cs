using RaidMemberBot.ExtensionMethods;
using RaidMemberBot.Helpers.GreyMagic.Internals;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using funcs = RaidMemberBot.Constants.Offsets.Functions;

namespace RaidMemberBot.Mem
{
    internal sealed class GetEndScene
    {
        private static readonly object lockObject = new object();

        private static readonly Lazy<GetEndScene> _instance =
            new Lazy<GetEndScene>(() => new GetEndScene());

        private Direct3D9ISceneEnd _iSceneEndDelegate;
        private Detour _isSceneEndHook;

        private IntPtr EndSceneVTablePtr = IntPtr.Zero;

        internal static GetEndScene Instance
        {
            get
            {
                lock (lockObject)
                {
                    return _instance.Value;
                }
            }
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        internal IntPtr ToVTablePointer()
        {
            if (EndSceneVTablePtr != IntPtr.Zero) return EndSceneVTablePtr;

            _iSceneEndDelegate =
                Memory.Reader.RegisterDelegate<Direct3D9ISceneEnd>(funcs.IsSceneEnd);
            _isSceneEndHook =
                Memory.Reader.Detours.CreateAndApply(
                    _iSceneEndDelegate,
                    new Direct3D9ISceneEnd(IsSceneEndHook),
                    "IsSceneEnd");

            while (EndSceneVTablePtr == IntPtr.Zero)
                Task.Delay(5).Wait();

            return EndSceneVTablePtr;
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private IntPtr IsSceneEndHook(IntPtr unk)
        {
            EndSceneVTablePtr = unk.Add(0x38A8).ReadAs<IntPtr>().ReadAs<IntPtr>().Add(0xa8);
            _isSceneEndHook.Remove();
            return _iSceneEndDelegate(unk);
        }


        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate IntPtr Direct3D9ISceneEnd(IntPtr unk);
    }
}
