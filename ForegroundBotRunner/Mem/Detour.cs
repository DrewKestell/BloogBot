using System.Runtime.InteropServices;

namespace ForegroundBotRunner.Mem
{
    internal class Detour
    {
        private readonly nint hook;
        private readonly Delegate hookDelegate; // pinned here to prevent GC
        private readonly nint target;
        private readonly Delegate targetDelegate; // pinned here to prevent GC
        private readonly List<byte> newBytes;
        private readonly List<byte> orginalBytes;

        internal Detour(Delegate target, Delegate hook, string name)
        {
            Name = name;
            targetDelegate = target;
            this.target = Marshal.GetFunctionPointerForDelegate(target);
            hookDelegate = hook;
            this.hook = Marshal.GetFunctionPointerForDelegate(hook);

            //Store the orginal bytes
            orginalBytes = [.. MemoryManager.ReadBytes(this.target, 6)];

            //Setup the detour bytes
            newBytes = [0x68];
            var tmp = BitConverter.GetBytes(this.hook.ToInt32());
            newBytes.AddRange(tmp);
            newBytes.Add(0xC3);

            var hack = new Hack(Name, this.target, [.. newBytes]);
            HackManager.AddHack(hack);
        }

        public bool IsApplied { get; private set; }

        public string Name { get; }

        public void Apply()
        {
            Console.WriteLine($"[DETOUR] Hack applied {Name}");
            MemoryManager.WriteBytes(target, [.. newBytes]);
        }

        public void Remove()
        {
            MemoryManager.WriteBytes(target, [.. orginalBytes]);
        }
    }
}
