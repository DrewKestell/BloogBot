// VMapManager2.cpp — enhanced diagnostics for file I/O and model loading
// Based on original at :contentReference[oaicite:0]{index=0}

#include "VMapManager2.h"
#include "WorldModel.h"
#include <filesystem>
#include <fstream>
#include <iostream>
#include <stdio.h>
#include <algorithm>   // for std::replace
#include <cerrno>      // for errno
#include <cstring>     // for std::strerror

using namespace std;
using namespace wow::vmap;

EXTERN_C IMAGE_DOS_HEADER __ImageBase;

VMapManager2& VMapManager2::instance()
{
    static VMapManager2 s;
    return s;
}

static std::filesystem::path GetVMapTilePath(uint32 mapId, int x, int y)
{
    char name[32];
    std::snprintf(name, sizeof(name), "%03u_%02d_%02d.vmtile", mapId, x, y);

    // get module folder
    wchar_t dllPathW[MAX_PATH]{};
    GetModuleFileNameW((HINSTANCE)&__ImageBase, dllPathW, MAX_PATH);
    std::wstring ws(dllPathW);
    std::string dllPath(ws.begin(), ws.end());
    auto pos = dllPath.find_last_of("\\/");
    if (pos != std::string::npos)
        dllPath.resize(pos + 1);

    // append vmaps/
    std::string vmapsDir = dllPath + "vmaps/";
    return std::filesystem::path(vmapsDir) / name;
}

//------------------------------------------------------------------------------
// Load one .vmtile, with full diagnostics
bool VMapManager2::loadMapTile(uint32 mapId, int tileX, int tileY)
{
    // get path and convert to narrow string for printf/fopen
    const auto   fullPath = GetVMapTilePath(mapId, tileX, tileY);
    const string fp = fullPath.string();

    std::fprintf(stderr, "[VMap] Attempting open: %s\n", fp.c_str());

    FILE* f = std::fopen(fp.c_str(), "rb");
    if (!f)
    {
        int err = errno;
        std::fprintf(stderr,
            "[VMap] failed to open \"%s\" (errno=%d: %s)\n",
            fp.c_str(), err, std::strerror(err));
        return false;  // server‐style silent miss
    }

    std::printf("[VMap] + %s\n", fp.c_str());

    // read header
    uint32 nModels = 0;
    if (std::fread(&nModels, sizeof(nModels), 1, f) != 1)
    {
        std::fprintf(stderr, "[VMap] bad header in \"%s\"\n", fp.c_str());
        std::fclose(f);
        return false;
    }
    std::printf("[VMap] header: nModels=%u\n", nModels);

    // skip empty tiles
    if (nModels == 0)
    {
        std::printf("[VMap] tile %03u_%02d_%02d has no models, skipping\n",
            mapId, tileX, tileY);
        std::fclose(f);
        return true;
    }

    // ensure MapTree exists
    auto& mapNode = _maps[mapId];
    if (!mapNode.tree)
    {
        std::printf("[VMap] creating MapTree for map %u\n", mapId);
        mapNode.tree = std::make_unique<MapTree>();
    }

    static unordered_map<string, unique_ptr<WorldModel>> cache;
    // use the header‐declared GetVmapsRoot()
    string vmapsRoot = GetVmapsRootString() + "/";

    // iterate models
    for (uint32 i = 0; i < nModels; ++i)
    {
        // read relative‐path length
        uint32 len = 0;
        if (std::fread(&len, sizeof(len), 1, f) != 1 || len == 0 || len > 4096)
        {
            std::fprintf(stderr,
                "[VMap] model %u: bad path length %u in \"%s\"\n",
                i, len, fp.c_str());
            std::fclose(f);
            return false;
        }

        // read relative‐path
        string rel(len, '\0');
        if (std::fread(rel.data(), 1, len, f) != len)
        {
            std::fprintf(stderr,
                "[VMap] model %u: failed to read rel path in \"%s\"\n",
                i, fp.c_str());
            std::fclose(f);
            return false;
        }
        rel.resize(len);
        std::printf("[VMap] model %u rel path: '%s'\n", i, rel.c_str());

        // read transform matrix
        float mat[16];
        if (std::fread(mat, sizeof(mat), 1, f) != 1)
        {
            std::fprintf(stderr,
                "[VMap] model %u: matrix read fail in \"%s\"\n",
                i, fp.c_str());
            std::fclose(f);
            return false;
        }

        // load WorldModel
        string vmoPath = vmapsRoot + rel;
        std::printf("[VMap] model %u vmo file: '%s'\n", i, vmoPath.c_str());
        auto& wmPtr = cache[vmoPath];
        if (!wmPtr)
        {
            std::printf("[VMap] loading WorldModel from '%s'\n", vmoPath.c_str());
            wmPtr = make_unique<WorldModel>(vmoPath);
        }

        mapNode.tree->addModel(wmPtr.get(), Mat4(mat));
    }

    std::fclose(f);

    // rebuild BIH
    std::printf("[VMap] rebuilding BIH for tile %03u_%02d_%02d (%u models)\n",
        mapId, tileX, tileY, nModels);
    mapNode.tree->finalize();

    return true;
}


void VMapManager2::unloadMap(uint32 mapId)
{
    _maps.erase(mapId);
}

bool VMapManager2::isInLineOfSight(uint32 id,
    float x1, float y1, float z1,
    float x2, float y2, float z2) const
{
    auto it = _maps.find(id);
    if (it == _maps.end()) return true;
    return it->second.tree->isInLineOfSight({ x1,y1,z1 }, { x2,y2,z2 });
}

float VMapManager2::getHeight(uint32 id,
    float x, float y, float z,
    float maxDist) const
{
    auto it = _maps.find(id);
    if (it == _maps.end()) return VMAP_INVALID_HEIGHT;
    return it->second.tree->getHeight({ x,y,z }, maxDist);
}

bool VMapManager2::getObjectHitPos(uint32 id,
    float x1, float y1, float z1,
    float x2, float y2, float z2,
    float& hx, float& hy, float& hz,
    float padding) const
{
    auto it = _maps.find(id);
    if (it == _maps.end()) return false;
    Vec3 out;
    if (!it->second.tree->getObjectHitPos({ x1,y1,z1 }, { x2,y2,z2 }, out, padding))
        return false;
    hx = out.x; hy = out.y; hz = out.z;
    return true;
}
std::string VMapManager2::GetVmapsRootString()
{
    wchar_t dllPathW[MAX_PATH]{};
    GetModuleFileNameW((HINSTANCE)&__ImageBase, dllPathW, MAX_PATH);
    std::wstring ws(dllPathW);
    std::string path(ws.begin(), ws.end());
    auto pos = path.find_last_of("\\/");
    if (pos != string::npos)
        path.resize(pos + 1);
    path += "vmaps/";
    return path;
}
