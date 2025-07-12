#include "Navigation.h"
#include <windows.h>
#include <iostream>

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

    __declspec(dllexport) bool GetNavmeshFloorHeight(uint32_t mapId,
        float     posX,
        float     posY,
        float     zGuess,
        float* outHeight)
    {
        if (!outHeight)
            return false;

        auto* mgr = MMAP::MMapFactory::createOrGetMMapManager();

        auto* query = mgr->GetNavMeshQuery(mapId, 0);
        if (!query)
            return false;

        float point[3] = { posY, zGuess + 0.5f, posX };

        float extents[3] = { 3.0f, 5.0f, 3.0f };

        dtQueryFilter filter;
        filter.setIncludeFlags(0x01);  // DT_POLYAREA_GROUND
        filter.setExcludeFlags(0x00);

        dtPolyRef polyRef = 0;
        float nearest[3] = { 0,0,0 };
        dtStatus st = query->findNearestPoly(point, extents, &filter, &polyRef, nearest);

        if (dtStatusFailed(st) || polyRef == 0)
        {
            extents[1] = 200.0f;

            st = query->findNearestPoly(point, extents, &filter, &polyRef, nearest);

            if (dtStatusFailed(st) || polyRef == 0)
                return false;
        }

        float closest[3] = { 0,0,0 };
        dtStatus cst = query->closestPointOnPoly(polyRef, point, closest, nullptr);
        if (dtStatusFailed(cst))
            return false;

        *outHeight = closest[1];
        return true;
    }

    // ----------------------------------------------------------------------------
    //  NEW 1/3 – Line of sight
    // ----------------------------------------------------------------------------
    __declspec(dllexport)
        bool LineOfSight(uint32_t mapId, XYZ from, XYZ to)
    {
        return Navigation::GetInstance()->IsLineOfSight(mapId, from, to);
    }

    // ----------------------------------------------------------------------------
    //  NEW 2/3 – Capsule overlap
    // ----------------------------------------------------------------------------
    __declspec(dllexport)
        NavPoly* CapsuleOverlap(uint32_t mapId, XYZ pos,
            float radius, float height,
            int* outCount)
    {
        if (!outCount) return nullptr;

        std::vector<NavPoly> v;
        try
        {
            v = Navigation::GetInstance()->CapsuleOverlap(mapId, pos, radius, height);
        }
        catch (const std::exception& ex)
        {
            return nullptr;
        }

        *outCount = static_cast<int>(v.size());

        if (v.empty()) return nullptr;

        size_t bytes = v.size() * sizeof(NavPoly);

        auto* buf = static_cast<NavPoly*>(::CoTaskMemAlloc(bytes));
        if (!buf) return nullptr;

        std::memcpy(buf, v.data(), bytes);
        return buf;
    }


    // ----------------------------------------------------------------------------
    //  NEW 3/3 – free() for CapsuleOverlap
    // ----------------------------------------------------------------------------
    __declspec(dllexport) void FreeNavPolyArr(NavPoly* p)
    {
        if (p) ::CoTaskMemFree(p);
    }
}

BOOL APIENTRY DllMain(HMODULE hModule, DWORD ul_reason_for_call, LPVOID lpReserved)
{
    if (ul_reason_for_call == DLL_PROCESS_ATTACH)
    {
        if (auto* navigation = Navigation::GetInstance()) navigation->Initialize();
    }
    else if (ul_reason_for_call == DLL_PROCESS_DETACH)
    {
        if (auto* navigation = Navigation::GetInstance()) navigation->Release();
    }
    return TRUE;
}