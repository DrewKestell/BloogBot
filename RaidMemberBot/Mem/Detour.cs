using RaidMemberBot.Game;
using RaidMemberBot.Mem;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace RaidMemberBot.Helpers.GreyMagic.Internals
{
    internal class Detour
    {
        readonly IntPtr hook;
        readonly Delegate hookDelegate; // pinned here to prevent GC
        readonly IntPtr target;
        readonly Delegate targetDelegate; // pinned here to prevent GC
        readonly List<byte> newBytes;
        readonly List<byte> orginalBytes;

        internal Detour(Delegate target, Delegate hook, string name)
        {
            Name = name;
            targetDelegate = target;
            this.target = Marshal.GetFunctionPointerForDelegate(target);
            hookDelegate = hook;
            this.hook = Marshal.GetFunctionPointerForDelegate(hook);

            //Store the orginal bytes
            orginalBytes = new List<byte>();
            orginalBytes.AddRange(MemoryManager.ReadBytes(this.target, 6));

            //Setup the detour bytes
            newBytes = new List<byte> { 0x68 };
            var tmp = BitConverter.GetBytes(this.hook.ToInt32());
            newBytes.AddRange(tmp);
            newBytes.Add(0xC3);

            var hack = new Hack(Name, this.target, newBytes.ToArray());
            HackManager.AddHack(hack);
        }

        public bool IsApplied { get; private set; }

        public string Name { get; }

        public void Apply()
        {
            Console.WriteLine($"[DETOUR] Hack applied {Name}");
            MemoryManager.WriteBytes(target, newBytes.ToArray());
        }

        public void Remove()
        {
            MemoryManager.WriteBytes(target, orginalBytes.ToArray());
        }
    }
}
