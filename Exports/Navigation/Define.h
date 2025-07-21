// wow/vmap/Define.h
#pragma once
#include <cstdint>

namespace wow::vmap
{
    using uint8 = std::uint8_t;
    using uint16 = std::uint16_t;
    using uint32 = std::uint32_t;
    using uint64 = std::uint64_t;

    // A sentinel used by GetHeight when nothing is hit.
    constexpr float VMAP_INVALID_HEIGHT = -100000.0f;

    // Map tile constants
    constexpr int   VMAP_COORDINATE_FACTOR = 0x10000;      // 65536
    constexpr float TILE_SIZE_YARDS = 533.333333f;  // classic cell size
}
