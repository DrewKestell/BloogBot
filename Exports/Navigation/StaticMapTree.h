// StaticMapTree.h - Add this method declaration
#pragma once

#include <unordered_map>
#include <string>
#include <memory>
#include "BIH.h"
#include "Vector3.h"
#include "Ray.h"
#include "ModelInstance.h"

namespace VMAP
{
    class VMapManager2;
    class GroupModel;
    class WorldModel;

    class StaticMapTree
    {
    private:
        uint32_t iMapID;
        std::string iBasePath;
        bool iIsTiled;

        BIH iTree;
        ModelInstance* iTreeValues;
        uint32_t iNTreeValues;

        std::unordered_map<uint32_t, uint32_t> iLoadedSpawns;
        std::unordered_map<uint32_t, bool> iLoadedTiles;

        // NEW: Preload all tiles for maximum performance
        bool PreloadAllTiles(VMapManager2* vm);

    public:
        StaticMapTree(uint32_t mapId, const std::string& basePath);
        ~StaticMapTree();

        bool InitMap(const std::string& fname, VMapManager2* vm);
        bool LoadMapTile(uint32_t tileX, uint32_t tileY, VMapManager2* vm);
        void UnloadMapTile(uint32_t tileX, uint32_t tileY, VMapManager2* vm);
        void UnloadMap(VMapManager2* vm);

        // Collision and height queries
        bool isInLineOfSight(const G3D::Vector3& pos1, const G3D::Vector3& pos2, bool ignoreM2Model) const;
        bool getObjectHitPos(const G3D::Vector3& pos1, const G3D::Vector3& pos2,
            G3D::Vector3& resultHitPos, float modifyDist) const;
        float getHeight(const G3D::Vector3& pos, float maxSearchDist) const;
        bool getAreaInfo(G3D::Vector3& pos, uint32_t& flags, int32_t& adtId,
            int32_t& rootId, int32_t& groupId) const;
        bool GetLocationInfo(const G3D::Vector3& pos, LocationInfo& info) const;
        bool isUnderModel(G3D::Vector3& pos, float* outDist = nullptr, float* inDist = nullptr) const;

        bool getIntersectionTime(const G3D::Ray& ray, float& maxDist,
            bool stopAtFirstHit, bool ignoreM2Model) const;

        ModelInstance* FindCollisionModel(const G3D::Vector3& pos1, const G3D::Vector3& pos2);

        // Utility functions
        static uint32_t packTileID(uint32_t tileX, uint32_t tileY);
        static void unpackTileID(uint32_t ID, uint32_t& tileX, uint32_t& tileY);
        static std::string getTileFileName(uint32_t mapID, uint32_t tileX, uint32_t tileY);
        static bool CanLoadMap(const std::string& vmapPath, uint32_t mapID, uint32_t tileX, uint32_t tileY);

        // Getters
        bool isTiled() const;
        uint32_t numLoadedTiles() const;

#ifdef MMAP_GENERATOR
        void getModelInstances(ModelInstance*& models, uint32_t& count);
#endif
    };
}