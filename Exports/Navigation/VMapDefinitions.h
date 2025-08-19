// VMapDefinitions.h - Complete VMAP definitions
#pragma once

#include "PhysicsEngine.h"
#include <cstdint>
#include <cstdio>
#include <cstring>
#include <string>

namespace VMAP
{
    // vMaNGOS VMAP format for WoW 1.12.1
    constexpr char VMAP_MAGIC[] = "VMAP_7.0";

    // Simple helpers
    inline bool IsValidHeight(float h) { return h > PhysicsConstants::INVALID_HEIGHT; }

    constexpr float LIQUID_TILE_SIZE = (533.333f / 128.f);

    enum VMAPLoadResult
    {
        VMAP_LOAD_RESULT_ERROR,
        VMAP_LOAD_RESULT_OK,
        VMAP_LOAD_RESULT_IGNORED,
    };

    enum ModelFlags
    {
        MOD_M2 = 1,
        MOD_WORLDSPAWN = 1 << 1,
        MOD_HAS_BOUND = 1 << 2,
        MOD_NO_BREAK_LOS = 1 << 3
    };

    // Helper functions
    inline bool readChunk(FILE* rf, char* dest, const char* compare, uint32_t len)
    {
        if (fread(dest, 1, len, rf) != len) return false;
        return memcmp(dest, compare, len) == 0;
    }

    inline uint32_t floatToRawIntBits(float f)
    {
        union { uint32_t ival; float fval; } temp;
        temp.fval = f;
        return temp.ival;
    }

    inline float intBitsToFloat(uint32_t i)
    {
        union { uint32_t ival; float fval; } temp;
        temp.ival = i;
        return temp.fval;
    }

    enum LiquidTypeMask
    {
        MAP_LIQUID_TYPE_NO_WATER = 0x00,
        MAP_LIQUID_TYPE_MAGMA = 0x01,
        MAP_LIQUID_TYPE_OCEAN = 0x02,
        MAP_LIQUID_TYPE_SLIME = 0x04,
        MAP_LIQUID_TYPE_WATER = 0x08,
        MAP_LIQUID_TYPE_DARK_WATER = 0x10,
        MAP_LIQUID_TYPE_ALL_LIQUIDS = 0xFF
    };

    inline uint32_t GetLiquidMask(uint32_t liquidType)
    {
        switch (liquidType)
        {
        case 0: return MAP_LIQUID_TYPE_WATER;
        case 1: return MAP_LIQUID_TYPE_OCEAN;
        case 2: return MAP_LIQUID_TYPE_MAGMA;
        case 3: return MAP_LIQUID_TYPE_SLIME;
        default: return MAP_LIQUID_TYPE_WATER;
        }
    }

    // File name helpers
    inline std::string getMapFileName(uint32_t mapId)
    {
        char buffer[256];
        snprintf(buffer, sizeof(buffer), "%03u.vmtree", mapId);
        return std::string(buffer);
    }

    inline std::string getTileFileName(uint32_t mapId, uint32_t tileX, uint32_t tileY)
    {
        char buffer[256];
        snprintf(buffer, sizeof(buffer), "%03u_%02u_%02u.vmtile", mapId, tileX, tileY);
        return std::string(buffer);
    }

    // Coordinate conversion
    inline float convertPositionX(float x)
    {
        float const mid = 0.5f * 64.0f * 533.33333333f;
        return mid - x;
    }

    inline float convertPositionY(float y)
    {
        float const mid = 0.5f * 64.0f * 533.33333333f;
        return mid - y;
    }

    inline float convertPositionZ(float z)
    {
        return z;
    }

    // Tile packing/unpacking
    inline uint32_t packTileID(uint32_t tileX, uint32_t tileY)
    {
        return (tileX << 16) | tileY;
    }

    inline void unpackTileID(uint32_t ID, uint32_t& tileX, uint32_t& tileY)
    {
        tileX = (ID >> 16);
        tileY = (ID & 0xFFFF);
    }
}