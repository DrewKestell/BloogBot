// wow/vmap/VMapDefinitions.h
#pragma once
#include "Define.h"

namespace wow::vmap
{
    enum class ModelFlags : uint8
    {
        MOD_M2 = 0x01,   // .m2 model
        MOD_WMO = 0x02,   // .wmo group
        MOD_HAS_BOUNDARY = 0x04    // has BIH built
    };

    // Stored in VMAP tile header
    struct WmoLiquidFlags
    {
        uint32 type : 8;       // 0 = water, 1 = ocean, 2 = magma, 3 = slime
        uint32 fishable : 1;   // classic fishing flag
        uint32 ­unused : 23;
    };
}
