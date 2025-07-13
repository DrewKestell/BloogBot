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

    __declspec(dllexport)
        bool LineOfSight(uint32_t mapId, XYZ from, XYZ to)
    {
        return Navigation::GetInstance()->IsLineOfSight(mapId, from, to);
    }

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