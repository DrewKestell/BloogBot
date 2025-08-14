// PhysicsBridge.h - Physics Input/Output structures for DLL interface
#pragma once
#include <cstdint>

// Movement flags from WoW client
enum MovementFlags : uint32_t
{
    MOVEFLAG_NONE = 0x00000000,
    MOVEFLAG_FORWARD = 0x00000001,
    MOVEFLAG_BACKWARD = 0x00000002,
    MOVEFLAG_STRAFE_LEFT = 0x00000004,
    MOVEFLAG_STRAFE_RIGHT = 0x00000008,
    MOVEFLAG_TURN_LEFT = 0x00000010,
    MOVEFLAG_TURN_RIGHT = 0x00000020,
    MOVEFLAG_PITCH_UP = 0x00000040,
    MOVEFLAG_PITCH_DOWN = 0x00000080,
    MOVEFLAG_WALK_MODE = 0x00000100,
    MOVEFLAG_ONTRANSPORT = 0x00000200,
    MOVEFLAG_LEVITATING = 0x00000400,
    MOVEFLAG_ROOT = 0x00000800,
    MOVEFLAG_FALLING = 0x00001000,
    MOVEFLAG_FALLINGFAR = 0x00002000,
    MOVEFLAG_PENDINGSTOP = 0x00004000,
    MOVEFLAG_PENDINGSTRAFESTOP = 0x00008000,
    MOVEFLAG_PENDINGFORWARD = 0x00010000,
    MOVEFLAG_PENDINGBACKWARD = 0x00020000,
    MOVEFLAG_PENDINGSTRAFELEFT = 0x00040000,
    MOVEFLAG_PENDINGSTRAFERIGHT = 0x00080000,
    MOVEFLAG_PENDINGROOT = 0x00100000,
    MOVEFLAG_SWIMMING = 0x00200000,
    MOVEFLAG_ASCENDING = 0x00400000,
    MOVEFLAG_DESCENDING = 0x00800000,
    MOVEFLAG_CAN_FLY = 0x01000000,
    MOVEFLAG_FLYING = 0x02000000,
    MOVEFLAG_SPLINE_ELEVATION = 0x04000000,
    MOVEFLAG_SPLINE_ENABLED = 0x08000000,
    MOVEFLAG_WATERWALKING = 0x10000000,
    MOVEFLAG_SAFE_FALL = 0x20000000,
    MOVEFLAG_HOVER = 0x40000000,
    MOVEFLAG_JUMP = 0x80000000
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