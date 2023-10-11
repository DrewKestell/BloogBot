using RaidMemberBot.Constants;
using RaidMemberBot.ExtensionMethods;
using System;
using System.Runtime.InteropServices;

namespace RaidMemberBot.Mem.Hooks
{
    public class SignalEventManager
    {
        /// <summary>
        ///     Delegate for our c# function
        /// </summary>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate void SignalEventDelegate(string eventName, string format, uint firstArgPtr);

        /// <summary>
        ///     Delegate for our c# function
        /// </summary>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate void SignalEventNoArgsDelegate(string eventName);

        static SignalEventManager()
        {
            InitializeSignalEventHook();
            InitializeSignalEventHookNoArgs();
        }

        #region InitializeSignalEventHook
        static SignalEventDelegate signalEventDelegate;

        static void InitializeSignalEventHook()
        {
            signalEventDelegate = new SignalEventDelegate(SignalEventHook);
            var addrToDetour = Marshal.GetFunctionPointerForDelegate(signalEventDelegate);

            var instructions = new[]
            {
                "push ebx",
                "push esi",
                "call 0x007040D0",
                "pushfd",
                "pushad",
                "mov eax, ebp",
                "add eax, 0x10",
                "push eax",
                "mov eax, [ebp + 0xC]",
                "push eax",
                "mov edi, [edi]",
                "push edi",
                $"call 0x{(uint) addrToDetour:X}",
                "popad",
                "popfd",
                $"jmp 0x{(uint) (Offsets.Hooks.SignalEvent + 7):X}"
            };
            // Inject the asm code which calls our c# function
            var codeCave = Memory.InjectAsm(instructions, "EventSignalDetour");
            // set the jmp from WoWs code to my injected code
            Memory.InjectAsm((uint)Offsets.Hooks.SignalEvent, "jmp " + codeCave, "EventSignalDetourJmp");
        }

        static void SignalEventHook(string eventName, string typesArg, uint firstArgPtr)
        {
            var types = typesArg.TrimStart('%').Split('%');
            var list = new object[types.Length];
            for (var i = 0; i < types.Length; i++)
            {
                var tmpPtr = firstArgPtr + (uint)i * 4;
                if (types[i] == "s")
                {
                    var ptr = ((IntPtr)tmpPtr).ReadAs<int>();
                    var str = ((IntPtr)ptr).ReadString();

                    list[i] = str;
                }
                else if (types[i] == "f")
                {
                    var val = ((IntPtr)tmpPtr).ReadAs<float>();
                    list[i] = val;
                }
                else if (types[i] == "u")
                {
                    var val = ((IntPtr)tmpPtr).ReadAs<uint>();
                    list[i] = val;
                }
                else if (types[i] == "d")
                {
                    var val = ((IntPtr)tmpPtr).ReadAs<int>();
                    list[i] = val;
                }
                else if (types[i] == "b")
                {
                    var val = ((IntPtr)tmpPtr).ReadAs<int>();
                    list[i] = Convert.ToBoolean(val);
                }
            }

            OnNewEventSignalEvent(eventName, list);
        }

        static internal void OnNewEventSignalEvent(string parEvent, params object[] parList) =>
            OnNewSignalEvent?.Invoke(parEvent, parList);

        internal delegate void SignalEventEventHandler(string parEvent, params object[] parArgs);

        internal static event SignalEventEventHandler OnNewSignalEvent;
        #endregion

        #region InitializeSignalEventHookNoArgs
        static SignalEventNoArgsDelegate signalEventNoArgsDelegate;

        static void InitializeSignalEventHookNoArgs()
        {
            signalEventNoArgsDelegate = new SignalEventNoArgsDelegate(SignalEventNoArgsHook);
            var addrToDetour = Marshal.GetFunctionPointerForDelegate(signalEventNoArgsDelegate);

            var instructions = new[]
            {
                "push esi",
                "call 0x007040D0",
                "pushfd",
                "pushad",
                "mov edi, [edi]",
                "push edi",
                $"call 0x{(uint) addrToDetour:X}",
                "popad",
                "popfd",
                $"jmp 0x{(uint) (Offsets.Hooks.SignalEvent_0 + 6):X}"
            };
            // Inject the asm code which calls our c# function
            var codeCave = Memory.InjectAsm(instructions, "EventSignal_0Detour");
            // set the jmp from WoWs code to my injected code
            Memory.InjectAsm((uint)Offsets.Hooks.SignalEvent_0, "jmp " + codeCave, "EventSignal_0DetourJmp");
        }

        static void SignalEventNoArgsHook(string eventName)
        {
            OnNewSignalEventNoArgs?.Invoke(eventName);
        }

        internal delegate void SignalEventNoArgsEventHandler(string parEvent, params object[] parArgs);

        internal static event SignalEventNoArgsEventHandler OnNewSignalEventNoArgs;
        #endregion
    }
}
