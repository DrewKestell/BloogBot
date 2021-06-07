#include "Navigation.h"
#include "MoveMap.h"
#include "PathFinder.h"
#include <vector>
#include <iostream>
#include <fstream>
#include <filesystem>
#include <stdio.h>

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

void Navigation::InitializeMapsForContinent(MMAP::MMapManager* manager, unsigned int mapId)
{
    if ((mapId == 0 && !manager->hasLoadedEasternContinent) || (mapId == 1 && !manager->hasLoadedWesternContinent))
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

                if (filename[0] == '0' && filename[1] == '0' && filename[2] == std::to_string(mapId)[0])
                    manager->loadMap(mapId, x, y);
            }
        }
    }

    if (mapId == 0)
        manager->hasLoadedEasternContinent = true;
    if (mapId == 1)
        manager->hasLoadedWesternContinent = true;
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