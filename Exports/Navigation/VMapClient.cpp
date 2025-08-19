// VMapClient.cpp - Fixed with better error handling
#include "VMapClient.h"
#include "VMapFactory.h"
#include "VMapManager2.h"
#include <iostream>
#include <filesystem>
#include "VMapLog.h"
#include "PhysicsEngine.h"

VMapClient::VMapClient(const std::string& dataPath)
    : vmapPath(dataPath), initialized(false), vmapManager(nullptr)
{
    try
    {
        // Cast the interface pointer to the concrete implementation
        vmapManager = static_cast<VMAP::VMapManager2*>(VMAP::VMapFactory::createOrGetVMapManager());

        if (!vmapManager)
        {
            return;
        }

        // If no path provided, try to auto-detect
        if (vmapPath.empty())
        {
            vmapPath = VMAP::VMapFactory::getVMapsPath();
        }
        else
        {
            // Ensure path ends with separator
            if (!vmapPath.empty() && vmapPath.back() != '/' && vmapPath.back() != '\\')
                vmapPath += "/";
        }

        // Set the base path in the manager
        vmapManager->setBasePath(vmapPath);
    }
    catch (const std::exception& e)
    {
        std::cerr << "[VMapClient] Exception in constructor: " << e.what() << std::endl;
        vmapManager = nullptr;
    }
    catch (...)
    {
        std::cerr << "[VMapClient] Unknown exception in constructor" << std::endl;
        vmapManager = nullptr;
    }
}

VMapClient::~VMapClient()
{

}

void VMapClient::initialize()
{
    if (initialized)
    {
        return;
    }

    try
    {
        // Initialize the factory
        VMAP::VMapFactory::initialize();

        // Re-get the manager in case it was created during factory init
        if (!vmapManager)
        {
            vmapManager = static_cast<VMAP::VMapManager2*>(VMAP::VMapFactory::createOrGetVMapManager());
        }

        initialized = (vmapManager != nullptr);
    }
    catch (const std::exception& e)
    {
        std::cerr << "[VMapClient] Exception during initialization: " << e.what() << std::endl;
        initialized = false;
    }
    catch (...)
    {
        std::cerr << "[VMapClient] Unknown exception during initialization" << std::endl;
        initialized = false;
    }
}

void VMapClient::preloadMap(uint32_t mapId)
{
    LOG_INFO("==================== VMapClient::preloadMap START ====================");
    LOG_INFO("Preloading map " << mapId);
    LOG_INFO("VMapClient initialized: " << (initialized ? "YES" : "NO"));
    LOG_INFO("VMapManager exists: " << (vmapManager ? "YES" : "NO"));
    LOG_INFO("VMAP path: " << vmapPath);

    if (!vmapManager)
    {
        LOG_ERROR("No VMapManager available!");
        LOG_INFO("==================== VMapClient::preloadMap END (no manager) ====================");
        return;
    }

    try
    {
        // Check if map file exists
        std::string mapFile = vmapPath;
        char filename[256];
        snprintf(filename, sizeof(filename), "%03u.vmtree", mapId);
        mapFile += filename;

        LOG_DEBUG("Checking for map file: " << mapFile);

        if (!std::filesystem::exists(mapFile))
        {
            LOG_WARN("Map file does not exist: " << mapFile);
            LOG_INFO("==================== VMapClient::preloadMap END (no file) ====================");
            return;
        }

        LOG_INFO("Map file exists - size: " << std::filesystem::file_size(mapFile) << " bytes");

        LOG_DEBUG("Calling VMapFactory::initializeMapForContinent...");
        VMAP::VMapFactory::initializeMapForContinent(mapId);
        LOG_INFO("Map initialization call completed");
    }
    catch (const std::exception& e)
    {
        LOG_ERROR("Exception preloading map " << mapId << ": " << e.what());
    }
    catch (...)
    {
        LOG_ERROR("Unknown exception preloading map " << mapId);
    }

    LOG_INFO("==================== VMapClient::preloadMap END ====================");
}

bool VMapClient::loadMapTile(uint32_t mapId, int x, int y)
{
    LOG_INFO("==================== VMapClient::loadMapTile START ====================");
    LOG_INFO("Loading tile - Map:" << mapId << " Tile:[" << x << "," << y << "]");

    if (!vmapManager)
    {
        LOG_ERROR("No VMapManager available!");
        LOG_INFO("==================== VMapClient::loadMapTile END (no manager) ====================");
        return false;
    }

    // Ensure map is initialized
    if (!vmapManager->isMapInitialized(mapId))
    {
        LOG_INFO("Map " << mapId << " not initialized - calling preloadMap");
        preloadMap(mapId);

        if (!vmapManager->isMapInitialized(mapId))
        {
            LOG_WARN("Map still not initialized after preload attempt");
        }
    }
    else
    {
        LOG_DEBUG("Map " << mapId << " already initialized");
    }

    LOG_DEBUG("Calling VMapManager::loadMap...");
    VMAP::VMAPLoadResult result = vmapManager->loadMap(vmapPath.c_str(), mapId, x, y);

    switch (result)
    {
    case VMAP::VMAP_LOAD_RESULT_OK:
        LOG_INFO("Tile loaded successfully");
        break;
    case VMAP::VMAP_LOAD_RESULT_ERROR:
        LOG_ERROR("Error loading tile");
        break;
    case VMAP::VMAP_LOAD_RESULT_IGNORED:
        LOG_WARN("Tile load ignored");
        break;
    default:
        LOG_WARN("Unknown load result: " << (int)result);
    }

    LOG_INFO("==================== VMapClient::loadMapTile END (result=" << (int)result << ") ====================");
    return result == VMAP::VMAP_LOAD_RESULT_OK;
}

void VMapClient::unloadMapTile(uint32_t mapId, int x, int y)
{
    if (vmapManager)
        vmapManager->unloadMap(mapId, x, y);
}

void VMapClient::unloadMap(uint32_t mapId)
{
    if (vmapManager)
        vmapManager->unloadMap(mapId);
}

float VMapClient::getGroundHeight(uint32_t mapId, float x, float y, float z, float searchDistance)
{
    LOG_TRACE("VMapClient::getGroundHeight - Map:" << mapId
        << " Pos:(" << x << "," << y << "," << z << ")"
        << " SearchDist:" << searchDistance);

    if (!vmapManager)
    {
        LOG_ERROR("No VMapManager in getGroundHeight!");
        return PhysicsConstants::INVALID_HEIGHT;
    }

    // IMPORTANT: Try to load the tile at this position!
    // Calculate which tile contains this position
    const float GRID_SIZE = 533.33333f;
    const float MID = 32.0f * GRID_SIZE;

    int tileX = (int)((MID - y) / GRID_SIZE);
    int tileY = (int)((MID - x) / GRID_SIZE);

    LOG_INFO("Position (" << x << "," << y << ") maps to tile [" << tileX << "," << tileY << "]");
    LOG_INFO("Attempting to load tile before height query...");

    // Try to load the tile
    bool tileLoaded = loadMapTile(mapId, tileX, tileY);
    LOG_INFO("Tile load attempt result: " << (tileLoaded ? "SUCCESS" : "FAILED"));

    // Now query the height
    LOG_DEBUG("Querying VMAP height...");
    float height = vmapManager->getHeight(mapId, x, y, z, searchDistance);

    if (height > PhysicsConstants::INVALID_HEIGHT)
    {
        LOG_INFO("VMAP height found: " << height);
    }
    else
    {
        LOG_DEBUG("No VMAP height found");
    }

    return height;
}

bool VMapClient::isLineOfSight(uint32_t mapId,
    float x1, float y1, float z1,
    float x2, float y2, float z2,
    bool ignoreM2Models)
{
    if (!vmapManager)
        return true;  // No collision data = clear line of sight

    return vmapManager->isInLineOfSight(mapId, x1, y1, z1, x2, y2, z2, ignoreM2Models);
}

bool VMapClient::getCollisionPoint(uint32_t mapId,
    float x1, float y1, float z1,
    float x2, float y2, float z2,
    float& hitX, float& hitY, float& hitZ)
{
    if (!vmapManager)
        return false;

    return vmapManager->getObjectHitPos(mapId, x1, y1, z1, x2, y2, z2, hitX, hitY, hitZ, 0.0f);
}

void VMapClient::getAreaInfo(uint32_t mapId, float x, float y, float& z,
    uint32_t& flags, int32_t& adtId, int32_t& rootId, int32_t& groupId)
{
    if (vmapManager)
        vmapManager->getAreaInfo(mapId, x, y, z, flags, adtId, rootId, groupId);
}

bool VMapClient::getLiquidLevel(uint32_t mapId, float x, float y, float z,
    float& liquidLevel, float& liquidFloor)
{
    if (!vmapManager)
        return false;

    uint32_t type;
    return vmapManager->GetLiquidLevel(mapId, x, y, z, 0xFF, liquidLevel, liquidFloor, type);
}