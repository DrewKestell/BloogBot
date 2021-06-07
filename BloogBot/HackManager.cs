using System.Collections.Generic;

namespace BloogBot
{
    static class HackManager
    {
        static internal IList<Hack> Hacks { get; } = new List<Hack>();

        static internal void AddHack(Hack hack)
        {
            Hacks.Add(hack);
            EnableHack(hack);
        }

        static internal void EnableHack(Hack hack) => MemoryManager.WriteBytes(hack.Address, hack.NewBytes);

        static internal void DisableHack(Hack hack) => MemoryManager.WriteBytes(hack.Address, hack.OriginalBytes);
    }
}
