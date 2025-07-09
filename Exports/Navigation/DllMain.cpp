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
    // NEW: returns true and writes the floor-Z into outHeight,
    // or false if no valid ground poly was found.
    __declspec(dllexport) bool GetNavmeshFloorHeight(uint32_t mapId,
        float     posX,
        float     posY,
        float     zGuess,
        float* outHeight)
    {
        std::cout << "[GetNavmeshFloorHeight] Enter: mapId=" << mapId
            << " pos=(" << posX << "," << posY << "," << zGuess << ")\n";

        if (!outHeight)
        {
            std::cout << "[GetNavmeshFloorHeight] ERROR: outHeight pointer is null\n";
            return false;
        }

        // (Maps already initialized externally.)

        // 1) Grab or create the MMapManager
        auto* mgr = MMAP::MMapFactory::createOrGetMMapManager();
        std::cout << "[GetNavmeshFloorHeight] MMapManager ptr=" << mgr << "\n";

        // 2) Get the nav-mesh query
        auto* query = mgr->GetNavMeshQuery(mapId, 0);
        std::cout << "[GetNavmeshFloorHeight] NavMeshQuery ptr=" << query << "\n";
        if (!query)
        {
            std::cout << "[GetNavmeshFloorHeight] ERROR: NavMeshQuery is null\n";
            return false;
        }

        // 3) Build our Detour-space point: [X]=worldY, [Y]=worldZ+0.5, [Z]=worldX
        float point[3] = { posY, zGuess + 0.5f, posX };
        std::cout << "[GetNavmeshFloorHeight] point (Detour coords): ("
            << point[0] << "," << point[1] << "," << point[2] << ")\n";

        // 4) Prepare search extents: ±3m X/Z, ±5m vertical
        float extents[3] = { 3.0f, 5.0f, 3.0f };
        std::cout << "[GetNavmeshFloorHeight] initial extents: ("
            << extents[0] << "," << extents[1] << "," << extents[2] << ")\n";

        // 5) Find nearest ground polygon, with fallback to taller box
        dtQueryFilter filter;
        filter.setIncludeFlags(0x01);  // DT_POLYAREA_GROUND
        filter.setExcludeFlags(0x00);

        dtPolyRef polyRef = 0;
        float nearest[3] = { 0,0,0 };
        dtStatus st = query->findNearestPoly(point, extents, &filter, &polyRef, nearest);
        std::cout << "[GetNavmeshFloorHeight] findNearestPoly status=" << st
            << " polyRef=" << polyRef
            << " nearest=(X=" << nearest[0]
            << ",Y=" << nearest[1]
            << ",Z=" << nearest[2] << ")\n";

        if (dtStatusFailed(st) || polyRef == 0)
        {
            // fallback: increase vertical extent to cover large drops
            extents[1] = 200.0f;
            std::cout << "[GetNavmeshFloorHeight] fallback extents: ("
                << extents[0] << "," << extents[1] << "," << extents[2] << ")\n";
            st = query->findNearestPoly(point, extents, &filter, &polyRef, nearest);
            std::cout << "[GetNavmeshFloorHeight] fallback findNearestPoly status=" << st
                << " polyRef=" << polyRef
                << " nearest=(X=" << nearest[0]
                << ",Y=" << nearest[1]
                << ",Z=" << nearest[2] << ")\n";
            if (dtStatusFailed(st) || polyRef == 0)
            {
                std::cout << "[GetNavmeshFloorHeight] No valid poly found after fallback, abort\n";
                return false;
            }
        }

        std::cout << "[GetNavmeshFloorHeight] Found polyRef=" << polyRef << "\n";

        // 6) Project our point onto that polygon to get exact floor height
        float closest[3] = { 0,0,0 };
        dtStatus cst = query->closestPointOnPoly(polyRef, point, closest, nullptr);
        std::cout << "[GetNavmeshFloorHeight] closestPointOnPoly status=" << cst
            << " closest=(X=" << closest[0]
            << ",Y=" << closest[1]
            << ",Z=" << closest[2] << ")\n";
        if (dtStatusFailed(cst))
        {
            std::cout << "[GetNavmeshFloorHeight] ERROR: closestPointOnPoly failed\n";
            return false;
        }

        // 7) closest[1] is the vertical axis (WoW-Z)
        *outHeight = closest[1];
        std::cout << "[GetNavmeshFloorHeight] outHeight set to " << *outHeight << "\n";
        return true;
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