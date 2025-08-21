// PhysicsEngine.cpp
#include "PhysicsEngine.h"
#include "VMapDefinitions.h"
#include "Navigation.h"
#include "VMapClient.h"
#include "VMapFactory.h"
#include "MapLoader.h"
#include "VMapLog.h"
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
	if (m_currentMapId != mapId)
	{
		if (m_vmapClient)
		{
			m_vmapClient->preloadMap(mapId);
		}

		m_currentMapId = mapId;
	}
}


// Main vMaNGOS-style GetHeight implementation
float PhysicsEngine::GetVMapHeight(uint32_t mapId, float x, float y, float z, float maxSearchDist)
{
	if (!m_vmapClient)
	{
		return PhysicsConstants::INVALID_HEIGHT;
	}

	if (!m_vmapHeightEnabled)
	{
		return PhysicsConstants::INVALID_HEIGHT;
	}

	return m_vmapClient->getGroundHeight(mapId, x, y, z, maxSearchDist);
}

// Get height from ADT terrain data
float PhysicsEngine::GetADTHeight(uint32_t mapId, float x, float y, float z)
{
	if (!m_mapLoader || !m_mapLoader->IsInitialized())
	{
		return PhysicsConstants::INVALID_HEIGHT;
	}

	// MapLoader expects world coordinates directly
	float adtHeight = m_mapLoader->GetHeight(mapId, x, y);
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
	if (vmapHeight <= PhysicsConstants::INVALID_HEIGHT && adtHeight <= PhysicsConstants::INVALID_HEIGHT)
	{
		LOG_DEBUG("Both heights invalid - returning INVALID");
		return PhysicsConstants::INVALID_HEIGHT;
	}

	// Only ADT valid
	if (vmapHeight <= PhysicsConstants::INVALID_HEIGHT)
	{
		LOG_DEBUG("Only ADT valid - returning ADT height");
		return adtHeight;
	}

	// Only VMAP valid
	if (adtHeight <= PhysicsConstants::INVALID_HEIGHT)
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
	LOG_INFO("========== PHYSICS STEP START ==========");
	LOG_INFO("Input Position: (" << input.x << ", " << input.y << ", " << input.z << ")");
	LOG_INFO("Input Velocity: (" << input.vx << ", " << input.vy << ", " << input.vz << ")");
	LOG_INFO("Input MoveFlags: 0x" << std::hex << input.moveFlags << std::dec);
	LOG_INFO("Delta Time: " << dt);

	PhysicsOutput output = {};

	// Quick passthrough if not initialized
	if (!m_initialized)
	{
		LOG_WARN("Physics not initialized - passthrough mode");
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

	// Ensure map is loaded (only happens on map change)
	EnsureMapLoaded(input.mapId);

	// ========== STEP 1: Initialize Movement State ==========
	MovementState state{};
	state.x = input.x;
	state.y = input.y;
	state.z = input.z;
	state.orientation = input.orientation;
	state.pitch = input.pitch;
	state.vx = 0;  // Don't inherit velocity for normal movement
	state.vy = 0;
	state.vz = 0;
	state.fallTime = input.fallTime;
	state.fallStartZ = input.z;

	// ========== STEP 2: Query Environment Using New System ==========
	LOG_INFO("Querying environment with full collision info...");

	CollisionInfo collision = QueryEnvironment(input.mapId, state.x, state.y, state.z,
		input.radius, input.height);

	LOG_INFO("Environment query results:");
	LOG_INFO("  hasGround: " << collision.hasGround << ", groundZ: " << collision.groundZ);
	LOG_INFO("  isIndoors: " << collision.isIndoors);
	LOG_INFO("  hasLiquid: " << collision.hasLiquid << ", liquidZ: " << collision.liquidZ);
	LOG_INFO("  vmapValid: " << collision.vmapValid << ", vmapHeight: " << collision.vmapHeight);
	LOG_INFO("  adtValid: " << collision.adtValid << ", adtHeight: " << collision.adtHeight);

	// ========== STEP 3: Determine Movement State ==========

	// Check if unit can fly/swim based on flags
	bool canFly = (input.moveFlags & MOVEFLAG_FLYING) != 0;
	bool canSwim = (input.moveFlags & MOVEFLAG_SWIMMING) != 0;

	// Calculate precise distance to ground
	float distToGround = collision.hasGround ? (state.z - collision.groundZ) : PhysicsConstants::MAX_HEIGHT;

	LOG_INFO("Distance to ground: " << distToGround);

	// Determine if grounded (using tight tolerance)
	bool wasGrounded = state.isGrounded;
	state.isGrounded = collision.hasGround &&
		distToGround >= -PhysicsConstants::GROUND_HEIGHT_TOLERANCE &&
		distToGround <= PhysicsConstants::GROUND_HEIGHT_TOLERANCE;

	// Special case: If indoors and close to WMO floor, snap to it
	if (collision.isIndoors && collision.vmapValid)
	{
		float distToWmoFloor = std::abs(state.z - collision.vmapHeight);
		if (distToWmoFloor < PhysicsConstants::STEP_HEIGHT)
		{
			LOG_INFO("Indoor WMO floor detected - using precise WMO height: " << collision.vmapHeight);
			collision.groundZ = collision.vmapHeight;  // Prefer WMO floor when indoors
			distToGround = state.z - collision.groundZ;
			state.isGrounded = distToGround >= -PhysicsConstants::GROUND_HEIGHT_TOLERANCE &&
				distToGround <= PhysicsConstants::GROUND_HEIGHT_TOLERANCE;
		}
	}

	LOG_INFO("Grounded check - wasGrounded: " << wasGrounded
		<< ", isGrounded: " << state.isGrounded
		<< ", isIndoors: " << collision.isIndoors);

	// Determine if swimming
	state.isSwimming = collision.hasLiquid && canSwim &&
		(state.z < collision.liquidZ) &&
		(state.z > collision.groundZ + 1.0f);  // Not walking on bottom

	state.isFlying = canFly;

	LOG_INFO("Movement state - isSwimming: " << state.isSwimming
		<< ", isFlying: " << state.isFlying);

	// ========== STEP 4: Calculate Movement Direction ==========
	float moveX = 0, moveY = 0, moveZ = 0;

	if (input.moveFlags & MOVEFLAG_FORWARD)
	{
		moveX = std::cos(state.orientation);
		moveY = std::sin(state.orientation);
		LOG_INFO("Moving FORWARD");
	}
	else if (input.moveFlags & MOVEFLAG_BACKWARD)
	{
		moveX = -std::cos(state.orientation);
		moveY = -std::sin(state.orientation);
		LOG_INFO("Moving BACKWARD");
	}

	if (input.moveFlags & MOVEFLAG_STRAFE_LEFT)
	{
		moveX -= std::sin(state.orientation);
		moveY += std::cos(state.orientation);
		LOG_INFO("Strafing LEFT");
	}
	else if (input.moveFlags & MOVEFLAG_STRAFE_RIGHT)
	{
		moveX += std::sin(state.orientation);
		moveY -= std::cos(state.orientation);
		LOG_INFO("Strafing RIGHT");
	}

	// Normalize diagonal movement
	float moveLength = std::sqrt(moveX * moveX + moveY * moveY);
	if (moveLength > 1.0f)
	{
		moveX /= moveLength;
		moveY /= moveLength;
		LOG_INFO("Normalized diagonal movement");
	}

	// Handle pitch for swimming/flying
	if (state.isSwimming || state.isFlying)
	{
		moveZ = std::sin(state.pitch);
		float horizontalScale = std::cos(state.pitch);
		moveX *= horizontalScale;
		moveY *= horizontalScale;
		LOG_INFO("Applied pitch for 3D movement - moveZ: " << moveZ);
	}

	// ========== STEP 5: Apply Movement Based on State ==========

	if (state.isFlying)
	{
		LOG_INFO("Movement handler: FLYING");

		float speed = (input.moveFlags & MOVEFLAG_WALK_MODE) ? input.walkSpeed : input.runSpeed;

		// Flying allows free 3D movement
		state.x += moveX * speed * dt;
		state.y += moveY * speed * dt;
		state.z += moveZ * speed * dt;

		LOG_INFO("Flying movement applied - new pos: ("
			<< state.x << ", " << state.y << ", " << state.z << ")");
	}
	else if (state.isSwimming)
	{
		LOG_INFO("Movement handler: SWIMMING");

		float speed = (input.moveFlags & MOVEFLAG_BACKWARD) ? input.swimBackSpeed : input.swimSpeed;

		// Apply swimming movement
		state.x += moveX * speed * dt;
		state.y += moveY * speed * dt;
		state.z += moveZ * speed * dt;

		// Constrain to water column
		if (collision.hasGround && state.z < collision.groundZ + PhysicsConstants::GROUND_HEIGHT_TOLERANCE)
		{
			state.z = collision.groundZ + PhysicsConstants::GROUND_HEIGHT_TOLERANCE;
			LOG_INFO("Clamped to ground while swimming");
		}
		else if (state.z > collision.liquidZ - 0.1f)
		{
			state.z = collision.liquidZ - 0.1f;
			LOG_INFO("Clamped to water surface");
		}

		LOG_INFO("Swimming movement applied - new pos: ("
			<< state.x << ", " << state.y << ", " << state.z << ")");
	}
	else if (state.isGrounded)
	{
		LOG_INFO("Movement handler: GROUND");

		// Determine speed based on movement flags
		float speed = (input.moveFlags & MOVEFLAG_WALK_MODE) ? input.walkSpeed :
			(input.moveFlags & MOVEFLAG_BACKWARD) ? input.runBackSpeed :
			input.runSpeed;

		LOG_INFO("Ground speed: " << speed << " (walk="
			<< ((input.moveFlags & MOVEFLAG_WALK_MODE) ? "YES" : "NO") << ")");

		// Handle jumping
		if (input.moveFlags & MOVEFLAG_JUMP)
		{
			LOG_INFO("JUMPING initiated with velocity: " << PhysicsConstants::JUMP_VELOCITY);
			state.vz = PhysicsConstants::JUMP_VELOCITY;
			state.isGrounded = false;
			state.fallStartZ = state.z;
			state.fallTime = 0;
		}

		// Apply DIRECT position change for ground movement (NO VELOCITY)
		float newX = state.x + moveX * speed * dt;
		float newY = state.y + moveY * speed * dt;

		LOG_INFO("Checking new position: (" << newX << ", " << newY << ")");

		// Query environment at new position
		CollisionInfo newCollision = QueryEnvironment(input.mapId, newX, newY, state.z,
			input.radius, input.height);

		if (newCollision.hasGround)
		{
			float newGroundZ = newCollision.groundZ;

			// If indoors, prefer WMO height
			if (newCollision.isIndoors && newCollision.vmapValid)
			{
				LOG_INFO("Using indoor WMO height at new position: " << newCollision.vmapHeight);
				newGroundZ = newCollision.vmapHeight;
			}

			float heightDiff = newGroundZ - state.z;
			LOG_INFO("Height difference at new position: " << heightDiff
				<< " (new: " << newGroundZ << ", current: " << state.z << ")");

			// Check if we can step up/down
			if (std::abs(heightDiff) <= PhysicsConstants::STEP_HEIGHT)
			{
				// Normal step - update position
				LOG_INFO("Stepping " << (heightDiff > 0 ? "UP" : "DOWN")
					<< " to height " << newGroundZ);
				state.x = newX;
				state.y = newY;
				state.z = newGroundZ + PhysicsConstants::GROUND_HEIGHT_TOLERANCE;
			}
			else if (heightDiff > PhysicsConstants::STEP_HEIGHT)
			{
				// Check if it's a climbable slope
				float horizontalDist = std::sqrt((newX - state.x) * (newX - state.x) +
					(newY - state.y) * (newY - state.y));
				float slope = heightDiff / (horizontalDist > 0.01f ? horizontalDist : 1.0f);

				LOG_INFO("Slope check: slope=" << slope << " (45deg = 1.0)");

				if (slope < 1.0f)  // Less than 45 degrees
				{
					LOG_INFO("Walking up slope to height " << newGroundZ);
					state.x = newX;
					state.y = newY;
					state.z = newGroundZ + PhysicsConstants::GROUND_HEIGHT_TOLERANCE;
				}
				else
				{
					LOG_INFO("Slope too steep - movement BLOCKED");
					// Don't update position
				}
			}
			else  // heightDiff < -STEP_HEIGHT
			{
				// Falling off a ledge
				LOG_INFO("Falling off ledge - transitioning to AIR movement");
				state.x = newX;
				state.y = newY;
				state.isGrounded = false;
				state.fallStartZ = state.z;
				state.fallTime = 0;
				// Don't snap to ground - let gravity handle it
			}
		}
		else
		{
			LOG_WARN("No valid ground at new position - movement BLOCKED");
			// Don't update position
		}
	}
	else  // Airborne (falling/jumping)
	{
		LOG_INFO("Movement handler: AIRBORNE (falling/jumping)");

		// Update fall time
		state.fallTime += dt;

		// Apply gravity
		state.vz -= PhysicsConstants::GRAVITY * dt;

		// Cap at terminal velocity
		if (state.vz < -54.0f)
		{
			state.vz = -54.0f;
			LOG_INFO("Terminal velocity reached");
		}

		LOG_INFO("Airborne - vz: " << state.vz << ", fallTime: " << state.fallTime);

		// Apply horizontal air movement (with reduced control)
		float airControl = PhysicsConstants::AIR_CONTROL_FACTOR;
		float speed = (input.moveFlags & MOVEFLAG_WALK_MODE) ? input.walkSpeed : input.runSpeed;

		state.x += moveX * speed * airControl * dt;
		state.y += moveY * speed * airControl * dt;
		state.z += state.vz * dt;

		// Check for landing using collision info
		CollisionInfo airCollision = QueryEnvironment(input.mapId, state.x, state.y, state.z,
			input.radius, input.height);

		if (airCollision.hasGround && state.vz <= 0)  // Falling down
		{
			float groundZ = airCollision.groundZ;

			// Special handling for indoor landings
			if (airCollision.isIndoors && airCollision.vmapValid)
			{
				LOG_INFO("Landing check for indoor area - WMO floor at: " << airCollision.vmapHeight);
				groundZ = airCollision.vmapHeight;
			}

			float newDistToGround = state.z - groundZ;

			if (newDistToGround <= PhysicsConstants::GROUND_HEIGHT_TOLERANCE)
			{
				// Landed!
				float fallDistance = state.fallStartZ - groundZ;
				LOG_INFO("LANDED at height " << groundZ
					<< " (fall distance: " << fallDistance << ")"
					<< (airCollision.isIndoors ? " [INDOOR]" : " [OUTDOOR]"));

				state.z = groundZ + PhysicsConstants::GROUND_HEIGHT_TOLERANCE;
				state.vz = 0;
				state.isGrounded = true;
				state.fallTime = 0;

				// TODO: Calculate fall damage here if needed
				if (fallDistance > PhysicsConstants::SAFE_FALL_HEIGHT)
				{
					LOG_WARN("Fall damage should be applied - distance: " << fallDistance);
				}
			}
		}

		LOG_INFO("Airborne position: (" << state.x << ", " << state.y << ", " << state.z << ")");
	}

	// ========== STEP 6: Apply External Forces (Knockback) ==========
	if (std::abs(input.vx) > 0.01f || std::abs(input.vy) > 0.01f || std::abs(input.vz) > 0.01f)
	{
		LOG_INFO("Applying knockback/external velocity: ("
			<< input.vx << ", " << input.vy << ", " << input.vz << ")");

		state.x += input.vx * dt;
		state.y += input.vy * dt;

		if (!state.isGrounded)  // Only apply vertical knockback if airborne
		{
			state.vz += input.vz;  // Add to existing vertical velocity
		}
	}

	// ========== STEP 7: Final Position Validation ==========
	LOG_INFO("Final position validation...");

	// Do a final environment check at the final position
	CollisionInfo finalCollision = QueryEnvironment(input.mapId, state.x, state.y, state.z,
		input.radius, input.height);

	// Ensure we're not below terrain
	if (finalCollision.hasGround && state.z < finalCollision.groundZ)
	{
		LOG_WARN("Position below terrain! Correcting from " << state.z
			<< " to " << finalCollision.groundZ);

		// Use WMO floor if indoors
		if (finalCollision.isIndoors && finalCollision.vmapValid)
		{
			state.z = finalCollision.vmapHeight + PhysicsConstants::GROUND_HEIGHT_TOLERANCE;
			LOG_INFO("Snapped to indoor WMO floor: " << state.z);
		}
		else
		{
			state.z = finalCollision.groundZ + PhysicsConstants::GROUND_HEIGHT_TOLERANCE;
		}

		if (!state.isGrounded)
		{
			// Force landing if we're below ground
			state.isGrounded = true;
			state.vz = 0;
			state.fallTime = 0;
			LOG_INFO("Forced landing due to below-terrain position");
		}
	}

	// Apply position limits for safety
	if (state.z > PhysicsConstants::MAX_HEIGHT)
	{
		LOG_WARN("Position above max height! Clamping to " << PhysicsConstants::MAX_HEIGHT);
		state.z = PhysicsConstants::MAX_HEIGHT;
	}
	else if (state.z < -PhysicsConstants::MAX_HEIGHT)
	{
		LOG_WARN("Position below min height! Clamping to " << -PhysicsConstants::MAX_HEIGHT);
		state.z = -PhysicsConstants::MAX_HEIGHT;
	}

	// ========== STEP 8: Prepare Output ==========
	output.x = state.x;
	output.y = state.y;
	output.z = state.z;
	output.orientation = state.orientation;
	output.pitch = state.pitch;

	// Only pass velocity for knockback or falling
	output.vx = (std::abs(input.vx) > 0.01f) ? input.vx : 0;
	output.vy = (std::abs(input.vy) > 0.01f) ? input.vy : 0;
	output.vz = state.isGrounded ? 0 : state.vz;

	// Update movement flags
	output.moveFlags = input.moveFlags;
	if (state.isGrounded)
	{
		output.moveFlags &= ~MOVEFLAG_JUMP;
		output.moveFlags &= ~MOVEFLAG_FALLING;
	}
	else if (state.vz < 0)
	{
		output.moveFlags |= MOVEFLAG_FALLING;
	}

	// Update fall time
	output.fallTime = state.fallTime;

	// Add indoor flag if needed (custom flag for debugging)
	if (finalCollision.isIndoors)
	{
		LOG_INFO("Player is INDOORS");
	}

	LOG_INFO("Output Position: (" << output.x << ", " << output.y << ", " << output.z << ")");
	LOG_INFO("Output Velocity: (" << output.vx << ", " << output.vy << ", " << output.vz << ")");
	LOG_INFO("Output MoveFlags: 0x" << std::hex << output.moveFlags << std::dec);
	LOG_INFO("========== PHYSICS STEP END ==========\n");

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

	// Get terrain height from ADT data
	LOG_DEBUG("Getting ADT terrain height...");
	float adtHeight = GetADTHeight(mapId, x, y, z);
	LOG_INFO("ADT height: " << adtHeight);

	if (!checkVMap || !m_vmapHeightEnabled)
	{
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
	LOG_INFO("VMAP height: " << vmapHeight);

	// Select best height using vMaNGOS logic
	LOG_DEBUG("Selecting best height between VMAP and ADT...");
	float finalHeight = SelectBestHeight(vmapHeight, adtHeight, z, maxSearchDist);

	LOG_INFO("Final height selected: " << finalHeight);
	LOG_INFO("  ADT valid: " << (adtHeight > PhysicsConstants::INVALID_HEIGHT ? "YES" : "NO"));
	LOG_INFO("  VMAP valid: " << (vmapHeight > PhysicsConstants::INVALID_HEIGHT ? "YES" : "NO"));
	LOG_INFO("  Selection logic: " <<
		(finalHeight == vmapHeight ? "VMAP" :
			finalHeight == adtHeight ? "ADT" : "INVALID"));

	LOG_INFO("==================== PhysicsEngine::GetHeight END ====================");
	return finalHeight;
}

PhysicsEngine::CollisionInfo PhysicsEngine::QueryEnvironment(uint32_t mapId, float x, float y, float z,
	float radius, float height)
{
	LOG_INFO("==================== QueryEnvironment START ====================");
	LOG_INFO("Position: (" << x << ", " << y << ", " << z << ")");
	LOG_INFO("MapId: " << mapId << ", Radius: " << radius << ", Height: " << height);

	CollisionInfo info = {};

	// ========== STEP 1: Check Area Info (WMO/Indoor Detection) ==========
	LOG_INFO("Checking area info for indoor/WMO detection...");

	uint32_t flags = 0;
	int32_t adtId = 0, rootId = 0, groupId = 0;
	float areaCheckZ = z;  // This will be MODIFIED by getAreaInfo!
	float originalZ = z;

	if (m_vmapClient)
	{
		LOG_INFO("Calling getAreaInfo with Z=" << areaCheckZ);
		m_vmapClient->getAreaInfo(mapId, x, y, areaCheckZ, flags, adtId, rootId, groupId);

		LOG_INFO("After getAreaInfo:");
		LOG_INFO("  Z changed: " << (areaCheckZ != originalZ ? "YES" : "NO")
			<< " (from " << originalZ << " to " << areaCheckZ << ")");
		LOG_INFO("  Flags: 0x" << std::hex << flags << std::dec);
		LOG_INFO("  AdtId: " << adtId << ", RootId: " << rootId << ", GroupId: " << groupId);

		// Check indoor flag (0x8 = outdoor WMO flag, so NOT having it means indoor)
		info.isIndoors = (flags & 0x8) == 0 && rootId >= 0;
		LOG_INFO("  Indoor detection: flags & 0x8 = " << (flags & 0x8)
			<< ", rootId = " << rootId
			<< " -> isIndoors = " << info.isIndoors);

		// If Z changed, we found a WMO floor!
		if (std::abs(areaCheckZ - originalZ) > 0.001f)
		{
			LOG_INFO("WMO FLOOR DETECTED via getAreaInfo!");
			LOG_INFO("  WMO ground height: " << areaCheckZ);
			info.vmapHeight = areaCheckZ;
			info.vmapValid = true;

			// This is our primary ground if we're inside a building
			if (info.isIndoors || areaCheckZ > originalZ - 5.0f)  // Within reasonable range
			{
				LOG_INFO("  Using WMO floor as primary ground");
				info.hasGround = true;
				info.groundZ = areaCheckZ;
			}
		}
		else
		{
			LOG_INFO("No WMO floor found via getAreaInfo (Z unchanged)");
		}
	}
	else
	{
		LOG_WARN("No VMapClient available for area info check");
	}

	// ========== STEP 2: Get Traditional Height (ADT + VMAP Ray) ==========
	LOG_INFO("Getting traditional height via GetHeight...");

	float combinedHeight = GetVMapHeight(mapId, x, y, z, 50.0f);
	LOG_INFO("GetHeight returned: " << combinedHeight);

	// Parse out ADT height separately if needed
	float adtHeight = GetADTHeight(mapId, x, y, z);
	LOG_INFO("ADT terrain height: " << adtHeight);

	if (adtHeight > PhysicsConstants::INVALID_HEIGHT)
	{
		info.adtHeight = adtHeight;
		info.adtValid = true;
		LOG_INFO("ADT height is valid");
	}

	// ========== STEP 3: Determine Final Ground ==========
	LOG_INFO("Determining final ground height...");

	// If we already have WMO ground from getAreaInfo, compare with other heights
	if (info.vmapValid && info.hasGround)
	{
		LOG_INFO("Have WMO ground at " << info.groundZ);

		// Check if combined height gives us something different
		if (combinedHeight > PhysicsConstants::INVALID_HEIGHT &&
			combinedHeight > info.groundZ + 0.1f)
		{
			LOG_INFO("Combined height (" << combinedHeight
				<< ") is higher than WMO floor - might be on upper level");

			// Use the height closer to our current position
			float distToWmo = std::abs(z - info.groundZ);
			float distToCombined = std::abs(z - combinedHeight);

			if (distToCombined < distToWmo)
			{
				LOG_INFO("Using combined height (closer to current position)");
				info.groundZ = combinedHeight;
			}
			else
			{
				LOG_INFO("Keeping WMO floor (closer to current position)");
			}
		}
	}
	else if (combinedHeight > PhysicsConstants::INVALID_HEIGHT)
	{
		// No WMO floor, use traditional height
		LOG_INFO("No WMO floor - using combined height: " << combinedHeight);
		info.hasGround = true;
		info.groundZ = combinedHeight;

		// Try to determine if this is VMAP or ADT
		if (!info.vmapValid && combinedHeight != adtHeight)
		{
			LOG_INFO("Combined height differs from ADT - likely VMAP");
			info.vmapHeight = combinedHeight;
			info.vmapValid = true;
		}
	}
	else if (info.adtValid)
	{
		// Only ADT available
		LOG_INFO("Only ADT height available: " << adtHeight);
		info.hasGround = true;
		info.groundZ = adtHeight;
	}
	else
	{
		LOG_WARN("No valid ground found!");
		info.hasGround = false;
		info.groundZ = PhysicsConstants::INVALID_HEIGHT;
	}

	// ========== STEP 4: Check Liquid ==========
	LOG_INFO("Checking for liquid...");

	float liquidLevel = PhysicsConstants::INVALID_HEIGHT;
	float liquidFloor = PhysicsConstants::INVALID_HEIGHT;

	// Check MapLoader (ADT) liquids
	if (m_mapLoader && m_mapLoader->IsInitialized())
	{
		liquidLevel = m_mapLoader->GetLiquidLevel(mapId, x, y);
		if (liquidLevel > PhysicsConstants::INVALID_HEIGHT)
		{
			info.liquidType = m_mapLoader->GetLiquidType(mapId, x, y);
			info.liquidZ = liquidLevel;
			info.hasLiquid = true;
			LOG_INFO("ADT liquid found at height: " << liquidLevel
				<< ", type: 0x" << std::hex << info.liquidType << std::dec);
		}
	}

	// Check VMAP liquids (WMO water)
	if (m_vmapClient && !info.hasLiquid)
	{
		if (m_vmapClient->getLiquidLevel(mapId, x, y, z, liquidLevel, liquidFloor))
		{
			info.liquidZ = liquidLevel;
			info.liquidType = 0;  // TODO: Get actual type from VMAP
			info.hasLiquid = true;
			LOG_INFO("VMAP liquid found at height: " << liquidLevel);
		}
	}

	if (!info.hasLiquid)
	{
		LOG_INFO("No liquid found");
		info.liquidZ = PhysicsConstants::INVALID_HEIGHT;
	}

	// ========== STEP 5: Final Validation ==========
	LOG_INFO("Final collision info:");
	LOG_INFO("  hasGround: " << info.hasGround << ", groundZ: " << info.groundZ);
	LOG_INFO("  isIndoors: " << info.isIndoors);
	LOG_INFO("  hasLiquid: " << info.hasLiquid << ", liquidZ: " << info.liquidZ);
	LOG_INFO("  vmapValid: " << info.vmapValid << ", vmapHeight: " << info.vmapHeight);
	LOG_INFO("  adtValid: " << info.adtValid << ", adtHeight: " << info.adtHeight);

	// Sanity check - ensure ground is reasonable
	if (info.hasGround)
	{
		if (info.groundZ > PhysicsConstants::MAX_HEIGHT)
		{
			LOG_WARN("Ground height exceeds max! Clamping from " << info.groundZ
				<< " to " << PhysicsConstants::MAX_HEIGHT);
			info.groundZ = PhysicsConstants::MAX_HEIGHT;
		}
		else if (info.groundZ < -PhysicsConstants::MAX_HEIGHT)
		{
			LOG_WARN("Ground height below min! Clamping from " << info.groundZ
				<< " to " << -PhysicsConstants::MAX_HEIGHT);
			info.groundZ = -PhysicsConstants::MAX_HEIGHT;
		}
	}

	LOG_INFO("==================== QueryEnvironment END ====================\n");
	return info;
}
bool PhysicsEngine::GetLiquidInfo(uint32_t mapId, float x, float y, float z, float& liquidLevel, float& liquidFloor, uint32_t& liquidType)
{
	// First try to get liquid from MapLoader (ADT data)
	if (m_mapLoader && m_mapLoader->IsInitialized())
	{
		liquidLevel = m_mapLoader->GetLiquidLevel(mapId, x, y);
		if (liquidLevel > PhysicsConstants::INVALID_HEIGHT)
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
	std::cout << "HandleGroundMovement - Start Position: (" << state.x << ", " << state.y << ", " << state.z << ")" << std::endl;
	std::cout << "  Velocity: (" << state.vx << ", " << state.vy << ", " << state.vz << ")" << std::endl;

	// Calculate movement direction and speed
	float moveX = 0, moveY = 0;
	if (input.moveFlags & MOVEFLAG_FORWARD)
	{
		moveX = std::cos(state.orientation);
		moveY = std::sin(state.orientation);
	}
	else if (input.moveFlags & MOVEFLAG_BACKWARD)
	{
		moveX = -std::cos(state.orientation);
		moveY = -std::sin(state.orientation);
	}

	if (input.moveFlags & MOVEFLAG_STRAFE_LEFT)
	{
		moveX -= std::sin(state.orientation);
		moveY += std::cos(state.orientation);
	}
	else if (input.moveFlags & MOVEFLAG_STRAFE_RIGHT)
	{
		moveX += std::sin(state.orientation);
		moveY -= std::cos(state.orientation);
	}

	// Normalize diagonal movement
	float moveLength = std::sqrt(moveX * moveX + moveY * moveY);
	if (moveLength > 1.0f)
	{
		moveX /= moveLength;
		moveY /= moveLength;
	}

	// Get movement speed
	float speed = CalculateMoveSpeed(input, false, false);

	// FIX: DON'T SET VELOCITY FOR NORMAL MOVEMENT
	// DELETE THESE LINES:
	// state.vx = moveX * speed;
	// state.vy = moveY * speed;

	// FIX: Update position DIRECTLY without using velocity
	state.x += moveX * speed * dt;
	state.y += moveY * speed * dt;

	// FIX: Also apply any existing knockback velocity (but don't modify it)
	state.x += state.vx * dt;
	state.y += state.vy * dt;

	// Store old position for collision recovery
	float oldX = state.x;
	float oldY = state.y;
	float oldZ = state.z;

	// Get ground height at new position
	float newGroundZ = GetHeight(input.mapId, state.x, state.y, state.z,
		m_vmapHeightEnabled, DEFAULT_HEIGHT_SEARCH);

	std::cout << "New ground height at (" << state.x << ", " << state.y << "): " << newGroundZ << std::endl;

	// Handle stepping up/down terrain
	if (newGroundZ > PhysicsConstants::INVALID_HEIGHT)
	{
		float heightDiff = newGroundZ - oldZ;
		std::cout << "Height difference: " << heightDiff << " (new: " << newGroundZ
			<< ", old: " << oldZ << ")" << std::endl;

		if (heightDiff >= 0 && heightDiff < STEP_HEIGHT)
		{
			// Step up - within allowed step height
			std::cout << "Stepping up from " << oldZ << " to " << newGroundZ
				<< " (diff: " << heightDiff << ")" << std::endl;
			state.z = newGroundZ;
		}
		else if (heightDiff < 0 && std::abs(heightDiff) < STEP_HEIGHT)
		{
			// Step down - within allowed step height
			std::cout << "Stepping down from " << oldZ << " to " << newGroundZ
				<< " (diff: " << heightDiff << ")" << std::endl;
			state.z = newGroundZ;
		}
		// NEW CODE - ALLOW WALKING UP SLOPES
		else if (heightDiff > STEP_HEIGHT)
		{
			// This could be a steep slope, not a wall
			// Check if we can walk on this slope by checking the distance
			float horizontalDist = std::sqrt((state.x - oldX) * (state.x - oldX) +
				(state.y - oldY) * (state.y - oldY));
			float slope = heightDiff / horizontalDist;

			// If slope is walkable (less than 45 degrees = slope < 1.0)
			if (slope < 1.0f && horizontalDist > 0.01f)
			{
				std::cout << "Walking up slope (slope: " << slope << ")" << std::endl;
				state.z = newGroundZ;  // Allow walking up the slope
			}
			else
			{
				// Too steep or vertical wall
				std::cout << "Too steep to walk (slope: " << slope << ")" << std::endl;
				// Revert position
				state.x = oldX;
				state.y = oldY;
				state.z = oldZ;
			}
		}
		else
		{
			// Going down a slope steeper than step height - let gravity handle it
			std::cout << "Steep drop (" << heightDiff << ") - becoming airborne" << std::endl;
			state.isGrounded = false;
			// Keep current Z, physics will handle falling
		}
	}
	else
	{
		// No valid ground at new position (might be a hole or edge)
		std::cout << "No valid ground at new position - reverting movement" << std::endl;
		state.x = oldX;
		state.y = oldY;
		state.z = oldZ;
		// FIX: DON'T CLEAR KNOCKBACK VELOCITY
		// state.vx = 0;  // DELETE THIS
		// state.vy = 0;  // DELETE THIS
	}

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

		float moveX = forward * cos_o - strafe * sin_o;
		float moveY = forward * sin_o + strafe * cos_o;

		float speed = CalculateMoveSpeed(input, false, (input.moveFlags & MOVEFLAG_FLYING) != 0);

		state.x += moveX * speed * dt;
		state.y += moveY * speed * dt;
		state.z += (initial_vz + state.vz) * 0.5f * dt;
	}

	// Update orientation for turning
	if (input.moveFlags & MOVEFLAG_TURN_LEFT)
	{
		state.orientation -= TURN_SPEED * dt;
		if (state.orientation < 0)
			state.orientation += 2 * 3.14159f;
	}
	else if (input.moveFlags & MOVEFLAG_TURN_RIGHT)
	{
		state.orientation += TURN_SPEED * dt;
		if (state.orientation > 2 * 3.14159f)
			state.orientation -= 2 * 3.14159f;
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
	state.x += state.vx * dt / 1000;
	state.y += state.vy * dt / 1000;
	state.z += state.vz * dt / 1000;

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
	/* float factor = std::exp(-friction * dt);
	 state.vx *= factor;
	 state.vy *= factor;
	 if (!state.isGrounded)
		 state.vz *= factor;*/
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
		return input.runBackSpeed;
	return input.runSpeed;
}

float PhysicsEngine::GetLiquidHeight(uint32_t mapId, float x, float y, float z, uint32_t& liquidType)
{
	// First try to get liquid from MapLoader (ADT data)
	if (m_mapLoader && m_mapLoader->IsInitialized())
	{
		float liquidLevel = m_mapLoader->GetLiquidLevel(mapId, x, y);
		if (liquidLevel > PhysicsConstants::INVALID_HEIGHT)
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

	return PhysicsConstants::INVALID_HEIGHT;
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
			float distToCollision = std::sqrt(dx * dx + dy * dy + dz * dz);

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

	if (groundZ <= PhysicsConstants::INVALID_HEIGHT)
		return false;

	float distToGround = z - groundZ;

	// Check if within step height (2.0f) of ground
	bool grounded = (distToGround >= 0 && distToGround < STEP_HEIGHT) ||
		std::abs(distToGround) < GROUND_HEIGHT_TOLERANCE;

	return grounded;
}

bool PhysicsEngine::IsInWater(uint32_t mapId, float x, float y, float z, float height)
{
	EnsureMapLoaded(mapId);
	uint32_t liquidType;
	float liquidZ = GetLiquidHeight(mapId, x, y, z, liquidType);

	if (liquidZ <= PhysicsConstants::INVALID_HEIGHT)
		return false;

	// Match server logic: 2.0 units below surface = swimming
	// Add small tolerance for floating point precision
	const float SWIM_DEPTH_THRESHOLD = 2.0f;
	float depth = liquidZ - z;

	return depth >= SWIM_DEPTH_THRESHOLD - 0.05f; // Small tolerance
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