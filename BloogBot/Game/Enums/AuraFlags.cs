using System;

namespace BloogBot.Game.Enums
{
    [Flags]
    public enum AuraFlags
    {
        Active = 0x80,
        Passive = 0x10, // Check if !Active
        Harmful = 0x20
    }
}
