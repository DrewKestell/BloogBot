// IVMapManager.h - Interface for VMapManager
#pragma once

#include <string>
#include <cstdint>
#include "VMapDefinitions.h"

namespace VMAP
{
    class ModelInstance;

    //===========================================================
    class IVMapManager
    {
    private:
        bool iEnableLineOfSightCalc;
        bool iEnableHeightCalc;

    public:
        IVMapManager() : iEnableLineOfSightCalc(true), iEnableHeightCalc(true) {}

        virtual ~IVMapManager() {}

        // Map loading
        virtual VMAPLoadResult loadMap(const char* pBasePath, unsigned int pMapId, int x, int y) = 0;
        virtual void unloadMap(unsigned int pMapId, int x, int y) = 0;
        virtual void unloadMap(unsigned int pMapId) = 0;

        // Collision and height queries
        virtual bool isInLineOfSight(unsigned int pMapId, float x1, float y1, float z1,
            float x2, float y2, float z2, bool ignoreM2Model) = 0;
        virtual float getHeight(unsigned int pMapId, float x, float y, float z, float maxSearchDist) = 0;
        virtual bool getObjectHitPos(unsigned int pMapId, float x1, float y1, float z1,
            float x2, float y2, float z2,
            float& rx, float& ry, float& rz, float pModifyDist) = 0;
        virtual ModelInstance* FindCollisionModel(unsigned int mapId, float x0, float y0, float z0,
            float x1, float y1, float z1) = 0;

        // Area and liquid queries
        virtual bool getAreaInfo(unsigned int pMapId, float x, float y, float& z,
            uint32_t& flags, int32_t& adtId, int32_t& rootId, int32_t& groupId) const = 0;
        virtual bool isUnderModel(unsigned int pMapId, float x, float y, float z,
            float* outDist = nullptr, float* inDist = nullptr) const = 0;
        virtual bool GetLiquidLevel(uint32_t pMapId, float x, float y, float z,
            uint8_t ReqLiquidTypeMask, float& level, float& floor, uint32_t& type) const = 0;

        // Debug
        virtual bool processCommand(char* pCommand) = 0;

        // Configuration
        void setEnableLineOfSightCalc(bool pVal) { iEnableLineOfSightCalc = pVal; }
        void setEnableHeightCalc(bool pVal) { iEnableHeightCalc = pVal; }

        bool isLineOfSightCalcEnabled() const { return iEnableLineOfSightCalc; }
        bool isHeightCalcEnabled() const { return iEnableHeightCalc; }
        bool isMapLoadingEnabled() const { return iEnableLineOfSightCalc || iEnableHeightCalc; }
    };
}