#include "Navigation.h"
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