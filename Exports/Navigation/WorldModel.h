// wow/vmap/WorldModel.h
#pragma once
#include <vector>
#include <string>
#include "Math.h"
#include "BIH.h"
#include "VMapDefinitions.h"

namespace wow::vmap
{
    /** A single static model (.vmo) with its own BIH */
    class WorldModel
    {
    public:
        explicit WorldModel(const std::string& vmoPath);

        const BIH& bih()    const { return _bih; }
        ModelFlags   flags()  const { return _flags; }
        const AABox& bounds() const { return _bounds; }

    private:
        void loadVmo(const std::string& path);

        std::vector<BIH::Triangle> _tris;
        BIH                         _bih;
        AABox                       _bounds;
        ModelFlags                  _flags = ModelFlags::MOD_WMO;
    };
}
