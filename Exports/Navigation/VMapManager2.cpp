// VMapManager2.cpp - Fixed to properly find and load all VMap models
#include "VMapManager2.h"
#include "StaticMapTree.h"
#include "WorldModel.h"
#include "VMapDefinitions.h"
#include <sstream>
#include <iomanip>
#include <filesystem>
#include <iostream>
#include <fstream>
#include <algorithm>
#include <unordered_map>
#include "VMapLog.h"

namespace VMAP
{
    // Global model name to path mapping
    static std::unordered_map<std::string, std::string> modelNameToPath;
    static bool modelMappingLoaded = false;

    // Scan entire vmaps directory and build complete model mapping
    void BuildCompleteModelMapping(const std::string& basePath)
    {
        if (modelMappingLoaded)
            return;

        modelNameToPath.clear();

        // Helper function to add a model to our mapping
        auto addModel = [](const std::filesystem::path& path) {
            std::string fullPath = path.string();
            std::string filename = path.filename().string();

            // Normalize path separators
            std::replace(fullPath.begin(), fullPath.end(), '\\', '/');

            // Store with multiple key variations for robust lookup
            std::string lowerName = filename;
            std::transform(lowerName.begin(), lowerName.end(), lowerName.begin(), ::tolower);
            modelNameToPath[lowerName] = fullPath;

            // Also store without extension
            size_t dotPos = lowerName.find_last_of('.');
            if (dotPos != std::string::npos)
            {
                std::string nameNoExt = lowerName.substr(0, dotPos);
                modelNameToPath[nameNoExt] = fullPath;

                // Store with different extensions for lookup
                modelNameToPath[nameNoExt + ".wmo"] = fullPath;
                modelNameToPath[nameNoExt + ".m2"] = fullPath;
                modelNameToPath[nameNoExt + ".mdx"] = fullPath;
                modelNameToPath[nameNoExt + ".mdl"] = fullPath;
            }

            // Also store original case
            modelNameToPath[filename] = fullPath;
            };

        try
        {
            // Recursively scan entire vmaps directory
            for (const auto& entry : std::filesystem::recursive_directory_iterator(basePath))
            {
                if (entry.is_regular_file())
                {
                    std::string ext = entry.path().extension().string();
                    std::transform(ext.begin(), ext.end(), ext.begin(), ::tolower);

                    if (ext == ".vmo")
                    {
                        addModel(entry.path());
                    }
                }
            }

            // Enhanced breakdown by type
            int vmoCount = 0, dtreeCount = 0;
            for (const auto& pair : modelNameToPath) {
                if (pair.second.find("GameObjectModels") != std::string::npos) dtreeCount++;
                else vmoCount++;
            }

            // Also try to load GameObjectModels.dtree if it exists
            std::string dtreeFile = basePath + "GameObjectModels.dtree";
            if (std::filesystem::exists(dtreeFile))
            {
                FILE* rf = fopen(dtreeFile.c_str(), "rb");
                if (rf)
                {
                    char magic[8];
                    if (fread(magic, 1, 8, rf) == 8)
                    {
                        uint32_t numModels;
                        if (fread(&numModels, sizeof(uint32_t), 1, rf) == 1)
                        {
                            for (uint32_t i = 0; i < numModels; ++i)
                            {
                                uint32_t fileId;
                                uint32_t nameLen;

                                if (fread(&fileId, sizeof(uint32_t), 1, rf) != 1) break;
                                if (fread(&nameLen, sizeof(uint32_t), 1, rf) != 1) break;

                                if (nameLen > 0 && nameLen < 512)
                                {
                                    std::vector<char> nameBuff(nameLen + 1, 0);
                                    if (fread(nameBuff.data(), 1, nameLen, rf) == nameLen)
                                    {
                                        std::string modelName(nameBuff.data());

                                        // Try to find the corresponding .vmo file
                                        std::stringstream ss;
                                        ss << basePath << "GameObjectModels/"
                                            << std::setfill('0') << std::setw(8) << fileId << ".vmo";

                                        std::string vmoPath = ss.str();
                                        if (std::filesystem::exists(vmoPath))
                                        {
                                            // Clean up model name
                                            size_t lastSlash = modelName.find_last_of("/\\");
                                            if (lastSlash != std::string::npos)
                                                modelName = modelName.substr(lastSlash + 1);

                                            std::string lowerName = modelName;
                                            std::transform(lowerName.begin(), lowerName.end(), lowerName.begin(), ::tolower);

                                            modelNameToPath[lowerName] = vmoPath;
                                            modelNameToPath[modelName] = vmoPath;

                                            // Also without extension
                                            size_t dotPos = lowerName.find_last_of('.');
                                            if (dotPos != std::string::npos)
                                            {
                                                modelNameToPath[lowerName.substr(0, dotPos)] = vmoPath;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    fclose(rf);
                }
            }
        }
        catch (const std::exception& e)
        {
            std::cerr << "Error building model mapping: " << e.what();
        }

        modelMappingLoaded = true;
    }

    // Resolve model name to actual file path
    std::string ResolveModelPath(const std::string& basePath, const std::string& modelName)
    {
        // Ensure mapping is built
        if (!modelMappingLoaded)
        {
            BuildCompleteModelMapping(basePath);
        }

        // Clean up the model name
        std::string searchName = modelName;

        // Remove any path components
        size_t lastSlash = searchName.find_last_of("/\\");
        if (lastSlash != std::string::npos)
            searchName = searchName.substr(lastSlash + 1);

        // Try lowercase lookup first
        std::string lowerName = searchName;
        std::transform(lowerName.begin(), lowerName.end(), lowerName.begin(), ::tolower);

        auto it = modelNameToPath.find(lowerName);
        if (it != modelNameToPath.end())
        {
            if (std::filesystem::exists(it->second))
            {
                //LOG_DEBUG("Resolved " << modelName << " to " << it->second);
                return it->second;
            }
        }

        // Try original case
        it = modelNameToPath.find(searchName);
        if (it != modelNameToPath.end())
        {
            if (std::filesystem::exists(it->second))
            {
                //LOG_DEBUG("Resolved " << modelName << " to " << it->second);
                return it->second;
            }
        }

        // Try without extension
        size_t dotPos = lowerName.find_last_of('.');
        if (dotPos != std::string::npos)
        {
            std::string nameNoExt = lowerName.substr(0, dotPos);
            it = modelNameToPath.find(nameNoExt);
            if (it != modelNameToPath.end())
            {
                if (std::filesystem::exists(it->second))
                {
                    //LOG_DEBUG("Resolved " << modelName << " to " << it->second);
                    return it->second;
                }
            }
        }

        // Last resort - try direct paths
        std::vector<std::string> tryPaths = {
            basePath + searchName,
            basePath + lowerName,
            basePath + "GameObjectModels/" + searchName,
            basePath + "GameObjectModels/" + lowerName
        };

        // If it's a .wmo or .m2, try with .vmo extension
        if (dotPos != std::string::npos)
        {
            std::string nameNoExt = searchName.substr(0, dotPos);
            tryPaths.push_back(basePath + nameNoExt + ".vmo");
            tryPaths.push_back(basePath + "GameObjectModels/" + nameNoExt + ".vmo");
        }

        for (const auto& path : tryPaths)
        {
            if (std::filesystem::exists(path))
            {
                //LOG_DEBUG("Found via direct path: " << path);
                return path;
            }
        }

        return "";
    }

    // Constructor
    VMapManager2::VMapManager2()
    {

    }

    // Destructor
    VMapManager2::~VMapManager2()
    {
        try
        {
            for (auto& pair : iInstanceMapTrees)
            {
                delete pair.second;
            }
            iInstanceMapTrees.clear();
            iLoadedModelFiles.clear();
            iLoadedMaps.clear();
        }
        catch (const std::exception& e)
        {
            std::cerr << "Exception in destructor: " << e.what();
        }
    }

    void VMapManager2::setBasePath(const std::string& path)
    {
        iBasePath = path;
        if (!iBasePath.empty() && iBasePath.back() != '/' && iBasePath.back() != '\\')
            iBasePath += "/";

        // Build the complete model mapping when base path is set
        BuildCompleteModelMapping(iBasePath);
    }

    void VMapManager2::initializeMap(uint32_t mapId)
    {
        LOG_INFO("==================== VMapManager2::initializeMap START ====================");
        LOG_INFO("Initializing map " << mapId);

        try
        {
            if (iLoadedMaps.count(mapId) > 0)
            {
                LOG_DEBUG("Map already in loaded set");
                LOG_INFO("==================== VMapManager2::initializeMap END (already loaded) ====================");
                return;
            }

            std::string mapFileName = getMapFileName(mapId);
            std::string fullPath = iBasePath + mapFileName;

            LOG_INFO("Looking for map file: " << mapFileName);
            LOG_INFO("Full path: " << fullPath);

            if (!std::filesystem::exists(fullPath))
            {
                LOG_WARN("Map file does not exist - map has no VMAP data");
                LOG_INFO("==================== VMapManager2::initializeMap END (no file) ====================");
                return;
            }

            // Get file size
            auto fileSize = std::filesystem::file_size(fullPath);
            LOG_INFO("Map file size: " << fileSize << " bytes");

            // Quick check if file is readable
            FILE* rf = fopen(fullPath.c_str(), "rb");
            if (!rf)
            {
                LOG_ERROR("Cannot open map file! Error: " << strerror(errno));
                LOG_INFO("==================== VMapManager2::initializeMap END (cannot open) ====================");
                return;
            }
            fclose(rf);
            LOG_DEBUG("Map file is readable");

            StaticMapTree* newTree = nullptr;
            try
            {
                LOG_DEBUG("Creating StaticMapTree for map " << mapId);
                newTree = new StaticMapTree(mapId, iBasePath);

                LOG_DEBUG("Initializing tree from file...");
                if (newTree->InitMap(mapFileName, this))
                {
                    LOG_INFO("Tree initialization successful");
                    LOG_INFO("  Is tiled: " << (newTree->isTiled() ? "YES" : "NO"));

                    iInstanceMapTrees[mapId] = newTree;
                    iLoadedMaps.insert(mapId);

                    LOG_INFO("Map " << mapId << " added to loaded maps set");
                }
                else
                {
                    LOG_ERROR("Tree initialization failed!");
                    delete newTree;
                }
            }
            catch (const std::bad_alloc& e)
            {
                LOG_ERROR("Bad allocation while initializing map " << mapId << ": " << e.what());
                delete newTree;
                throw;
            }
            catch (const std::exception& e)
            {
                LOG_ERROR("Exception while initializing map " << mapId << ": " << e.what());
                delete newTree;
            }
        }
        catch (const std::bad_alloc& e)
        {
            LOG_ERROR("Memory allocation failed for map " << mapId << ": " << e.what());
            throw;
        }
        catch (const std::exception& e)
        {
            LOG_ERROR("Failed to initialize map " << mapId << ": " << e.what());
        }

        LOG_INFO("==================== VMapManager2::initializeMap END ====================");
    }

    bool VMapManager2::isUnderModel(unsigned int pMapId, float x, float y, float z,
        float* outDist, float* inDist) const
    {
        auto instanceTree = iInstanceMapTrees.find(pMapId);
        if (instanceTree != iInstanceMapTrees.end())
        {
            G3D::Vector3 pos = convertPositionToInternalRep(x, y, z);
            return instanceTree->second->isUnderModel(pos, outDist, inDist);
        }
        return false;
    }

    void VMapManager2::listAvailableVMapFiles() const
    {
        std::cout << "[VMAP] Searching for VMAP files in: " << iBasePath << std::endl;

        try
        {
            std::cout << "[VMAP] Available maps (.vmtree files):" << std::endl;
            for (const auto& entry : std::filesystem::directory_iterator(iBasePath))
            {
                if (entry.path().extension() == ".vmtree")
                {
                    std::cout << "  - " << entry.path().filename() << std::endl;
                }
            }

            std::cout << "[VMAP] Available tiles (.vmtile files):" << std::endl;
            int tileCount = 0;
            for (const auto& entry : std::filesystem::directory_iterator(iBasePath))
            {
                if (entry.path().extension() == ".vmtile")
                {
                    tileCount++;
                }
            }
            std::cout << "  Found " << tileCount << " tile files" << std::endl;

            std::cout << "[VMAP] Model mappings loaded: " << modelNameToPath.size() << std::endl;
        }
        catch (const std::exception& e)
        {
            std::cerr << "[VMAP] Error listing files: " << e.what() << std::endl;
        }
    }

    std::string VMapManager2::getMapFileName(unsigned int pMapId)
    {
        std::stringstream fname;
        fname << std::setfill('0') << std::setw(3) << pMapId << ".vmtree";
        return fname.str();
    }

    G3D::Vector3 VMapManager2::convertPositionToInternalRep(float x, float y, float z) const
    {
        float const mid = 0.5f * 64.0f * 533.33333333f;
        return G3D::Vector3(mid - x, mid - y, z);
    }

    VMAPLoadResult VMapManager2::loadMap(const char* pBasePath, unsigned int pMapId, int x, int y)
    {
        LOG_INFO("==================== VMapManager2::loadMap START ====================");
        LOG_INFO("loadMap called - Map:" << pMapId << " Tile:[" << x << "," << y << "]");
        LOG_INFO("Base path provided: " << (pBasePath ? pBasePath : "NULL"));

        try
        {
            // Update base path if provided
            if (pBasePath && strlen(pBasePath) > 0)
            {
                std::string oldPath = iBasePath;
                iBasePath = pBasePath;
                if (!iBasePath.empty() && iBasePath.back() != '/' && iBasePath.back() != '\\')
                    iBasePath += "/";

                LOG_INFO("Base path updated from: " << oldPath << " to: " << iBasePath);

                // Rebuild model mapping if path changed
                if (oldPath != iBasePath)
                {
                    LOG_DEBUG("Path changed - rebuilding model mapping");
                    BuildCompleteModelMapping(iBasePath);
                }
            }

            LOG_INFO("Current base path: " << iBasePath);

            // Check if base path exists
            if (!std::filesystem::exists(iBasePath))
            {
                LOG_ERROR("Base path does not exist: " << iBasePath);
                LOG_INFO("==================== VMapManager2::loadMap END (ERROR) ====================");
                return VMAP_LOAD_RESULT_ERROR;
            }

            // Check if map is initialized
            LOG_DEBUG("Checking if map " << pMapId << " is initialized...");
            if (!isMapInitialized(pMapId))
            {
                LOG_INFO("Map " << pMapId << " not initialized - attempting to initialize");

                try
                {
                    initializeMap(pMapId);

                    if (isMapInitialized(pMapId))
                    {
                        LOG_INFO("Map initialization successful");
                    }
                    else
                    {
                        LOG_WARN("Map initialization completed but map still not marked as initialized");
                    }
                }
                catch (const std::bad_alloc& e)
                {
                    LOG_ERROR("Failed to initialize map due to memory allocation failure: " << e.what());
                    LOG_INFO("==================== VMapManager2::loadMap END (ERROR) ====================");
                    return VMAP_LOAD_RESULT_ERROR;
                }
                catch (const std::exception& e)
                {
                    LOG_ERROR("Failed to initialize map: " << e.what());
                    LOG_INFO("==================== VMapManager2::loadMap END (ERROR) ====================");
                    return VMAP_LOAD_RESULT_ERROR;
                }
            }
            else
            {
                LOG_DEBUG("Map " << pMapId << " already initialized");
            }

            // Final check
            if (!isMapInitialized(pMapId))
            {
                LOG_WARN("Map " << pMapId << " could not be initialized - no vmtree file?");
                LOG_INFO("==================== VMapManager2::loadMap END (IGNORED) ====================");
                return VMAP_LOAD_RESULT_IGNORED;
            }

            // Call internal load function
            LOG_INFO("Calling _loadMap for actual tile loading...");
            bool result = _loadMap(pMapId, iBasePath, x, y);

            VMAPLoadResult loadResult = result ? VMAP_LOAD_RESULT_OK : VMAP_LOAD_RESULT_ERROR;

            LOG_INFO("Tile load result: " << (result ? "SUCCESS" : "FAILED"));
            LOG_INFO("==================== VMapManager2::loadMap END ("
                << (loadResult == VMAP_LOAD_RESULT_OK ? "OK" :
                    loadResult == VMAP_LOAD_RESULT_ERROR ? "ERROR" : "IGNORED")
                << ") ====================");

            return loadResult;
        }
        catch (const std::exception& e)
        {
            LOG_ERROR("Exception in loadMap: " << e.what());
            LOG_INFO("==================== VMapManager2::loadMap END (EXCEPTION) ====================");
            return VMAP_LOAD_RESULT_ERROR;
        }
        catch (...)
        {
            LOG_ERROR("Unknown exception in loadMap");
            LOG_INFO("==================== VMapManager2::loadMap END (EXCEPTION) ====================");
            return VMAP_LOAD_RESULT_ERROR;
        }
    }

    bool VMapManager2::_loadMap(uint32_t pMapId, const std::string& basePath, uint32_t tileX, uint32_t tileY)
    {
        LOG_INFO("==================== VMapManager2::_loadMap START ====================");
        LOG_INFO("_loadMap called - Map:" << pMapId << " Tile:[" << tileX << "," << tileY << "]");
        LOG_INFO("Base path: " << basePath);

        try
        {
            // Find or create the map tree
            auto instanceTree = iInstanceMapTrees.find(pMapId);

            if (instanceTree == iInstanceMapTrees.end())
            {
                LOG_INFO("Map tree not found in cache - creating new tree");

                std::string mapFileName = getMapFileName(pMapId);
                std::string fullPath = basePath + mapFileName;

                LOG_INFO("Map tree file: " << mapFileName);
                LOG_INFO("Full path: " << fullPath);

                // Check if file exists
                bool fileExists = std::filesystem::exists(fullPath);
                LOG_INFO("File exists: " << (fileExists ? "YES" : "NO"));

                if (!fileExists)
                {
                    LOG_ERROR("Map tree file does not exist!");
                    LOG_INFO("==================== VMapManager2::_loadMap END (no file) ====================");
                    return false;
                }

                // Get file size
                auto fileSize = std::filesystem::file_size(fullPath);
                LOG_INFO("File size: " << fileSize << " bytes");

                StaticMapTree* newTree = nullptr;
                try
                {
                    LOG_DEBUG("Creating new StaticMapTree instance...");
                    newTree = new StaticMapTree(pMapId, basePath);

                    LOG_DEBUG("Initializing map tree from file...");
                    if (!newTree->InitMap(mapFileName, this))
                    {
                        LOG_ERROR("Failed to initialize map tree!");
                        delete newTree;
                        LOG_INFO("==================== VMapManager2::_loadMap END (init failed) ====================");
                        return false;
                    }

                    LOG_INFO("Map tree initialized successfully");
                    LOG_INFO("Tree is tiled: " << (newTree->isTiled() ? "YES" : "NO"));
                    LOG_INFO("Number of loaded tiles: " << newTree->numLoadedTiles());

                    // Store in cache
                    iInstanceMapTrees[pMapId] = newTree;
                    instanceTree = iInstanceMapTrees.find(pMapId);

                    LOG_DEBUG("Map tree stored in cache");
                }
                catch (const std::bad_alloc& e)
                {
                    LOG_ERROR("Memory allocation failed while creating map tree: " << e.what());
                    delete newTree;
                    throw;
                }
                catch (const std::exception& e)
                {
                    LOG_ERROR("Exception while creating map tree: " << e.what());
                    delete newTree;
                    LOG_INFO("==================== VMapManager2::_loadMap END (exception) ====================");
                    return false;
                }
            }
            else
            {
                LOG_DEBUG("Map tree found in cache");
                LOG_DEBUG("Tree is tiled: " << (instanceTree->second->isTiled() ? "YES" : "NO"));
                LOG_DEBUG("Number of loaded tiles: " << instanceTree->second->numLoadedTiles());
            }

            // Now load the specific tile
            LOG_INFO("Calling LoadMapTile on StaticMapTree...");
            bool tileLoadResult = instanceTree->second->LoadMapTile(tileX, tileY, this);

            LOG_INFO("LoadMapTile returned: " << (tileLoadResult ? "SUCCESS" : "FAILED"));

            // Report final statistics
            LOG_INFO("Final tree statistics:");
            LOG_INFO("  Tiles loaded: " << instanceTree->second->numLoadedTiles());

            LOG_INFO("==================== VMapManager2::_loadMap END (result=" << tileLoadResult << ") ====================");
            return tileLoadResult;
        }
        catch (const std::exception& e)
        {
            LOG_ERROR("Exception in _loadMap: " << e.what());
            LOG_INFO("==================== VMapManager2::_loadMap END (exception) ====================");
            return false;
        }
        catch (...)
        {
            LOG_ERROR("Unknown exception in _loadMap");
            LOG_INFO("==================== VMapManager2::_loadMap END (exception) ====================");
            return false;
        }
    }

    void VMapManager2::unloadMap(unsigned int pMapId, int x, int y)
    {
        auto instanceTree = iInstanceMapTrees.find(pMapId);
        if (instanceTree != iInstanceMapTrees.end())
        {
            instanceTree->second->UnloadMapTile(x, y, this);
        }
    }

    void VMapManager2::unloadMap(unsigned int pMapId)
    {
        auto instanceTree = iInstanceMapTrees.find(pMapId);
        if (instanceTree != iInstanceMapTrees.end())
        {
            instanceTree->second->UnloadMap(this);
            delete instanceTree->second;
            iInstanceMapTrees.erase(instanceTree);
        }

        iLoadedMaps.erase(pMapId);
    }

    bool VMapManager2::isInLineOfSight(unsigned int pMapId, float x1, float y1, float z1,
        float x2, float y2, float z2, bool ignoreM2Model)
    {
        LOG_TRACE("VMapManager2::isInLineOfSight ENTER - Map:" << pMapId
            << " From:(" << x1 << "," << y1 << "," << z1 << ")"
            << " To:(" << x2 << "," << y2 << "," << z2 << ")"
            << " IgnoreM2:" << ignoreM2Model);

        if (!isLineOfSightCalcEnabled())
        {
            LOG_DEBUG("Line of sight calculation disabled globally");
            return true;
        }

        auto instanceTree = iInstanceMapTrees.find(pMapId);
        if (instanceTree != iInstanceMapTrees.end())
        {
            LOG_DEBUG("Found map tree for LOS check");

            G3D::Vector3 pos1 = convertPositionToInternalRep(x1, y1, z1);
            G3D::Vector3 pos2 = convertPositionToInternalRep(x2, y2, z2);

            LOG_VECTOR3("Internal pos1", pos1);
            LOG_VECTOR3("Internal pos2", pos2);

            bool result = instanceTree->second->isInLineOfSight(pos1, pos2, ignoreM2Model);

            LOG_INFO("LOS check result: " << (result ? "CLEAR" : "BLOCKED"));
            return result;
        }
        else
        {
            LOG_WARN("No map tree for LOS check - defaulting to clear");
        }

        LOG_TRACE("VMapManager2::isInLineOfSight EXIT - Default true");
        return true;
    }

    ModelInstance* VMapManager2::FindCollisionModel(unsigned int mapId, float x0, float y0, float z0,
        float x1, float y1, float z1)
    {
        auto instanceTree = iInstanceMapTrees.find(mapId);
        if (instanceTree != iInstanceMapTrees.end())
        {
            G3D::Vector3 pos1 = convertPositionToInternalRep(x0, y0, z0);
            G3D::Vector3 pos2 = convertPositionToInternalRep(x1, y1, z1);

            return instanceTree->second->FindCollisionModel(pos1, pos2);
        }

        return nullptr;
    }

    bool VMapManager2::getObjectHitPos(unsigned int pMapId, float x1, float y1, float z1,
        float x2, float y2, float z2,
        float& rx, float& ry, float& rz, float pModifyDist)
    {
        LOG_TRACE("VMapManager2::getObjectHitPos ENTER - Map:" << pMapId
            << " Ray from:(" << x1 << "," << y1 << "," << z1 << ")"
            << " to:(" << x2 << "," << y2 << "," << z2 << ")"
            << " ModifyDist:" << pModifyDist);

        auto instanceTree = iInstanceMapTrees.find(pMapId);
        if (instanceTree != iInstanceMapTrees.end())
        {
            G3D::Vector3 pos1 = convertPositionToInternalRep(x1, y1, z1);
            G3D::Vector3 pos2 = convertPositionToInternalRep(x2, y2, z2);
            G3D::Vector3 resultPos;

            LOG_VECTOR3("Internal start", pos1);
            LOG_VECTOR3("Internal end", pos2);

            bool result = instanceTree->second->getObjectHitPos(pos1, pos2, resultPos, pModifyDist);

            if (result)
            {
                // Convert back to world coordinates
                float const mid = 0.5f * 64.0f * 533.33333333f;
                rx = mid - resultPos.x;
                ry = mid - resultPos.y;
                rz = resultPos.z;

                LOG_INFO("Hit found at world pos: (" << rx << "," << ry << "," << rz << ")");
                LOG_VECTOR3("Hit internal pos", resultPos);
                return true;
            }
            else
            {
                LOG_DEBUG("No hit detected along ray");
            }
        }
        else
        {
            LOG_WARN("No map tree for collision check");
        }

        LOG_TRACE("VMapManager2::getObjectHitPos EXIT - No hit");
        return false;
    }

    float VMapManager2::getHeight(unsigned int pMapId, float x, float y, float z, float maxSearchDist)
    {
        LOG_TRACE("VMapManager2::getHeight ENTER - Map:" << pMapId
            << " Pos:(" << x << "," << y << "," << z << ")"
            << " MaxSearch:" << maxSearchDist);

        if (!isHeightCalcEnabled())
        {
            LOG_DEBUG("Height calculation disabled globally");
            return VMAP_INVALID_HEIGHT_VALUE;
        }

        auto instanceTree = iInstanceMapTrees.find(pMapId);
        if (instanceTree != iInstanceMapTrees.end())
        {
            LOG_DEBUG("Found map tree for map " << pMapId);

            // Convert to internal coordinates
            G3D::Vector3 pos = convertPositionToInternalRep(x, y, z);
            LOG_VECTOR3("Internal position", pos);

            // Query the static map tree
            float height = instanceTree->second->getHeight(pos, maxSearchDist);

            if (height > -G3D::inf() && height < G3D::inf())
            {
                LOG_INFO("Found height: " << height << " at (" << x << "," << y << "," << z << ")");
                return height;
            }
            else
            {
                LOG_DEBUG("No valid height found (inf/-inf result)");
            }
        }
        else
        {
            LOG_WARN("No map tree loaded for map " << pMapId);
        }

        LOG_TRACE("VMapManager2::getHeight EXIT - Returning INVALID");
        return VMAP_INVALID_HEIGHT_VALUE;
    }

    bool VMapManager2::getAreaInfo(unsigned int pMapId, float x, float y, float& z,
        uint32_t& flags, int32_t& adtId, int32_t& rootId, int32_t& groupId) const
    {
        LOG_TRACE("VMapManager2::getAreaInfo ENTER - Map:" << pMapId
            << " Pos:(" << x << "," << y << "," << z << ")");

        auto instanceTree = iInstanceMapTrees.find(pMapId);
        if (instanceTree != iInstanceMapTrees.end())
        {
            G3D::Vector3 pos = convertPositionToInternalRep(x, y, z);
            LOG_VECTOR3("Internal position", pos);

            bool result = instanceTree->second->getAreaInfo(pos, flags, adtId, rootId, groupId);

            if (result)
            {
                z = pos.z;
                LOG_INFO("Area info found - Flags:" << std::hex << flags << std::dec
                    << " AdtId:" << adtId << " RootId:" << rootId
                    << " GroupId:" << groupId << " NewZ:" << z);
                return true;
            }
            else
            {
                LOG_DEBUG("No area info at position");
            }
        }
        else
        {
            LOG_WARN("No map tree for area info query");
        }

        flags = 0;
        adtId = -1;
        rootId = -1;
        groupId = -1;

        LOG_TRACE("VMapManager2::getAreaInfo EXIT - Not found");
        return false;
    }

    bool VMapManager2::GetLiquidLevel(uint32_t pMapId, float x, float y, float z,
        uint8_t ReqLiquidTypeMask, float& level, float& floor, uint32_t& type) const
    {
        LOG_TRACE("VMapManager2::GetLiquidLevel ENTER - Map:" << pMapId
            << " Pos:(" << x << "," << y << "," << z << ")"
            << " ReqMask:" << std::hex << (int)ReqLiquidTypeMask << std::dec);

        auto instanceTree = iInstanceMapTrees.find(pMapId);
        if (instanceTree != iInstanceMapTrees.end())
        {
            G3D::Vector3 pos = convertPositionToInternalRep(x, y, z);
            LocationInfo info;

            LOG_VECTOR3("Internal position", pos);

            if (instanceTree->second->GetLocationInfo(pos, info))
            {
                LOG_DEBUG("Got location info - HitModel:" << (info.hitModel ? "YES" : "NO"));

                if (info.hitModel)
                {
                    float liqHeight;
                    if (info.hitInstance && info.hitInstance->GetLiquidLevel(pos, const_cast<LocationInfo&>(info), liqHeight))
                    {
                        uint32_t liquidType = info.hitModel->GetLiquidType();
                        uint32_t liquidMask = GetLiquidMask(liquidType);

                        LOG_DEBUG("Liquid found - Type:" << liquidType
                            << " Mask:" << std::hex << liquidMask << std::dec
                            << " Height:" << liqHeight);

                        if (ReqLiquidTypeMask & liquidMask)
                        {
                            level = liqHeight;
                            floor = info.ground_Z;
                            type = liquidType;

                            LOG_INFO("Liquid match - Level:" << level
                                << " Floor:" << floor << " Type:" << type);
                            return true;
                        }
                        else
                        {
                            LOG_DEBUG("Liquid type doesn't match requested mask");
                        }
                    }
                }
            }
        }
        else
        {
            LOG_WARN("No map tree for liquid query");
        }

        LOG_TRACE("VMapManager2::GetLiquidLevel EXIT - Not found");
        return false;
    }

    std::shared_ptr<WorldModel> VMapManager2::acquireModelInstance(const std::string& basepath, const std::string& filename)
    {
        try
        {
            std::lock_guard<std::shared_mutex> lock(m_modelsLock);

            auto it = iLoadedModelFiles.find(filename);
            if (it != iLoadedModelFiles.end())
            {
                return it->second;
            }

            // Ensure model mapping is built
            if (!modelMappingLoaded)
            {
                BuildCompleteModelMapping(basepath);
            }

            // Resolve the actual model file path
            std::string fullPath = ResolveModelPath(basepath, filename);

            if (fullPath.empty() || !std::filesystem::exists(fullPath))
            {
                return nullptr;
            }

            std::shared_ptr<WorldModel> worldmodel;
            try
            {
                worldmodel = std::make_shared<WorldModel>();
            }
            catch (const std::bad_alloc& e)
            {
                std::cerr << "Failed to allocate WorldModel: " << e.what();
                return nullptr;
            }

            if (!worldmodel->readFile(fullPath))
            {
                std::cerr << "Failed to read model file: " << fullPath;
                return nullptr;
            }

            //LOG_DEBUG("Successfully loaded model: " << filename << " from " << fullPath);
            iLoadedModelFiles[filename] = worldmodel;

            return worldmodel;
        }
        catch (const std::exception& e)
        {
            std::cerr << "Exception in acquireModelInstance: " << e.what();
            return nullptr;
        }
    }

} // namespace VMAP