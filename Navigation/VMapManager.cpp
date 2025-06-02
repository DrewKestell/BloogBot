#include "VMapManager.h"
#include <iostream>
#include <fstream>
#include <filesystem>
#include <windows.h>
#include <string>
#include <unordered_map>
#include <cmath>
#include <stdio.h>

#include "MapTree.h"
#include "VMapManager2.h"

using namespace VMAP;

EXTERN_C IMAGE_DOS_HEADER __ImageBase;

VMapManager::VMapManager(const std::string& dataDir)
    : _dataPath(dataDir)
{
}

VMapManager::~VMapManager()
{
    for (auto& entry : _loadedMaps)
    {
        if (entry.second)
        {
            entry.second->unloadMap(entry.first);
            delete entry.second;
        }
    }
    _loadedMaps.clear();
}

bool VMapManager::LoadMap(unsigned int mapId)
{
    if (_loadedMaps.find(mapId) != _loadedMaps.end())
        return true;

    VMapManager2* manager = new VMapManager2();
    _loadedMaps[mapId] = manager;
    return true;
}

bool VMapManager::LoadTile(unsigned int mapId, int tileX, int tileY)
{
    auto it = _loadedMaps.find(mapId);
    if (it == _loadedMaps.end())
    {
        if (!LoadMap(mapId))
            return false;
        it = _loadedMaps.find(mapId);
    }

    VMapManager2* manager = it->second;
    VMAPLoadResult result = manager->loadMap(_dataPath.c_str(), mapId, tileX, tileY);
    return result == VMAP_LOAD_RESULT_OK;
}

bool VMapManager::UnloadMap(unsigned int mapId)
{
    auto it = _loadedMaps.find(mapId);
    if (it != _loadedMaps.end())
    {
        it->second->unloadMap(mapId);
        delete it->second;
        _loadedMaps.erase(it);
        return true;
    }
    return false;
}

bool VMapManager::Raycast(unsigned int mapId,
    float startX, float startY, float startZ,
    float endX, float endY, float endZ,
    float& hitZ)
{
    auto it = _loadedMaps.find(mapId);
    if (it == _loadedMaps.end())
        return false;

    float rx, ry, rz;
    bool hit = it->second->getObjectHitPos(mapId, startX, startY, startZ, endX, endY, endZ, rx, ry, rz, -0.001f);
    if (hit)
    {
        hitZ = rz;
        return true;
    }

    return false;
}

std::string VMapManager::GetVmapsPath()
{
    WCHAR DllPath[MAX_PATH] = { 0 };
    GetModuleFileNameW((HINSTANCE)&__ImageBase, DllPath, _countof(DllPath));
    std::wstring ws(DllPath);
    std::string pathAndFile(ws.begin(), ws.end());
    size_t lastOccur = pathAndFile.find_last_of("\\");
    std::string pathToVmap = pathAndFile.substr(0, lastOccur + 1);
    pathToVmap += "vmaps\\";
    return pathToVmap;
}

void VMapManager::InitializeMapsForContinent(unsigned int mapId)
{
    if (_initializedMaps.contains(mapId))
        return;

    std::string basePath = GetVmapsPath();

    for (const auto& p : std::filesystem::directory_iterator(basePath))
    {
        std::string path = p.path().string();
        if (path.find(".vmap") == std::string::npos)
            continue;

        std::string filename = p.path().filename().string();

        std::string mapIdStr = (mapId < 10 ? "00" : mapId < 100 ? "0" : "") + std::to_string(mapId);
        if (filename.compare(0, 3, mapIdStr) == 0)
        {
            LoadMap(mapId);
            break;
        }
    }

    _initializedMaps.insert(mapId);
}