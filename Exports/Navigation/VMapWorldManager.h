#pragma once

#include <string>
#include <unordered_map>
#include <set>
#include <memory>

namespace VMAP
{
    class VMapManager2;
}

class VMapWorldManager
{
public:
    explicit VMapWorldManager(const std::string& dataDir);
    ~VMapWorldManager();

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
    std::unordered_map<unsigned int, std::unique_ptr<VMAP::VMapManager2>> _loadedMaps;
    std::set<unsigned int> _initializedMaps;
};
