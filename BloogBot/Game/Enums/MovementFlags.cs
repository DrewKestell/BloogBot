using System;

// https://github.com/cmangos/mangos-tbc/blob/77daf8bf20174767872eaa4dbc56c2a62259e3d2/src/game/Entities/Unit.h
namespace BloogBot.Game.Enums
{
    // used in most movement packets (send and received)
    [Flags]
    public enum MovementFlags
    {
        MOVEFLAG_NONE             = 0x00000000,
        MOVEFLAG_FORWARD          = 0x00000001,
        MOVEFLAG_BACKWARD         = 0x00000002,
        MOVEFLAG_STRAFE_LEFT      = 0x00000004,
        MOVEFLAG_STRAFE_RIGHT     = 0x00000008,
        MOVEFLAG_TURN_LEFT        = 0x00000010,
        MOVEFLAG_TURN_RIGHT       = 0x00000020,
        MOVEFLAG_PITCH_UP         = 0x00000040, // ??
        MOVEFLAG_PITCH_DOWN       = 0x00000080, // ??
        MOVEFLAG_WALK_MODE        = 0x00000100, // Walking
        MOVEFLAG_ONTRANSPORT      = 0x00000200, // Used for flying on some creatures
        MOVEFLAG_LEVITATING       = 0x00000400,
        MOVEFLAG_ROOT             = 0x00000800,
        MOVEFLAG_FALLING          = 0x00001000,
        MOVEFLAG_FALLINGFAR       = 0x00004000, // ??
        MOVEFLAG_SWIMMING         = 0x00200000, // appears with fly flag also
        MOVEFLAG_ASCENDING        = 0x00400000, // swim up also
        MOVEFLAG_CAN_FLY          = 0x00800000, // ??
        MOVEFLAG_FLYING           = 0x01000000, // ??
        MOVEFLAG_FLYING2          = 0x02000000, // Actual flying mode
        MOVEFLAG_SPLINE_ELEVATION = 0x04000000, // used for flight paths
        MOVEFLAG_SPLINE_ENABLED   = 0x08000000, // used for flight paths
        MOVEFLAG_WATERWALKING     = 0x10000000, // prevent unit from falling through water
        MOVEFLAG_SAFE_FALL        = 0x20000000, // active rogue safe fall spell (passive)
        MOVEFLAG_HOVER            = 0x40000000
    };
}
