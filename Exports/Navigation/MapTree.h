// wow/vmap/MapTree.h
#pragma once
#include <vector>
#include "Math.h"
#include "GroupModel_Raw.h"
#include "BIH.h"

namespace wow::vmap
{
    /** A collection of WorldModels making up a VMAP tile or whole map. */
    class MapTree
    {
    public:
        MapTree();
        void   addModel(const WorldModel* wm, const Mat4& placement);
        void   finalize();                                ///< build top-level BIH

        bool   isInLineOfSight(const Vec3& p, const Vec3& q) const;
        float  getHeight(const Vec3& pos, float maxSearchDist = 200.f) const;
        bool   getObjectHitPos(const Vec3& p, const Vec3& q,
            Vec3& out, float padding = 0.f) const;

        const AABox& bounds() const { return _bounds; }

    private:
        std::vector<GroupModel_Raw> _models;
        std::vector<BIH::Triangle>  _bihTris;
        BIH                         _bih;         ///< built from model bounds
        AABox                       _bounds;
        bool                        _finalized = false;
    };
}
