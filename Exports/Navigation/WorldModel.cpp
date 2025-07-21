// wow/vmap/WorldModel.cpp
#include "WorldModel.h"
#include <fstream>
#include <cstring>
#include "Log.h"

using namespace wow::vmap;

namespace
{
    template<typename T>
    void read(std::ifstream& f, T& out) { f.read(reinterpret_cast<char*>(&out), sizeof(T)); }

    struct RawHeader { char magic[4]; uint32 nGroups; uint32 ofsGroups; };
    struct RawGroup { uint32 nTris;  uint32 ofsTris; AABox box; };
    struct RawTri { Vec3 v[3]; };
}

WorldModel::WorldModel(const std::string& vmoPath)
    : _bih(_tris, 8)   // dummy init; will rebuild after load
{
    loadVmo(vmoPath);
    _bih = BIH(_tris, 8);
    for (auto& t : _tris) _bounds.expand(t.v[0]), _bounds.expand(t.v[1]), _bounds.expand(t.v[2]);
}

void WorldModel::loadVmo(const std::string& path)
{
    std::ifstream f(path, std::ios::binary);
    if (!f) { LogWarn("[VMap] cannot open %s\n", path.c_str()); return; }

    RawHeader hdr; read(f, hdr);
    if (std::strncmp(hdr.magic, "VMOD", 4) != 0) { LogError("[VMap] bad header in %s\n", path.c_str()); return; }

    // read group table
    std::vector<RawGroup> groups(hdr.nGroups);
    f.seekg(hdr.ofsGroups, std::ios::beg);
    f.read(reinterpret_cast<char*>(groups.data()), sizeof(RawGroup) * hdr.nGroups);

    // triangles
    for (const auto& g : groups)
    {
        f.seekg(g.ofsTris, std::ios::beg);
        for (uint32 i = 0; i < g.nTris; ++i)
        {
            RawTri rt; read(f, rt);
            BIH::Triangle tri;
            std::memcpy(tri.v, rt.v, sizeof(rt.v));
            tri.id = static_cast<uint32>(_tris.size());
            _tris.push_back(tri);
        }
    }
}
