// PhysicsEngine.cpp - Simplified version without splines, flying, or configuration
#include "PhysicsEngine.h"
#include "VMapDefinitions.h"
#include "Navigation.h"
#include "VMapManager2.h"
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
	m_currentMapId(UINT32_MAX),
	m_vmapManager(nullptr)
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

		// Initialize VMAP system directly using VMapManager2
		try
		{
			// Get or create the VMapManager2 instance through the factory
			m_vmapManager = static_cast<VMAP::VMapManager2*>(
				VMAP::VMapFactory::createOrGetVMapManager());

			if (m_vmapManager)
			{
				// Initialize the factory and set up paths
				VMAP::VMapFactory::initialize();

				// Find and set the vmaps path
				std::vector<std::string> vmapPaths = { "vmaps/", "Data/vmaps/", "../Data/vmaps/" };
				for (const auto& path : vmapPaths)
				{
					if (std::filesystem::exists(path))
					{
						m_vmapManager->setBasePath(path);
						break;
					}
				}
			}
		}
		catch (const std::exception& e)
		{
			m_vmapManager = nullptr;
		}

		// Get navigation instance
		m_navigation = Navigation::GetInstance();

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
	// Don't delete m_vmapManager as it's managed by the factory
	m_vmapManager = nullptr;
	m_mapLoader.reset();
	m_currentMapId = UINT32_MAX;
	m_initialized = false;
}

// Ensure map is loaded only once per map change
void PhysicsEngine::EnsureMapLoaded(uint32_t mapId)
{
	if (m_currentMapId != mapId)
	{
		if (m_vmapManager)
		{
			// Initialize the map if not already done
			if (!m_vmapManager->isMapInitialized(mapId))
			{
				m_vmapManager->initializeMap(mapId);
			}
		}

		m_currentMapId = mapId;
	}
}

// Main vMaNGOS-style GetHeight implementation
float PhysicsEngine::GetVMapHeight(uint32_t mapId, float x, float y, float z, float maxSearchDist)
{
	if (!m_vmapManager)
	{
		return PhysicsConstants::INVALID_HEIGHT;
	}

	// Calculate tile coordinates
	const float GRID_SIZE = 533.33333f;
	const float MID = 32.0f * GRID_SIZE;
	int tileX = (int)((MID - y) / GRID_SIZE);
	int tileY = (int)((MID - x) / GRID_SIZE);

	// Load the tile
	m_vmapManager->loadMap(nullptr, mapId, tileX, tileY);

	return m_vmapManager->getHeight(mapId, x, y, z, maxSearchDist);
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

float PhysicsEngine::GetHeight(uint32_t mapId, float x, float y, float z, bool checkVMap, float maxSearchDist)
{
	// Add comprehensive logging
	LOG_INFO("==================== PhysicsEngine::GetHeight START ====================");
	LOG_INFO("GetHeight called - Map:" << mapId
		<< " Pos:(" << x << "," << y << "," << z << ")"
		<< " checkVMap:" << checkVMap
		<< " maxSearchDist:" << maxSearchDist);

	// Initialize heights to invalid
	float mapHeight = PhysicsConstants::INVALID_HEIGHT;   // Height from terrain
	float vmapHeight = PhysicsConstants::INVALID_HEIGHT;  // Height from VMAP models

	// Look from slightly above the given position (server behavior)
	float z2 = z + 2.0f;

	// Get terrain height from ADT data
	LOG_DEBUG("Getting ADT terrain height...");
	mapHeight = GetADTHeight(mapId, x, y, z);
	LOG_INFO("ADT/map height: " << mapHeight);

	if (!checkVMap || !m_vmapManager)
	{
		LOG_INFO("Returning map height only - checkVMap:" << checkVMap
			<< " vmapManager:" << (m_vmapManager ? "available" : "null"));
		LOG_INFO("==================== PhysicsEngine::GetHeight END ====================");
		return mapHeight;
	}

	// VMAP height search using server strategy
	LOG_DEBUG("Getting VMAP height using server search strategy...");

	// Ensure the map and tile are loaded before querying
	LOG_INFO("Ensuring map is loaded before VMAP query...");
	EnsureMapLoaded(mapId);

	// Calculate which tile we need and try to load it
	if (m_vmapManager)
	{
		const float GRID_SIZE = 533.33333f;
		const float MID = 32.0f * GRID_SIZE;
		int tileX = (int)((MID - y) / GRID_SIZE);
		int tileY = (int)((MID - x) / GRID_SIZE);

		LOG_INFO("Position (" << x << "," << y << ") requires tile [" << tileX << "," << tileY << "]");
		LOG_INFO("Attempting to load tile...");

		VMAP::VMAPLoadResult result = m_vmapManager->loadMap(nullptr, mapId, tileX, tileY);
		LOG_INFO("Tile load attempt: " << (result == VMAP::VMAP_LOAD_RESULT_OK ? "SUCCESS" : "FAILED"));

		// Dynamic search distance adjustment (server behavior)
		float searchDist = maxSearchDist;
		if (mapHeight > PhysicsConstants::INVALID_HEIGHT && z2 - mapHeight > maxSearchDist)
		{
			searchDist = z2 - mapHeight + 1.0f;
			LOG_INFO("Adjusted search distance from " << maxSearchDist << " to " << searchDist
				<< " (z2=" << z2 << ", mapHeight=" << mapHeight << ")");
		}

		// Strategy 1: Normal search from z2 with adjusted search distance
		LOG_INFO("VMAP Search 1: Normal search from z2=" << z2 << " with searchDist=" << searchDist);
		vmapHeight = m_vmapManager->getHeight(mapId, x, y, z2, searchDist);
		LOG_INFO("  Result: " << vmapHeight);

		// Strategy 2: If not found, search with very large range (far above floor, but below terrain)
		if (vmapHeight <= PhysicsConstants::INVALID_HEIGHT)
		{
			LOG_INFO("VMAP Search 2: Extended range search (10000.0f)");
			vmapHeight = m_vmapManager->getHeight(mapId, x, y, z2, 10000.0f);
			LOG_INFO("  Result: " << vmapHeight);
		}

		// Strategy 3: Look upwards if still not found and conditions are met
		if (vmapHeight <= PhysicsConstants::INVALID_HEIGHT && mapHeight > z2 && std::abs(z2 - mapHeight) > 30.0f)
		{
			LOG_INFO("VMAP Search 3: Upward search (negative maxSearchDist)");
			vmapHeight = m_vmapManager->getHeight(mapId, x, y, z2, -maxSearchDist);
			LOG_INFO("  Result: " << vmapHeight);
		}

		// Strategy 4: Search near terrain height if still not found
		if (vmapHeight <= PhysicsConstants::INVALID_HEIGHT && mapHeight > PhysicsConstants::INVALID_HEIGHT && z2 < mapHeight)
		{
			LOG_INFO("VMAP Search 4: Near terrain height search");
			vmapHeight = m_vmapManager->getHeight(mapId, x, y, mapHeight + 2.0f, PhysicsConstants::DEFAULT_HEIGHT_SEARCH);
			LOG_INFO("  Result: " << vmapHeight);
		}
	}

	// Server-style height selection logic
	LOG_DEBUG("Selecting best height using server logic...");
	float finalHeight;

	if (vmapHeight > PhysicsConstants::INVALID_HEIGHT)
	{
		if (mapHeight > PhysicsConstants::INVALID_HEIGHT)
		{
			// We have both mapHeight and vmapHeight - select more appropriate
			// If we are already under the surface OR vmap height is above map height
			if (z < mapHeight || vmapHeight > mapHeight)
			{
				finalHeight = vmapHeight;
				LOG_INFO("Selected VMAP height (z < mapHeight or vmapHeight > mapHeight)");
			}
			else
			{
				finalHeight = mapHeight;
				LOG_INFO("Selected map height (better to use surface)");
			}
		}
		else
		{
			// Only vmapHeight is valid
			finalHeight = vmapHeight;
			LOG_INFO("Selected VMAP height (only valid height)");
		}
	}
	else
	{
		// Only mapHeight available (or both invalid)
		finalHeight = mapHeight;
		LOG_INFO("Selected map height (VMAP not available)");
	}

	LOG_INFO("Final height selected: " << finalHeight);
	LOG_INFO("  Map height valid: " << (mapHeight > PhysicsConstants::INVALID_HEIGHT ? "YES" : "NO"));
	LOG_INFO("  VMAP height valid: " << (vmapHeight > PhysicsConstants::INVALID_HEIGHT ? "YES" : "NO"));
	LOG_INFO("==================== PhysicsEngine::GetHeight END ====================");

	return finalHeight;
}

PhysicsEngine::CollisionInfo PhysicsEngine::QueryEnvironment(uint32_t mapId, float x, float y, float z,
	float radius, float height)
{
	CollisionInfo info = {};

	LOG_DEBUG("QueryEnvironment: mapId=" << mapId << " pos=(" << x << "," << y << "," << z << ")");

	// Ensure map is loaded
	EnsureMapLoaded(mapId);

	// Single height query combining VMAP and ADT
	float groundHeight = GetHeight(mapId, x, y, z, true, 50.0f);
	if (groundHeight > PhysicsConstants::INVALID_HEIGHT)
	{
		info.hasGround = true;
		info.groundZ = groundHeight;
		LOG_DEBUG("  Ground found at Z=" << groundHeight);
	}
	else
	{
		LOG_DEBUG("  No ground found");
	}

	// Check for liquid using server-style logic
	uint32_t liquidType = 0;
	float liquidLevel = GetLiquidHeight(mapId, x, y, z, liquidType);
	if (liquidLevel > PhysicsConstants::INVALID_HEIGHT)
	{
		info.hasLiquid = true;
		info.liquidZ = liquidLevel;
		info.liquidType = liquidType;
		LOG_DEBUG("  Liquid found at Z=" << liquidLevel << " type=" << liquidType);
	}
	else
	{
		LOG_DEBUG("  No liquid found");
	}

	// Quick indoor check using VMAP area info
	if (m_vmapManager)
	{
		uint32_t flags = 0;
		int32_t adtId = 0, rootId = 0, groupId = 0;
		float areaZ = z;

		m_vmapManager->getAreaInfo(mapId, x, y, areaZ, flags, adtId, rootId, groupId);

		// Indoor if we're in a WMO and not marked as outdoor
		info.isIndoors = (flags & 0x8) == 0 && rootId >= 0;
		LOG_DEBUG("  Area info: flags=0x" << std::hex << flags << std::dec
			<< " rootId=" << rootId << " indoors=" << (info.isIndoors ? "YES" : "NO"));
	}

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
			liquidFloor = liquidLevel - 2.0f;  // Estimate floor
			return true;
		}
	}

	// Then try VMAP for WMO liquids
	if (m_vmapManager)
	{
		try
		{
			if (m_vmapManager->GetLiquidLevel(mapId, x, y, z, 0xFF, liquidLevel, liquidFloor, liquidType))
			{
				// liquidType is already set by GetLiquidLevel
				return true;
			}
		}
		catch (const std::exception& e)
		{
		}
	}

	return false;
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
	if (m_vmapManager)
	{
		try
		{
			float liquidLevel, liquidFloor;
			uint32_t vmapLiquidType;
			if (m_vmapManager->GetLiquidLevel(mapId, x, y, z, 0xFF, liquidLevel, liquidFloor, vmapLiquidType))
			{
				liquidType = vmapLiquidType;
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
}

bool PhysicsEngine::CheckCollision(uint32_t mapId, float startX, float startY, float startZ,
	float endX, float endY, float endZ, float radius, float height,
	float& hitX, float& hitY, float& hitZ)
{
	// Temporarily disable collision checking until we can fix false positives
	return false;
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

bool PhysicsEngine::CanWalkOn(uint32_t mapId, float x, float y, float z)
{
	// Check if the surface is walkable based on slope
	EnsureMapLoaded(mapId);

	// Get height at the position
	float groundZ = GetHeight(mapId, x, y, z, true, DEFAULT_HEIGHT_SEARCH);
	if (groundZ <= PhysicsConstants::INVALID_HEIGHT)
		return false;

	// For now, assume all valid ground is walkable
	// In the future, could add slope checking here
	return true;
}

PhysicsOutput PhysicsEngine::Step(const PhysicsInput& input, float dt)
{
	PhysicsOutput output = {};

	// Quick passthrough if not initialized
	if (!m_initialized)
	{
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

	// Initialize state from input
	MovementState state{};
	state.x = input.x;
	state.y = input.y;
	state.z = input.z;
	state.orientation = input.orientation;
	state.pitch = input.pitch;
	state.vx = input.vx;
	state.vy = input.vy;
	state.vz = input.vz;
	state.fallTime = input.fallTime;

	// Log input state
	LOG_INFO("==================== PhysicsEngine::Step START ====================");
	LOG_INFO("INPUT STATE:");
	LOG_INFO("  Position: (" << input.x << ", " << input.y << ", " << input.z << ")");
	LOG_INFO("  Orientation: " << input.orientation << ", Pitch: " << input.pitch);
	LOG_INFO("  Velocity: (" << input.vx << ", " << input.vy << ", " << input.vz << ")");
	LOG_INFO("  MoveFlags: 0x" << std::hex << input.moveFlags << std::dec);
	LOG_INFO("  FallTime: " << input.fallTime);
	LOG_INFO("  Speeds: run=" << input.runSpeed << ", walk=" << input.walkSpeed << ", swim=" << input.swimSpeed);

	// Single environment query at current position
	CollisionInfo collision = QueryEnvironment(input.mapId, state.x, state.y, state.z, 0.5f, 2.0f);

	LOG_INFO("ENVIRONMENT SCAN:");
	LOG_INFO("  Ground: " << (collision.hasGround ? "YES" : "NO") << " at Z=" << collision.groundZ);
	LOG_INFO("  Liquid: " << (collision.hasLiquid ? "YES" : "NO") << " at Z=" << collision.liquidZ << " type=" << collision.liquidType);
	LOG_INFO("  Indoors: " << (collision.isIndoors ? "YES" : "NO"));

	// Determine movement state - fixed swimming logic
	float distToGround = collision.hasGround ? (state.z - collision.groundZ) : PhysicsConstants::MAX_HEIGHT;

	// Calculate swimming state based on position relative to water (server logic)
	bool inWater = false;
	bool wasSwimming = (input.moveFlags & MOVEFLAG_SWIMMING) != 0;
	float swimmingThreshold = PhysicsConstants::INVALID_HEIGHT;

	if (collision.hasLiquid)
	{
		// Use character-specific swimming depth (like GetMinSwimDepth())
		// TODO: Get actual character collision height from character data
		float characterHeight = 2.0f; // Standard character height - should be configurable
		float minSwimDepth = characterHeight * 0.75f; // 1.5f for standard character

		swimmingThreshold = collision.liquidZ - minSwimDepth;
		inWater = state.z < swimmingThreshold;
	}

	// Determine if character is grounded
	state.isGrounded = collision.hasGround &&
		(distToGround >= -PhysicsConstants::GROUND_HEIGHT_TOLERANCE) &&
		(distToGround <= PhysicsConstants::STEP_HEIGHT);

	// Determine if character is swimming
	// Character swims if they're in water and not touching ground
	state.isSwimming = inWater && !state.isGrounded;

	// Handle swimming transition - reset velocity when entering water
	if (state.isSwimming && !wasSwimming)
	{
		LOG_INFO("ENTERING SWIMMING MODE - resetting vertical velocity");
		state.vz = 0; // Stop falling when entering water
		state.fallTime = 0; // Reset fall time
	}

	LOG_INFO("MOVEMENT STATE DETERMINATION:");
	LOG_INFO("  DistToGround: " << distToGround);
	LOG_INFO("  LiquidLevel: " << collision.liquidZ);
	LOG_INFO("  SwimmingThreshold: " << swimmingThreshold);
	LOG_INFO("  MinSwimDepth: " << (collision.hasLiquid ? (collision.liquidZ - swimmingThreshold) : 0));
	LOG_INFO("  InWater: " << (inWater ? "YES" : "NO"));
	LOG_INFO("  IsGrounded: " << (state.isGrounded ? "YES" : "NO"));
	LOG_INFO("  IsSwimming: " << (state.isSwimming ? "YES" : "NO"));
	LOG_INFO("  WasSwimming: " << (wasSwimming ? "YES" : "NO"));

	// Calculate movement direction
	float moveX = 0, moveY = 0, moveZ = 0;

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

	LOG_INFO("MOVEMENT DIRECTION:");
	LOG_INFO("  Raw movement: (" << moveX << ", " << moveY << ", " << moveZ << ")");
	LOG_INFO("  Move length: " << moveLength);

	// Store old position for comparison
	float oldX = state.x, oldY = state.y, oldZ = state.z;

	// Apply movement based on state
	if (state.isSwimming)
	{
		LOG_INFO("APPLYING SWIMMING MOVEMENT:");

		// Swimming movement with pitch
		if (moveLength > 0.01f) // Only apply movement if there's input
		{
			moveZ = std::sin(state.pitch);
			float horizontalScale = std::cos(state.pitch);
			moveX *= horizontalScale;
			moveY *= horizontalScale;

			float speed = (input.moveFlags & MOVEFLAG_BACKWARD) ? input.swimBackSpeed : input.swimSpeed;

			LOG_INFO("  Pitch: " << state.pitch << " (moveZ=" << moveZ << ", hScale=" << horizontalScale << ")");
			LOG_INFO("  Speed: " << speed);

			state.x += moveX * speed * dt;
			state.y += moveY * speed * dt;
			state.z += moveZ * speed * dt;
		}
		else
		{
			LOG_INFO("  No movement input - floating in place");
			// When not moving, maintain current position (floating)
			state.vz = 0; // Ensure no vertical drift
		}

		// Clamp to water bounds
		if (collision.hasGround && state.z < collision.groundZ + 0.5f)
		{
			LOG_INFO("  Clamped to ground: " << state.z << " -> " << (collision.groundZ + 0.5f));
			state.z = collision.groundZ + 0.5f;
		}
		else if (state.z > collision.liquidZ - 0.1f)
		{
			LOG_INFO("  Clamped to water surface: " << state.z << " -> " << (collision.liquidZ - 0.1f));
			state.z = collision.liquidZ - 0.1f;
		}
	}
	else if (state.isGrounded)
	{
		LOG_INFO("APPLYING GROUND MOVEMENT:");

		// Ground movement
		float speed = (input.moveFlags & MOVEFLAG_WALK_MODE) ? input.walkSpeed :
			(input.moveFlags & MOVEFLAG_BACKWARD) ? input.runBackSpeed : input.runSpeed;

		LOG_INFO("  Speed: " << speed);

		// Handle jumping
		if (input.moveFlags & MOVEFLAG_JUMPING)
		{
			LOG_INFO("  JUMPING! vz=" << PhysicsConstants::JUMP_VELOCITY);
			state.vz = PhysicsConstants::JUMP_VELOCITY;
			state.isGrounded = false;
			state.fallStartZ = state.z;
			state.fallTime = 0;
		}

		// Move horizontally and check new ground height
		if (moveLength > 0.01f)
		{
			float newX = state.x + moveX * speed * dt;
			float newY = state.y + moveY * speed * dt;
			float newGroundZ = GetHeight(input.mapId, newX, newY, state.z, true, 50.0f);

			LOG_INFO("  Trying to move to: (" << newX << ", " << newY << ") newGroundZ=" << newGroundZ);

			if (newGroundZ > PhysicsConstants::INVALID_HEIGHT)
			{
				float heightDiff = newGroundZ - state.z;
				LOG_INFO("  Height difference: " << heightDiff);

				// Check if we can step to this height
				if (std::abs(heightDiff) <= PhysicsConstants::STEP_HEIGHT)
				{
					LOG_INFO("  ALLOWED: Moving and stepping to new height");
					state.x = newX;
					state.y = newY;
					state.z = newGroundZ + PhysicsConstants::GROUND_HEIGHT_TOLERANCE;
				}
				else if (heightDiff < -PhysicsConstants::STEP_HEIGHT)
				{
					LOG_INFO("  FALLING: Stepped off edge");
					// Falling off edge
					state.x = newX;
					state.y = newY;
					state.isGrounded = false;
					state.fallStartZ = state.z;
					state.fallTime = 0;
				}
				else
				{
					LOG_INFO("  BLOCKED: Height difference too large");
				}
			}
			else
			{
				LOG_INFO("  BLOCKED: No valid ground at new position");
			}
		}
		else
		{
			// Standing still - snap to ground
			LOG_INFO("  Standing still - snapping to ground: " << state.z << " -> " << (collision.groundZ + PhysicsConstants::GROUND_HEIGHT_TOLERANCE));
			state.z = collision.groundZ + PhysicsConstants::GROUND_HEIGHT_TOLERANCE;
		}
	}
	else
	{
		LOG_INFO("APPLYING AIRBORNE MOVEMENT:");

		// Airborne movement
		state.fallTime += dt;
		float oldVz = state.vz;
		state.vz -= PhysicsConstants::GRAVITY * dt;

		if (state.vz < -54.0f) // Terminal velocity
			state.vz = -54.0f;

		LOG_INFO("  Gravity applied: vz " << oldVz << " -> " << state.vz);
		LOG_INFO("  Fall time: " << state.fallTime);

		// Limited air control
		float speed = (input.moveFlags & MOVEFLAG_WALK_MODE) ? input.walkSpeed : input.runSpeed;

		state.x += moveX * speed * dt;
		state.y += moveY * speed * dt;
		state.z += state.vz * dt;

		LOG_INFO("  Air movement: speed=" << speed);

		// Check for landing
		if (state.vz <= 0)
		{
			float currentGroundZ = GetHeight(input.mapId, state.x, state.y, state.z, true, 50.0f);
			LOG_INFO("  Landing check: currentGroundZ=" << currentGroundZ << " playerZ=" << state.z);

			if (currentGroundZ > PhysicsConstants::INVALID_HEIGHT &&
				state.z - currentGroundZ <= PhysicsConstants::GROUND_HEIGHT_TOLERANCE)
			{
				LOG_INFO("  LANDED! Touching ground");
				// Landed
				state.z = currentGroundZ + PhysicsConstants::GROUND_HEIGHT_TOLERANCE;
				state.vz = 0;
				state.isGrounded = true;
				state.fallTime = 0;
			}
		}
	}

	// Apply external forces (knockback)
	if (std::abs(input.vx) > 0.01f || std::abs(input.vy) > 0.01f)
	{
		LOG_INFO("APPLYING EXTERNAL FORCES:");
		LOG_INFO("  Input velocity: (" << input.vx << ", " << input.vy << ", " << input.vz << ")");

		state.x += input.vx * dt;
		state.y += input.vy * dt;

		if (!state.isGrounded && std::abs(input.vz) > 0.01f)
		{
			state.vz += input.vz;
			LOG_INFO("  Applied vz knockback: " << input.vz);
		}
	}

	// Clamp to reasonable bounds
	oldZ = state.z;
	state.z = std::max(-PhysicsConstants::MAX_HEIGHT,
		std::min(PhysicsConstants::MAX_HEIGHT, state.z));

	if (oldZ != state.z)
	{
		LOG_INFO("CLAMPED Z: " << oldZ << " -> " << state.z);
	}

	LOG_INFO("POSITION CHANGE:");
	LOG_INFO("  Old: (" << oldX << ", " << oldY << ", " << oldZ << ")");
	LOG_INFO("  New: (" << state.x << ", " << state.y << ", " << state.z << ")");
	LOG_INFO("  Delta: (" << (state.x - oldX) << ", " << (state.y - oldY) << ", " << (state.z - oldZ) << ")");

	// Prepare output
	output.x = state.x;
	output.y = state.y;
	output.z = state.z;
	output.orientation = state.orientation;
	output.pitch = state.pitch;
	output.vx = (std::abs(input.vx) > 0.01f) ? input.vx : 0;
	output.vy = (std::abs(input.vy) > 0.01f) ? input.vy : 0;
	output.vz = (state.isGrounded || state.isSwimming) ? 0 : state.vz; // Zero vz when grounded OR swimming
	output.fallTime = state.isSwimming ? 0 : state.fallTime; // Reset fall time when swimming

	// Update movement flags based on calculated state
	output.moveFlags = input.moveFlags;

	// Set swimming flag based on calculated swimming state
	if (state.isSwimming)
	{
		output.moveFlags |= MOVEFLAG_SWIMMING;
	}
	else
	{
		output.moveFlags &= ~MOVEFLAG_SWIMMING;
	}

	// Handle other state-based flags
	if (state.isGrounded)
	{
		output.moveFlags &= ~MOVEFLAG_JUMPING;
		output.moveFlags &= ~MOVEFLAG_FALLINGFAR;
	}
	else if (state.isSwimming)
	{
		// Swimming - clear falling and jump flags
		output.moveFlags &= ~MOVEFLAG_JUMPING;
		output.moveFlags &= ~MOVEFLAG_FALLINGFAR;
	}
	else if (state.vz < 0)
	{
		output.moveFlags |= MOVEFLAG_FALLINGFAR;
	}

	LOG_INFO("OUTPUT STATE:");
	LOG_INFO("  Position: (" << output.x << ", " << output.y << ", " << output.z << ")");
	LOG_INFO("  Orientation: " << output.orientation << ", Pitch: " << output.pitch);
	LOG_INFO("  Velocity: (" << output.vx << ", " << output.vy << ", " << output.vz << ")");
	LOG_INFO("  MoveFlags: 0x" << std::hex << output.moveFlags << std::dec);
	LOG_INFO("  FallTime: " << output.fallTime);
	LOG_INFO("==================== PhysicsEngine::Step END ====================");

	return output;
}

// Movement handlers
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
	float speed = CalculateMoveSpeed(input, false);

	// Update position DIRECTLY without using velocity
	state.x += moveX * speed * dt;
	state.y += moveY * speed * dt;

	// Also apply any existing knockback velocity (but don't modify it)
	state.x += state.vx * dt;
	state.y += state.vy * dt;

	// Store old position for collision recovery
	float oldX = state.x;
	float oldY = state.y;
	float oldZ = state.z;

	// Get ground height at new position
	float newGroundZ = GetHeight(input.mapId, state.x, state.y, state.z,
		true, DEFAULT_HEIGHT_SEARCH);

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
		else if (heightDiff > STEP_HEIGHT)
		{
			// This could be a steep slope, not a wall
			// Check if we can walk on this slope by checking the distance
			float horizontalDist = std::sqrt((state.x - oldX) * (state.x - oldX) +
				(state.y - oldY) * (state.y - oldY));
			float slope = heightDiff / (horizontalDist > 0.01f ? horizontalDist : 1.0f);

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
	}

	return state;
}

PhysicsEngine::MovementState PhysicsEngine::HandleAirMovement(const PhysicsInput& input,
	MovementState& state, float dt)
{
	// Store initial velocity for proper integration
	float initial_vz = state.vz;

	// Apply gravity
	ApplyGravity(state, dt);
	state.fallTime += dt;

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
		// WoW coordinate system
		float cos_o = std::cos(state.orientation);
		float sin_o = std::sin(state.orientation);

		float moveX = moveForward * cos_o - moveStrafe * sin_o;
		float moveY = moveForward * sin_o + moveStrafe * cos_o;

		float speed = CalculateMoveSpeed(input, false);

		state.x += moveX * speed * dt;
		state.y += moveY * speed * dt;
		state.z += (initial_vz + state.vz) * 0.5f * dt;
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

void PhysicsEngine::ApplyGravity(MovementState& state, float dt)
{
	state.vz -= GRAVITY * dt;

	if (state.vz < -54.0f)  // Terminal velocity (vmangos value)
		state.vz = -54.0f;
}

void PhysicsEngine::ApplyKnockback(MovementState& state, float vx, float vy, float vz)
{
	state.vx += vx;
	state.vy += vy;
	state.vz += vz;
}

float PhysicsEngine::CalculateMoveSpeed(const PhysicsInput& input, bool isSwimming)
{
	if (isSwimming)
		return input.swimSpeed;
	if (input.moveFlags & MOVEFLAG_WALK_MODE)
		return input.walkSpeed;
	if (input.moveFlags & MOVEFLAG_BACKWARD)  // Moving backward
		return input.runBackSpeed;
	return input.runSpeed;
}