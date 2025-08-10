// PhysicsEngine.h - Complete with MapLoader instead of NavMesh for heights
#pragma once

#include "PhysicsBridge.h"
#include <memory>
#include <vector>
#include <cmath>
#include <algorithm>
#include <string>

// Forward declarations
namespace VMAP { class VMapManager2; }
class VMapClient;
class Navigation;
class MapLoader;

// WoW 1.12.1 Physics Constants
namespace PhysicsConstants
{
    // Gravity in yards/second^2 (WoW uses ~19.29 y/s²)
    constexpr float GRAVITY = 19.2911f;

    // Jump initial velocity in yards/second
    constexpr float JUMP_VELOCITY = 7.95f;  // Results in ~1.6 yard jump height

    // Water entry/exit thresholds
    constexpr float WATER_LEVEL_DELTA = 2.0f;

    // Ground detection distance - matching vMaNGOS
    constexpr float GROUND_HEIGHT_TOLERANCE = 0.05f;  // vMaNGOS uses 0.05f for ground contact
    constexpr float DEFAULT_HEIGHT_SEARCH = 4.0f;     // vMaNGOS default maxSearchDist
    constexpr float GROUND_SEARCH_RANGE = 100.0f;     // Maximum search range

    // Collision detection
    constexpr float STEP_HEIGHT = 0.5f;  // Maximum step up height
    constexpr float MIN_WALK_NORMAL_Z = 0.7071f;  // 45 degree slope

    // Fall damage thresholds
    constexpr float SAFE_FALL_HEIGHT = 10.0f;
    constexpr float LETHAL_FALL_HEIGHT = 60.0f;

    // Movement constants
    constexpr float TURN_SPEED = 3.14159f;  // Radians per second
    constexpr float AIR_CONTROL_FACTOR = 0.1f;  // Reduced control while airborne

    // Spline movement
    constexpr float SPLINE_SMOOTH_FACTOR = 0.5f;

    // Height calculation constants (matching vMaNGOS)
    constexpr float VMAP_INVALID_HEIGHT_VALUE = -100000.0f;
    constexpr float INVALID_HEIGHT_VALUE = -100000.0f;
}

class PhysicsEngine
{
private:
    static PhysicsEngine* s_instance;
    std::unique_ptr<VMapClient> m_vmapClient;
    std::unique_ptr<MapLoader> m_mapLoader;  // Use MapLoader for terrain heights
    Navigation* m_navigation;
    bool m_initialized;

    // Configuration flags (matching vMaNGOS)
    bool m_vmapHeightEnabled;
    bool m_vmapIndoorCheckEnabled;
    bool m_vmapLOSEnabled;

    uint32_t m_currentMapId;  // Track currently loaded map
    void EnsureMapLoaded(uint32_t mapId);  // Add this method
    // Helper structures
    struct CollisionInfo
    {
        bool hasGround;
        float groundZ;
        float groundNormalZ;
        bool hasLiquid;
        float liquidZ;
        uint32_t liquidType;
        bool isIndoors;

        // Additional fields for vMaNGOS compatibility
        float vmapHeight;
        float adtHeight;
        bool vmapValid;
        bool adtValid;
    };

    struct MovementState
    {
        float x, y, z;
        float vx, vy, vz;
        float orientation;
        float pitch;
        bool isGrounded;
        bool isSwimming;
        bool isFlying;
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
    MovementState HandleSplineMovement(const PhysicsInput& input, MovementState& state, float dt);

    // Height calculation methods (vMaNGOS-style)
    float GetVMapHeight(uint32_t mapId, float x, float y, float z, float maxSearchDist);
    float GetADTHeight(uint32_t mapId, float x, float y, float z);  // Changed from GetNavMeshHeight
    bool GetLiquidInfo(uint32_t mapId, float x, float y, float z, float& liquidLevel, float& liquidFloor, uint32_t& liquidType);

    // Height selection logic (vMaNGOS-style)
    float SelectBestHeight(float vmapHeight, float adtHeight, float currentZ, float maxSearchDist);

    // Collision handling
    bool CheckCollision(uint32_t mapId, float startX, float startY, float startZ,
        float endX, float endY, float endZ, float radius, float height,
        float& hitX, float& hitY, float& hitZ);
    float GetGroundHeight(uint32_t mapId, float x, float y, float z);
    float GetLiquidHeight(uint32_t mapId, float x, float y, float z, uint32_t& liquidType);
    void ResolveCollisions(uint32_t mapId, MovementState& state, float radius, float height);

    // Movement helpers
    float CalculateMoveSpeed(const PhysicsInput& input, bool isSwimming, bool isFlying);
    void ApplyGravity(MovementState& state, float dt);
    void ApplyFriction(MovementState& state, float friction, float dt);
    void ApplyKnockback(MovementState& state, float vx, float vy, float vz);
    void UpdateOrientation(MovementState& state, float targetOrientation, float turnSpeed, float dt);

    // Spline helpers
    void CalculateSplineVelocity(const PhysicsInput& input, MovementState& state);
    float GetSplineProgress(float x, float y, float z, const float* points, int index);

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
    float GetFallDamage(float fallDistance, bool hasSafeFall);

    // Configuration
    void SetVMapHeightEnabled(bool enabled) { m_vmapHeightEnabled = enabled; }
    void SetVMapIndoorCheckEnabled(bool enabled) { m_vmapIndoorCheckEnabled = enabled; }
    void SetVMapLOSEnabled(bool enabled) { m_vmapLOSEnabled = enabled; }
    bool IsVMapHeightEnabled() const { return m_vmapHeightEnabled; }
    bool IsVMapIndoorCheckEnabled() const { return m_vmapIndoorCheckEnabled; }
    bool IsVMapLOSEnabled() const { return m_vmapLOSEnabled; }
};