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
        std::cout << "[StaticMapTree] InitMap called for: " << fname << std::endl;

        std::string fullPath = iBasePath + fname;
        FILE* rf = fopen(fullPath.c_str(), "rb");
        if (!rf)
        {
            std::cerr << "[StaticMapTree] ERROR: Failed to open file: " << fullPath << std::endl;
            return false;
        }

        try
        {
            // 1. Read magic (8 bytes) - "VMAP_x.x"
            char magic[8];
            if (fread(magic, 1, 8, rf) != 8)
            {
                std::cerr << "[StaticMapTree] ERROR: Failed to read magic" << std::endl;
                fclose(rf);
                return false;
            }

            if (memcmp(magic, "VMAP", 4) != 0)
            {
                std::cerr << "[StaticMapTree] ERROR: Invalid magic" << std::endl;
                fclose(rf);
                return false;
            }

            std::cout << "[StaticMapTree] Magic: " << std::string(magic, 8) << std::endl;

            // 2. Read tiled flag (1 byte)
            char tiledFlag;
            if (fread(&tiledFlag, sizeof(char), 1, rf) != 1)
            {
                std::cerr << "[StaticMapTree] ERROR: Failed to read tiled flag" << std::endl;
                fclose(rf);
                return false;
            }
            iIsTiled = (tiledFlag != 0);
            std::cout << "[StaticMapTree] Is tiled: " << (iIsTiled ? "yes" : "no") << std::endl;

            // 3. Read NODE chunk and BIH tree
            char chunk[4];
            if (fread(chunk, 1, 4, rf) != 4 || memcmp(chunk, "NODE", 4) != 0)
            {
                std::cerr << "[StaticMapTree] ERROR: Failed to read NODE chunk" << std::endl;
                fclose(rf);
                return false;
            }
            std::cout << "[StaticMapTree] NODE format detected" << std::endl;

            // 4. Read BIH tree
            std::cout << "[StaticMapTree] Reading BIH tree..." << std::endl;
            if (!iTree.readFromFile(rf))
            {
                std::cerr << "[StaticMapTree] ERROR: Failed to read BIH tree" << std::endl;
                fclose(rf);
                return false;
            }

            // 5. Get the tree size and allocate instance array
            iNTreeValues = iTree.primCount();
            std::cout << "[StaticMapTree] BIH tree references " << iNTreeValues << " model slots" << std::endl;

            if (iNTreeValues > 0)
            {
                // For large maps, show memory estimate
                size_t estimatedMemory = iNTreeValues * sizeof(ModelInstance);
                std::cout << "[StaticMapTree] Allocating " << (estimatedMemory / (1024 * 1024))
                    << " MB for model instance array" << std::endl;

                try
                {
                    iTreeValues = new ModelInstance[iNTreeValues];
                    std::cout << "[StaticMapTree] Successfully allocated " << iNTreeValues
                        << " model instance slots" << std::endl;
                }
                catch (const std::bad_alloc& e)
                {
                    std::cerr << "[StaticMapTree] ERROR: Failed to allocate memory for "
                        << iNTreeValues << " instances: " << e.what() << std::endl;
                    std::cerr << "[StaticMapTree] Try increasing available memory or using lazy loading" << std::endl;
                    iTreeValues = nullptr;
                    iNTreeValues = 0;
                    fclose(rf);
                    return false;
                }
            }

            // 6. Handle tiled vs non-tiled maps
            if (iIsTiled)
            {
                std::cout << "[StaticMapTree] Tiled map - checking for preload option..." << std::endl;

                // Check environment variable for preload preference
                const char* preloadEnv = std::getenv("VMAP_PRELOAD_ALL");
                bool preloadAll = (preloadEnv && std::string(preloadEnv) == "1");

                if (true)
                {
                    std::cout << "[StaticMapTree] VMAP_PRELOAD_ALL=1 detected, preloading all tiles..." << std::endl;
                    fclose(rf);  // Close the main file before loading tiles

                    if (!PreloadAllTiles(vm))
                    {
                        std::cerr << "[StaticMapTree] Warning: Some tiles failed to load" << std::endl;
                    }
                    else
                    {
                        std::cout << "[StaticMapTree] All tiles preloaded successfully" << std::endl;
                    }

                    return true;  // Early return after preloading
                }
                else
                {
                    std::cout << "[StaticMapTree] Tiles will be loaded on demand (set VMAP_PRELOAD_ALL=1 to preload)" << std::endl;
                }
            }
            else
            {
                // Non-tiled map - load all spawns from GOBJ chunk
                std::cout << "[StaticMapTree] Non-tiled map - loading all spawns..." << std::endl;

                // Skip to GOBJ chunk
                if (fread(chunk, 1, 4, rf) == 4 && memcmp(chunk, "GOBJ", 4) == 0)
                {
                    std::cout << "[StaticMapTree] Found GOBJ chunk" << std::endl;

                    uint32_t loadedCount = 0;
                    ModelSpawn spawn;

                    while (!feof(rf))
                    {
                        long currentPos = ftell(rf);

                        if (!ModelSpawn::readFromFile(rf, spawn))
                        {
                            fseek(rf, currentPos, SEEK_SET);
                            break;
                        }

                        uint32_t referencedVal;
                        if (fread(&referencedVal, sizeof(uint32_t), 1, rf) != 1)
                            break;

                        if (referencedVal >= iNTreeValues)
                        {
                            std::cerr << "[StaticMapTree] Warning: Invalid reference " << referencedVal << std::endl;
                            continue;
                        }

                        // Load the model
                        std::shared_ptr<WorldModel> model = nullptr;
                        if (!spawn.name.empty())
                        {
                            model = vm->acquireModelInstance(iBasePath, spawn.name);
                            if (model)
                            {
                                model->setModelFlags(spawn.flags);
                            }
                        }

                        iTreeValues[referencedVal] = ModelInstance(spawn, model);
                        iLoadedSpawns[spawn.ID] = referencedVal;
                        loadedCount++;
                    }

                    std::cout << "[StaticMapTree] Loaded " << loadedCount << " model spawns" << std::endl;
                }
            }

            fclose(rf);
            std::cout << "[StaticMapTree] Map " << iMapID << " initialized successfully" << std::endl;
            return true;
        }
        catch (const std::exception& e)
        {
            std::cerr << "[StaticMapTree] Exception: " << e.what() << std::endl;
            if (iTreeValues)
            {
                delete[] iTreeValues;
                iTreeValues = nullptr;
                iNTreeValues = 0;
            }
            fclose(rf);
            return false;
        }
    }

    // New method to preload all tiles
    bool StaticMapTree::PreloadAllTiles(VMapManager2* vm)
    {
        if (!iIsTiled)
            return true;

        std::cout << "[StaticMapTree] Scanning for tiles to preload..." << std::endl;

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
                    if (tilesLoaded % 10 == 0)
                    {
                        std::cout << "[StaticMapTree] Loaded " << tilesLoaded << " tiles..." << std::endl;
                    }
                }
                else
                {
                    tilesFailed++;
                    std::cerr << "[StaticMapTree] Failed to load tile " << tilefile << std::endl;
                }
            }
        }

        std::cout << "[StaticMapTree] Preload complete: " << tilesLoaded << " tiles loaded";
        if (tilesFailed > 0)
        {
            std::cout << ", " << tilesFailed << " failed";
        }
        std::cout << std::endl;

        // Calculate memory usage
        size_t totalModels = 0;
        for (uint32_t i = 0; i < iNTreeValues; ++i)
        {
            if (iTreeValues[i].iModel)
                totalModels++;
        }

        std::cout << "[StaticMapTree] " << totalModels << " of " << iNTreeValues
            << " model slots populated ("
            << (totalModels * 100 / iNTreeValues) << "%)" << std::endl;

        return tilesFailed == 0;
    }

    // LoadMapTile implementation
    bool StaticMapTree::LoadMapTile(uint32_t tileX, uint32_t tileY, VMapManager2* vm)
    {
        if (!iIsTiled)
        {
            // Non-tiled maps have everything loaded already
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

        try
        {
            // Read VMAP magic
            char chunk[8];
            if (!readChunk(rf, chunk, VMAP_MAGIC, 8))
            {
                std::cerr << "[StaticMapTree] ERROR: Invalid tile file magic in " << tilefile << std::endl;
                fclose(rf);
                return false;
            }

            // Read number of model spawns in this tile
            uint32_t numSpawns;
            if (fread(&numSpawns, sizeof(uint32_t), 1, rf) != 1)
            {
                std::cerr << "[StaticMapTree] ERROR: Failed to read spawn count" << std::endl;
                fclose(rf);
                return false;
            }

            // Read each spawn and its referenced tree index
            uint32_t loadedCount = 0;
            for (uint32_t i = 0; i < numSpawns; ++i)
            {
                ModelSpawn spawn;
                if (!ModelSpawn::readFromFile(rf, spawn))
                {
                    std::cerr << "[StaticMapTree] ERROR: Failed to read spawn " << i << std::endl;
                    fclose(rf);
                    return false;
                }

                // Read the tree index this spawn references
                uint32_t referencedVal;
                if (fread(&referencedVal, sizeof(uint32_t), 1, rf) != 1)
                {
                    std::cerr << "[StaticMapTree] ERROR: Failed to read reference value" << std::endl;
                    fclose(rf);
                    return false;
                }

                // Check bounds
                if (!iTreeValues || referencedVal >= iNTreeValues)
                {
                    std::cerr << "[StaticMapTree] ERROR: Invalid tree reference: " << referencedVal
                        << " (max: " << iNTreeValues << ")" << std::endl;
                    continue;
                }

                // Check if this spawn is already loaded
                auto spawnItr = iLoadedSpawns.find(spawn.ID);
                if (spawnItr == iLoadedSpawns.end())
                {
                    // First time loading this spawn
                    std::shared_ptr<WorldModel> model = nullptr;
                    if (!spawn.name.empty())
                    {
                        try
                        {
                            model = vm->acquireModelInstance(iBasePath, spawn.name);
                            if (model)
                            {
                                model->setModelFlags(spawn.flags);
                            }
                        }
                        catch (const std::exception& e)
                        {
                            std::cerr << "[StaticMapTree] Failed to load model " << spawn.name
                                << ": " << e.what() << std::endl;
                        }
                    }

                    iTreeValues[referencedVal] = ModelInstance(spawn, model);
                    iLoadedSpawns[spawn.ID] = referencedVal;
                    loadedCount++;
                }
            }

            fclose(rf);
            iLoadedTiles[tileID] = true;

            return true;
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

        G3D::Ray ray(pos + G3D::Vector3(0, 0, maxSearchDist), G3D::Vector3(0, 0, -1));
        float distance = maxSearchDist * 2.0f;

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

#ifdef MMAP_GENERATOR
    void StaticMapTree::getModelInstances(ModelInstance*& models, uint32_t& count)
    {
        models = iTreeValues;
        count = iNTreeValues;
    }
#endif

} // namespace VMAP