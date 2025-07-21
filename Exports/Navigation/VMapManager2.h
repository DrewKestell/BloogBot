// wow/vmap/VMapManager2.h
#pragma once
#include <unordered_map>
#include <memory>
#include "MapTree.h"
#include "Log.h"
#include <filesystem>

namespace wow::vmap
{
    class VMapManager2
    {
    public:
        static VMapManager2& instance();

        bool loadMapTree(uint32 mapId);
        bool loadMapTile(uint32 mapId, int tileX, int tileY); 
        void unloadMap(uint32 mapId);

        // API used by physics / navigation
        bool isInLineOfSight(uint32 mapId,
            float x1, float y1, float z1,
            float x2, float y2, float z2) const;

        float getHeight(uint32 mapId,
            float x, float y, float z,
            float maxDist = 200.f) const;

        bool  getObjectHitPos(uint32 mapId,
            float x1, float y1, float z1,
            float x2, float y2, float z2,
            float& hx, float& hy, float& hz,
            float padding = 0.f) const;

       static std::string GetVmapsRootString();
    private:
        VMapManager2() = default;
        struct MapData { std::unique_ptr<MapTree> tree; };
        std::unordered_map<uint32, MapData> _maps; 
    };
}
