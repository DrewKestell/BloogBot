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

#define LOG_INFO(msg) std::cout << "[VMAP] " << msg << std::endl
#define LOG_ERROR(msg) std::cerr << "[VMAP ERROR] " << msg << std::endl
#define LOG_DEBUG(msg) std::cout << "[VMAP DEBUG] " << msg << std::endl

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

        LOG_INFO("Building complete model mapping from: " << basePath);
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

            LOG_INFO("Found " << modelNameToPath.size() << " model mappings");

            // Also try to load GameObjectModels.dtree if it exists
            std::string dtreeFile = basePath + "GameObjectModels.dtree";
            if (std::filesystem::exists(dtreeFile))
            {
                LOG_INFO("Loading additional mappings from GameObjectModels.dtree");
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

            LOG_INFO("Total model mappings after dtree: " << modelNameToPath.size());
        }
        catch (const std::exception& e)
        {
            LOG_ERROR("Error building model mapping: " << e.what());
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

        LOG_DEBUG("Could not resolve model path for: " << modelName);
        return "";
    }

    // Constructor
    VMapManager2::VMapManager2()
    {
        LOG_INFO("VMapManager2 constructor called");
    }

    // Destructor
    VMapManager2::~VMapManager2()
    {
        LOG_INFO("VMapManager2 destructor called");

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
            LOG_ERROR("Exception in destructor: " << e.what());
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
        try
        {
            if (iLoadedMaps.count(mapId) > 0)
            {
                LOG_DEBUG("Map " << mapId << " already initialized");
                return;
            }

            std::string mapFileName = getMapFileName(mapId);
            std::string fullPath = iBasePath + mapFileName;

            if (!std::filesystem::exists(fullPath))
            {
                LOG_DEBUG("Map file not found: " << fullPath);
                return;
            }

            FILE* rf = fopen(fullPath.c_str(), "rb");
            if (!rf)
            {
                LOG_ERROR("Failed to open map file: " << fullPath);
                return;
            }
            fclose(rf);

            StaticMapTree* newTree = nullptr;
            try
            {
                newTree = new StaticMapTree(mapId, iBasePath);

                if (newTree->InitMap(mapFileName, this))
                {
                    iInstanceMapTrees[mapId] = newTree;
                    iLoadedMaps.insert(mapId);
                    LOG_INFO("Initialized map " << mapId);
                }
                else
                {
                    delete newTree;
                    LOG_ERROR("Failed to initialize map " << mapId);
                }
            }
            catch (const std::bad_alloc& e)
            {
                delete newTree;
                LOG_ERROR("Bad allocation while initializing map " << mapId << ": " << e.what());
                throw;
            }
            catch (const std::exception& e)
            {
                delete newTree;
                LOG_ERROR("Exception while initializing map " << mapId << ": " << e.what());
            }
        }
        catch (const std::bad_alloc& e)
        {
            LOG_ERROR("Memory allocation failed for map " << mapId);
            throw;
        }
        catch (const std::exception& e)
        {
            LOG_ERROR("Failed to initialize map " << mapId << ": " << e.what());
        }
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
        try
        {
            LOG_DEBUG("loadMap called - Map: " << pMapId << " Tile: (" << x << ", " << y << ")");

            if (pBasePath && strlen(pBasePath) > 0)
            {
                iBasePath = pBasePath;
                if (!iBasePath.empty() && iBasePath.back() != '/' && iBasePath.back() != '\\')
                    iBasePath += "/";
                BuildCompleteModelMapping(iBasePath);
            }

            if (!std::filesystem::exists(iBasePath))
            {
                LOG_ERROR("Base path does not exist: " << iBasePath);
                return VMAP_LOAD_RESULT_ERROR;
            }

            if (!isMapInitialized(pMapId))
            {
                try
                {
                    initializeMap(pMapId);
                }
                catch (const std::bad_alloc& e)
                {
                    LOG_ERROR("Failed to initialize map due to memory allocation failure");
                    return VMAP_LOAD_RESULT_ERROR;
                }
            }

            if (!isMapInitialized(pMapId))
            {
                LOG_DEBUG("Map " << pMapId << " not available");
                return VMAP_LOAD_RESULT_IGNORED;
            }

            bool result = _loadMap(pMapId, iBasePath, x, y);

            return result ? VMAP_LOAD_RESULT_OK : VMAP_LOAD_RESULT_ERROR;
        }
        catch (const std::exception& e)
        {
            LOG_ERROR("Exception in loadMap: " << e.what());
            return VMAP_LOAD_RESULT_ERROR;
        }
    }

    bool VMapManager2::_loadMap(uint32_t pMapId, const std::string& basePath, uint32_t tileX, uint32_t tileY)
    {
        try
        {
            LOG_DEBUG("_loadMap internal - Map: " << pMapId << " Tile: (" << tileX << ", " << tileY << ")");

            auto instanceTree = iInstanceMapTrees.find(pMapId);

            if (instanceTree == iInstanceMapTrees.end())
            {
                std::string mapFileName = getMapFileName(pMapId);
                std::string fullPath = basePath + mapFileName;

                if (!std::filesystem::exists(fullPath))
                {
                    LOG_DEBUG("Map file does not exist: " << fullPath);
                    return false;
                }

                StaticMapTree* newTree = nullptr;
                try
                {
                    newTree = new StaticMapTree(pMapId, basePath);

                    if (!newTree->InitMap(mapFileName, this))
                    {
                        LOG_ERROR("Failed to init map tree for map " << pMapId);
                        delete newTree;
                        return false;
                    }

                    iInstanceMapTrees[pMapId] = newTree;
                    instanceTree = iInstanceMapTrees.find(pMapId);
                }
                catch (const std::bad_alloc& e)
                {
                    delete newTree;
                    LOG_ERROR("Memory allocation failed while loading map " << pMapId);
                    throw;
                }
            }

            return instanceTree->second->LoadMapTile(tileX, tileY, this);
        }
        catch (const std::exception& e)
        {
            LOG_ERROR("Exception in _loadMap: " << e.what());
            return false;
        }
    }

    void VMapManager2::unloadMap(unsigned int pMapId, int x, int y)
    {
        LOG_DEBUG("unloadMap tile - Map: " << pMapId << " Tile: (" << x << ", " << y << ")");

        auto instanceTree = iInstanceMapTrees.find(pMapId);
        if (instanceTree != iInstanceMapTrees.end())
        {
            instanceTree->second->UnloadMapTile(x, y, this);
        }
    }

    void VMapManager2::unloadMap(unsigned int pMapId)
    {
        LOG_DEBUG("unloadMap entire - Map: " << pMapId);

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
        if (!isLineOfSightCalcEnabled())
            return true;

        auto instanceTree = iInstanceMapTrees.find(pMapId);
        if (instanceTree != iInstanceMapTrees.end())
        {
            G3D::Vector3 pos1 = convertPositionToInternalRep(x1, y1, z1);
            G3D::Vector3 pos2 = convertPositionToInternalRep(x2, y2, z2);

            return instanceTree->second->isInLineOfSight(pos1, pos2, ignoreM2Model);
        }

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
        auto instanceTree = iInstanceMapTrees.find(pMapId);
        if (instanceTree != iInstanceMapTrees.end())
        {
            G3D::Vector3 pos1 = convertPositionToInternalRep(x1, y1, z1);
            G3D::Vector3 pos2 = convertPositionToInternalRep(x2, y2, z2);
            G3D::Vector3 resultPos;

            bool result = instanceTree->second->getObjectHitPos(pos1, pos2, resultPos, pModifyDist);

            if (result)
            {
                float const mid = 0.5f * 64.0f * 533.33333333f;
                rx = mid - resultPos.x;
                ry = mid - resultPos.y;
                rz = resultPos.z;
                return true;
            }
        }

        return false;
    }

    float VMapManager2::getHeight(unsigned int pMapId, float x, float y, float z, float maxSearchDist)
    {
        if (!isHeightCalcEnabled())
            return VMAP_INVALID_HEIGHT_VALUE;

        auto instanceTree = iInstanceMapTrees.find(pMapId);
        if (instanceTree != iInstanceMapTrees.end())
        {
            G3D::Vector3 pos = convertPositionToInternalRep(x, y, z);
            float height = instanceTree->second->getHeight(pos, maxSearchDist);

            if (height > -G3D::inf() && height < G3D::inf())
                return height;
        }

        return VMAP_INVALID_HEIGHT_VALUE;
    }

    bool VMapManager2::getAreaInfo(unsigned int pMapId, float x, float y, float& z,
        uint32_t& flags, int32_t& adtId, int32_t& rootId, int32_t& groupId) const
    {
        auto instanceTree = iInstanceMapTrees.find(pMapId);
        if (instanceTree != iInstanceMapTrees.end())
        {
            G3D::Vector3 pos = convertPositionToInternalRep(x, y, z);
            bool result = instanceTree->second->getAreaInfo(pos, flags, adtId, rootId, groupId);

            if (result)
            {
                z = pos.z;
                return true;
            }
        }

        flags = 0;
        adtId = -1;
        rootId = -1;
        groupId = -1;
        return false;
    }

    bool VMapManager2::GetLiquidLevel(uint32_t pMapId, float x, float y, float z,
        uint8_t ReqLiquidTypeMask, float& level, float& floor, uint32_t& type) const
    {
        auto instanceTree = iInstanceMapTrees.find(pMapId);
        if (instanceTree != iInstanceMapTrees.end())
        {
            G3D::Vector3 pos = convertPositionToInternalRep(x, y, z);
            LocationInfo info;

            if (instanceTree->second->GetLocationInfo(pos, info))
            {
                if (info.hitModel)
                {
                    float liqHeight;
                    if (info.hitInstance && info.hitInstance->GetLiquidLevel(pos, const_cast<LocationInfo&>(info), liqHeight))
                    {
                        uint32_t liquidType = info.hitModel->GetLiquidType();
                        uint32_t liquidMask = GetLiquidMask(liquidType);

                        if (ReqLiquidTypeMask & liquidMask)
                        {
                            level = liqHeight;
                            floor = info.ground_Z;
                            type = liquidType;
                            return true;
                        }
                    }
                }
            }
        }

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
                LOG_DEBUG("WMO model not found (may not be extracted): " << filename);
                return nullptr;
            }

            std::shared_ptr<WorldModel> worldmodel;
            try
            {
                worldmodel = std::make_shared<WorldModel>();
            }
            catch (const std::bad_alloc& e)
            {
                LOG_ERROR("Failed to allocate WorldModel: " << e.what());
                return nullptr;
            }

            if (!worldmodel->readFile(fullPath))
            {
                LOG_ERROR("Failed to read model file: " << fullPath);
                return nullptr;
            }

            //LOG_DEBUG("Successfully loaded model: " << filename << " from " << fullPath);
            iLoadedModelFiles[filename] = worldmodel;

            return worldmodel;
        }
        catch (const std::exception& e)
        {
            LOG_ERROR("Exception in acquireModelInstance: " << e.what());
            return nullptr;
        }
    }

} // namespace VMAP