using System;
using System.Runtime.InteropServices;

namespace PathfindingService
{
    public static class StormLib
    {
        [DllImport("StormLib.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public static extern bool SFileOpenArchive(
                [MarshalAs(UnmanagedType.LPWStr)] string mpqName,
                uint dwPriority,
                uint dwFlags,
                out IntPtr phArchive);

        [DllImport("StormLib.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public static extern bool SFileOpenFileEx(IntPtr hArchive, string fileName, uint searchScope, out IntPtr phFile);

        [DllImport("StormLib.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool SFileReadFile(IntPtr hFile, [Out] byte[] buffer, uint toRead, out uint bytesRead, IntPtr overlapped);

        [DllImport("StormLib.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool SFileCloseFile(IntPtr hFile);

        [DllImport("StormLib.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool SFileCloseArchive(IntPtr hArchive);

        [DllImport("StormLib.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public static extern bool SFileHasFile(IntPtr hArchive, string fileName);

        [DllImport("StormLib.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern uint SFileGetFileSize(IntPtr hFile, IntPtr lpFileSizeHigh = default);

        [DllImport("StormLib.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public static extern bool SFileFindFirstFile(IntPtr hArchive, string searchMask, out SFILE_FIND_DATA findFileData, out IntPtr hFind);

        [DllImport("StormLib.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool SFileFindNextFile(IntPtr hFind, out SFILE_FIND_DATA findFileData);

        [DllImport("StormLib.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool SFileFindClose(IntPtr hFind);
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct SFILE_FIND_DATA
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string cFileName;

        public IntPtr szPlainName;
        public uint dwHashIndex;
        public uint dwBlockIndex;
        public uint dwFileSize;
        public uint dwFileFlags;
        public uint dwCompSize;
        public uint dwFileTimeLo;
        public uint dwFileTimeHi;
        public uint lcLocale;
    }

}
