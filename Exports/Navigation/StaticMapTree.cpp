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
#include "VMapLog.h"

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
        LOG_INFO("==================== LoadMapTile START ====================");
        LOG_INFO("LoadMapTile called for Map:" << iMapID
            << " Tile:[" << tileX << "," << tileY << "]"
            << " Tiled:" << (iIsTiled ? "YES" : "NO"));

        if (!iIsTiled)
        {
            LOG_WARN("Map " << iMapID << " is NOT TILED - all data should be in base file");
            LOG_DEBUG("Setting tile as loaded (false flag) since non-tiled");
            iLoadedTiles[packTileID(tileX, tileY)] = false;
            LOG_INFO("==================== LoadMapTile END (non-tiled) ====================");
            return true;
        }

        uint32_t tileID = packTileID(tileX, tileY);
        LOG_DEBUG("Packed tile ID: " << tileID << " (0x" << std::hex << tileID << std::dec << ")");

        // Check if already loaded
        if (iLoadedTiles.find(tileID) != iLoadedTiles.end())
        {
            bool isLoaded = iLoadedTiles[tileID];
            LOG_INFO("Tile already processed - Loaded:" << (isLoaded ? "YES" : "NO (empty tile)"));
            LOG_INFO("==================== LoadMapTile END (already loaded) ====================");
            return true;
        }

        // Build tile filename
        std::string tilefile = getTileFileName(iMapID, tileX, tileY);
        std::string fullPath = iBasePath + tilefile;

        LOG_INFO("Attempting to load tile file:");
        LOG_INFO("  Filename: " << tilefile);
        LOG_INFO("  Full path: " << fullPath);
        LOG_INFO("  Base path: " << iBasePath);

        // Check if file exists
        bool fileExists = std::filesystem::exists(fullPath);
        LOG_INFO("File exists: " << (fileExists ? "YES" : "NO"));

        if (!fileExists)
        {
            LOG_WARN("Tile file does not exist - marking as empty tile");
            LOG_DEBUG("This is normal for areas with no models");
            iLoadedTiles[tileID] = false;
            LOG_INFO("==================== LoadMapTile END (no file) ====================");
            return true;
        }

        // Get file size for validation
        auto fileSize = std::filesystem::file_size(fullPath);
        LOG_INFO("File size: " << fileSize << " bytes");

        FILE* rf = fopen(fullPath.c_str(), "rb");
        if (!rf)
        {
            LOG_ERROR("Failed to open tile file! Error: " << strerror(errno));
            iLoadedTiles[tileID] = false;
            LOG_INFO("==================== LoadMapTile END (open failed) ====================");
            return false;
        }

        LOG_INFO("File opened successfully");

        bool success = true;
        uint32_t totalSpawnsLoaded = 0;
        uint32_t newModelsLoaded = 0;
        uint32_t referencesIncremented = 0;

        try
        {
            char chunk[8];

            // Read VMAP magic
            LOG_DEBUG("Reading VMAP magic header...");
            if (!readChunk(rf, chunk, VMAP_MAGIC, 8))
            {
                LOG_ERROR("Invalid VMAP magic in tile file!");
                LOG_ERROR("Expected: " << VMAP_MAGIC << " Got: " << std::string(chunk, 8));
                success = false;
            }
            else
            {
                LOG_DEBUG("VMAP magic verified: " << VMAP_MAGIC);
            }

            if (success)
            {
                // Read number of model spawns in this tile
                uint32_t numSpawns;
                if (fread(&numSpawns, sizeof(uint32_t), 1, rf) != 1)
                {
                    LOG_ERROR("Failed to read spawn count!");
                    success = false;
                }
                else
                {
                    LOG_INFO("Number of spawns in tile: " << numSpawns);

                    if (numSpawns > 10000)
                    {
                        LOG_WARN("Suspicious spawn count: " << numSpawns << " - possible corruption?");
                    }

                    // Read each spawn
                    for (uint32_t i = 0; i < numSpawns && success; ++i)
                    {
                        LOG_TRACE("Reading spawn " << (i + 1) << "/" << numSpawns);

                        ModelSpawn spawn;
                        if (!ModelSpawn::readFromFile(rf, spawn))
                        {
                            LOG_ERROR("Failed to read spawn " << i << "!");
                            success = false;
                            break;
                        }

                        LOG_DEBUG("Spawn " << i << " read successfully:");
                        LOG_DEBUG("  Name: " << spawn.name);
                        LOG_DEBUG("  ID: " << spawn.ID);
                        LOG_DEBUG("  Flags: 0x" << std::hex << spawn.flags << std::dec);
                        LOG_DEBUG("  Position: (" << spawn.iPos.x << "," << spawn.iPos.y << "," << spawn.iPos.z << ")");
                        LOG_DEBUG("  Rotation: (" << spawn.iRot.x << "," << spawn.iRot.y << "," << spawn.iRot.z << ")");
                        LOG_DEBUG("  Scale: " << spawn.iScale);

                        // Read the tree index
                        uint32_t referencedVal;
                        if (fread(&referencedVal, sizeof(uint32_t), 1, rf) != 1)
                        {
                            LOG_ERROR("Failed to read tree index for spawn " << i << "!");
                            success = false;
                            break;
                        }

                        LOG_DEBUG("  Tree index: " << referencedVal << " (of " << iNTreeValues << " total)");

                        // Check bounds
                        if (!iTreeValues)
                        {
                            LOG_ERROR("Tree values array is NULL!");
                            success = false;
                            break;
                        }

                        if (referencedVal >= iNTreeValues)
                        {
                            LOG_ERROR("Tree index " << referencedVal << " out of bounds! Max: " << iNTreeValues);
                            continue;  // Skip but don't fail completely
                        }

                        // Check if already loaded
                        if (!iLoadedSpawns.count(referencedVal))
                        {
                            LOG_INFO("Loading NEW model for tree index " << referencedVal);

                            // First time loading this tree index
                            std::shared_ptr<WorldModel> model = nullptr;

                            if (!spawn.name.empty())
                            {
                                LOG_DEBUG("Acquiring model instance: " << spawn.name);
                                model = vm->acquireModelInstance(iBasePath, spawn.name);

                                if (model)
                                {
                                    LOG_INFO("Model " << spawn.name << " loaded successfully!");
                                    model->setModelFlags(spawn.flags);
                                    newModelsLoaded++;
                                }
                                else
                                {
                                    LOG_ERROR("FAILED to load model: " << spawn.name);
                                    LOG_ERROR("Model will be missing from collision detection!");
                                }
                            }
                            else
                            {
                                LOG_WARN("Empty model name for spawn " << i);
                            }

                            // Store the model instance (even if null)
                            LOG_DEBUG("Storing ModelInstance at tree index " << referencedVal);
                            iTreeValues[referencedVal] = ModelInstance(spawn, model);
                            iLoadedSpawns[referencedVal] = 1;  // First reference

                            LOG_DEBUG("ModelInstance stored. Has model: " << (model ? "YES" : "NO"));
                        }
                        else
                        {
                            // Already loaded this tree index, just increment reference count
                            LOG_DEBUG("Tree index " << referencedVal << " already loaded - incrementing reference");
                            ++iLoadedSpawns[referencedVal];
                            referencesIncremented++;

                            // Validate that we're loading the same spawn
                            if (iTreeValues[referencedVal].ID != spawn.ID)
                            {
                                LOG_WARN("Spawn ID mismatch! Tree has ID:" << iTreeValues[referencedVal].ID
                                    << " File has ID:" << spawn.ID);
                            }

                            if (iTreeValues[referencedVal].name != spawn.name)
                            {
                                LOG_WARN("Name mismatch! Tree has:" << iTreeValues[referencedVal].name
                                    << " File has:" << spawn.name);
                            }
                        }

                        totalSpawnsLoaded++;
                    }

                    LOG_INFO("Finished processing spawns:");
                    LOG_INFO("  Total spawns processed: " << totalSpawnsLoaded << "/" << numSpawns);
                    LOG_INFO("  New models loaded: " << newModelsLoaded);
                    LOG_INFO("  References incremented: " << referencesIncremented);
                }
            }

            fclose(rf);

            if (success)
            {
                LOG_INFO("Tile loaded SUCCESSFULLY!");
                iLoadedTiles[tileID] = true;

                // Count loaded models in the entire tree
                int totalLoadedModels = 0;
                for (uint32_t i = 0; i < iNTreeValues; ++i)
                {
                    if (iTreeValues[i].iModel)
                        totalLoadedModels++;
                }

                LOG_INFO("Total models now loaded in tree: " << totalLoadedModels << "/" << iNTreeValues);
                LOG_INFO("Total tiles now loaded: " << iLoadedTiles.size());
                LOG_INFO("Total spawn references: " << iLoadedSpawns.size());
            }
            else
            {
                LOG_ERROR("Tile loading FAILED!");
            }
        }
        catch (const std::bad_alloc& e)
        {
            LOG_ERROR("Memory allocation failed: " << e.what());
            fclose(rf);
            success = false;
        }
        catch (const std::exception& e)
        {
            LOG_ERROR("Exception loading tile: " << e.what());
            fclose(rf);
            success = false;
        }
        catch (...)
        {
            LOG_ERROR("Unknown exception loading tile!");
            fclose(rf);
            success = false;
        }

        LOG_INFO("==================== LoadMapTile END (success=" << success << ") ====================");
        return success;
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
        LOG_DEBUG("=== StaticMapTree::isInLineOfSight ===");
        LOG_VECTOR3("  Start", pos1);
        LOG_VECTOR3("  End", pos2);
        LOG_DEBUG("  IgnoreM2: " << (ignoreM2Model ? "YES" : "NO"));
        LOG_DEBUG("  Tree values: " << (iTreeValues ? "LOADED" : "NULL") << ", Count: " << iNTreeValues);

        if (!iTreeValues || iNTreeValues == 0)
        {
            LOG_DEBUG("  No tree values - returning clear LOS");
            return true;
        }

        float maxDist = (pos2 - pos1).magnitude();
        LOG_DEBUG("  Max distance: " << maxDist);

        if (maxDist < 0.001f)
        {
            LOG_DEBUG("  Distance too small - returning clear LOS");
            return true;
        }

        G3D::Ray ray = G3D::Ray::fromOriginAndDirection(pos1, (pos2 - pos1) / maxDist);
        LOG_RAY("  Ray", ray);

        // Count loaded models for logging
        int loadedCount = 0;
        for (uint32_t i = 0; i < iNTreeValues; ++i)
        {
            if (iTreeValues[i].iModel)
                loadedCount++;
        }
        LOG_DEBUG("  Models loaded: " << loadedCount << "/" << iNTreeValues);

        float intersectDist = maxDist;
        bool hit = getIntersectionTime(ray, intersectDist, true, ignoreM2Model);

        LOG_DEBUG("  Intersection result: " << (hit ? "BLOCKED" : "CLEAR"));
        if (hit)
        {
            LOG_DEBUG("  Hit distance: " << intersectDist);
        }

        return !hit;
    }

    bool StaticMapTree::getObjectHitPos(const G3D::Vector3& pos1, const G3D::Vector3& pos2,
        G3D::Vector3& resultHitPos, float modifyDist) const
    {
        LOG_TRACE("StaticMapTree::getObjectHitPos ENTER - MapId:" << iMapID
            << " From:(" << pos1.x << "," << pos1.y << "," << pos1.z << ")"
            << " To:(" << pos2.x << "," << pos2.y << "," << pos2.z << ")"
            << " ModifyDist:" << modifyDist);

        if (!iTreeValues || iNTreeValues == 0)
        {
            LOG_DEBUG("No collision data - no hit possible");
            resultHitPos = pos2;
            return false;
        }

        float maxDist = (pos2 - pos1).magnitude();
        if (maxDist < 0.001f)
        {
            LOG_DEBUG("Start and end too close - no hit");
            resultHitPos = pos2;
            return false;
        }

        G3D::Vector3 dir = (pos2 - pos1) / maxDist;
        G3D::Ray ray = G3D::Ray::fromOriginAndDirection(pos1, dir);

        LOG_RAY("Collision ray", ray);

        float distance = maxDist;
        if (getIntersectionTime(ray, distance, true, false))
        {
            resultHitPos = pos1 + dir * distance;

            LOG_INFO("Hit detected! Distance:" << distance
                << " HitPos:(" << resultHitPos.x << "," << resultHitPos.y << "," << resultHitPos.z << ")");

            if (modifyDist > 0 && distance > modifyDist)
            {
                resultHitPos = pos1 + dir * (distance - modifyDist);
                LOG_DEBUG("Modified hit position by " << modifyDist
                    << " New:(" << resultHitPos.x << "," << resultHitPos.y << "," << resultHitPos.z << ")");
            }
            return true;
        }

        LOG_DEBUG("No hit detected");
        resultHitPos = pos2;
        return false;
    }

    float StaticMapTree::getHeight(const G3D::Vector3& pos, float maxSearchDist) const
    {
        LOG_TRACE("StaticMapTree::getHeight ENTER - MapId:" << iMapID
            << " Pos:(" << pos.x << "," << pos.y << "," << pos.z << ")"
            << " MaxSearch:" << maxSearchDist);

        float height = -G3D::inf();

        if (!iTreeValues || iNTreeValues == 0)
        {
            LOG_WARN("No tree values loaded - MapId:" << iMapID
                << " TreeValues:" << (iTreeValues ? "ALLOCATED" : "NULL")
                << " Count:" << iNTreeValues);
            return height;
        }

        // Count loaded models for debugging
        int loadedCount = 0;
        int checkedCount = 0;
        for (uint32_t i = 0; i < iNTreeValues; ++i)
        {
            if (iTreeValues[i].iModel)
                loadedCount++;
        }

        LOG_DEBUG("Tree stats - Total nodes:" << iNTreeValues
            << " Loaded models:" << loadedCount
            << " Tiled:" << (iIsTiled ? "YES" : "NO")
            << " LoadedTiles:" << iLoadedTiles.size());

        // The ray shoots downward from above
        G3D::Vector3 rayStart = pos;
        G3D::Ray ray(rayStart, G3D::Vector3(0, 0, -1));
        float distance = maxSearchDist * 2;

        LOG_RAY("Height search ray", ray);
        LOG_DEBUG("Search distance: " << distance);

        float originalDistance = distance;
        if (getIntersectionTime(ray, distance, true, false))
        {
            height = pos.z - distance;
            LOG_INFO("Height found! Distance:" << distance
                << " Height:" << height
                << " (z+" << maxSearchDist << "-" << distance << ")");
        }
        else
        {
            LOG_DEBUG("No intersection found - returning -inf");
        }

        LOG_TRACE("StaticMapTree::getHeight EXIT - Height:" << height);
        return height;
    }

    bool StaticMapTree::getAreaInfo(G3D::Vector3& pos, uint32_t& flags, int32_t& adtId,
        int32_t& rootId, int32_t& groupId) const
    {
        LOG_TRACE("StaticMapTree::getAreaInfo ENTER - MapId:" << iMapID
            << " Pos:(" << pos.x << "," << pos.y << "," << pos.z << ")");

        if (!iTreeValues || iNTreeValues == 0)
        {
            LOG_DEBUG("No tree values - no area info");
            return false;
        }

        // Define the callback class for area info collection
        class AreaInfoCallback
        {
        public:
            AreaInfoCallback(ModelInstance* val) : prims(val), checkedModels(0), modelsWithArea(0) {}

            void operator()(const G3D::Vector3& point, uint32_t entry)
            {
                LOG_TRACE("AreaInfoCallback checking model at index " << entry);

                if (!prims[entry].iModel)
                {
                    LOG_DEBUG("Model at index " << entry << " not loaded - skipping");
                    return;
                }

                checkedModels++;

                // Make a copy of the point since intersectPoint may modify it
                G3D::Vector3 testPoint = point;
                prims[entry].intersectPoint(testPoint, aInfo);

                if (aInfo.result)
                {
                    modelsWithArea++;
                    LOG_DEBUG("Model " << entry << " has area info"
                        << " Flags:" << std::hex << aInfo.flags << std::dec
                        << " AdtId:" << aInfo.adtId
                        << " RootId:" << aInfo.rootId
                        << " GroupId:" << aInfo.groupId
                        << " GroundZ:" << aInfo.ground_Z
                        << " Name:" << prims[entry].name);
                }
            }

            ModelInstance* prims;
            AreaInfo aInfo;
            int checkedModels;
            int modelsWithArea;
        };

        // Create callback and use BIH tree to find intersections
        AreaInfoCallback intersectionCallback(iTreeValues);
        iTree.intersectPoint(pos, intersectionCallback);

        LOG_DEBUG("Area check complete - Checked:" << intersectionCallback.checkedModels
            << " WithArea:" << intersectionCallback.modelsWithArea);

        if (intersectionCallback.aInfo.result)
        {
            flags = intersectionCallback.aInfo.flags;
            adtId = intersectionCallback.aInfo.adtId;
            rootId = intersectionCallback.aInfo.rootId;
            groupId = intersectionCallback.aInfo.groupId;
            pos.z = intersectionCallback.aInfo.ground_Z;

            LOG_INFO("Area info found - Final Z:" << pos.z
                << " Flags:0x" << std::hex << flags << std::dec
                << " AdtId:" << adtId
                << " RootId:" << rootId
                << " GroupId:" << groupId);
            return true;
        }

        LOG_TRACE("StaticMapTree::getAreaInfo EXIT - Not found");
        return false;
    }

    bool StaticMapTree::GetLocationInfo(const G3D::Vector3& pos, LocationInfo& info) const
    {
        LOG_TRACE("StaticMapTree::GetLocationInfo ENTER - MapId:" << iMapID
            << " Pos:(" << pos.x << "," << pos.y << "," << pos.z << ")");

        if (!iTreeValues || iNTreeValues == 0)
        {
            LOG_DEBUG("No tree values - no location info");
            return false;
        }

        int checkedModels = 0;

        for (uint32_t i = 0; i < iNTreeValues; ++i)
        {
            if (iTreeValues[i].iModel)
            {
                checkedModels++;

                if (iTreeValues[i].GetLocationInfo(pos, info))
                {
                    LOG_INFO("Location info found in model " << i
                        << " RootId:" << info.rootId
                        << " GroundZ:" << info.ground_Z
                        << " Name:" << iTreeValues[i].name);
                    return true;
                }
            }
        }

        LOG_DEBUG("No location info found after checking " << checkedModels << " models");
        LOG_TRACE("StaticMapTree::GetLocationInfo EXIT - Not found");
        return false;
    }

    bool StaticMapTree::getIntersectionTime(const G3D::Ray& ray, float& maxDist,
        bool stopAtFirstHit, bool ignoreM2Model) const
    {
        LOG_TRACE("StaticMapTree::getIntersectionTime ENTER"
            << " MaxDist:" << maxDist
            << " StopAtFirst:" << stopAtFirstHit
            << " IgnoreM2:" << ignoreM2Model);

        if (!iTreeValues || iNTreeValues == 0)
        {
            LOG_DEBUG("No tree values - no intersection possible");
            return false;
        }

        int testedModels = 0;
        int hitModels = 0;

        auto intersectCallback = [this, ignoreM2Model, &testedModels, &hitModels](const G3D::Ray& r, uint32_t idx,
            float& d, bool stopAtFirst, bool ignoreM2) -> bool
            {
                if (!iTreeValues || idx >= iNTreeValues)
                {
                    LOG_ERROR("Invalid tree index: " << idx << " (max:" << iNTreeValues << ")");
                    return false;
                }

                // Only test models that are actually loaded
                if (!iTreeValues[idx].iModel)
                {
                    LOG_TRACE("Model at index " << idx << " not loaded - skipping");
                    return false;
                }

                testedModels++;
                float oldDist = d;
                bool hit = iTreeValues[idx].intersectRay(r, d, stopAtFirst, ignoreM2);

                if (hit)
                {
                    hitModels++;
                    LOG_DEBUG("Model " << idx << " HIT at distance " << d
                        << " (was " << oldDist << ") Name:" << iTreeValues[idx].name);
                }

                return hit;
            };

        float oldDist = maxDist;

        LOG_DEBUG("Starting BIH tree traversal - Initial maxDist:" << maxDist);

        iTree.intersectRay(ray, intersectCallback, maxDist, stopAtFirstHit, ignoreM2Model);

        bool hasHit = maxDist < oldDist;

        LOG_INFO("Intersection result - TestedModels:" << testedModels
            << " HitModels:" << hitModels
            << " FinalDist:" << maxDist
            << " HasHit:" << hasHit);

        LOG_TRACE("StaticMapTree::getIntersectionTime EXIT - Hit:" << hasHit);
        return hasHit;
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