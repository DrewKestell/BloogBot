// wow/vmap/GroupModel_Raw.h
#pragma once
#include "WorldModel.h"

namespace wow::vmap
{
    /** Thin wrapper that stores placement matrix & a pointer to shared WorldModel */
    struct GroupModel_Raw
    {
        const WorldModel* model;
        Mat4              world;     ///< placement (from WMO root)
        AABox             boundsWS;  ///< bbox in world space
    };
}
