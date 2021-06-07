using System;

namespace BloogBot.Game.Enums
{
    [Flags]
    public enum ControlBits
    {
        Nothing = 0x00000000,
        CtmWalk = 0x00001000,
        Front = 0x00000010,
        Back = 0x00000020,
        Jump = 0x00002000,
        Left = 0x00000100,
        Right = 0x00000200,
        MovingFrontOrBack = 0x00010000,
        StrafeLeft = 0x00000040,
        StrafeRight = 0x00000080,
        Strafing = 0x00020000,
        Turning = 0x00040000
    }
}
