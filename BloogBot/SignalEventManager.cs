using BloogBot.Game;
using System;
using System.Runtime.InteropServices;

namespace BloogBot
{
    public class SignalEventManager
    {
        delegate void SignalEventDelegate(string eventName, string format, uint firstArgPtr);
        delegate void SignalEventNoArgsDelegate(string eventName);

        static SignalEventManager()
        {
            //InitializeSignalEventHook();
            //InitializeSignalEventHookNoArgs();
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
                $"call 0x{((uint) addrToDetour).ToString("X")}",
                "popad",
                "popfd",
                $"jmp 0x{((uint) (MemoryAddresses.SignalEventFunPtr + 7)).ToString("X")}"
            };
            var signalEventDetour = MemoryManager.InjectAssembly("SignalEventDetour", instructions);
            MemoryManager.InjectAssembly("SignalEventHook", (uint)MemoryAddresses.SignalEventFunPtr, "jmp " + signalEventDetour);
        }

        static void SignalEventHook(string eventName, string typesArg, uint firstArgPtr)
        {
            Logger.LogVerbose(eventName);

            var types = typesArg.TrimStart('%').Split('%');
            var list = new object[types.Length];
            for (var i = 0; i < types.Length; i++)
            {
                var tmpPtr = firstArgPtr + (uint)i * 4;
                if (types[i] == "s")
                {
                    var ptr = MemoryManager.ReadInt((IntPtr)tmpPtr);
                    var str = MemoryManager.ReadString((IntPtr)ptr);
                    if (!string.IsNullOrWhiteSpace(str))
                        Logger.LogVerbose(str);
                    else
                        Logger.LogVerbose("null");
                    list[i] = str;
                }
                else if (types[i] == "f")
                {
                    var val = MemoryManager.ReadFloat((IntPtr)tmpPtr);
                    Logger.LogVerbose(val);
                    list[i] = val;
                }
                else if (types[i] == "u")
                {
                    var val = MemoryManager.ReadUint((IntPtr)tmpPtr);
                    Logger.LogVerbose(val);
                    list[i] = val;
                }
                else if (types[i] == "d")
                {
                    var val = MemoryManager.ReadInt((IntPtr)tmpPtr);
                    Logger.LogVerbose(val);
                    list[i] = val;
                }
                else if (types[i] == "b")
                {
                    var val = MemoryManager.ReadInt((IntPtr)tmpPtr);
                    Logger.LogVerbose(val);
                    list[i] = Convert.ToBoolean(val);
                }
            }

            Logger.LogVerbose("");

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
                $"call 0x{((uint) addrToDetour).ToString("X")}",
                "popad",
                "popfd",
                $"jmp 0x{((uint) MemoryAddresses.SignalEventNoParamsFunPtr + 6).ToString("X")}"
            };
            var signalEventNoArgsDetour = MemoryManager.InjectAssembly("SignalEventNoArgsDetour", instructions);
            MemoryManager.InjectAssembly("SignalEventNoArgsHook", (uint)MemoryAddresses.SignalEventNoParamsFunPtr, "jmp " + signalEventNoArgsDetour);
        }

        static void SignalEventNoArgsHook(string eventName)
        {
            Logger.LogVerbose(eventName + "\n");
            OnNewSignalEventNoArgs?.Invoke(eventName);
        }

        internal delegate void SignalEventNoArgsEventHandler(string parEvent, params object[] parArgs);

        internal static event SignalEventNoArgsEventHandler OnNewSignalEventNoArgs;
        #endregion
    }
}
