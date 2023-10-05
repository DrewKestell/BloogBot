using System;
using System.IO;
using System.Runtime.InteropServices;

namespace RaidMemberBot.Helpers.GreyMagic.Native
{
    /// <summary>
    ///     A class to extract PE header information from modules or PE files.
    /// </summary>
    internal class PeHeaderParser
    {
        /// <summary>
        ///     The handle, or base address, to the current PE file.
        /// </summary>
        internal IntPtr ModulePtr;

        /// <summary>
        ///     Creates a new instance of the PeHeaderParser class, using the specified path to a PE file.
        /// </summary>
        /// <param name="peFile"></param>
        internal PeHeaderParser(string peFile)
        {
            // Yes, I know, this is some delicious copy/pasta.
            ModulePtr = Imports.LoadLibrary(peFile);

            if (ModulePtr == IntPtr.Zero)
                throw new FileNotFoundException();

            ParseHeaders();
            Imports.FreeLibrary(ModulePtr);
        }

        /// <summary>
        ///     Creates a new instance of the PeHeaderParser class, using the handle or base address, to the specified module.
        /// </summary>
        /// <param name="hModule">The hModule.</param>
        internal PeHeaderParser(IntPtr hModule)
        {
            if (hModule == IntPtr.Zero)
                throw new FileNotFoundException();

            ModulePtr = hModule;
            ParseHeaders();
        }

        /// <summary>
        ///     Retrieves the IMAGE_DOS_HEADER for this PE file.
        /// </summary>
        internal ImageDosHeader DosHeader { get; private set; }

        /// <summary>
        ///     Retrieves the IMAGE_NT_HEADER for this PE file. (This includes and nested structs, etc)
        /// </summary>
        internal ImageNtHeader NtHeader { get; private set; }

        private void ParseHeaders()
        {
            DosHeader = (ImageDosHeader)Marshal.PtrToStructure(ModulePtr, typeof(ImageDosHeader));

            if (DosHeader.e_magic == PeHeaderConstants.IMAGE_DOS_SIGNATURE)
                NtHeader =
                    (ImageNtHeader)Marshal.PtrToStructure(ModulePtr + DosHeader.e_lfanew, typeof(ImageNtHeader));
        }

        // ReSharper disable InconsistentNaming

        #region Nested type: PeHeaderConstants

        /// <summary>
        ///     Contains constants ripped from WinNT.h
        /// </summary>
        internal class PeHeaderConstants
        {
            internal const int IMAGE_DOS_SIGNATURE = 0x5A4D;
            internal const int IMAGE_FILE_32BIT_MACHINE = 0x0100;
            internal const int IMAGE_FILE_AGGRESIVE_WS_TRIM = 0x0010;
            internal const int IMAGE_FILE_BYTES_REVERSED_HI = 0x8000;
            internal const int IMAGE_FILE_BYTES_REVERSED_LO = 0x0080;
            internal const int IMAGE_FILE_DEBUG_STRIPPED = 0x0200;
            internal const int IMAGE_FILE_DLL = 0x2000;
            internal const int IMAGE_FILE_EXECUTABLE_IMAGE = 0x0002;
            internal const int IMAGE_FILE_LARGE_ADDRESS_AWARE = 0x0020;
            internal const int IMAGE_FILE_LINE_NUMS_STRIPPED = 0x0004;
            internal const int IMAGE_FILE_LOCAL_SYMS_STRIPPED = 0x0008;
            internal const int IMAGE_FILE_MACHINE_ALPHA = 0x0184;
            internal const int IMAGE_FILE_MACHINE_ALPHA64 = 0x0284;
            internal const int IMAGE_FILE_MACHINE_AM33 = 0x01d3;
            internal const int IMAGE_FILE_MACHINE_AMD64 = 0x8664;
            internal const int IMAGE_FILE_MACHINE_ARM = 0x01c0;
            internal const int IMAGE_FILE_MACHINE_CEE = 0xC0EE;
            internal const int IMAGE_FILE_MACHINE_CEF = 0x0CEF;
            internal const int IMAGE_FILE_MACHINE_EBC = 0x0EBC;
            internal const int IMAGE_FILE_MACHINE_I386 = 0x014c;
            internal const int IMAGE_FILE_MACHINE_IA64 = 0x0200;
            internal const int IMAGE_FILE_MACHINE_M32R = 0x9041;
            internal const int IMAGE_FILE_MACHINE_MIPS16 = 0x0266;
            internal const int IMAGE_FILE_MACHINE_MIPSFPU = 0x0366;
            internal const int IMAGE_FILE_MACHINE_MIPSFPU16 = 0x0466;
            internal const int IMAGE_FILE_MACHINE_POWERPC = 0x01F0;
            internal const int IMAGE_FILE_MACHINE_POWERPCFP = 0x01f1;
            internal const int IMAGE_FILE_MACHINE_R10000 = 0x0168;
            internal const int IMAGE_FILE_MACHINE_R3000 = 0x0162;
            internal const int IMAGE_FILE_MACHINE_R4000 = 0x0166;
            internal const int IMAGE_FILE_MACHINE_SH3 = 0x01a2;
            internal const int IMAGE_FILE_MACHINE_SH3DSP = 0x01a3;
            internal const int IMAGE_FILE_MACHINE_SH3E = 0x01a4;
            internal const int IMAGE_FILE_MACHINE_SH4 = 0x01a6;
            internal const int IMAGE_FILE_MACHINE_SH5 = 0x01a8;
            internal const int IMAGE_FILE_MACHINE_THUMB = 0x01c2;
            internal const int IMAGE_FILE_MACHINE_TRICORE = 0x0520;
            internal const int IMAGE_FILE_MACHINE_UNKNOWN = 0;
            internal const int IMAGE_FILE_MACHINE_WCEMIPSV2 = 0x0169;
            internal const int IMAGE_FILE_NET_RUN_FROM_SWAP = 0x0800;
            internal const int IMAGE_FILE_RELOCS_STRIPPED = 0x0001;
            internal const int IMAGE_FILE_REMOVABLE_RUN_FROM_SWAP = 0x0400;
            internal const int IMAGE_FILE_SYSTEM = 0x1000;
            internal const int IMAGE_FILE_UP_SYSTEM_ONLY = 0x4000;
            internal const int IMAGE_NT_OPTIONAL_HDR32_MAGIC = 0x10b;
            internal const int IMAGE_NT_OPTIONAL_HDR64_MAGIC = 0x20b;
            internal const int IMAGE_NT_SIGNATURE = 0x50450000;
            internal const int IMAGE_OS2_SIGNATURE = 0x4E45;
            internal const int IMAGE_OS2_SIGNATURE_LE = 0x4C45;
            internal const int IMAGE_ROM_OPTIONAL_HDR_MAGIC = 0x107;
            internal const int IMAGE_SIZEOF_FILE_HEADER = 20;
        }

        #endregion

        // ReSharper restore InconsistentNaming

        #region PE Header Shit

        #region Nested type: ImageDataDirectory

        [StructLayout(LayoutKind.Sequential)]
        internal struct ImageDataDirectory
        {
            internal uint VirtualAddress;
            internal uint Size;
        }

        #endregion

        #region Nested type: ImageDosHeader

        [StructLayout(LayoutKind.Sequential)]
        internal struct ImageDosHeader
        {
            internal ushort e_magic; // Magic number
            internal ushort e_cblp; // Bytes on last page of file
            internal ushort e_cp; // Pages in file
            internal ushort e_crlc; // Relocations
            internal ushort e_cparhdr; // Size of header in paragraphs
            internal ushort e_minalloc; // Minimum extra paragraphs needed
            internal ushort e_maxalloc; // Maximum extra paragraphs needed
            internal ushort e_ss; // Initial (relative) SS value
            internal ushort e_sp; // Initial SP value
            internal ushort e_csum; // Checksum
            internal ushort e_ip; // Initial IP value
            internal ushort e_cs; // Initial (relative) CS value
            internal ushort e_lfarlc; // File address of relocation table
            internal ushort e_ovno; // Overlay number

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] internal ushort[] e_res1; // Reserved words

            internal ushort e_oemid; // OEM identifier (for e_oeminfo)
            internal ushort e_oeminfo; // OEM information; e_oemid specific

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)] internal ushort[] e_res2; // Reserved words

            internal int e_lfanew; // File address of new exe header
        }

        #endregion

        #region Nested type: ImageFileHeader

        [StructLayout(LayoutKind.Sequential)]
        internal struct ImageFileHeader
        {
            internal ushort Machine;
            internal ushort NumberOfSections;
            internal uint TimeDateStamp;
            internal uint PointerToSymbolTable;
            internal uint NumberOfSymbols;
            internal ushort SizeOfOptionalHeader;
            internal ushort Characteristics;
        }

        #endregion

        #region Nested type: ImageNtHeader

        [StructLayout(LayoutKind.Sequential)]
        internal struct ImageNtHeader
        {
            internal uint Signature;
            internal ImageFileHeader FileHeader;
            internal ImageOptionalHeader OptionalHeader;
        }

        #endregion

        #region Nested type: ImageOptionalHeader

        [StructLayout(LayoutKind.Sequential)]
        internal struct ImageOptionalHeader
        {
            // Standard fields.

            internal ushort Magic;
            internal byte MajorLinkerVersion;
            internal byte MinorLinkerVersion;
            internal uint SizeOfCode;
            internal uint SizeOfInitializedData;
            internal uint SizeOfUninitializedData;
            internal uint AddressOfEntryPoint;
            internal uint BaseOfCode;
            internal uint BaseOfData;

            // NT additional fields.

            internal uint ImageBase;
            internal uint SectionAlignment;
            internal uint FileAlignment;
            internal ushort MajorOperatingSystemVersion;
            internal ushort MinorOperatingSystemVersion;
            internal ushort MajorImageVersion;
            internal ushort MinorImageVersion;
            internal ushort MajorSubsystemVersion;
            internal ushort MinorSubsystemVersion;
            internal uint Win32VersionValue;
            internal uint SizeOfImage;
            internal uint SizeOfHeaders;
            internal uint CheckSum;
            internal ushort Subsystem;
            internal ushort DllCharacteristics;
            internal uint SizeOfStackReserve;
            internal uint SizeOfStackCommit;
            internal uint SizeOfHeapReserve;
            internal uint SizeOfHeapCommit;
            internal uint LoaderFlags;
            internal uint NumberOfRvaAndSizes;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] internal ImageDataDirectory[] DataDirectory;
        }

        #endregion

        #endregion
    }
}
