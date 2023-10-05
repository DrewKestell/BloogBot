using Binarysharp.Assemblers.Fasm;
using RaidMemberBot.Constants;
using RaidMemberBot.ExtensionMethods;
using RaidMemberBot.Helpers;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;

namespace RaidMemberBot.Mem
{
    internal static class Memory
    {
        private static InProcessMemoryReader _Reader;
        private static FasmNet Asm;
        private static bool Applied;

        /// <summary>
        ///     Memory Reader Instance
        /// </summary>
        internal static InProcessMemoryReader Reader
            => _Reader ?? (_Reader = new InProcessMemoryReader(Process.GetCurrentProcess()));

        internal static void ErasePeHeader(string name)
        {
            var handle = WinImports.GetModuleHandle(name);
            ErasePeHeader(handle);
        }

        internal static void ErasePeHeader(IntPtr modulePtr)
        {
            if (modulePtr == IntPtr.Zero) return;
            WinImports.Protection prot;

            var dosHeader = modulePtr.ReadAs<WinImports.IMAGE_DOS_HEADER>();
            var sizeDosHeader = Marshal.SizeOf(typeof(WinImports.IMAGE_DOS_HEADER));
            var sizePeHeader = Marshal.SizeOf(typeof(WinImports.IMAGE_FILE_HEADER));

            var peHeaderPtr = modulePtr.Add(dosHeader.e_lfanew);
            var fileHeader = peHeaderPtr.ReadAs<WinImports.IMAGE_FILE_HEADER>();

            var optionalHeaderSize = fileHeader.mSizeOfOptionalHeader;
            if (optionalHeaderSize != 0)
            {
                var optionalHeaderPtr = modulePtr.Add(dosHeader.e_lfanew).Add(sizePeHeader);
                var optionalHeader = optionalHeaderPtr.ReadAs<WinImports.IMAGE_OPTIONAL_HEADER32>();

                WinImports.VirtualProtect(optionalHeaderPtr, (uint)optionalHeaderSize, WinImports.Protection.PAGE_EXECUTE_READWRITE, out prot);
                for (var i = 0; i < optionalHeaderSize; i++)
                {
                    optionalHeaderPtr.Add(i).WriteTo<byte>(0);
                }
                WinImports.VirtualProtect(optionalHeaderPtr, (uint)optionalHeaderSize, prot, out prot);
            }

            WinImports.VirtualProtect(modulePtr, (uint)sizeDosHeader, WinImports.Protection.PAGE_EXECUTE_READWRITE, out prot);
            for (var i = 0; i < sizeDosHeader; i++)
            {
                modulePtr.Add(i).WriteTo<byte>(0);
            }
            WinImports.VirtualProtect(modulePtr, (uint)sizeDosHeader, prot, out prot);

            WinImports.VirtualProtect(peHeaderPtr, (uint)sizePeHeader, WinImports.Protection.PAGE_EXECUTE_READWRITE, out prot);
            for (var i = 0; i < sizePeHeader; i++)
            {
                peHeaderPtr.Add(i).WriteTo<byte>(0);
            }
            WinImports.VirtualProtect(modulePtr, (uint)sizeDosHeader, prot, out prot);
        }

        internal static void UnlinkFromPeb(string moduleName)
        {
            var store = Reader.Alloc(4);
            var addrToAsm = InjectAsm(new[]
            {
                "push ebp",
                "mov ebp, esp",
                "pushad",
                "mov eax, [fs:48]",
                "mov [0x" + store.ToString("X") + "], eax",
                "popad",
                "mov esp, ebp",
                "pop ebp",
                "retn"
            }, "GetPeb");
            var callAsm = Reader.RegisterDelegate<NoParamFunc>(addrToAsm);
            callAsm();
            var pebPtr = store.ReadAs<IntPtr>();
            Reader.Dealloc(store);
            Reader.Dealloc(addrToAsm);
            var ldrData = pebPtr.Add(12).ReadAs<IntPtr>().ReadAs<WinImports.PEB_LDR_DATA>();

            var startModulePtr = ldrData.InInitOrderModuleListPtr;
            var curEntry = ldrData.InInitOrderModuleList;
            while (true)
            {
                var curEntryPtr = curEntry.Header.Flink;
                curEntry = curEntry.Header.Fwd;
                if (curEntryPtr == startModulePtr) break;
                var nextModule = curEntry.Header.Flink;
                var prevModule = curEntry.Header.Blink;
                if (curEntry.Body.BaseDllName != moduleName) continue;
                prevModule.WriteTo(nextModule);
                nextModule.Add(4).WriteTo(prevModule);
                break;
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void NoParamFunc();


        internal static IntPtr InjectAsm(string[] parInstructions, string parPatchName)
        {
            if (Asm == null) Asm = new FasmNet();
            Asm.Clear();
            Asm.AddLine("use32");
            foreach (var x in parInstructions)
                Asm.AddLine(x);

            var byteCode = new byte[0];
            try
            {
                byteCode = Asm.Assemble();
            }
            catch (FasmAssemblerException ex)
            {
                MessageBox.Show(
                    $"Error definition: {ex.ErrorCode}; Error code: {(int)ex.ErrorCode}; Error line: {ex.ErrorLine}; Error offset: {ex.ErrorOffset}; Mnemonics: {ex.Mnemonics}");
            }

            var start = Reader.Alloc(byteCode.Length);
            Asm.Clear();
            Asm.AddLine("use32");
            foreach (var x in parInstructions)
                Asm.AddLine(x);
            byteCode = Asm.Assemble(start);

            var originalBytes = Reader.ReadBytes(start, byteCode.Length);
            if (parPatchName != "")
            {
                var parHack = new Hack(start,
                    byteCode,
                    originalBytes, parPatchName);

                parHack.Apply();
            }
            else
            {
                Reader.WriteBytes(start, byteCode);
            }
            return start;
        }
        internal static void InjectAsm(uint parPtr, string parInstructions, string parPatchName)
        {
            Asm.Clear();
            Asm.AddLine("use32");
            Asm.AddLine(parInstructions);
            var start = new IntPtr(parPtr);

            byte[] byteCode;
            try
            {
                byteCode = Asm.Assemble(start);
            }
            catch (FasmAssemblerException ex)
            {
                MessageBox.Show(
                    $"Error definition: {ex.ErrorCode}; Error code: {(int)ex.ErrorCode}; Error line: {ex.ErrorLine}; Error offset: {ex.ErrorOffset}; Mnemonics: {ex.Mnemonics}");
                return;
            }

            var originalBytes = Reader.ReadBytes(start, byteCode.Length);
            if (parPatchName != "")
            {
                var parHack = new Hack(start,
                    byteCode,
                    originalBytes, parPatchName);
                parHack.Apply();
            }
            else
            {
                Reader.WriteBytes(start, byteCode);
            }
        }
    }
}
