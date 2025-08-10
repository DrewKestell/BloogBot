// VMapClient.cpp - Fixed with better error handling
#include "VMapClient.h"
#include "VMapFactory.h"
#include "VMapManager2.h"
#include <iostream>
#include <filesystem>

VMapClient::VMapClient(const std::string& dataPath)
    : vmapPath(dataPath), initialized(false), vmapManager(nullptr)
{
    std::cout << "[VMapClient] Constructor called with path: " << dataPath << std::endl;

    // Get or create the VMapManager instance
    try
    {
        // Cast the interface pointer to the concrete implementation
        vmapManager = static_cast<VMAP::VMapManager2*>(VMAP::VMapFactory::createOrGetVMapManager());

        if (!vmapManager)
        {
            std::cerr << "[VMapClient] Failed to get VMapManager instance!" << std::endl;
            return;
        }

        // If no path provided, try to auto-detect
        if (vmapPath.empty())
        {
            vmapPath = VMAP::VMapFactory::getVMapsPath();
            std::cout << "[VMapClient] Auto-detected VMAP path: " << vmapPath << std::endl;
        }
        else
        {
            // Ensure path ends with separator
            if (!vmapPath.empty() && vmapPath.back() != '/' && vmapPath.back() != '\\')
                vmapPath += "/";
        }

        // Verify the path exists
        if (!std::filesystem::exists(vmapPath))
        {
            std::cerr << "[VMapClient] Warning: VMAP path does not exist: " << vmapPath << std::endl;
            std::cerr << "[VMapClient] Creating directory..." << std::endl;

            try
            {
                std::filesystem::create_directories(vmapPath);
            }
            catch (const std::exception& e)
            {
                std::cerr << "[VMapClient] Failed to create directory: " << e.what() << std::endl;
            }
        }

        // Set the base path in the manager
        vmapManager->setBasePath(vmapPath);
        std::cout << "[VMapClient] VMapManager base path set to: " << vmapPath << std::endl;
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
    std::cout << "[VMapClient] Destructor called" << std::endl;
    // Note: Don't call VMapFactory::clear() here as it's a singleton
    // and might be used by other instances
}

void VMapClient::initialize()
{
    std::cout << "[VMapClient] initialize() called" << std::endl;

    if (initialized)
    {
        std::cout << "[VMapClient] Already initialized" << std::endl;
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

        if (initialized)
        {
            std::cout << "[VMapClient] Initialization successful" << std::endl;
        }
        else
        {
            std::cerr << "[VMapClient] Initialization failed - no VMapManager" << std::endl;
        }
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
    std::cout << "[VMapClient] preloadMap(" << mapId << ") called" << std::endl;

    if (!vmapManager)
    {
        std::cerr << "[VMapClient] Cannot preload map - VMapManager is NULL" << std::endl;
        return;
    }

    try
    {
        // Don't force initialization if the map file doesn't exist
        std::string mapFile = vmapPath;
        char filename[256];
        snprintf(filename, sizeof(filename), "%03u.vmtree", mapId);
        mapFile += filename;

        if (!std::filesystem::exists(mapFile))
        {
            std::cout << "[VMapClient] Map file not found: " << mapFile << " (skipping)" << std::endl;
            return;
        }

        VMAP::VMapFactory::initializeMapForContinent(mapId);
        std::cout << "[VMapClient] Map " << mapId << " preload complete" << std::endl;
    }
    catch (const std::exception& e)
    {
        std::cerr << "[VMapClient] Exception preloading map " << mapId << ": " << e.what() << std::endl;
    }
    catch (...)
    {
        std::cerr << "[VMapClient] Unknown exception preloading map " << mapId << std::endl;
    }
}

bool VMapClient::loadMapTile(uint32_t mapId, int x, int y)
{
    if (!vmapManager)
    {
        std::cerr << "[VMapClient] Cannot load tile - VMapManager is NULL" << std::endl;
        return false;
    }

    // Ensure map is initialized
    if (!vmapManager->isMapInitialized(mapId))
    {
        preloadMap(mapId);
    }

    VMAP::VMAPLoadResult result = vmapManager->loadMap(vmapPath.c_str(), mapId, x, y);
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
    if (!vmapManager)
        return VMAP::VMAP_INVALID_HEIGHT_VALUE;

    return vmapManager->getHeight(mapId, x, y, z, searchDistance);
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