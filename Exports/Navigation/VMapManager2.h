// VMapManager2.h
#pragma once

#include "IVMapManager.h"
#include <unordered_map>
#include <unordered_set>
#include <memory>
#include <shared_mutex>
#include <string>
#include "Vector3.h"

namespace VMAP
{
    class StaticMapTree;
    class WorldModel;
    class ModelInstance;

    typedef std::unordered_map<uint32_t, StaticMapTree*> InstanceTreeMap;
    typedef std::unordered_map<std::string, std::shared_ptr<WorldModel>> ModelFileMap;

    class VMapManager2 : public IVMapManager
    {
    protected:
        // Tree to check collision
        ModelFileMap iLoadedModelFiles;
        InstanceTreeMap iInstanceMapTrees;
        std::unordered_set<uint32_t> iLoadedMaps;
        std::string iBasePath;

        bool _loadMap(uint32_t pMapId, const std::string& basePath, uint32_t tileX, uint32_t tileY);

        mutable std::shared_mutex m_modelsLock;

    public:
        // public for debug
        G3D::Vector3 convertPositionToInternalRep(float x, float y, float z) const;
        static std::string getMapFileName(unsigned int pMapId);

        VMapManager2();
        ~VMapManager2();

        // Map management
        void setBasePath(const std::string& path);

        void initializeMap(uint32_t mapId);
        bool isMapInitialized(uint32_t mapId) const {
            return iLoadedMaps.count(mapId) > 0;
        }

        // IVMapManager interface implementation
        VMAPLoadResult loadMap(const char* pBasePath, unsigned int pMapId, int x, int y) override;
        void unloadMap(unsigned int pMapId, int x, int y) override;
        void unloadMap(unsigned int pMapId) override;

        bool isInLineOfSight(unsigned int pMapId, float x1, float y1, float z1,
            float x2, float y2, float z2, bool ignoreM2Model) override;
        ModelInstance* FindCollisionModel(unsigned int mapId, float x0, float y0, float z0,
            float x1, float y1, float z1) override;
        bool getObjectHitPos(unsigned int pMapId, float x1, float y1, float z1,
            float x2, float y2, float z2,
            float& rx, float& ry, float& rz, float pModifyDist) override;
        float getHeight(unsigned int pMapId, float x, float y, float z, float maxSearchDist) override;

        bool processCommand(char* /*pCommand*/) override { return false; }

        bool getAreaInfo(unsigned int pMapId, float x, float y, float& z,
            uint32_t& flags, int32_t& adtId, int32_t& rootId, int32_t& groupId) const override;
        bool isUnderModel(unsigned int pMapId, float x, float y, float z,
            float* outDist = nullptr, float* inDist = nullptr) const override;
        bool GetLiquidLevel(uint32_t pMapId, float x, float y, float z,
            uint8_t ReqLiquidTypeMask, float& level, float& floor, uint32_t& type) const override;

        std::shared_ptr<WorldModel> acquireModelInstance(const std::string& basepath, const std::string& filename);
    };
}