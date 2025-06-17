#include "Navigation.h"
#include "VMapManager2.h"
#include "Vec3Ray.h"
#include <windows.h>

extern "C"
{
    __declspec(dllexport) XYZ* CalculatePath(uint32_t mapId, XYZ start, XYZ end, bool straightPath, int* length)
    {
        if (!length) return nullptr;
        auto nav = Navigation::GetInstance();
        return nav ? nav->CalculatePath(mapId, start, end, straightPath, length) : nullptr;
    }

    __declspec(dllexport) void FreePathArr(XYZ* path)
    {
        if (!path) return;
        if (auto* nav = Navigation::GetInstance())
            nav->FreePathArr(path);
    }

    __declspec(dllexport) bool VMAP_IsInLineOfSight(uint32_t mapId, float x1, float y1, float z1, float x2, float y2, float z2)
    {
        auto* vmap = VMAP::VMapManager2::Instance();
        return vmap && vmap->isInLineOfSight(mapId, x1, y1, z1, x2, y2, z2, false);
    }

    __declspec(dllexport) float VMAP_GetHeight(uint32_t mapId, float x, float y, float z, float maxSearchDist)
    {
        auto* vmap = VMAP::VMapManager2::Instance();
        if (!vmap)
            return -std::numeric_limits<float>::infinity();

        float result = vmap->getHeight(mapId, x, y, z, maxSearchDist);
        return result > VMAP_INVALID_HEIGHT_VALUE ? result : -std::numeric_limits<float>::infinity();
    }

    __declspec(dllexport) bool VMAP_GetObjectHitPos(uint32_t mapId,
        float x1, float y1, float z1, float x2, float y2, float z2,
        float* hitX, float* hitY, float* hitZ, float modifyDist)
    {
        if (!hitX || !hitY || !hitZ) return false;
        auto* vmap = VMAP::VMapManager2::Instance();
        return vmap && vmap->getObjectHitPos(mapId, x1, y1, z1, x2, y2, z2, *hitX, *hitY, *hitZ, modifyDist);
    }

    __declspec(dllexport) bool VMAP_GetLocationInfo(uint32_t mapId, float x, float y, float z, float* groundZ)
    {
        if (!groundZ) return false;
        auto* vmap = VMAP::VMapManager2::Instance();
        if (!vmap) return false;

        VMAP::LocationInfo info;
        auto it = vmap->iInstanceMapTrees.find(mapId);
        if (it == vmap->iInstanceMapTrees.end() || !it->second) return false;

        Vec3 pos = vmap->convertPositionToInternalRep(x, y, z);
        if (!it->second->GetLocationInfo(pos, info)) return false;

        *groundZ = info.ground_Z;
        return true;
    }

    __declspec(dllexport) bool VMAP_GetAreaInfo(uint32_t mapId, float x, float y, float z, uint32_t* flags, int* adtId, int* rootId, int* groupId)
    {
        if (!flags || !adtId || !rootId || !groupId) return false;
        auto* vmap = VMAP::VMapManager2::Instance();
        return vmap && vmap->getAreaInfo(mapId, x, y, z, *flags, *adtId, *rootId, *groupId);
    }

    __declspec(dllexport) bool VMAP_GetLiquidLevel(uint32_t mapId, float x, float y, float z, uint8_t reqLiquidType, float* level, float* floor, uint32_t* type)
    {
        if (!level || !floor || !type) return false;
        auto* vmap = VMAP::VMapManager2::Instance();
        return vmap && vmap->GetLiquidLevel(mapId, x, y, z, reqLiquidType, *level, *floor, *type);
    }
}

BOOL APIENTRY DllMain(HMODULE hModule, DWORD ul_reason_for_call, LPVOID lpReserved)
{
    if (ul_reason_for_call == DLL_PROCESS_ATTACH)
    {
        if (auto* navigation = Navigation::GetInstance()) navigation->Initialize();
        if (auto* vmapManager = VMAP::VMapManager2::Instance()) vmapManager->Initialize();
    }
    else if (ul_reason_for_call == DLL_PROCESS_DETACH)
    {
        if (auto* navigation = Navigation::GetInstance()) navigation->Release();
    }
    return TRUE;
}