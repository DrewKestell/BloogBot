using System;

namespace BloogBot.Game.Enums
{
    [Flags]
    public enum DynamicFlags
    {
        None =   0x0,
        CanBeLooted = 0x1,
        IsMarked =    0x2,
        Tapped =      0x4, // Makes creature name tag appear grey
        TappedByMe =  0x8
    }
}
