using RaidMemberBot.Game.Statics;
using RaidMemberBot.Helpers.GreyMagic.Internals;
using System;
using System.Runtime.InteropServices;

namespace RaidMemberBot.Mem.Hooks
{
    internal sealed class UnloadWorldHook
    {
        private readonly FuncDelegate _unloadWorldDelegate;
        private readonly Detour _unloadWorldHook;

        private UnloadWorldHook()
        {
            Console.WriteLine("UnloadWorldHook loaded");
            _unloadWorldDelegate =
                Memory.Reader.RegisterDelegate<FuncDelegate>((IntPtr)0x490CE0);
            _unloadWorldHook =
                Memory.Reader.Detours.CreateAndApply(
                    _unloadWorldDelegate,
                    new FuncDelegate(Unregister),
                    "UnloadWorldHook");
        }

        internal static UnloadWorldHook Instance { get; } = new UnloadWorldHook();

        private int Unregister()
        {
            ObjectManager.Instance.DcKillswitch();
            return (int)_unloadWorldHook.CallOriginal();
        }

        /// <summary>
        ///     Delegate for our c# function
        /// </summary>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int FuncDelegate();
    }
}
