#pragma once

#include <string>
#include <unordered_map>
#include <set>

namespace VMAP
{
    class VMapManager2;
}

class VMapManager
{
public:
    explicit VMapManager(const std::string& dataDir);
    ~VMapManager();

    bool LoadMap(unsigned int mapId);
    bool LoadTile(unsigned int mapId, int tileX, int tileY);
    bool UnloadMap(unsigned int mapId);
    bool Raycast(unsigned int mapId,
        float startX, float startY, float startZ,
        float endX, float endY, float endZ,
        float& hitZ);

    void InitializeMapsForContinent(unsigned int mapId);
    static std::string GetVmapsPath();

private:
    std::string _dataPath;
    std::unordered_map<unsigned int, VMAP::VMapManager2*> _loadedMaps;
    std::set<unsigned int> _initializedMaps;
};
