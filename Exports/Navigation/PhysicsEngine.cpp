// PhysicsEngine.cpp
#include "PhysicsEngine.h"
#include "VMapDefinitions.h"
#include "Navigation.h"
#include "VMapClient.h"
#include "VMapFactory.h"
#include "MapLoader.h"
#include <cmath>
#include <algorithm>
#include <iostream>
#include <iomanip>
#include <fstream>
#include <sstream>
#include <chrono>
#include <cstring>
#include <filesystem>
#include "PhysicsMath.h"
#include "VMapLog.h"

using namespace PhysicsConstants;
using namespace PhysicsMath;

PhysicsEngine* PhysicsEngine::s_instance = nullptr;

PhysicsEngine* PhysicsEngine::Instance()
{
    if (!s_instance)
    {
        s_instance = new PhysicsEngine();
    }
    return s_instance;
}

void PhysicsEngine::Destroy()
{
    if (s_instance)
    {
        delete s_instance;
        s_instance = nullptr;
    }
}

PhysicsEngine::PhysicsEngine()
    : m_navigation(nullptr), m_initialized(false),
    m_vmapHeightEnabled(true),
    m_vmapIndoorCheckEnabled(true),
    m_vmapLOSEnabled(true),
    m_currentMapId(UINT32_MAX)  // Add this to track loaded map
{
}

void PhysicsEngine::Initialize()
{
    if (m_initialized)
    {
        return;
    }

    try
    {
        // Initialize MapLoader for terrain data
        m_mapLoader = std::make_unique<MapLoader>();

        std::vector<std::string> mapPaths = { "maps/", "Data/maps/", "../Data/maps/" };
        bool mapLoaderInitialized = false;

        for (const auto& path : mapPaths)
        {
            if (std::filesystem::exists(path))
            {
                if (m_mapLoader->Initialize(path))
                {
                    mapLoaderInitialized = true;
                    break;
                }
            }
        }

        // Initialize VMAP system with better error handling
        try
        {
            m_vmapClient = std::make_unique<VMapClient>();

            if (m_vmapClient)
            {
                try
                {
                    m_vmapClient->initialize();

                    if (!m_vmapClient->isInitialized())
                    {
                        m_vmapClient.reset();
                    }
                }
                catch (const std::exception& e)
                {
                    m_vmapClient.reset();
                }
            }
        }
        catch (const std::bad_alloc& e)
        {
            m_vmapClient.reset();
        }
        catch (const std::exception& e)
        {
            m_vmapClient.reset();
        }

        // DON'T preload all maps - let them load on demand
        // Remove the entire map preloading section

        // Get navigation instance
        m_navigation = Navigation::GetInstance();

        // Set default configuration matching vMaNGOS
        m_vmapHeightEnabled = (m_vmapClient != nullptr);
        m_vmapIndoorCheckEnabled = (m_vmapClient != nullptr);
        m_vmapLOSEnabled = (m_vmapClient != nullptr);

        m_initialized = true;
    }
    catch (const std::exception& e)
    {
        m_initialized = true;
    }
    catch (...)
    {
        m_initialized = true;
    }
}

void PhysicsEngine::Shutdown()
{
    m_vmapClient.reset();
    m_mapLoader.reset();
    m_currentMapId = UINT32_MAX;
    m_initialized = false;
}

// Ensure map is loaded only once per map change
void PhysicsEngine::EnsureMapLoaded(uint32_t mapId)
{
    LOG_INFO("==================== EnsureMapLoaded START ====================");
    LOG_INFO("Ensuring map " << mapId << " is loaded");
    LOG_INFO("Current loaded map ID: " << (m_currentMapId == UINT32_MAX ? "NONE" : std::to_string(m_currentMapId)));

    if (m_currentMapId != mapId)
    {
        LOG_INFO("Map change detected - need to load new map");

        if (m_vmapClient)
        {
            LOG_INFO("VMapClient exists - calling preloadMap");

            try
            {
                m_vmapClient->preloadMap(mapId);
                LOG_INFO("preloadMap completed for map " << mapId);
            }
            catch (const std::exception& e)
            {
                LOG_ERROR("Exception during preloadMap: " << e.what());
            }
            catch (...)
            {
                LOG_ERROR("Unknown exception during preloadMap");
            }
        }
        else
        {
            LOG_WARN("No VMapClient available - VMAP features disabled");
        }

        m_currentMapId = mapId;
        LOG_INFO("Current map ID updated to: " << m_currentMapId);
    }
    else
    {
        LOG_DEBUG("Map " << mapId << " already current - no reload needed");
    }

    LOG_INFO("==================== EnsureMapLoaded END ====================");
}


// Main vMaNGOS-style GetHeight implementation
float PhysicsEngine::GetVMapHeight(uint32_t mapId, float x, float y, float z, float maxSearchDist)
{
    LOG_INFO("==================== GetVMapHeight START ====================");
    LOG_INFO("GetVMapHeight called - Map:" << mapId
        << " Pos:(" << x << "," << y << "," << z << ")"
        << " SearchDist:" << maxSearchDist);

    auto start = std::chrono::high_resolution_clock::now();

    if (!m_vmapClient)
    {
        LOG_WARN("No VMapClient - returning invalid height");
        LOG_INFO("==================== GetVMapHeight END (no client) ====================");
        return VMAP_INVALID_HEIGHT_VALUE;
    }

    if (!m_vmapHeightEnabled)
    {
        LOG_WARN("VMAP height calculation disabled");
        LOG_INFO("==================== GetVMapHeight END (disabled) ====================");
        return VMAP_INVALID_HEIGHT_VALUE;
    }

    try
    {
        LOG_DEBUG("Calling VMapClient::getGroundHeight...");
        float height = m_vmapClient->getGroundHeight(mapId, x, y, z, maxSearchDist);

        auto end = std::chrono::high_resolution_clock::now();
        auto duration = std::chrono::duration_cast<std::chrono::microseconds>(end - start);

        if (height > VMAP_INVALID_HEIGHT_VALUE)
        {
            LOG_INFO("VMAP height found: " << height << " (took " << duration.count() << " us)");
        }
        else
        {
            LOG_DEBUG("No VMAP height found (took " << duration.count() << " us)");
        }

        LOG_INFO("==================== GetVMapHeight END ====================");
        return height;
    }
    catch (const std::exception& e)
    {
        LOG_ERROR("Exception getting VMAP height: " << e.what());
        LOG_INFO("==================== GetVMapHeight END (exception) ====================");
        return VMAP_INVALID_HEIGHT_VALUE;
    }
    catch (...)
    {
        LOG_ERROR("Unknown exception getting VMAP height");
        LOG_INFO("==================== GetVMapHeight END (exception) ====================");
        return VMAP_INVALID_HEIGHT_VALUE;
    }
}

// Get height from ADT terrain data
float PhysicsEngine::GetADTHeight(uint32_t mapId, float x, float y, float z)
{
    std::cout << "[Physics DEBUG] GetADTHeight called - Map: " << mapId
        << ", Pos: (" << x << ", " << y << ", " << z << ")" << std::endl;

    if (!m_mapLoader || !m_mapLoader->IsInitialized())
    {
        std::cout << "[Physics DEBUG] MapLoader not initialized" << std::endl;
        return INVALID_HEIGHT_VALUE;
    }

    // MapLoader expects world coordinates directly
    float adtHeight = m_mapLoader->GetHeight(mapId, x, y, z);
    std::cout << "[Physics DEBUG] ADT height result: " << adtHeight << std::endl;
    return adtHeight;
}

// Select best height using vMaNGOS logic
float PhysicsEngine::SelectBestHeight(float vmapHeight, float adtHeight, float currentZ, float maxSearchDist)
{
    LOG_DEBUG("SelectBestHeight - VMAP:" << vmapHeight
        << " ADT:" << adtHeight
        << " CurrentZ:" << currentZ
        << " MaxSearch:" << maxSearchDist);

    // Both invalid - return invalid
    if (vmapHeight <= VMAP_INVALID_HEIGHT_VALUE && adtHeight <= INVALID_HEIGHT_VALUE)
    {
        LOG_DEBUG("Both heights invalid - returning INVALID");
        return INVALID_HEIGHT_VALUE;
    }

    // Only ADT valid
    if (vmapHeight <= VMAP_INVALID_HEIGHT_VALUE)
    {
        LOG_DEBUG("Only ADT valid - returning ADT height");
        return adtHeight;
    }

    // Only VMAP valid
    if (adtHeight <= INVALID_HEIGHT_VALUE)
    {
        LOG_DEBUG("Only VMAP valid - returning VMAP height");
        return vmapHeight;
    }

    // Both valid - return the higher one (you stand on whatever is above)
    float selectedHeight = std::max(vmapHeight, adtHeight);
    LOG_DEBUG("Both valid - selected " << (selectedHeight == vmapHeight ? "VMAP" : "ADT")
        << " (higher value)");
    return selectedHeight;
}

PhysicsOutput PhysicsEngine::Step(const PhysicsInput& input, float dt)
{
    std::cout << "\n========== PHYSICS STEP START ==========" << std::endl;
    std::cout << "Input Position: (" << input.x << ", " << input.y << ", " << input.z << ")" << std::endl;
    std::cout << "Input Velocity: (" << input.vx << ", " << input.vy << ", " << input.vz << ")" << std::endl;
    std::cout << "Input MoveFlags: 0x" << std::hex << input.moveFlags << std::dec << std::endl;
    std::cout << "Input Height: " << input.height << ", Radius: " << input.radius << std::endl;
    std::cout << "Delta Time: " << dt << std::endl;

    PhysicsOutput output = {};

    if (!m_initialized)
    {
        std::cout << "Physics not initialized - passthrough mode" << std::endl;
        output.x = input.x;
        output.y = input.y;
        output.z = input.z;
        output.orientation = input.orientation;
        output.pitch = input.pitch;
        output.vx = input.vx;
        output.vy = input.vy;
        output.vz = input.vz;
        output.moveFlags = input.moveFlags;
        return output;
    }

    // Ensure map is loaded ONCE at the start of the step
    EnsureMapLoaded(input.mapId);

    // Query environment at current position
    std::cout << "Querying environment..." << std::endl;
    CollisionInfo collision = QueryEnvironment(input.mapId, input.x, input.y, input.z,
        input.radius, input.height);

    std::cout << "Collision Info:" << std::endl;
    std::cout << "  hasGround: " << collision.hasGround << ", groundZ: " << collision.groundZ << std::endl;
    std::cout << "  hasLiquid: " << collision.hasLiquid << ", liquidZ: " << collision.liquidZ
        << ", liquidType: 0x" << std::hex << collision.liquidType << std::dec << std::endl;
    std::cout << "  isIndoors: " << collision.isIndoors << std::endl;

    // Initialize movement state from input
    MovementState state;
    state.x = input.x;
    state.y = input.y;
    state.z = input.z;
    state.vx = input.vx;
    state.vy = input.vy;
    state.vz = input.vz;
    state.orientation = input.orientation;
    state.pitch = input.pitch;
    state.fallTime = 0;
    state.fallStartZ = input.z;

    // Determine if grounded
    bool shouldBeGrounded = false;
    if (!(input.moveFlags & MOVEFLAG_FLYING) && collision.hasGround)
    {
        float distToGround = input.z - collision.groundZ;
        std::cout << "Distance to ground: " << distToGround << std::endl;

        if (input.z <= collision.groundZ)
        {
            std::cout << "Below ground - will be grounded" << std::endl;
            shouldBeGrounded = true;
        }
        else if (std::abs(distToGround) < GROUND_HEIGHT_TOLERANCE)
        {
            std::cout << "Close to ground (within " << GROUND_HEIGHT_TOLERANCE << ") - will be grounded" << std::endl;
            shouldBeGrounded = true;
        }
        else
        {
            std::cout << "Too far from ground - not grounded" << std::endl;
        }
    }
    else
    {
        std::cout << "Not checking ground (flying=" << ((input.moveFlags & MOVEFLAG_FLYING) != 0)
            << ", hasGround=" << collision.hasGround << ")" << std::endl;
    }

    state.isGrounded = shouldBeGrounded;
    std::cout << "IsGrounded: " << state.isGrounded << std::endl;

    // Determine if swimming - only if NOT grounded
    float swimDepth = input.height * 0.7f;  // vmangos value
    std::cout << "Swimming check:" << std::endl;
    std::cout << "  Not grounded: " << !state.isGrounded << std::endl;
    std::cout << "  Has liquid: " << collision.hasLiquid << std::endl;
    std::cout << "  Liquid type has water: " << ((collision.liquidType & (VMAP::MAP_LIQUID_TYPE_WATER | VMAP::MAP_LIQUID_TYPE_OCEAN)) != 0) << std::endl;
    std::cout << "  Swim depth threshold: " << swimDepth << " (height * 0.7)" << std::endl;
    std::cout << "  Position below liquid: z=" << input.z << " < liquidZ-swimDepth=" << (collision.liquidZ - swimDepth)
        << " = " << (input.z < (collision.liquidZ - swimDepth + 0.02f)) << std::endl;

    // Only swim if NOT grounded AND in deep enough water
    state.isSwimming = !state.isGrounded && collision.hasLiquid &&
        (collision.liquidType & (VMAP::MAP_LIQUID_TYPE_WATER | VMAP::MAP_LIQUID_TYPE_OCEAN)) &&
        input.z < (collision.liquidZ - swimDepth + 0.02f);

    std::cout << "IsSwimming: " << state.isSwimming << std::endl;

    state.isFlying = (input.moveFlags & MOVEFLAG_FLYING) != 0;
    std::cout << "IsFlying: " << state.isFlying << std::endl;

    // Apply knockback if any
    if (input.knockbackVx != 0 || input.knockbackVy != 0 || input.knockbackVz != 0)
    {
        std::cout << "Applying knockback: (" << input.knockbackVx << ", " << input.knockbackVy << ", " << input.knockbackVz << ")" << std::endl;
        ApplyKnockback(state, input.knockbackVx, input.knockbackVy, input.knockbackVz);
        state.isGrounded = false;
    }

    // Apply jump velocity
    if (input.jumpVelocity > 0 && state.isGrounded && !state.isSwimming)
    {
        std::cout << "Jumping with velocity: " << input.jumpVelocity << std::endl;
        state.vz = input.jumpVelocity;
        state.isGrounded = false;
        state.fallStartZ = state.z;  // Set fall start position
    }

    // Update movement based on state
    std::cout << "Movement handler: ";
    if (input.hasSplinePath)
    {
        std::cout << "SPLINE" << std::endl;
        state = HandleSplineMovement(input, state, dt);
    }
    else if (state.isFlying)
    {
        std::cout << "FLYING" << std::endl;
        state = HandleAirMovement(input, state, dt);
    }
    else if (state.isSwimming)
    {
        std::cout << "SWIMMING" << std::endl;
        state = HandleSwimMovement(input, state, dt);
    }
    else if (state.isGrounded)
    {
        std::cout << "GROUND" << std::endl;
        state = HandleGroundMovement(input, state, dt);
    }
    else
    {
        std::cout << "AIR (falling)" << std::endl;
        state = HandleAirMovement(input, state, dt);
    }

    std::cout << "After movement - Position: (" << state.x << ", " << state.y << ", " << state.z << ")" << std::endl;
    std::cout << "After movement - Velocity: (" << state.vx << ", " << state.vy << ", " << state.vz << ")" << std::endl;

    // Recheck swimming after position update (in case we fell into water while airborne)
    // IMPORTANT: Only check if NOT grounded - grounded characters wade, they don't swim
    if (!state.isGrounded && !state.isSwimming && collision.hasLiquid &&
        (collision.liquidType & (VMAP::MAP_LIQUID_TYPE_WATER | VMAP::MAP_LIQUID_TYPE_OCEAN)))
    {
        float currentSwimDepth = input.height * 0.7f;
        if (state.z < (collision.liquidZ - currentSwimDepth + 0.02f))
        {
            std::cout << "Entered water after movement - now swimming!" << std::endl;
            state.isSwimming = true;
            state.vz = std::max(state.vz, -2.0f);  // Limit fall speed in water
        }
    }

    // Resolve collisions
    std::cout << "Resolving collisions..." << std::endl;
    ResolveCollisions(input.mapId, state, input.radius, input.height);
    std::cout << "After collisions - Position: (" << state.x << ", " << state.y << ", " << state.z << ")" << std::endl;

    // Re-query ground at new position if moved
    if (state.x != input.x || state.y != input.y)
    {
        std::cout << "Re-querying ground at new position..." << std::endl;
        float newGroundZ = GetHeight(input.mapId, state.x, state.y, state.z,
            m_vmapHeightEnabled, DEFAULT_HEIGHT_SEARCH);
        if (newGroundZ > INVALID_HEIGHT_VALUE)
        {
            collision.groundZ = newGroundZ;
            collision.hasGround = true;
            std::cout << "New ground height: " << newGroundZ << std::endl;
        }
    }

    // Final ground placement
    if (!state.isFlying && !(input.moveFlags & (MOVEFLAG_FALLINGFAR | MOVEFLAG_FALLING)) &&
        !state.isSwimming && collision.hasGround && collision.groundZ > INVALID_HEIGHT_VALUE)
    {
        std::cout << "Final ground placement check:" << std::endl;
        std::cout << "  Current Z: " << state.z << ", Ground Z: " << collision.groundZ << std::endl;

        if (state.z < collision.groundZ)
        {
            std::cout << "  Below ground - snapping to ground" << std::endl;
            state.z = collision.groundZ;
            state.vz = 0;
            state.isGrounded = true;
        }
        else if (std::abs(state.z - collision.groundZ) < GROUND_HEIGHT_TOLERANCE)
        {
            std::cout << "  Very close to ground - snapping" << std::endl;
            state.z = collision.groundZ;
            state.vz = 0;
            state.isGrounded = true;
        }
        else if (state.z - collision.groundZ < 0.5f)
        {
            std::cout << "  On ground (within 0.5)" << std::endl;
            state.isGrounded = true;
        }
        else
        {
            std::cout << "  Too far above ground - not grounded" << std::endl;
            state.isGrounded = false;
        }
    }

    // Fill output structure
    output.x = state.x;
    output.y = state.y;
    output.z = state.z;
    output.orientation = state.orientation;
    output.pitch = state.pitch;
    output.vx = state.vx;
    output.vy = state.vy;
    output.vz = state.vz;
    output.isGrounded = state.isGrounded;
    output.isSwimming = state.isSwimming;
    output.isFlying = state.isFlying;
    output.groundZ = collision.groundZ;
    output.liquidZ = collision.liquidZ;
    output.collided = false;

    // Update movement flags
    std::cout << "Updating movement flags..." << std::endl;
    std::cout << "  Input flags: 0x" << std::hex << input.moveFlags << std::dec << std::endl;
    output.moveFlags = input.moveFlags;

    if (!state.isGrounded && !state.isSwimming)
    {
        std::cout << "  Adding FALLING flag" << std::endl;
        output.moveFlags |= MOVEFLAG_FALLING;
    }
    else
    {
        std::cout << "  Removing FALLING flag" << std::endl;
        output.moveFlags &= ~MOVEFLAG_FALLING;
    }

    if (state.isSwimming)
    {
        std::cout << "  Adding SWIMMING flag" << std::endl;
        output.moveFlags |= MOVEFLAG_SWIMMING;
    }
    else
    {
        std::cout << "  Removing SWIMMING flag" << std::endl;
        output.moveFlags &= ~MOVEFLAG_SWIMMING;
    }

    std::cout << "  Output flags: 0x" << std::hex << output.moveFlags << std::dec << std::endl;

    // Calculate fall damage info
    if (!state.isGrounded && state.vz < 0)
    {
        output.fallTime = state.fallTime;
        output.fallDistance = state.fallStartZ - state.z;
        std::cout << "Falling - Time: " << output.fallTime << ", Distance: " << output.fallDistance << std::endl;
    }
    else
    {
        output.fallTime = 0;
        output.fallDistance = 0;
    }

    // Update spline progress
    if (input.hasSplinePath)
    {
        output.currentSplineIndex = input.currentSplineIndex;
        output.splineProgress = GetSplineProgress(state.x, state.y, state.z,
            input.splinePoints, input.currentSplineIndex);
    }

    std::cout << "Final Output:" << std::endl;
    std::cout << "  Position: (" << output.x << ", " << output.y << ", " << output.z << ")" << std::endl;
    std::cout << "  Velocity: (" << output.vx << ", " << output.vy << ", " << output.vz << ")" << std::endl;
    std::cout << "  Grounded: " << output.isGrounded << ", Swimming: " << output.isSwimming << ", Flying: " << output.isFlying << std::endl;
    std::cout << "  MoveFlags: 0x" << std::hex << output.moveFlags << std::dec << std::endl;
    std::cout << "========== PHYSICS STEP END ==========\n" << std::endl;

    return output;
}

float PhysicsEngine::GetHeight(uint32_t mapId, float x, float y, float z, bool checkVMap, float maxSearchDist)
{
    // Add comprehensive logging
    LOG_INFO("==================== PhysicsEngine::GetHeight START ====================");
    LOG_INFO("GetHeight called - Map:" << mapId
        << " Pos:(" << x << "," << y << "," << z << ")"
        << " checkVMap:" << checkVMap
        << " maxSearchDist:" << maxSearchDist);

    std::cout << "[Physics::GetHeight] Called - Map: " << mapId
        << ", Pos: (" << x << ", " << y << ", " << z << ")"
        << ", checkVMap: " << checkVMap << ", maxSearchDist: " << maxSearchDist << std::endl;

    // Get terrain height from ADT data
    LOG_DEBUG("Getting ADT terrain height...");
    float adtHeight = GetADTHeight(mapId, x, y, z);
    std::cout << "[Physics::GetHeight] ADT height: " << adtHeight << std::endl;
    LOG_INFO("ADT height: " << adtHeight);

    if (!checkVMap || !m_vmapHeightEnabled)
    {
        std::cout << "[Physics::GetHeight] Returning ADT height (no VMAP check)" << std::endl;
        LOG_INFO("Returning ADT height only - checkVMap:" << checkVMap
            << " vmapEnabled:" << m_vmapHeightEnabled);
        LOG_INFO("==================== PhysicsEngine::GetHeight END ====================");
        return adtHeight;
    }

    // Get VMAP height
    LOG_DEBUG("Getting VMAP height...");

    // ADD THIS: Ensure the map and tile are loaded before querying
    LOG_INFO("Ensuring map is loaded before VMAP query...");
    EnsureMapLoaded(mapId);

    // ADD THIS: Calculate which tile we need and try to load it
    if (m_vmapClient)
    {
        const float GRID_SIZE = 533.33333f;
        const float MID = 32.0f * GRID_SIZE;
        int tileX = (int)((MID - y) / GRID_SIZE);
        int tileY = (int)((MID - x) / GRID_SIZE);

        LOG_INFO("Position (" << x << "," << y << ") requires tile [" << tileX << "," << tileY << "]");
        LOG_INFO("Attempting to load tile...");

        bool tileLoaded = m_vmapClient->loadMapTile(mapId, tileX, tileY);
        LOG_INFO("Tile load attempt: " << (tileLoaded ? "SUCCESS" : "FAILED"));
    }

    float vmapHeight = GetVMapHeight(mapId, x, y, z, maxSearchDist);
    std::cout << "[Physics::GetHeight] VMAP height: " << vmapHeight << std::endl;
    LOG_INFO("VMAP height: " << vmapHeight);

    // Select best height using vMaNGOS logic
    LOG_DEBUG("Selecting best height between VMAP and ADT...");
    float finalHeight = SelectBestHeight(vmapHeight, adtHeight, z, maxSearchDist);
    std::cout << "[Physics::GetHeight] Final height: " << finalHeight << std::endl;

    LOG_INFO("Final height selected: " << finalHeight);
    LOG_INFO("  ADT valid: " << (adtHeight > INVALID_HEIGHT_VALUE ? "YES" : "NO"));
    LOG_INFO("  VMAP valid: " << (vmapHeight > VMAP_INVALID_HEIGHT_VALUE ? "YES" : "NO"));
    LOG_INFO("  Selection logic: " <<
        (finalHeight == vmapHeight ? "VMAP" :
            finalHeight == adtHeight ? "ADT" : "INVALID"));

    LOG_INFO("==================== PhysicsEngine::GetHeight END ====================");
    return finalHeight;
}

PhysicsEngine::CollisionInfo PhysicsEngine::QueryEnvironment(uint32_t mapId, float x, float y, float z,
    float radius, float height)
{
    std::cout << "\n[Physics::QueryEnvironment] Map: " << mapId
        << ", Pos: (" << x << ", " << y << ", " << z << ")" << std::endl;

    CollisionInfo info = {};

    // Get ground height using vMaNGOS-style logic
    float finalHeight = GetHeight(mapId, x, y, z, m_vmapHeightEnabled, DEFAULT_HEIGHT_SEARCH);

    info.hasGround = (finalHeight > INVALID_HEIGHT_VALUE);
    info.groundZ = info.hasGround ? finalHeight : INVALID_HEIGHT_VALUE;

    // Store individual height sources for debugging
    info.vmapHeight = GetVMapHeight(mapId, x, y, z, DEFAULT_HEIGHT_SEARCH);

    info.adtHeight = finalHeight;  // Just use the result we already have
    info.vmapValid = (info.vmapHeight > VMAP_INVALID_HEIGHT_VALUE);
    info.adtValid = (info.adtHeight > INVALID_HEIGHT_VALUE);

    std::cout << "[Physics::QueryEnvironment] Ground: " << (info.hasGround ? "YES" : "NO")
        << ", Z=" << info.groundZ << std::endl;

    // Get liquid info
    float liquidFloor;
    if (GetLiquidInfo(mapId, x, y, z, info.liquidZ, liquidFloor, info.liquidType))
    {
        info.hasLiquid = true;
        std::cout << "[Physics::QueryEnvironment] Liquid: YES, Z=" << info.liquidZ
            << ", Type=" << info.liquidType << std::endl;
    }
    else
    {
        info.hasLiquid = false;
        info.liquidZ = INVALID_HEIGHT_VALUE;
        std::cout << "[Physics::QueryEnvironment] Liquid: NO" << std::endl;
    }

    // Check if indoors (using VMAP area info)
    if (m_vmapIndoorCheckEnabled && m_vmapClient)
    {
        uint32_t flags;
        int32_t adtId, rootId, groupId;
        float checkZ = z;
        m_vmapClient->getAreaInfo(mapId, x, y, checkZ, flags, adtId, rootId, groupId);
        info.isIndoors = (rootId >= 0 && groupId >= 0);  // Inside a WMO
        std::cout << "[Physics::QueryEnvironment] Indoor: " << (info.isIndoors ? "YES" : "NO") << std::endl;
    }

    // Calculate ground normal (simplified - assumes flat for now)
    info.groundNormalZ = 1.0f;

    return info;
}

bool PhysicsEngine::GetLiquidInfo(uint32_t mapId, float x, float y, float z, float& liquidLevel, float& liquidFloor, uint32_t& liquidType)
{
    // First try to get liquid from MapLoader (ADT data)
    if (m_mapLoader && m_mapLoader->IsInitialized())
    {
        liquidLevel = m_mapLoader->GetLiquidLevel(mapId, x, y, z);
        if (liquidLevel > INVALID_HEIGHT_VALUE)
        {
            liquidType = m_mapLoader->GetLiquidType(mapId, x, y);
            // Convert to vmangos liquid mask

            liquidFloor = liquidLevel - 2.0f;  // Estimate floor
            return true;
        }
    }

    // Then try VMAP for WMO liquids
    if (m_vmapClient)
    {
        try
        {
            if (m_vmapClient->getLiquidLevel(mapId, x, y, z, liquidLevel, liquidFloor))
            {
                liquidType = VMAP::MAP_LIQUID_TYPE_WATER;  // Default for VMAP liquids
                return true;
            }
        }
        catch (const std::exception& e)
        {
        }
    }

    return false;
}

// Movement handlers remain the same...
PhysicsEngine::MovementState PhysicsEngine::HandleGroundMovement(const PhysicsInput& input,
    MovementState& state, float dt)
{
    // Calculate movement direction from flags
    float moveForward = 0;
    float moveStrafe = 0;

    if (input.moveFlags & MOVEFLAG_FORWARD)
        moveForward = 1.0f;
    else if (input.moveFlags & MOVEFLAG_BACKWARD)
        moveForward = -1.0f;

    if (input.moveFlags & MOVEFLAG_STRAFE_LEFT)
        moveStrafe = -1.0f;
    else if (input.moveFlags & MOVEFLAG_STRAFE_RIGHT)
        moveStrafe = 1.0f;

    float moveX = 0, moveY = 0;
    float speed = CalculateMoveSpeed(input, false, false);

    if (moveForward != 0 || moveStrafe != 0)
    {
        std::cout << "=== MOVEMENT DEBUG ===" << std::endl;
        std::cout << "Movement - Forward: " << moveForward << ", Strafe: " << moveStrafe << std::endl;
        std::cout << "Orientation (rad): " << state.orientation << " (deg): " << (state.orientation * 180.0f / 3.14159f) << std::endl;

        float cos_o = std::cos(state.orientation);
        float sin_o = std::sin(state.orientation);

        std::cout << "Trig values - cos: " << cos_o << ", sin: " << sin_o << std::endl;

        // Calculate each component separately for debugging
        float forward_x = moveForward * cos_o;
        float forward_y = moveForward * sin_o;
        float strafe_x = moveStrafe * sin_o;
        float strafe_y = -moveStrafe * cos_o;

        std::cout << "Forward contribution: X=" << forward_x << ", Y=" << forward_y << std::endl;
        std::cout << "Strafe contribution: X=" << strafe_x << ", Y=" << strafe_y << std::endl;

        moveX = forward_x + strafe_x;
        moveY = forward_y + strafe_y;

        std::cout << "Combined movement: X=" << moveX << ", Y=" << moveY << std::endl;

        // Show what the expected position change will be
        float expectedDeltaX = moveX * speed * dt;
        float expectedDeltaY = moveY * speed * dt;
        std::cout << "Speed: " << speed << ", dt: " << dt << std::endl;
        std::cout << "Expected position delta: X=" << expectedDeltaX << ", Y=" << expectedDeltaY << std::endl;
        std::cout << "Expected new position: X=" << (state.x + expectedDeltaX) << ", Y=" << (state.y + expectedDeltaY) << std::endl;

        // Verify the orientation is what we expect
        float north_angle = 1.5708f; // 90 degrees
        float east_angle = 0.0f;      // 0 degrees
        float south_angle = 4.7124f;  // 270 degrees
        float west_angle = 3.14159f;  // 180 degrees

        std::cout << "Reference angles - North: " << north_angle << " East: " << east_angle
            << " South: " << south_angle << " West: " << west_angle << std::endl;

        // Determine approximate cardinal direction
        const char* facing_dir = "Unknown";
        float deg = state.orientation * 180.0f / 3.14159f;
        if (deg < 0) deg += 360.0f;

        if (deg >= 337.5f || deg < 22.5f) facing_dir = "East";
        else if (deg >= 22.5f && deg < 67.5f) facing_dir = "NorthEast";
        else if (deg >= 67.5f && deg < 112.5f) facing_dir = "North";
        else if (deg >= 112.5f && deg < 157.5f) facing_dir = "NorthWest";
        else if (deg >= 157.5f && deg < 202.5f) facing_dir = "West";
        else if (deg >= 202.5f && deg < 247.5f) facing_dir = "SouthWest";
        else if (deg >= 247.5f && deg < 292.5f) facing_dir = "South";
        else if (deg >= 292.5f && deg < 337.5f) facing_dir = "SouthEast";

        std::cout << "Facing direction: " << facing_dir << std::endl;
    }

    // Apply movement
    state.vx = moveX * speed;
    state.vy = moveY * speed;
    state.vz = 0;

    // Update position
    state.x += state.vx * dt;
    state.y += state.vy * dt;

    return state;
}

PhysicsEngine::MovementState PhysicsEngine::HandleAirMovement(const PhysicsInput& input,
    MovementState& state, float dt)
{
    // Store initial velocity for proper integration
    float initial_vz = state.vz;

    // Apply gravity if not flying
    if (!(input.moveFlags & MOVEFLAG_FLYING))
    {
        ApplyGravity(state, dt);
        state.fallTime += dt;
    }

    // Calculate movement from flags for limited air control
    float moveForward = 0;
    float moveStrafe = 0;

    if (input.moveFlags & MOVEFLAG_FORWARD)
        moveForward = 1.0f;
    else if (input.moveFlags & MOVEFLAG_BACKWARD)
        moveForward = -1.0f;

    if (input.moveFlags & MOVEFLAG_STRAFE_LEFT)
        moveStrafe = -1.0f;
    else if (input.moveFlags & MOVEFLAG_STRAFE_RIGHT)
        moveStrafe = 1.0f;

    if (moveForward != 0 || moveStrafe != 0)
    {
        float forward = moveForward * AIR_CONTROL_FACTOR;
        float strafe = moveStrafe * AIR_CONTROL_FACTOR;

        // WoW coordinate system
        float cos_o = std::cos(state.orientation);
        float sin_o = std::sin(state.orientation);

        float moveX = forward * sin_o + strafe * cos_o;
        float moveY = forward * cos_o - strafe * sin_o;

        float speed = CalculateMoveSpeed(input, false, true);

        state.vx += moveX * speed * dt;
        state.vy += moveY * speed * dt;
    }

    // Update position using average velocity for proper integration
    state.x += state.vx * dt;
    state.y += state.vy * dt;
    state.z += (initial_vz + state.vz) * 0.5f * dt;  // Use average velocity for Z

    // Update orientation for turning
    if (input.moveFlags & MOVEFLAG_TURN_LEFT)
    {
        state.orientation -= TURN_SPEED * dt;
        state.orientation = NormalizeAngle(state.orientation);
    }
    else if (input.moveFlags & MOVEFLAG_TURN_RIGHT)
    {
        state.orientation += TURN_SPEED * dt;
        state.orientation = NormalizeAngle(state.orientation);
    }

    return state;
}

PhysicsEngine::MovementState PhysicsEngine::HandleSwimMovement(const PhysicsInput& input,
    MovementState& state, float dt)
{
    // Calculate movement from flags
    float moveForward = 0;
    float moveStrafe = 0;

    if (input.moveFlags & MOVEFLAG_FORWARD)
        moveForward = 1.0f;
    else if (input.moveFlags & MOVEFLAG_BACKWARD)
        moveForward = -1.0f;

    if (input.moveFlags & MOVEFLAG_STRAFE_LEFT)
        moveStrafe = -1.0f;
    else if (input.moveFlags & MOVEFLAG_STRAFE_RIGHT)
        moveStrafe = 1.0f;

    // Swimming uses full 3D movement
    float moveX = 0, moveY = 0, moveZ = 0;

    if (moveForward != 0 || moveStrafe != 0)
    {
        float forward = moveForward;
        float strafe = moveStrafe;

        // WoW coordinate system
        float cos_o = std::cos(state.orientation);
        float sin_o = std::sin(state.orientation);
        float cos_p = std::cos(state.pitch);
        float sin_p = std::sin(state.pitch);

        moveX = forward * sin_o + strafe * cos_o;
        moveY = forward * cos_o - strafe * sin_o;
        moveZ = forward * sin_p;

        // Normalize
        float len = std::sqrt(moveX * moveX + moveY * moveY + moveZ * moveZ);
        if (len > 0.0001f)
        {
            moveX /= len;
            moveY /= len;
            moveZ /= len;
        }
    }

    // Get swim speed
    float speed = input.swimSpeed;

    // Apply movement
    state.vx = moveX * speed;
    state.vy = moveY * speed;
    state.vz = moveZ * speed;

    // Update position
    state.x += state.vx * dt;
    state.y += state.vy * dt;
    state.z += state.vz * dt;

    // Update orientation for turning
    if (input.moveFlags & MOVEFLAG_TURN_LEFT)
    {
        state.orientation -= TURN_SPEED * dt;
        state.orientation = NormalizeAngle(state.orientation);
    }
    else if (input.moveFlags & MOVEFLAG_TURN_RIGHT)
    {
        state.orientation += TURN_SPEED * dt;
        state.orientation = NormalizeAngle(state.orientation);
    }

    // Update pitch for swimming up/down
    if (input.moveFlags & MOVEFLAG_PITCH_UP)
    {
        state.pitch = Clamp(state.pitch - dt, -1.57f, 1.57f);
    }
    else if (input.moveFlags & MOVEFLAG_PITCH_DOWN)
    {
        state.pitch = Clamp(state.pitch + dt, -1.57f, 1.57f);
    }

    return state;
}

PhysicsEngine::MovementState PhysicsEngine::HandleSplineMovement(const PhysicsInput& input,
    MovementState& state, float dt)
{
    if (!input.splinePoints || input.splinePointCount < 2)
    {
        return state;
    }

    // Get current and next spline points
    int currentIndex = Clamp(input.currentSplineIndex, 0, input.splinePointCount - 2);

    float* current = const_cast<float*>(&input.splinePoints[currentIndex * 3]);
    float* next = const_cast<float*>(&input.splinePoints[(currentIndex + 1) * 3]);

    // Calculate direction to next point
    float dx = next[0] - current[0];
    float dy = next[1] - current[1];
    float dz = next[2] - current[2];

    float distance = std::sqrt(dx * dx + dy * dy + dz * dz);

    if (distance > 0.0001f)
    {
        // Normalize direction
        dx /= distance;
        dy /= distance;
        dz /= distance;

        // Set velocity
        state.vx = dx * input.splineSpeed;
        state.vy = dy * input.splineSpeed;
        state.vz = dz * input.splineSpeed;

        // Update position
        state.x += state.vx * dt;
        state.y += state.vy * dt;
        state.z += state.vz * dt;

        // Update orientation to face movement direction
        state.orientation = std::atan2(dy, dx);
    }

    return state;
}

void PhysicsEngine::ApplyGravity(MovementState& state, float dt)
{
    state.vz -= GRAVITY * dt;

    if (state.vz < -54.0f)  // Terminal velocity (vmangos value)
        state.vz = -54.0f;
}

void PhysicsEngine::ApplyFriction(MovementState& state, float friction, float dt)
{
    float factor = std::exp(-friction * dt);
    state.vx *= factor;
    state.vy *= factor;
    if (!state.isGrounded)
        state.vz *= factor;
}

void PhysicsEngine::ApplyKnockback(MovementState& state, float vx, float vy, float vz)
{
    state.vx += vx;
    state.vy += vy;
    state.vz += vz;
}

float PhysicsEngine::CalculateMoveSpeed(const PhysicsInput& input, bool isSwimming, bool isFlying)
{
    if (isFlying)
        return input.flightSpeed;
    if (isSwimming)
        return input.swimSpeed;
    if (input.moveFlags & MOVEFLAG_WALK_MODE)
        return input.walkSpeed;
    if (input.moveFlags & MOVEFLAG_BACKWARD)  // Moving backward
        return input.backSpeed;
    return input.runSpeed;
}

float PhysicsEngine::GetGroundHeight(uint32_t mapId, float x, float y, float z)
{
    // Use the main GetHeight function with vMaNGOS logic
    return GetHeight(mapId, x, y, z, m_vmapHeightEnabled, DEFAULT_HEIGHT_SEARCH);
}

float PhysicsEngine::GetLiquidHeight(uint32_t mapId, float x, float y, float z, uint32_t& liquidType)
{
    // First try to get liquid from MapLoader (ADT data)
    if (m_mapLoader && m_mapLoader->IsInitialized())
    {
        float liquidLevel = m_mapLoader->GetLiquidLevel(mapId, x, y, z);
        if (liquidLevel > INVALID_HEIGHT_VALUE)
        {
            liquidType = m_mapLoader->GetLiquidType(mapId, x, y);
            return liquidLevel;
        }
    }

    // Then try VMAP for WMO liquids
    if (m_vmapClient)
    {
        try
        {
            float liquidLevel, liquidFloor;
            if (m_vmapClient->getLiquidLevel(mapId, x, y, z, liquidLevel, liquidFloor))
            {
                liquidType = 0;  // TODO: Get actual liquid type from VMAP
                return liquidLevel;
            }
        }
        catch (const std::exception& e)
        {
        }
    }

    return INVALID_HEIGHT_VALUE;
}

void PhysicsEngine::ResolveCollisions(uint32_t mapId, MovementState& state, float radius, float height)
{
    // For now, disable collision resolution entirely since it's causing false positives
    // This is temporary until we can properly tune the collision detection
    return;

    /* COMMENTED OUT - Collision system needs tuning */
    if (!m_vmapClient || !m_navigation)
    {
        return;
    }

    try
    {
        // Only check for collision if we're actually moving significantly
        float moveSpeed = std::sqrt(state.vx * state.vx + state.vy * state.vy);
        if (moveSpeed < 0.1f)  // Not moving fast enough to worry about collision
        {
            return;
        }

        // Check further ahead based on actual movement speed
        float checkTime = 0.5f;  // Check 0.5 seconds ahead
        float nextX = state.x + state.vx * checkTime;
        float nextY = state.y + state.vy * checkTime;
        float nextZ = state.z + state.vz * checkTime;

        float hitX, hitY, hitZ;
        bool hasCollision = CheckCollision(mapId,
            state.x, state.y, state.z,
            nextX, nextY, nextZ,
            radius, height, hitX, hitY, hitZ);

        if (hasCollision)
        {
            // Calculate distance to collision
            float dx = hitX - state.x;
            float dy = hitY - state.y;
            float dz = hitZ - state.z;
            float distToCollision = std::sqrt(dx*dx + dy*dy + dz*dz);

            // Only react if collision is actually close
            if (distToCollision < 5.0f)  // Within 5 yards
            {
                // Just stop movement, don't try to slide or adjust position
                state.vx = 0;
                state.vy = 0;
                if (state.vz < 0)  // Only stop downward velocity
                    state.vz = 0;

                // Log collision for debugging
                std::cout << "[COLLISION] Detected at distance " << distToCollision << " - stopping movement" << std::endl;
            }
        }
    }
    catch (const std::exception& e)
    {
        // On error, just return without modifying anything
    }
}

bool PhysicsEngine::CheckCollision(uint32_t mapId, float startX, float startY, float startZ,
    float endX, float endY, float endZ, float radius, float height,
    float& hitX, float& hitY, float& hitZ)
{
    // Temporarily disable collision checking until we can fix false positives
    return false;

    /* COMMENTED OUT - Needs proper tuning
    if (!m_vmapClient)
    {
        return false;
    }

    try
    {
        // Check VMAP for collisions (buildings, walls, etc)
        bool vmapHit = m_vmapClient->getCollisionPoint(mapId, startX, startY, startZ,
            endX, endY, endZ, hitX, hitY, hitZ);

        if (vmapHit)
        {
            // Additional validation - make sure it's a real collision
            float dx = hitX - startX;
            float dy = hitY - startY;
            float dz = hitZ - startZ;
            float dist = std::sqrt(dx*dx + dy*dy + dz*dz);

            // Ignore very small distances (likely false positives from terrain)
            if (dist < 0.5f)
            {
                return false;
            }

            std::cout << "[VMAP Collision] Hit at (" << hitX << ", " << hitY << ", " << hitZ
                      << ") distance: " << dist << std::endl;
            return true;
        }

        // Also check NavMesh if available for additional collision
        if (m_navigation)
        {
            XYZ from = { startX, startY, startZ };
            XYZ to = { endX, endY, endZ };
            bool navMeshLOS = m_navigation->IsLineOfSight(mapId, from, to);

            if (!navMeshLOS)
            {
                // NavMesh reports collision but we need to be careful about false positives
                // Only trust it if we're checking a reasonable distance
                float checkDist = std::sqrt(
                    (endX-startX)*(endX-startX) +
                    (endY-startY)*(endY-startY) +
                    (endZ-startZ)*(endZ-startZ)
                );

                if (checkDist > 1.0f && checkDist < 10.0f)  // Reasonable range
                {
                    hitX = endX;
                    hitY = endY;
                    hitZ = endZ;
                    std::cout << "[NavMesh Collision] No LOS to target" << std::endl;
                    return true;
                }
            }
        }

        return false;
    }
    catch (const std::exception& e)
    {
        return false;
    }
    */
}

float PhysicsEngine::GetSplineProgress(float x, float y, float z, const float* points, int index)
{
    if (!points || index < 0)
        return 0.0f;

    const float* current = &points[index * 3];
    const float* next = &points[(index + 1) * 3];

    float totalDist = Distance3D(current[0], current[1], current[2],
        next[0], next[1], next[2]);
    float currentDist = Distance3D(current[0], current[1], current[2],
        x, y, z);

    if (totalDist > 0.0001f)
        return Clamp(currentDist / totalDist, 0.0f, 1.0f);

    return 0.0f;
}

bool PhysicsEngine::IsGrounded(uint32_t mapId, float x, float y, float z, float radius, float height)
{
    EnsureMapLoaded(mapId);
    float groundZ = GetHeight(mapId, x, y, z, true, DEFAULT_HEIGHT_SEARCH);
    bool grounded = (groundZ > INVALID_HEIGHT_VALUE &&
        std::abs(z - groundZ) < GROUND_HEIGHT_TOLERANCE);
    return grounded;
}

bool PhysicsEngine::IsInWater(uint32_t mapId, float x, float y, float z, float height)
{
    EnsureMapLoaded(mapId);
    uint32_t liquidType;
    float liquidZ = GetLiquidHeight(mapId, x, y, z, liquidType);

    // Use the same swimming depth calculation as in Step() with tolerance
    float swimDepth = height * 0.75f;
    bool inWater = (liquidZ > INVALID_HEIGHT_VALUE && z < (liquidZ - swimDepth + 0.02f));

    return inWater;
}

float PhysicsEngine::GetFallDamage(float fallDistance, bool hasSafeFall)
{
    if (hasSafeFall || fallDistance < SAFE_FALL_HEIGHT)
        return 0.0f;

    if (fallDistance > LETHAL_FALL_HEIGHT)
        return 10000.0f;  // Lethal

    // Linear interpolation between safe and lethal
    float factor = (fallDistance - SAFE_FALL_HEIGHT) / (LETHAL_FALL_HEIGHT - SAFE_FALL_HEIGHT);
    float damage = factor * 1000.0f;

    return damage;
}