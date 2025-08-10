// VMapClient.h - Fixed header
#pragma once

#include <string>
#include <cstdint>

namespace VMAP
{
    class VMapManager2;  // Forward declare the concrete class
}

class VMapClient
{
private:
    VMAP::VMapManager2* vmapManager;  // Use the concrete type
    std::string vmapPath;
    bool initialized;

public:
    VMapClient(const std::string& dataPath = "");
    ~VMapClient();

    // Initialize the VMAP system
    void initialize();

    // Check if initialized
    bool isInitialized() const { return initialized && vmapManager != nullptr; }

    // Map management
    void preloadMap(uint32_t mapId);
    bool loadMapTile(uint32_t mapId, int x, int y);
    void unloadMapTile(uint32_t mapId, int x, int y);
    void unloadMap(uint32_t mapId);
    const std::string& getVMapPath() const { return vmapPath; }
    // Height queries
    float getGroundHeight(uint32_t mapId, float x, float y, float z, float searchDistance = 1000.0f);

    // Line of sight
    bool isLineOfSight(uint32_t mapId,
        float x1, float y1, float z1,
        float x2, float y2, float z2,
        bool ignoreM2Models = false);

    // Collision detection
    bool getCollisionPoint(uint32_t mapId,
        float x1, float y1, float z1,
        float x2, float y2, float z2,
        float& hitX, float& hitY, float& hitZ);

    // Area information
    void getAreaInfo(uint32_t mapId, float x, float y, float& z,
        uint32_t& flags, int32_t& adtId, int32_t& rootId, int32_t& groupId);

    // Liquid queries
    bool getLiquidLevel(uint32_t mapId, float x, float y, float z,
        float& liquidLevel, float& liquidFloor);
};