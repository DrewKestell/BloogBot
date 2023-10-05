using RaidMemberBot.Constants;
using System;
using System.IO;

namespace RaidMemberBot.Mem
{
    internal sealed class Libs
    {
        internal const string FastCall = "FastCall.dll";
        internal const string Pathfinder = "038.mmap";
        internal readonly IntPtr PathfinderPtr = IntPtr.Zero;
        private readonly IntPtr FastCallPtr = IntPtr.Zero;

        private Libs()
        {
            File.Delete(pathFastCall);

            if (FastCallPtr == IntPtr.Zero)
            {
                FastCallPtr = WinImports.LoadLibrary(pathFastCall);
            }
            if (PathfinderPtr == IntPtr.Zero)
                PathfinderPtr = WinImports.LoadLibrary(pathPathfinder);
        }

        /// <summary>
        ///     Access to the instance
        /// </summary>
        public static Libs Instance => _instance;

        private static readonly Libs _instance = new Libs();
        private string pathFastCall => "\\" + FastCall;
        private string pathPathfinder => "\\mmaps\\038.mmap";
    }
}

