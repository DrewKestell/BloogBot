// wow/vmap/BIH.cpp
#include "BIH.h"
#include <algorithm>
#include <stack>

using namespace wow::vmap;

namespace
{
    struct TriBox
    {
        AABox box;
        uint32 idx;
    };
}

/* ---------- BIH constructor ------------------------------------------------ */

BIH::BIH(const std::vector<Triangle>& tris, uint32 leaf)
    : _tris(tris), _leafSize(std::max<uint32>(1, leaf))
{
    // Compute per-triangle bounds & world bounds
    std::vector<TriBox> tb(tris.size());
    for (size_t i = 0; i < tris.size(); ++i)
    {
        AABox b;
        b.expand(tris[i].v[0]);
        b.expand(tris[i].v[1]);
        b.expand(tris[i].v[2]);
        tb[i] = { b, static_cast<uint32>(i) };
        _world.expand(b);
    }

    // Build in place – node 0 = root
    _nodes.resize(tris.size() * 2);      // safe upper bound
    _nodes[0] = BIHNode{};
    build(0, 0, static_cast<uint32>(tb.size()), 0);

    // reorder triangles in leaf order for cache locality
}

/* ---------- recursive build ----------------------------------------------- */

void BIH::build(uint32 nodeIdx, uint32 begin, uint32 end, uint8 depth)
{
    BIHNode& n = _nodes[nodeIdx];
    const uint32 count = end - begin;

    if (count <= _leafSize)
    {
        n.axis = 3;           // leaf
        n.firstTri = begin;
        n.triCount = count;
        return;
    }

    // choose longest axis
    AABox bounds;
    for (uint32 i = begin; i < end; ++i) bounds.expand(_tris[i].v[0]); // rough
    const Vec3 extent = bounds.max - bounds.min;
    n.axis = wow::vmap::maxAxis(extent);

    // partition around median
    const float mid = bounds.min[n.axis] + extent[n.axis] * 0.5f;
    auto midIt = std::partition(_tris.begin() + begin,
        _tris.begin() + end,
        [&](const Triangle& t)
        {
            float c = (t.v[0][n.axis] +
                t.v[1][n.axis] +
                t.v[2][n.axis]) / 3.0f;
            return c < mid;
        });

    const uint32 midIdx = static_cast<uint32>(midIt - _tris.begin());
    if (midIdx == begin || midIdx == end)
    {
        // partition failed – fallback to equal split
        n.split = mid;
        n.leftChild = static_cast<uint32>(_nodes.size());
        _nodes.emplace_back();
        _nodes.emplace_back();
        build(n.leftChild, begin, begin + count / 2, depth + 1);
        build(n.leftChild + 1, begin + count / 2, end, depth + 1);
        return;
    }

    n.split = mid;
    n.leftChild = static_cast<uint32>(_nodes.size());
    _nodes.emplace_back();
    _nodes.emplace_back();
    build(n.leftChild, begin, midIdx, depth + 1);
    build(n.leftChild + 1, midIdx, end, depth + 1);
}

bool BIH::intersectRay(const Vec3& o,
    const Vec3& dir,
    float& outT,
    Vec3& outN) const
{
    Vec3 invDir = wow::vmap::reciprocal(dir);
    int  dirIsNeg[3] = { dir.x < 0, dir.y < 0, dir.z < 0 };

    float tEnter = 0.f, tExit = std::numeric_limits<float>::max();
    if (!_world.rayIntersect(o, invDir, dirIsNeg, tEnter, tExit))
        return false;

    outT = tExit;
    return traverse(0, o, invDir, dirIsNeg, tEnter, tExit, outT, outN);
}
/* ---------- ray / triangle helpers ---------------------------------------- */

static inline bool rayTri(const Vec3& o, const Vec3& d,
    const Vec3& a, const Vec3& b, const Vec3& c,
    float& t, Vec3& n)
{
    const Vec3 ab = b - a;
    const Vec3 ac = c - a;
    n = ab.cross(ac);
    float det = -d.dot(n);
    if (std::abs(det) < 1e-6f) return false;

    Vec3 ao = o - a;
    Vec3 dao = ao.cross(d);

    float u = ao.dot(n) / det;
    float v = -d.dot(dao) / det;
    float w = ao.dot(dao) / det;

    if (u < 0 || v < 0 || w < 0) return false;
    t = ac.dot(dao) / det;
    return t >= 0;
}

/* ---------- ray traversal -------------------------------------------------- */

bool BIH::traverse(uint32 nodeIdx,
    const Vec3& o,
    const Vec3& invDir,
    const int    dirIsNeg[3],
    float& tMin,
    float& tMax,
    float& outT,
    Vec3& outN) const
{
    const BIHNode& n = _nodes[nodeIdx];

    if (n.isLeaf())
    {
        bool hit = false;
        for (uint32 i = 0; i < n.triCount; ++i)
        {
            const Triangle& tri = _tris[n.firstTri + i];
            float t; Vec3 normal;
            if (rayTri(o, normal, tri.v[0], tri.v[1], tri.v[2], t, normal))
                if (t < outT) { outT = t; outN = normal; hit = true; }
        }
        return hit;
    }

    const float tPlane = (n.split - o[n.axis]) * invDir[n.axis];
    const uint32 nearIdx = dirIsNeg[n.axis] ? n.leftChild + 1 : n.leftChild;
    const uint32 farIdx = nearIdx ^ 1;

    bool hit = false;
    float tNear = tMin, tFar = std::min(tMax, tPlane);
    if (tPlane >= tMin)
        hit |= traverse(nearIdx, o, invDir, dirIsNeg, tNear, tFar, outT, outN);

    tNear = std::max(tMin, tPlane); tFar = tMax;
    if (tPlane <= tMax && (!hit || tPlane < outT))
        hit |= traverse(farIdx, o, invDir, dirIsNeg, tNear, tFar, outT, outN);

    return hit;
}
