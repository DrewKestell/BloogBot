// wow/vmap/BIH.h
#pragma once

#include <vector>
#include <limits>
#include "Math.h"
#include "Assert.h"
#include "Define.h"

namespace wow::vmap
{
    /** Axis-aligned bounding box */
    struct AABox
    {
        Vec3 min, max;
        AABox() :
            min(std::numeric_limits<float>::max(),
                std::numeric_limits<float>::max(),
                std::numeric_limits<float>::max()),
            max(-std::numeric_limits<float>::max(),
                -std::numeric_limits<float>::max(),
                -std::numeric_limits<float>::max()) {
        }

        void expand(const Vec3& p) { min = min.min(p); max = max.max(p); }
        void expand(const AABox& other) { expand(other.min); expand(other.max); }

        bool contains(const Vec3& p) const
        {
            return (p.x >= min.x && p.x <= max.x &&
                p.y >= min.y && p.y <= max.y &&
                p.z >= min.z && p.z <= max.z);
        }
        
        bool rayIntersect(const Vec3& o,
            const Vec3& invDir,
            const int   dirIsNeg[3],
            float& tMin, float& tMax) const
        {
            const float t1 = ((dirIsNeg[0] ? max.x : min.x) - o.x) * invDir.x;
            const float t2 = ((dirIsNeg[0] ? min.x : max.x) - o.x) * invDir.x;
            const float t3 = ((dirIsNeg[1] ? max.y : min.y) - o.y) * invDir.y;
            const float t4 = ((dirIsNeg[1] ? min.y : max.y) - o.y) * invDir.y;
            const float t5 = ((dirIsNeg[2] ? max.z : min.z) - o.z) * invDir.z;
            const float t6 = ((dirIsNeg[2] ? min.z : max.z) - o.z) * invDir.z;

            tMin = std::max({ tMin, std::min(t1, t2), std::min(t3, t4), std::min(t5, t6) });
            tMax = std::min({ tMax, std::max(t1, t2), std::max(t3, t4), std::max(t5, t6) });
            return tMax >= tMin;
        }
    };

    /** A BIH node: interior (left/right children) or leaf (triangle range) */
    struct BIHNode
    {
        // Interior:
        uint32  leftChild;     ///< index of left child (right = leftChild+1)
        uint8   axis;          ///< split axis: 0=x,1=y,2=z, 3=leaf
        float   split;         ///< split coordinate along axis

        // Leaf:
        uint32  firstTri;      ///< index of first triangle in leaf
        uint32  triCount;      ///< number of triangles in leaf

        BIHNode() : leftChild(0), axis(3), split(0), firstTri(0), triCount(0) {}
        bool isLeaf() const { return axis == 3; }
    };

    /** Bounding Interval Hierarchy built from (indexed) triangles */
    class BIH
    {
    public:
        struct Triangle
        {
            Vec3 v[3];
            uint32 id;           ///< arbitrary user index
        };

        explicit BIH(const std::vector<Triangle>& tris, uint32 leafSize = 6);

        size_t triangleCount() const { return _tris.size(); }

        // Ray cast: returns true if hit, t = distance (rayParam) and normal.
        bool intersectRay(const Vec3& origin,
            const Vec3& dir,
            float& outT,
            Vec3& outNormal) const;

        const AABox& bounds() const { return _world; }

    private:
        void build(uint32 nodeIdx,
            uint32 begin,
            uint32 end,
            uint8  depth);

        bool traverse(uint32 nodeIdx,
            const Vec3& origin,
            const Vec3& invDir,
            const int    dirIsNeg[3],
            float& tMin,
            float& tMax,
            float& outT,
            Vec3& outN) const;

        std::vector<BIHNode>      _nodes;
        std::vector<Triangle>     _tris;
        AABox                      _world;
        uint32                     _leafSize;
    };
} // namespace wow::vmap
