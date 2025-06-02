#include "Navigation.h"
#include "VMapManager2.h"
#include <windows.h>
#include <iostream>

extern "C"
{
    __declspec(dllexport) XYZ* __cdecl CalculatePath(uint32_t mapId, XYZ start, XYZ end, bool straightPath, int* length)
    {
        return Navigation::GetInstance()->CalculatePath(mapId, start, end, straightPath, length);
    }

    __declspec(dllexport) void __cdecl FreePathArr(XYZ* path)
    {
        Navigation::GetInstance()->FreePathArr(path);
    }

    __declspec(dllexport) bool __cdecl VMAP_IsInLineOfSight(unsigned int mapId, float x1, float y1, float z1, float x2, float y2, float z2)
    {
        return VMAP::VMapManager2::Instance()->isInLineOfSight(mapId, x1, y1, z1, x2, y2, z2);
    }

    __declspec(dllexport) float __cdecl VMAP_GetHeight(unsigned int mapId, float x, float y, float z, float maxSearchDist)
    {
        return VMAP::VMapManager2::Instance()->getHeight(mapId, x, y, z, maxSearchDist);
    }

    __declspec(dllexport) bool __cdecl VMAP_GetObjectHitPos(unsigned int mapId,
        float x1, float y1, float z1,
        float x2, float y2, float z2,
        float* hitX, float* hitY, float* hitZ,
        float modifyDist)
    {

        auto* vmapInstance = VMAP::VMapManager2::Instance();
        if (!vmapInstance)
        {
            return false;
        }

        if (!hitX || !hitY || !hitZ)
        {
            return false;
        }

        bool result = false;
        try
        {
            result = vmapInstance->getObjectHitPos(mapId, x1, y1, z1, x2, y2, z2, *hitX, *hitY, *hitZ, modifyDist);
        }
        catch (...)
        {
            return false;
        }

        return result;
    }
    __declspec(dllexport) bool __cdecl VMAP_GetLocationInfo(unsigned int mapId, float x, float y, float z, float* groundZ)
    {
        if (!groundZ)
            return false;

        try
        {
            Vec3 pos = VMAP::VMapManager2::Instance()->convertPositionToInternalRep(x, y, z);
            VMAP::LocationInfo info;
            if (VMAP::VMapManager2::Instance()->iInstanceMapTrees.count(mapId))
            {
                auto tree = VMAP::VMapManager2::Instance()->iInstanceMapTrees.at(mapId);
                if (tree && tree->GetLocationInfo(pos, info))
                {
                    *groundZ = info.ground_Z;
                    return true;
                }
            }
        }
        catch (...)
        {
        }

        return false;
    }
    __declspec(dllexport) bool __cdecl VMAP_GetAreaInfo(unsigned int mapId, float x, float y, float z,
        unsigned int* flags, int* adtId, int* rootId, int* groupId)
    {
        return VMAP::VMapManager2::Instance()->getAreaInfo(mapId, x, y, z, *flags, *adtId, *rootId, *groupId);
    }

    __declspec(dllexport) bool __cdecl VMAP_GetLiquidLevel(unsigned int mapId, float x, float y, float z, uint8_t reqLiquidType,
        float* level, float* floor, unsigned int* type)
    {
        return VMAP::VMapManager2::Instance()->GetLiquidLevel(mapId, x, y, z, reqLiquidType, *level, *floor, *type);
    }
};

BOOL APIENTRY DllMain(HMODULE hModule, DWORD ul_reason_for_call, LPVOID lpReserved)
{
    Navigation* navigation = Navigation::GetInstance();
    VMAP::VMapManager2* vmapManager = VMAP::VMapManager2::Instance();

    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
        navigation->Initialize();
        vmapManager->Initialize();
        break;

    case DLL_PROCESS_DETACH:
        navigation->Release();
        vmapManager->Release();
        break;

    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
        break;
    }
    return TRUE;
}

