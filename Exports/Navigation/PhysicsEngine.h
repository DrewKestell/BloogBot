// PhysicsEngine.h - WoW physics without friction, orientation updates, or air control
#pragma once

#include "PhysicsBridge.h"
#include <memory>
#include <vector>
#include <cmath>
#include <algorithm>
#include <string>

// Forward declarations
namespace VMAP { class VMapManager2; }
class Navigation;
class MapLoader;

// WoW 1.12.1 Physics Constants
namespace PhysicsConstants
{
    // Gravity in yards/second^2 (WoW uses ~19.29 y/s²)
    constexpr float GRAVITY = 19.2911f;

    // Jump initial velocity in yards/second
    constexpr float JUMP_VELOCITY = 7.95577f;  // Results in ~1.6 yard jump height

    // Water entry/exit thresholds
    constexpr float WATER_LEVEL_DELTA = 2.0f;

    // Ground detection distance - matching vMaNGOS
    constexpr float GROUND_HEIGHT_TOLERANCE = 0.05f;    // Safety margin for ground placement
    constexpr float GROUND_SEARCH_RANGE = 100.0f;      // Maximum search range

    // Collision detection - vMaNGOS values
    constexpr float STEP_HEIGHT = 2.0f;  // ATTACK_DISTANCE equivalent for step-up
    constexpr float MIN_WALK_NORMAL_Z = 0.7071f;  // 45 degree slope

    // Fall damage thresholds
    constexpr float SAFE_FALL_HEIGHT = 10.0f;
    constexpr float LETHAL_FALL_HEIGHT = 60.0f;

    // Height constants - using -200000.0f for everything
    constexpr float INVALID_HEIGHT = -200000.0f;
    constexpr float MAX_HEIGHT = 100000.0f;
    constexpr float MAX_FALL_DISTANCE = 250000.0f;
    constexpr float DEFAULT_HEIGHT_SEARCH = 10.0f;
    constexpr float DEFAULT_WATER_SEARCH = 50.0f;
}

class PhysicsEngine
{
private:
    static PhysicsEngine* s_instance;
    VMAP::VMapManager2* m_vmapManager;  // Direct pointer to VMapManager2
    std::unique_ptr<MapLoader> m_mapLoader;  // Use MapLoader for terrain heights
    Navigation* m_navigation;
    bool m_initialized;

    uint32_t m_currentMapId;  // Track currently loaded map
    void EnsureMapLoaded(uint32_t mapId);  // Ensure map tiles are loaded

    // Helper structures
    struct CollisionInfo {
        bool hasGround = false;
        float groundZ = PhysicsConstants::INVALID_HEIGHT;
        bool hasLiquid = false;
        float liquidZ = PhysicsConstants::INVALID_HEIGHT;
        uint32_t liquidType = 0;
        bool isIndoors = false;
    };

    struct MovementState
    {
        float x, y, z;
        float vx, vy, vz;
        float orientation;  // Pass-through from input, never modified
        float pitch;        // Pass-through from input, never modified
        bool isGrounded;
        bool isSwimming;
        float fallTime;
        float fallStartZ;
    };

    PhysicsEngine();

    // Physics calculations
    CollisionInfo QueryEnvironment(uint32_t mapId, float x, float y, float z, float radius, float height);
    MovementState UpdateMovement(const PhysicsInput& input, const CollisionInfo& collision, float dt);
    MovementState HandleGroundMovement(const PhysicsInput& input, MovementState& state, float dt);
    MovementState HandleAirMovement(const PhysicsInput& input, MovementState& state, float dt);
    MovementState HandleSwimMovement(const PhysicsInput& input, MovementState& state, float dt);

    // Height calculation methods (vMaNGOS-style)
    float GetVMapHeight(uint32_t mapId, float x, float y, float z, float maxSearchDist);
    float GetADTHeight(uint32_t mapId, float x, float y, float z);
    bool GetLiquidInfo(uint32_t mapId, float x, float y, float z, float& liquidLevel, float& liquidFloor, uint32_t& liquidType);

    // Collision handling
    bool CheckCollision(uint32_t mapId, float startX, float startY, float startZ,
        float endX, float endY, float endZ, float radius, float height,
        float& hitX, float& hitY, float& hitZ);
    float GetLiquidHeight(uint32_t mapId, float x, float y, float z, uint32_t& liquidType);
    void ResolveCollisions(uint32_t mapId, MovementState& state, float radius, float height);

    // Movement helpers
    float CalculateMoveSpeed(const PhysicsInput& input, bool isSwimming);
    void ApplyGravity(MovementState& state, float dt);
    void ApplyKnockback(MovementState& state, float vx, float vy, float vz);

public:
    static PhysicsEngine* Instance();
    static void Destroy();

    void Initialize();
    void Shutdown();

    PhysicsOutput Step(const PhysicsInput& input, float dt);

    // Utility functions (vMaNGOS compatible)
    bool IsGrounded(uint32_t mapId, float x, float y, float z, float radius, float height);
    bool IsInWater(uint32_t mapId, float x, float y, float z, float height);
    bool CanWalkOn(uint32_t mapId, float x, float y, float z);
    float GetHeight(uint32_t mapId, float x, float y, float z, bool checkVMap, float maxSearchDist);
};