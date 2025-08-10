// PhysicsBridge.h
#pragma once

#include <cstdint>

// Movement states matching WoW 1.12.1
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
    MOVEFLAG_ON_TRANSPORT = 0x00000200,
    MOVEFLAG_LEVITATING = 0x00000400,
    MOVEFLAG_FLYING = 0x00000800,
    MOVEFLAG_FALLING = 0x00001000,
    MOVEFLAG_FALLINGFAR = 0x00004000,
    MOVEFLAG_SWIMMING = 0x00200000,
    MOVEFLAG_SPLINE_ENABLED = 0x00400000,
    MOVEFLAG_CAN_FLY = 0x00800000,
    MOVEFLAG_FLYING2 = 0x01000000,
    MOVEFLAG_WATERWALKING = 0x02000000,
    MOVEFLAG_SAFE_FALL = 0x04000000,
    MOVEFLAG_HOVER = 0x08000000,
    MOVEFLAG_ROOT = 0x10000000,
};

// Unit movement type
enum MovementType : uint8_t
{
    MOVE_WALK = 0,
    MOVE_RUN = 1,
    MOVE_RUN_BACK = 2,
    MOVE_SWIM = 3,
    MOVE_SWIM_BACK = 4,
    MOVE_TURN_RATE = 5,
    MOVE_FLIGHT = 6,
    MOVE_FLIGHT_BACK = 7,
};

// Physics input from the game
struct PhysicsInput
{
    // Current position and orientation
    float x, y, z;
    float orientation;      // Facing direction in radians
    float pitch;           // Pitch for swimming/flying

    // Current velocity
    float vx, vy, vz;

    // Movement inputs
    float moveForward;     // -1.0 to 1.0 (backward to forward)
    float moveStrafe;      // -1.0 to 1.0 (left to right)
    float turnRate;        // -1.0 to 1.0 (turn speed)

    // Movement speeds (yards/second)
    float walkSpeed;       // Default: 2.5
    float runSpeed;        // Default: 7.0
    float swimSpeed;       // Default: 4.72
    float flightSpeed;     // Default: 7.0
    float backSpeed;       // Default: 4.5

    // State flags
    uint32_t moveFlags;
    uint32_t mapId;

    // Physics modifiers
    float jumpVelocity;    // Initial jump velocity (if jumping this frame)
    float knockbackVx;     // Knockback velocity X
    float knockbackVy;     // Knockback velocity Y
    float knockbackVz;     // Knockback velocity Z

    // Collision parameters
    float height;          // Unit height (for collision)
    float radius;          // Unit radius (for collision)

    // Spline movement (if following a path)
    bool hasSplinePath;
    float splineSpeed;
    float* splinePoints;   // Array of x,y,z coordinates
    int splinePointCount;
    int currentSplineIndex;

    // Time info
    float deltaTime;       // Time since last update
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

    // Collision info
    bool isGrounded;
    bool isSwimming;
    bool isFlying;
    bool collided;
    float groundZ;         // Ground height at position
    float liquidZ;         // Liquid surface height (if any)

    // Fall damage info
    float fallDistance;
    float fallTime;

    // Spline progress
    int currentSplineIndex;
    float splineProgress;  // 0.0 to 1.0 between current and next point
};