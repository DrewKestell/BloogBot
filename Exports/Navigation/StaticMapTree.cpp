// StaticMapTree.cpp - Fixed with optional full preloading
#include "StaticMapTree.h"
#include "ModelInstance.h"
#include "WorldModel.h"
#include "VMapManager2.h"
#include "VMapDefinitions.h"
#include <fstream>
#include <iostream>
#include <algorithm>
#include <cstring>
#include <filesystem>

namespace VMAP
{
    // Constructor
    StaticMapTree::StaticMapTree(uint32_t mapId, const std::string& basePath)
        : iMapID(mapId), iBasePath(basePath), iIsTiled(false),
        iTreeValues(nullptr), iNTreeValues(0)
    {
        if (!iBasePath.empty() && iBasePath.back() != '/' && iBasePath.back() != '\\')
            iBasePath += "/";
    }

    // Destructor
    StaticMapTree::~StaticMapTree()
    {
        UnloadMap(nullptr);
        delete[] iTreeValues;
    }

    // Initialize map from file with optional full preloading
    bool StaticMapTree::InitMap(const std::string& fname, VMapManager2* vm)
    {
        bool success = true;
        std::string fullPath = iBasePath + fname;
        FILE* rf = fopen(fullPath.c_str(), "rb");
        if (!rf)
            return false;

        try
        {
            char chunk[8];

            // 1. Read magic (8 bytes)
            if (!readChunk(rf, chunk, VMAP_MAGIC, 8))
                success = false;

            // 2. Read tiled flag
            char tiled = 0;
            if (success && fread(&tiled, sizeof(char), 1, rf) != 1)
                success = false;
            iIsTiled = bool(tiled);

            // 3. Read NODE chunk and BIH tree
            if (success && !readChunk(rf, chunk, "NODE", 4))
                success = false;

            if (success)
                success = iTree.readFromFile(rf);

            if (success)
            {
                iNTreeValues = iTree.primCount();

                if (iNTreeValues > 0)
                {
                    iTreeValues = new ModelInstance[iNTreeValues];
                }
            }

            // 4. FIXED: Always read GOBJ chunk (even for tiled maps)
            if (success && !readChunk(rf, chunk, "GOBJ", 4))
                success = false;

            // 5. Only non-tiled maps have spawns after GOBJ
            if (success && !iIsTiled)
            {
                ModelSpawn spawn;
                while (ModelSpawn::readFromFile(rf, spawn))
                {
                    // Acquire model instance
                    std::shared_ptr<WorldModel> model = nullptr;
                    if (!spawn.name.empty())
                    {
                        model = vm->acquireModelInstance(iBasePath, spawn.name);
                        if (model)
                            model->setModelFlags(spawn.flags);
                    }

                    // FIXED: Read referencedVal IMMEDIATELY after spawn
                    uint32_t referencedVal;
                    if (fread(&referencedVal, sizeof(uint32_t), 1, rf) != 1)
                        break;

                    // FIXED: Use iLoadedSpawns as reference counter (not ID map)
                    if (!iLoadedSpawns.count(referencedVal))
                    {
                        if (referencedVal >= iNTreeValues)
                        {
                            continue;
                        }

                        iTreeValues[referencedVal] = ModelInstance(spawn, model);
                        iLoadedSpawns[referencedVal] = 1;  // First reference
                    }
                    else
                    {
                        // FIXED: Increment reference count
                        ++iLoadedSpawns[referencedVal];
                    }
                }
            }

            fclose(rf);

            // Keep your preload functionality if needed
            if (success && iIsTiled)
            {
                const char* preloadEnv = std::getenv("VMAP_PRELOAD_ALL");
                bool preloadAll = (preloadEnv && std::string(preloadEnv) == "1");

                if (preloadAll)
                {
                    PreloadAllTiles(vm);
                }
            }

            return success;
        }
        catch (const std::exception& e)
        {
            std::cerr << "[StaticMapTree] Exception: " << e.what() << std::endl;
            fclose(rf);
            return false;
        }
    }

    // New method to preload all tiles
    bool StaticMapTree::PreloadAllTiles(VMapManager2* vm)
    {
        if (!iIsTiled)
            return true;

        int tilesLoaded = 0;
        int tilesFailed = 0;

        // Scan for all possible tiles (64x64 grid for WoW)
        for (uint32_t x = 0; x < 64; ++x)
        {
            for (uint32_t y = 0; y < 64; ++y)
            {
                std::string tilefile = getTileFileName(iMapID, x, y);
                std::string fullPath = iBasePath + tilefile;

                // Check if tile exists
                if (!std::filesystem::exists(fullPath))
                    continue;

                // Load the tile
                if (LoadMapTile(x, y, vm))
                {
                    tilesLoaded++;
                }
                else
                {
                    tilesFailed++;
                    std::cerr << "[StaticMapTree] Failed to load tile " << tilefile << std::endl;
                }
            }
        }

        // Calculate memory usage
        size_t totalModels = 0;
        for (uint32_t i = 0; i < iNTreeValues; ++i)
        {
            if (iTreeValues[i].iModel)
                totalModels++;
        }

        return tilesFailed == 0;
    }

    // LoadMapTile implementation
    bool StaticMapTree::LoadMapTile(uint32_t tileX, uint32_t tileY, VMapManager2* vm)
    {
        if (!iIsTiled)
        {
            // Non-tiled maps have everything loaded already in InitMap
            iLoadedTiles[packTileID(tileX, tileY)] = false;
            return true;
        }

        uint32_t tileID = packTileID(tileX, tileY);

        // Check if already loaded
        if (iLoadedTiles.find(tileID) != iLoadedTiles.end())
        {
            return true;
        }

        std::string tilefile = getTileFileName(iMapID, tileX, tileY);
        std::string fullPath = iBasePath + tilefile;

        FILE* rf = fopen(fullPath.c_str(), "rb");
        if (!rf)
        {
            // No tile file - this is normal for empty areas
            iLoadedTiles[tileID] = false;
            return true;
        }

        bool success = true;

        try
        {
            char chunk[8];

            // Read VMAP magic
            if (!readChunk(rf, chunk, VMAP_MAGIC, 8))
            {
                success = false;
            }

            if (success)
            {
                // Read number of model spawns in this tile
                uint32_t numSpawns;
                if (fread(&numSpawns, sizeof(uint32_t), 1, rf) != 1)
                {
                    success = false;
                }
                else
                {
                    // Read each spawn
                    for (uint32_t i = 0; i < numSpawns && success; ++i)
                    {
                        ModelSpawn spawn;
                        if (!ModelSpawn::readFromFile(rf, spawn))
                        {
                            success = false;
                            break;
                        }

                        // FIXED: Read the tree index IMMEDIATELY after spawn (matching reference)
                        uint32_t referencedVal;
                        if (fread(&referencedVal, sizeof(uint32_t), 1, rf) != 1)
                        {
                            success = false;
                            break;
                        }

                        // Check bounds
                        if (!iTreeValues || referencedVal >= iNTreeValues)
                        {
                            continue;  // Skip this spawn but continue loading
                        }

                        // FIXED: Use iLoadedSpawns as reference counter (not spawn ID map)
                        if (!iLoadedSpawns.count(referencedVal))
                        {
                            // First time loading this tree index
                            std::shared_ptr<WorldModel> model = nullptr;
                            if (!spawn.name.empty())
                            {
                                model = vm->acquireModelInstance(iBasePath, spawn.name);
                                if (model)
                                {
                                    model->setModelFlags(spawn.flags);
                                }
                            }

                            // Store the model instance
                            iTreeValues[referencedVal] = ModelInstance(spawn, model);
                            iLoadedSpawns[referencedVal] = 1;  // First reference
                        }
                        else
                        {
                            // Already loaded this tree index, just increment reference count
                            ++iLoadedSpawns[referencedVal];

                            // Validate that we're loading the same spawn
                            if (iTreeValues[referencedVal].ID != spawn.ID)
                            {
                                std::cerr << "[StaticMapTree] Error: trying to load wrong spawn in node!" << std::endl;
                            }
                            else if (iTreeValues[referencedVal].name != spawn.name)
                            {
                                std::cerr << "[StaticMapTree] Error: name mismatch on GUID=" << spawn.ID << std::endl;
                            }
                        }
                    }
                }
            }

            fclose(rf);

            if (success)
            {
                iLoadedTiles[tileID] = true;
            }

            return success;
        }
        catch (const std::exception& e)
        {
            std::cerr << "[StaticMapTree] Exception loading tile: " << e.what() << std::endl;
            fclose(rf);
            return false;
        }
    }

    void StaticMapTree::UnloadMapTile(uint32_t tileX, uint32_t tileY, VMapManager2* vm)
    {
        if (!iIsTiled)
            return;

        uint32_t tileID = packTileID(tileX, tileY);
        auto itr = iLoadedTiles.find(tileID);
        if (itr == iLoadedTiles.end())
            return;

        iLoadedTiles.erase(itr);
    }

    void StaticMapTree::UnloadMap(VMapManager2* vm)
    {
        if (iTreeValues)
        {
            for (uint32_t i = 0; i < iNTreeValues; ++i)
            {
                iTreeValues[i].setUnloaded();
            }
        }

        iLoadedTiles.clear();
        iLoadedSpawns.clear();
    }

    // All query methods remain the same with null checks
    bool StaticMapTree::isInLineOfSight(const G3D::Vector3& pos1, const G3D::Vector3& pos2, bool ignoreM2Model) const
    {
        if (!iTreeValues || iNTreeValues == 0)
            return true;

        float maxDist = (pos2 - pos1).magnitude();
        if (maxDist < 0.001f)
            return true;

        G3D::Ray ray = G3D::Ray::fromOriginAndDirection(pos1, (pos2 - pos1) / maxDist);

        if (getIntersectionTime(ray, maxDist, true, ignoreM2Model))
            return false;

        return true;
    }

    bool StaticMapTree::getObjectHitPos(const G3D::Vector3& pos1, const G3D::Vector3& pos2,
        G3D::Vector3& resultHitPos, float modifyDist) const
    {
        if (!iTreeValues || iNTreeValues == 0)
        {
            resultHitPos = pos2;
            return false;
        }

        float maxDist = (pos2 - pos1).magnitude();
        if (maxDist < 0.001f)
        {
            resultHitPos = pos2;
            return false;
        }

        G3D::Vector3 dir = (pos2 - pos1) / maxDist;
        G3D::Ray ray = G3D::Ray::fromOriginAndDirection(pos1, dir);

        float distance = maxDist;
        if (getIntersectionTime(ray, distance, true, false))
        {
            resultHitPos = pos1 + dir * distance;
            if (modifyDist > 0 && distance > modifyDist)
            {
                resultHitPos = pos1 + dir * (distance - modifyDist);
            }
            return true;
        }

        resultHitPos = pos2;
        return false;
    }

    float StaticMapTree::getHeight(const G3D::Vector3& pos, float maxSearchDist) const
    {
        float height = -G3D::inf();

        if (!iTreeValues || iNTreeValues == 0)
            return height;

        // The ray shoots downward from above
        G3D::Ray ray(pos + G3D::Vector3(0, 0, maxSearchDist), G3D::Vector3(0, 0, -1));
        float distance = maxSearchDist * 2.0f;

        // Count loaded models
        int loadedCount = 0;
        for (uint32_t i = 0; i < iNTreeValues; ++i) {
            if (iTreeValues[i].iModel) loadedCount++;
        }

        if (getIntersectionTime(ray, distance, true, false))
        {
            height = pos.z + maxSearchDist - distance;
        }

        return height;
    }

    bool StaticMapTree::getAreaInfo(G3D::Vector3& pos, uint32_t& flags, int32_t& adtId,
        int32_t& rootId, int32_t& groupId) const
    {
        if (!iTreeValues || iNTreeValues == 0)
            return false;

        AreaInfo info;

        for (uint32_t i = 0; i < iNTreeValues; ++i)
        {
            if (iTreeValues[i].iModel)
            {
                iTreeValues[i].intersectPoint(pos, info);
            }
        }

        if (info.result)
        {
            flags = info.flags;
            adtId = info.adtId;
            rootId = info.rootId;
            groupId = info.groupId;
            pos.z = info.ground_Z;
            return true;
        }

        return false;
    }

    bool StaticMapTree::GetLocationInfo(const G3D::Vector3& pos, LocationInfo& info) const
    {
        if (!iTreeValues || iNTreeValues == 0)
            return false;

        for (uint32_t i = 0; i < iNTreeValues; ++i)
        {
            if (iTreeValues[i].iModel)
            {
                if (iTreeValues[i].GetLocationInfo(pos, info))
                    return true;
            }
        }

        return false;
    }

    bool StaticMapTree::getIntersectionTime(const G3D::Ray& ray, float& maxDist,
        bool stopAtFirstHit, bool ignoreM2Model) const
    {
        if (!iTreeValues || iNTreeValues == 0)
            return false;

        auto intersectCallback = [this, ignoreM2Model](const G3D::Ray& r, uint32_t idx,
            float& d, bool stopAtFirst, bool ignoreM2) -> bool
            {
                if (!iTreeValues || idx >= iNTreeValues)
                    return false;

                // Only test models that are actually loaded
                if (!iTreeValues[idx].iModel)
                    return false;

                return iTreeValues[idx].intersectRay(r, d, stopAtFirst, ignoreM2);
            };

        float oldDist = maxDist;
        iTree.intersectRay(ray, intersectCallback, maxDist, stopAtFirstHit, ignoreM2Model);

        return maxDist < oldDist;
    }

    uint32_t StaticMapTree::packTileID(uint32_t tileX, uint32_t tileY)
    {
        return (tileX << 16) | tileY;
    }

    void StaticMapTree::unpackTileID(uint32_t ID, uint32_t& tileX, uint32_t& tileY)
    {
        tileX = (ID >> 16);
        tileY = (ID & 0xFFFF);
    }

    std::string StaticMapTree::getTileFileName(uint32_t mapID, uint32_t tileX, uint32_t tileY)
    {
        char buffer[256];
        snprintf(buffer, sizeof(buffer), "%03u_%02u_%02u.vmtile", mapID, tileX, tileY);
        return std::string(buffer);
    }

    bool StaticMapTree::CanLoadMap(const std::string& vmapPath, uint32_t mapID, uint32_t tileX, uint32_t tileY)
    {
        std::string fileName = vmapPath + getTileFileName(mapID, tileX, tileY);
        FILE* rf = fopen(fileName.c_str(), "rb");
        if (!rf)
            return false;

        fclose(rf);
        return true;
    }

    bool StaticMapTree::isTiled() const
    {
        return iIsTiled;
    }

    uint32_t StaticMapTree::numLoadedTiles() const
    {
        return iLoadedTiles.size();
    }

    bool StaticMapTree::isUnderModel(G3D::Vector3& pos, float* outDist, float* inDist) const
    {
        if (!iTreeValues || iNTreeValues == 0)
            return false;

        G3D::Ray ray(pos, G3D::Vector3(0, 0, 1));
        float maxDist = 100.0f;

        auto callback = [this](const G3D::Ray& r, uint32_t idx,
            float& d, bool stopAtFirst, bool ignoreM2) -> bool
            {
                if (!iTreeValues || idx >= iNTreeValues || !iTreeValues[idx].iModel)
                    return false;

                return iTreeValues[idx].intersectRay(r, d, stopAtFirst, ignoreM2);
            };

        float distance = maxDist;
        iTree.intersectRay(ray, callback, distance, true, false);

        if (distance < maxDist)
        {
            if (outDist)
                *outDist = distance;
            if (inDist)
                *inDist = 0.0f;
            return true;
        }

        return false;
    }

    ModelInstance* StaticMapTree::FindCollisionModel(const G3D::Vector3& pos1, const G3D::Vector3& pos2)
    {
        if (!iTreeValues || iNTreeValues == 0)
            return nullptr;

        float maxDist = (pos2 - pos1).magnitude();
        if (maxDist < 0.001f)
            return nullptr;

        G3D::Ray ray = G3D::Ray::fromOriginAndDirection(pos1, (pos2 - pos1) / maxDist);

        ModelInstance* hitModel = nullptr;
        float closestDist = maxDist;

        for (uint32_t i = 0; i < iNTreeValues; ++i)
        {
            if (!iTreeValues[i].iModel)
                continue;

            float dist = maxDist;
            if (iTreeValues[i].intersectRay(ray, dist, true, false))
            {
                if (dist < closestDist)
                {
                    closestDist = dist;
                    hitModel = &iTreeValues[i];
                }
            }
        }

        return hitModel;
    }
} // namespace VMAP