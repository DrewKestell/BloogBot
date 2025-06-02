#include "Navigation.h"
#include "MoveMap.h"
#include "PathFinder.h"
#include <vector>
#include <iostream>
#include <fstream>
#include <filesystem>
#include <stdio.h>

#include "DetourCommon.h"
using namespace std;

EXTERN_C IMAGE_DOS_HEADER __ImageBase;

Navigation* Navigation::s_singletonInstance = NULL;

Navigation* Navigation::GetInstance()
{
	if (s_singletonInstance == NULL)
		s_singletonInstance = new Navigation();
	return s_singletonInstance;
}

void Navigation::Initialize()
{
	dtAllocSetCustom(dtCustomAlloc, dtCustomFree);
}

void Navigation::Release()
{
	MMAP::MMapFactory::createOrGetMMapManager()->~MMapManager();
}

void Navigation::FreePathArr(XYZ* pathArr)
{
	delete[] pathArr;
}

XYZ* Navigation::CalculatePath(unsigned int mapId, XYZ start, XYZ end, bool straightPath, int* length)
{
	MMAP::MMapManager* manager = MMAP::MMapFactory::createOrGetMMapManager();

	InitializeMapsForContinent(manager, mapId);

	PathFinder pathFinder(mapId, 1);
	pathFinder.setUseStrightPath(straightPath);
	pathFinder.calculate(start.X, start.Y, start.Z, end.X, end.Y, end.Z);

	PointsArray pointPath = pathFinder.getPath();
	*length = pointPath.size();
	XYZ* pathArr = new XYZ[pointPath.size()];

	for (unsigned int i = 0; i < pointPath.size(); i++)
	{
		pathArr[i].X = pointPath[i].x;
		pathArr[i].Y = pointPath[i].y;
		pathArr[i].Z = pointPath[i].z;
	}

	return pathArr;
}

bool Navigation::RaycastToWmoMesh(unsigned int mapId, float startX, float startY, float startZ, float endX, float endY, float endZ, float* hitX, float* hitY, float* hitZ)
{
	MMAP::MMapManager* manager = MMAP::MMapFactory::createOrGetMMapManager();
	InitializeMapsForContinent(manager, mapId);

	const dtNavMesh* mesh = manager->GetNavMesh(mapId);
	const dtNavMeshQuery* query = manager->GetNavMeshQuery(mapId, 0);

	if (!mesh || !query)
	{
		printf("[Raycast] Failed to acquire mesh or query for mapId %u\n", mapId);
		return false;
	}

	float start[3] = { startX, startZ, startY };
	float end[3] = { endX, endZ, endY };
	float extents[3];
	dtQueryFilter filter;
	filter.setIncludeFlags(0xFFFF);

	const float expansions[][3] = {
		{0.5f, 10.0f, 0.5f},
		{1.0f, 20.0f, 1.0f},
		{1.5f, 40.0f, 1.5f},
		{2.0f, 80.0f, 2.0f},
		{2.5f, 160.0f, 2.5f}
	};

	dtPolyRef startRef = 0;
	float nearest[3] = { 0.0f, 0.0f, 0.0f };
	dtStatus nearestStatus = 0;

	for (const auto& e : expansions)
	{
		memcpy(extents, e, sizeof(extents));

		nearestStatus = query->findNearestPoly(start, extents, &filter, &startRef, nearest);
		if (!dtStatusFailed(nearestStatus) && startRef != 0)
			break;
	}

	if (dtStatusFailed(nearestStatus) || startRef == 0)
	{
		return false;
	}

	dtRaycastHit hit;
	dtStatus rayStatus = query->raycast(startRef, start, end, &filter, DT_RAYCAST_USE_COSTS, &hit);
	if (dtStatusFailed(rayStatus))
	{
		return false;
	}

	if (hit.t >= 1.0f || hit.t < 0.0f || isnan(hit.t))
	{
		return false;
	}

	float intersection[3];
	dtVlerp(intersection, start, end, hit.t);

	*hitX = intersection[0];
	*hitY = intersection[2];
	*hitZ = intersection[1];

	return true;
}


void Navigation::InitializeMapsForContinent(MMAP::MMapManager* manager, unsigned int mapId)
{
	if (!manager->zoneMap.contains(mapId))
	{
		for (auto& p : std::filesystem::directory_iterator(Navigation::GetMmapsPath()))
		{
			string path = p.path().string();
			string extension = path.substr(path.find_last_of(".") + 1);
			if (extension == "mmtile")
			{
				string filename = path.substr(path.find_last_of("\\") + 1);

				int xTens = filename[3] - '0';
				int xOnes = filename[4] - '0';
				int yTens = filename[5] - '0';
				int yOnes = filename[6] - '0';

				int x = (xTens * 10) + xOnes;
				int y = (yTens * 10) + yOnes;

				std::string mapIdString;
				if (mapId < 10)
					mapIdString = "00" + std::to_string(mapId);
				else if (mapId < 100)
					mapIdString = "0" + std::to_string(mapId);
				else
					mapIdString = std::to_string(mapId);

				if (filename[0] == mapIdString[0] && filename[1] == mapIdString[1] && filename[2] == mapIdString[2])
					manager->loadMap(mapId, x, y);
			}
		}

		manager->zoneMap.insert(std::pair<unsigned int, bool>(mapId, true));
	}
}

string Navigation::GetMmapsPath()
{
	WCHAR DllPath[MAX_PATH] = { 0 };
	GetModuleFileNameW((HINSTANCE)&__ImageBase, DllPath, _countof(DllPath));
	wstring ws(DllPath);
	string pathAndFile(ws.begin(), ws.end());
	char* c = const_cast<char*>(pathAndFile.c_str());
	int strLength = strlen(c);
	int lastOccur = 0;
	for (int i = 0; i < strLength; i++)
	{
		if (c[i] == '\\') lastOccur = i;
	}
	string pathToMmap = pathAndFile.substr(0, lastOccur + 1);
	pathToMmap = pathToMmap.append("mmaps\\");

	return pathToMmap;
}