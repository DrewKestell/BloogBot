#include "Navigation.h"

#include <windows.h>
#include <stdio.h>

extern "C"
{
	__declspec(dllexport) XYZ* CalculatePath(unsigned int mapId, XYZ start, XYZ end, bool parSmooth, int* length)
	{
		return Navigation::GetInstance()->CalculatePath(mapId, start, end, parSmooth, length);
	}

	__declspec(dllexport) XYZ* CalculatePathV2(unsigned int mapId, XYZ start, int startZoneID, XYZ end, int endZoneID, bool parSmooth, int* length)
	{
		return Navigation::GetInstance()->CalculatePath(mapId, start, startZoneID, end, endZoneID,  parSmooth, length);
	}

	__declspec(dllexport) void FreePathArr(XYZ* pathArr)
	{
		return Navigation::GetInstance()->FreePathArr(pathArr);
	}

	/*__declspec(dllexport) void GetPathArray(XYZ* path, int length)
	{
		Navigation::GetInstance()->GetPath(path, length);
	}*/
};

BOOL APIENTRY DllMain(HMODULE hModule, DWORD  ul_reason_for_call, LPVOID lpReserved)
{
	Navigation* navigation = Navigation::GetInstance();
	switch (ul_reason_for_call)
	{
		case DLL_PROCESS_ATTACH:
			//MessageBox(0, "proc attach", "proc attach", 0);
			navigation->Initialize();
			break;

		case DLL_PROCESS_DETACH:
			navigation->Release();
			break;

		case DLL_THREAD_ATTACH:
			break;

		case DLL_THREAD_DETACH:
			break;
	}
	return TRUE;
}
