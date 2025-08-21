// PhysicsBridge.h - Physics Input/Output structures for DLL interface
#pragma once
#include <cstdint>

// Movement flags from WoW client
enum MovementFlags
{
    MOVEFLAG_NONE = 0x00000000, // 0
    MOVEFLAG_FORWARD = 0x00000001, // 1
    MOVEFLAG_BACKWARD = 0x00000002, // 2
    MOVEFLAG_STRAFE_LEFT = 0x00000004, // 3
    MOVEFLAG_STRAFE_RIGHT = 0x00000008, // 4
    MOVEFLAG_TURN_LEFT = 0x00000010, // 5
    MOVEFLAG_TURN_RIGHT = 0x00000020, // 6
    MOVEFLAG_PITCH_UP = 0x00000040, // 7
    MOVEFLAG_PITCH_DOWN = 0x00000080, // 8
    MOVEFLAG_WALK_MODE = 0x00000100, // 9 Walking
    MOVEFLAG_UNUSED10 = 0x00000200, // 10 ??
    MOVEFLAG_LEVITATING = 0x00000400, // 11 ?? Seems not to work
    MOVEFLAG_FIXED_Z = 0x00000800, // 12 Fixed height. Jump => Glide across the entire map
    MOVEFLAG_ROOT = 0x00001000, // 13
    MOVEFLAG_JUMPING = 0x00002000, // 14
    MOVEFLAG_FALLINGFAR = 0x00004000, // 15
    MOVEFLAG_PENDING_STOP = 0x00008000, // 16 Only used in older client versions
    MOVEFLAG_PENDING_UNSTRAFE = 0x00010000, // 17 Only used in older client versions
    MOVEFLAG_PENDING_FORWARD = 0x00020000, // 18 Only used in older client versions
    MOVEFLAG_PENDING_BACKWARD = 0x00040000, // 19 Only used in older client versions
    MOVEFLAG_PENDING_STR_LEFT = 0x00080000, // 20 Only used in older client versions
    MOVEFLAG_PENDING_STR_RGHT = 0x00100000, // 21 Only used in older client versions
    MOVEFLAG_SWIMMING = 0x00200000, // 22 Ok
    MOVEFLAG_SPLINE_ENABLED = 0x00400000, // 23 Ok
    MOVEFLAG_MOVED = 0x00800000, // 24 Only used in older client versions
    MOVEFLAG_FLYING = 0x01000000, // 25 [-ZERO] is it really need and correct value
    MOVEFLAG_ONTRANSPORT = 0x02000000, // 26 Used for flying on some creatures
    MOVEFLAG_SPLINE_ELEVATION = 0x04000000, // 27 Used for flight paths
    MOVEFLAG_UNUSED28 = 0x08000000, // 28
    MOVEFLAG_WATERWALKING = 0x10000000, // 29 Prevent unit from falling through water
    MOVEFLAG_SAFE_FALL = 0x20000000, // 30 Active rogue safe fall spell (passive)
    MOVEFLAG_HOVER = 0x40000000, // 31
    MOVEFLAG_UNUSED32 = 0x80000000, // 32

    // Can not be present with MOVEFLAG_ROOT (otherwise client freeze)
    MOVEFLAG_MASK_MOVING =
    MOVEFLAG_FORWARD | MOVEFLAG_BACKWARD | MOVEFLAG_STRAFE_LEFT | MOVEFLAG_STRAFE_RIGHT |
    MOVEFLAG_PITCH_UP | MOVEFLAG_PITCH_DOWN | MOVEFLAG_JUMPING | MOVEFLAG_FALLINGFAR |
    MOVEFLAG_SPLINE_ELEVATION,
    MOVEFLAG_MASK_MOVING_OR_TURN = MOVEFLAG_MASK_MOVING | MOVEFLAG_TURN_LEFT | MOVEFLAG_TURN_RIGHT,

    // MovementFlags mask that only contains flags for x/z translations
    // this is to avoid that a jumping character that stands still triggers melee-leeway
    MOVEFLAG_MASK_XZ = MOVEFLAG_FORWARD | MOVEFLAG_BACKWARD | MOVEFLAG_STRAFE_LEFT | MOVEFLAG_STRAFE_RIGHT
};

// Physics input from the game
struct PhysicsInput
{
    // Movement state
    uint32_t moveFlags;        // Movement flags bitmap

    // Position & orientation
    float x, y, z;             // World position
    float orientation;         // Facing direction (radians)
    float pitch;               // Swimming/flying pitch

    // Velocity
    float vx, vy, vz;          // Current velocity vector

    // Movement speeds (yards/second)
    float walkSpeed;           // Default: 2.5
    float runSpeed;            // Default: 7.0
    float runBackSpeed;        // Default: 4.5
    float swimSpeed;           // Default: 4.72
    float swimBackSpeed;       // Default: 2.5
    float flightSpeed;         // Default: 7.0
    float turnSpeed;           // Radians/second

    // Transport (boats, zeppelins, elevators)
    uint64_t transportGuid;    // Transport object GUID
    float transportX;          // Position on transport
    float transportY;
    float transportZ;
    float transportO;          // Orientation on transport

    // Falling
    uint32_t fallTime;         // Time spent falling (ms)

    // Unit properties
    float height;              // Unit height (for collision)
    float radius;              // Unit radius (for collision)

    // Spline movement (if following a path)
    bool hasSplinePath;
    float splineSpeed;
    float* splinePoints;       // Array of x,y,z coordinates
    int splinePointCount;
    int currentSplineIndex;

    // Context
    uint32_t mapId;            // Current map ID
    float deltaTime;           // Time since last update
};

// Physics output back to the game
struct PhysicsOutput
{
    // New position
    float x, y, z;
    float orientation;
    float pitch;

    // New velocity
    float vx, vy, vz;

    // Updated movement flags
    uint32_t moveFlags;

    // State flags
    bool isGrounded;
    bool isSwimming;
    bool isFlying;
    bool collided;

    // Height information
    float groundZ;             // Ground height at position
    float liquidZ;             // Liquid surface height (if any)

    // Fall damage info
    float fallDistance;
    float fallTime;

    // Spline progress
    int currentSplineIndex;
    float splineProgress;      // 0.0 to 1.0 between current and next point
};