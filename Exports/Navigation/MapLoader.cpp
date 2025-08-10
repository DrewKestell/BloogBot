// MapLoader.cpp - Complete vMaNGOS-style map loader with separate load methods and detailed logging
#include "MapLoader.h"
#include <fstream>
#include <iostream>
#include <iomanip>
#include <sstream>
#include <cmath>
#include <filesystem>
#include <cstring>
#include <algorithm>
#include <map>
#include <set>

using namespace MapFormat;

// Hole detection tables for terrain holes
static uint16_t holetab_h[4] = { 0x1111, 0x2222, 0x4444, 0x8888 };
static uint16_t holetab_v[4] = { 0x000F, 0x00F0, 0x0F00, 0xF000 };

// ==================== GridMap Implementation ====================

GridMap::~GridMap()
{
    std::cout << "[GridMap] Destructor called, unloading data..." << std::endl;
    unloadData();
}

bool GridMap::loadAreaData(FILE* in, uint32_t offset, uint32_t size)
{
    std::cout << "\n[GridMap] === LOADING AREA DATA ===" << std::endl;
    std::cout << "[GridMap] Area section: offset=" << offset << ", size=" << size << " bytes" << std::endl;

    if (fseek(in, offset, SEEK_SET) != 0)
    {
        std::cout << "[GridMap] ERROR: Failed to seek to area offset!" << std::endl;
        return false;
    }

    // Check for AREA header or raw data
    char peekBuffer[8];
    if (fread(peekBuffer, 1, 8, in) != 8)
    {
        std::cout << "[GridMap] ERROR: Failed to read area peek buffer!" << std::endl;
        return false;
    }

    std::cout << "[GridMap] First 8 bytes at area offset: ";
    for (int i = 0; i < 8; i++)
    {
        std::cout << std::hex << std::setfill('0') << std::setw(2)
            << (int)(unsigned char)peekBuffer[i] << " ";
    }
    std::cout << std::dec << std::endl;

    fseek(in, offset, SEEK_SET); // Reset

    uint32_t possibleMagic = *reinterpret_cast<uint32_t*>(peekBuffer);
    uint32_t expectedAreaMagic = *reinterpret_cast<const uint32_t*>(MAP_AREA_MAGIC);

    std::cout << "[GridMap] Checking for AREA magic: 0x" << std::hex << possibleMagic
        << " vs expected 0x" << expectedAreaMagic << std::dec << std::endl;

    if (possibleMagic == expectedAreaMagic)
    {
        // Has AREA header
        MapAreaHeader areaHeader;
        if (fread(&areaHeader, sizeof(MapAreaHeader), 1, in) != 1)
        {
            std::cout << "[GridMap] ERROR: Failed to read area header!" << std::endl;
            return false;
        }

        m_areaHeader = new MapAreaHeader(areaHeader);
        std::cout << "[GridMap] AREA header found!" << std::endl;
        std::cout << "  Flags: 0x" << std::hex << areaHeader.flags << std::dec << std::endl;
        std::cout << "  Grid area: " << areaHeader.gridArea << std::endl;

        if (!(areaHeader.flags & MAP_AREA_NO_AREA))
        {
            m_areaMap = new uint16_t[16 * 16];
            if (fread(m_areaMap, sizeof(uint16_t), 16 * 16, in) != 16 * 16)
            {
                std::cout << "[GridMap] ERROR: Failed to read area map data!" << std::endl;
                delete[] m_areaMap;
                m_areaMap = nullptr;
                return false;
            }
            std::cout << "[GridMap] Loaded 16x16 area map" << std::endl;

            // Show sample values
            std::cout << "[GridMap] First 10 area values:" << std::endl;
            for (int i = 0; i < 10 && i < 256; i++)
            {
                std::cout << "  [" << i << "] = " << m_areaMap[i] << std::endl;
            }
        }
        else
        {
            std::cout << "[GridMap] MAP_AREA_NO_AREA flag set - no area data to load" << std::endl;
        }
    }
    else
    {
        // No header - raw data
        std::cout << "[GridMap] No AREA header - treating as raw uint16 data" << std::endl;

        // Assume it's a 16x16 array of uint16
        if (size >= 16 * 16 * sizeof(uint16_t))
        {
            m_areaMap = new uint16_t[16 * 16];
            fseek(in, offset, SEEK_SET); // Reset to offset
            if (fread(m_areaMap, sizeof(uint16_t), 16 * 16, in) != 16 * 16)
            {
                std::cout << "[GridMap] ERROR: Failed to read raw area data!" << std::endl;
                delete[] m_areaMap;
                m_areaMap = nullptr;
                return false;
            }
            std::cout << "[GridMap] Loaded 16x16 area map (raw format)" << std::endl;

            // Show sample values
            std::cout << "[GridMap] First 10 area values:" << std::endl;
            for (int i = 0; i < 10 && i < 256; i++)
            {
                std::cout << "  [" << i << "] = " << m_areaMap[i] << std::endl;
            }
        }
        else
        {
            std::cout << "[GridMap] WARNING: Insufficient size for area data!" << std::endl;
        }
    }

    return true;
}

bool GridMap::loadHeightData(FILE* in, uint32_t offset, uint32_t size)
{
    std::cout << "\n[GridMap] === LOADING HEIGHT DATA ===" << std::endl;
    std::cout << "[GridMap] Height section: offset=" << offset << ", size=" << size << " bytes" << std::endl;

    if (fseek(in, offset, SEEK_SET) != 0)
    {
        std::cout << "[GridMap] ERROR: Failed to seek to height offset!" << std::endl;
        return false;
    }

    // Peek at first 16 bytes to determine format
    char peekBuffer[16];
    if (fread(peekBuffer, 1, 16, in) != 16)
    {
        std::cout << "[GridMap] ERROR: Failed to read height peek buffer!" << std::endl;
        return false;
    }
    fseek(in, offset, SEEK_SET); // Reset position

    // Show raw bytes
    std::cout << "[GridMap] First 16 bytes of height data (hex):" << std::endl;
    for (int i = 0; i < 16; i++)
    {
        std::cout << std::hex << std::setfill('0') << std::setw(2)
            << (int)(unsigned char)peekBuffer[i] << " ";
        if ((i + 1) % 8 == 0) std::cout << std::endl;
    }
    std::cout << std::dec << std::endl;

    // Interpret as floats
    float* asFloats = reinterpret_cast<float*>(peekBuffer);
    std::cout << "[GridMap] First 4 floats: ";
    for (int i = 0; i < 4; i++)
    {
        std::cout << asFloats[i] << " ";
    }
    std::cout << std::endl;

    // Check if it's a MHGT header or raw data
    uint32_t possibleMagic = *reinterpret_cast<uint32_t*>(peekBuffer);
    uint32_t expectedHeightMagic = *reinterpret_cast<const uint32_t*>(MAP_HEIGHT_MAGIC);

    bool hasHeader = (possibleMagic == expectedHeightMagic);
    std::cout << "[GridMap] Format detection: " << (hasHeader ? "Has MHGT header" : "Raw float data (no header)") << std::endl;

    if (hasHeader)
    {
        // Read the header
        MapHeightHeader heightHeader;
        if (fread(&heightHeader, sizeof(MapHeightHeader), 1, in) != 1)
        {
            std::cout << "[GridMap] ERROR: Failed to read height header!" << std::endl;
            return false;
        }

        m_heightHeader = new MapHeightHeader(heightHeader);
        m_gridHeight = heightHeader.gridHeight;

        std::cout << "[GridMap] MHGT Header found:" << std::endl;
        std::cout << "  Flags: 0x" << std::hex << heightHeader.flags << std::dec << std::endl;
        std::cout << "  Grid height: " << heightHeader.gridHeight << std::endl;
        std::cout << "  Grid max height: " << heightHeader.gridMaxHeight << std::endl;

        if (!(heightHeader.flags & MAP_HEIGHT_NO_HEIGHT))
        {
            // Read data based on flags
            if (heightHeader.flags & MAP_HEIGHT_AS_INT16)
            {
                std::cout << "[GridMap] Loading UINT16 height format" << std::endl;
                m_uint16_V9 = new uint16_t[V9_SIZE_SQ];
                m_uint16_V8 = new uint16_t[V8_SIZE_SQ];

                if (fread(m_uint16_V9, sizeof(uint16_t), V9_SIZE_SQ, in) != V9_SIZE_SQ ||
                    fread(m_uint16_V8, sizeof(uint16_t), V8_SIZE_SQ, in) != V8_SIZE_SQ)
                {
                    std::cout << "[GridMap] ERROR: Failed to read uint16 height data!" << std::endl;
                    delete[] m_uint16_V9;
                    delete[] m_uint16_V8;
                    m_uint16_V9 = nullptr;
                    m_uint16_V8 = nullptr;
                    return false;
                }

                m_gridIntHeightMultiplier = (heightHeader.gridMaxHeight - heightHeader.gridHeight) / 65535;
                m_gridGetHeight = &GridMap::getHeightFromUint16;
                std::cout << "[GridMap] Height multiplier: " << m_gridIntHeightMultiplier << std::endl;
            }
            else if (heightHeader.flags & MAP_HEIGHT_AS_INT8)
            {
                std::cout << "[GridMap] Loading UINT8 height format" << std::endl;
                m_uint8_V9 = new uint8_t[V9_SIZE_SQ];
                m_uint8_V8 = new uint8_t[V8_SIZE_SQ];

                if (fread(m_uint8_V9, sizeof(uint8_t), V9_SIZE_SQ, in) != V9_SIZE_SQ ||
                    fread(m_uint8_V8, sizeof(uint8_t), V8_SIZE_SQ, in) != V8_SIZE_SQ)
                {
                    std::cout << "[GridMap] ERROR: Failed to read uint8 height data!" << std::endl;
                    delete[] m_uint8_V9;
                    delete[] m_uint8_V8;
                    m_uint8_V9 = nullptr;
                    m_uint8_V8 = nullptr;
                    return false;
                }

                m_gridIntHeightMultiplier = (heightHeader.gridMaxHeight - heightHeader.gridHeight) / 255;
                m_gridGetHeight = &GridMap::getHeightFromUint8;
                std::cout << "[GridMap] Height multiplier: " << m_gridIntHeightMultiplier << std::endl;
            }
            else
            {
                std::cout << "[GridMap] Loading FLOAT height format from header" << std::endl;
                m_V9 = new float[V9_SIZE_SQ];
                m_V8 = new float[V8_SIZE_SQ];

                if (fread(m_V9, sizeof(float), V9_SIZE_SQ, in) != V9_SIZE_SQ ||
                    fread(m_V8, sizeof(float), V8_SIZE_SQ, in) != V8_SIZE_SQ)
                {
                    std::cout << "[GridMap] ERROR: Failed to read float height data!" << std::endl;
                    delete[] m_V9;
                    delete[] m_V8;
                    m_V9 = nullptr;
                    m_V8 = nullptr;
                    return false;
                }

                // Debug: Show some sample values from the loaded arrays
                std::cout << "[GridMap] Sample V9 values after loading:" << std::endl;
                for (int i = 0; i < 5; i++)
                {
                    std::cout << "  V9[" << i << "] = " << m_V9[i] << std::endl;
                }
                std::cout << "  V9[2704] = " << m_V9[2704] << " (at x=20, y=124)" << std::endl;
                std::cout << "  V9[2833] = " << m_V9[2833] << " (at x=21, y=124)" << std::endl;

                std::cout << "[GridMap] Sample V8 values after loading:" << std::endl;
                for (int i = 0; i < 5; i++)
                {
                    std::cout << "  V8[" << i << "] = " << m_V8[i] << std::endl;
                }
                std::cout << "  V8[2684] = " << m_V8[2684] << " (at x=20, y=124)" << std::endl;

                // Check for suspicious patterns
                bool allSame = true;
                float firstVal = m_V9[0];
                for (int i = 1; i < 100 && i < V9_SIZE_SQ; i++)
                {
                    if (m_V9[i] != firstVal)
                    {
                        allSame = false;
                        break;
                    }
                }
                if (allSame)
                {
                    std::cout << "[GridMap] WARNING: First 100 V9 values are all the same: " << firstVal << std::endl;
                }

                m_gridGetHeight = &GridMap::getHeightFromFloat;
            }
        }
        else
        {
            std::cout << "[GridMap] NO_HEIGHT flag set - using flat terrain" << std::endl;
            m_gridGetHeight = &GridMap::getHeightFromFlat;
        }
    }
    else
    {
        // No header - assume raw float data (old format)
        std::cout << "[GridMap] No MHGT header found - treating as raw float data" << std::endl;

        // Calculate expected sizes
        size_t expectedV9Size = V9_SIZE_SQ * sizeof(float);
        size_t expectedV8Size = V8_SIZE_SQ * sizeof(float);
        size_t expectedTotalSize = expectedV9Size + expectedV8Size;

        std::cout << "[GridMap] Size calculations:" << std::endl;
        std::cout << "  V9 array: " << V9_SIZE << "x" << V9_SIZE << " = " << V9_SIZE_SQ
            << " floats = " << expectedV9Size << " bytes" << std::endl;
        std::cout << "  V8 array: " << V8_SIZE << "x" << V8_SIZE << " = " << V8_SIZE_SQ
            << " floats = " << expectedV8Size << " bytes" << std::endl;
        std::cout << "  Total expected: " << expectedTotalSize << " bytes" << std::endl;
        std::cout << "  Available: " << size << " bytes" << std::endl;

        // Allocate arrays
        m_V9 = new float[V9_SIZE_SQ];
        m_V8 = new float[V8_SIZE_SQ];

        // Initialize with invalid values
        std::cout << "[GridMap] Initializing arrays with INVALID_HEIGHT (-100000)" << std::endl;
        for (int i = 0; i < V9_SIZE_SQ; i++)
            m_V9[i] = INVALID_HEIGHT;
        for (int i = 0; i < V8_SIZE_SQ; i++)
            m_V8[i] = INVALID_HEIGHT;

        // Read as much data as available
        fseek(in, offset, SEEK_SET); // Reset to offset
        size_t v9BytesToRead = std::min((size_t)size, expectedV9Size);
        size_t v9FloatsRead = fread(m_V9, sizeof(float), v9BytesToRead / sizeof(float), in);
        std::cout << "[GridMap] Read " << v9FloatsRead << " floats into V9 array" << std::endl;

        if (size > expectedV9Size)
        {
            size_t v8BytesToRead = std::min((size_t)(size - expectedV9Size), expectedV8Size);
            size_t v8FloatsRead = fread(m_V8, sizeof(float), v8BytesToRead / sizeof(float), in);
            std::cout << "[GridMap] Read " << v8FloatsRead << " floats into V8 array" << std::endl;
        }
        else
        {
            std::cout << "[GridMap] Not enough data for V8 array!" << std::endl;
        }

        // Set the height method
        m_gridGetHeight = &GridMap::getHeightFromFloat;
        std::cout << "[GridMap] Height method set to: getHeightFromFloat" << std::endl;

        // Create a default header
        m_heightHeader = new MapHeightHeader();
        m_heightHeader->fourcc = expectedHeightMagic;
        m_heightHeader->flags = 0; // float format
        m_heightHeader->gridHeight = 0.0f;
        m_heightHeader->gridMaxHeight = 100.0f;
        m_gridHeight = 0.0f;

        // Analyze the data
        int validCount = 0, invalidCount = 0, zeroCount = 0;
        float minHeight = 100000.0f, maxHeight = -100000.0f;
        float sum = 0.0f;

        std::cout << "\n[GridMap] Analyzing V9 height data..." << std::endl;
        for (int i = 0; i < V9_SIZE_SQ; i++)
        {
            if (m_V9[i] > INVALID_HEIGHT_VALUE && m_V9[i] < 10000.0f)
            {
                validCount++;
                if (m_V9[i] < minHeight) minHeight = m_V9[i];
                if (m_V9[i] > maxHeight) maxHeight = m_V9[i];
                sum += m_V9[i];

                if (m_V9[i] == 0.0f) zeroCount++;
            }
            else
            {
                invalidCount++;
            }
        }

        std::cout << "[GridMap] V9 Height Statistics:" << std::endl;
        std::cout << "  Total values: " << V9_SIZE_SQ << std::endl;
        std::cout << "  Valid: " << validCount << " (" << (validCount * 100 / V9_SIZE_SQ) << "%)" << std::endl;
        std::cout << "  Invalid: " << invalidCount << " (" << (invalidCount * 100 / V9_SIZE_SQ) << "%)" << std::endl;
        std::cout << "  Zero values: " << zeroCount << std::endl;
        if (validCount > 0)
        {
            std::cout << "  Height range: " << minHeight << " to " << maxHeight << std::endl;
            std::cout << "  Average height: " << (sum / validCount) << std::endl;
        }

        // Show first 20 values
        std::cout << "\n[GridMap] First 20 V9 values:" << std::endl;
        for (int i = 0; i < 20 && i < V9_SIZE_SQ; i++)
        {
            std::cout << "  V9[" << i << "] = " << m_V9[i];
            if (m_V9[i] <= INVALID_HEIGHT_VALUE)
                std::cout << " (INVALID)";
            std::cout << std::endl;
        }
    }

    return true;
}

bool GridMap::loadHolesData(FILE* in, uint32_t offset, uint32_t size)
{
    std::cout << "\n[GridMap] === LOADING HOLES DATA ===" << std::endl;
    std::cout << "[GridMap] Holes offset: " << offset << ", size: " << size << " bytes" << std::endl;

    if (fseek(in, offset, SEEK_SET) != 0)
    {
        std::cout << "[GridMap] ERROR: Failed to seek to holes offset!" << std::endl;
        return false;
    }

    // Holes should be exactly 16 bytes (8 uint16 values)
    if (size < 16)
    {
        std::cout << "[GridMap] WARNING: Holes size too small: " << size << " bytes (expected at least 16)" << std::endl;
        return false;
    }

    uint8_t holesData[16];
    if (fread(holesData, 1, 16, in) != 16)
    {
        std::cout << "[GridMap] ERROR: Failed to read holes data!" << std::endl;
        return false;
    }

    std::cout << "[GridMap] Raw holes data (16 bytes):" << std::endl;
    for (int i = 0; i < 16; i++)
    {
        std::cout << std::hex << std::setfill('0') << std::setw(2) << (int)holesData[i] << " ";
        if ((i + 1) % 8 == 0) std::cout << std::endl;
    }
    std::cout << std::dec << std::endl;

    // Store as 8 uint16 values
    m_holes = new uint16_t[8];
    memcpy(m_holes, holesData, 16);

    std::cout << "[GridMap] Holes as uint16 values:" << std::endl;
    bool looksValid = true;
    for (int i = 0; i < 8; i++)
    {
        std::cout << "  holes[" << i << "] = 0x" << std::hex << m_holes[i] << std::dec;

        // Check for common invalid patterns
        if (m_holes[i] == 0xCDCD || m_holes[i] == 0xCCCC)  // MSVC debug uninitialized memory
        {
            std::cout << " (UNINITIALIZED MEMORY!)";
            looksValid = false;
        }
        else if (m_holes[i] == 0xFFFF)  // All holes
        {
            std::cout << " (all holes)";
        }
        else if (m_holes[i] == 0x0000)  // No holes
        {
            std::cout << " (no holes)";
        }
        std::cout << std::endl;
    }

    if (!looksValid)
    {
        std::cout << "[GridMap] WARNING: Holes data appears to be invalid/uninitialized!" << std::endl;
        std::cout << "[GridMap] Disabling holes detection" << std::endl;
        delete[] m_holes;
        m_holes = nullptr;
        return false;
    }

    std::cout << "[GridMap] Holes data loaded successfully" << std::endl;
    return true;
}

bool GridMap::loadLiquidData(FILE* in, uint32_t offset, uint32_t size)
{
    std::cout << "\n[GridMap] === LOADING LIQUID DATA ===" << std::endl;
    std::cout << "[GridMap] Liquid section: offset=" << offset << ", size=" << size << " bytes" << std::endl;

    if (fseek(in, offset, SEEK_SET) != 0)
    {
        std::cout << "[GridMap] ERROR: Failed to seek to liquid offset!" << std::endl;
        return false;
    }

    // Check for MLIQ header or raw data
    char peekBuffer[8];
    if (fread(peekBuffer, 1, 8, in) != 8)
    {
        std::cout << "[GridMap] ERROR: Failed to read liquid peek buffer!" << std::endl;
        return false;
    }
    fseek(in, offset, SEEK_SET); // Reset

    uint32_t possibleMagic = *reinterpret_cast<uint32_t*>(peekBuffer);
    uint32_t expectedLiquidMagic = *reinterpret_cast<const uint32_t*>(MAP_LIQUID_MAGIC);

    if (possibleMagic == expectedLiquidMagic)
    {
        // Has MLIQ header
        MapLiquidHeader liquidHeader;
        if (fread(&liquidHeader, sizeof(MapLiquidHeader), 1, in) != 1)
        {
            std::cout << "[GridMap] ERROR: Failed to read liquid header!" << std::endl;
            return false;
        }

        m_liquidHeader = new MapLiquidHeader(liquidHeader);
        std::cout << "[GridMap] MLIQ header found" << std::endl;
        std::cout << "  Flags: 0x" << std::hex << liquidHeader.flags << std::dec << std::endl;
        std::cout << "  Type: " << liquidHeader.liquidType << std::endl;
        std::cout << "  Dimensions: " << (int)liquidHeader.width << "x" << (int)liquidHeader.height << std::endl;
        std::cout << "  Level: " << liquidHeader.liquidLevel << std::endl;

        if (!(liquidHeader.flags & MAP_LIQUID_NO_TYPE))
        {
            uint32_t liquidSize = liquidHeader.width * liquidHeader.height;
            if (liquidSize > 0 && liquidSize < 1000000)  // Sanity check
            {
                std::cout << "[GridMap] Loading liquid type data, size=" << liquidSize << " entries" << std::endl;

                m_liquidEntry = new uint16_t[16 * 16];
                m_liquidFlags = new uint8_t[16 * 16];

                if (fread(m_liquidEntry, sizeof(uint16_t), 16 * 16, in) != 16 * 16 ||
                    fread(m_liquidFlags, sizeof(uint8_t), 16 * 16, in) != 16 * 16)
                {
                    std::cout << "[GridMap] ERROR: Failed to read liquid type/flags data!" << std::endl;
                    delete[] m_liquidEntry;
                    delete[] m_liquidFlags;
                    m_liquidEntry = nullptr;
                    m_liquidFlags = nullptr;
                    return false;
                }

                std::cout << "[GridMap] Loaded liquid type and flags arrays" << std::endl;
            }
            else
            {
                std::cout << "[GridMap] WARNING: Invalid liquid size: " << liquidSize << std::endl;
            }
        }

        if (!(liquidHeader.flags & MAP_LIQUID_NO_HEIGHT))
        {
            uint32_t liquidHeightSize = liquidHeader.width * liquidHeader.height;
            if (liquidHeightSize > 0 && liquidHeightSize < 1000000)  // Sanity check
            {
                m_liquidHeight = new float[liquidHeightSize];
                if (fread(m_liquidHeight, sizeof(float), liquidHeightSize, in) != liquidHeightSize)
                {
                    std::cout << "[GridMap] ERROR: Failed to read liquid height data!" << std::endl;
                    delete[] m_liquidHeight;
                    m_liquidHeight = nullptr;
                    return false;
                }

                std::cout << "[GridMap] Loaded " << liquidHeightSize << " liquid height values" << std::endl;
            }
        }
    }
    else
    {
        // No header - skip liquid for now since format is unknown without header
        std::cout << "[GridMap] No MLIQ header - liquid format unknown, skipping" << std::endl;
    }

    return true;
}

bool GridMap::loadData(const std::string& filename)
{
    std::cout << "\n========== GRIDMAP LOADING ULTRA DETAILED LOG ==========" << std::endl;
    std::cout << "[GridMap] Loading file: " << filename << std::endl;

    // Unload any existing data
    unloadData();

    // Check if file exists
    if (!std::filesystem::exists(filename))
    {
        std::cout << "[GridMap] ERROR: File does not exist: " << filename << std::endl;
        return false;
    }

    // Open file using FILE* (vMaNGOS style)
    FILE* in = fopen(filename.c_str(), "rb");
    if (!in)
    {
        std::cout << "[GridMap] ERROR: Failed to open file: " << filename << std::endl;
        return false;
    }

    // Get file size
    fseek(in, 0, SEEK_END);
    long fileSize = ftell(in);
    fseek(in, 0, SEEK_SET);
    std::cout << "[GridMap] File opened successfully, size: " << fileSize << " bytes" << std::endl;

    // Read header
    MapFileHeader header;
    if (fread(&header, sizeof(MapFileHeader), 1, in) != 1)
    {
        std::cout << "[GridMap] ERROR: Failed to read header!" << std::endl;
        fclose(in);
        return false;
    }

    std::cout << "\n[GridMap] === RAW HEADER DUMP (40 bytes) ===" << std::endl;
    uint8_t* headerBytes = reinterpret_cast<uint8_t*>(&header);
    for (int i = 0; i < 40; i++)
    {
        std::cout << std::hex << std::setfill('0') << std::setw(2) << (int)headerBytes[i] << " ";
        if ((i + 1) % 16 == 0) std::cout << std::endl;
    }
    std::cout << std::dec << std::endl;

    // Verify magic
    uint32_t expectedMapMagic = *reinterpret_cast<const uint32_t*>(MAP_MAGIC);
    uint32_t expectedVersionMagic = *reinterpret_cast<const uint32_t*>(MAP_VERSION_MAGIC);

    std::cout << "\n[GridMap] === HEADER VALIDATION ===" << std::endl;
    std::cout << "  Map Magic: 0x" << std::hex << header.mapMagic
        << " (expected: 0x" << expectedMapMagic << ") - "
        << (header.mapMagic == expectedMapMagic ? "OK" : "FAILED") << std::dec << std::endl;
    std::cout << "  Version Magic: 0x" << std::hex << header.versionMagic
        << " (expected: 0x" << expectedVersionMagic << ") - "
        << (header.versionMagic == expectedVersionMagic ? "OK" : "FAILED") << std::dec << std::endl;

    if (header.mapMagic != expectedMapMagic || header.versionMagic != expectedVersionMagic)
    {
        std::cerr << "[GridMap] FATAL ERROR: Invalid map file format!" << std::endl;
        fclose(in);
        return false;
    }

    std::cout << "\n[GridMap] === SECTION OFFSETS AND SIZES ===" << std::endl;
    std::cout << "  Area: offset=" << header.areaMapOffset << ", size=" << header.areaMapSize << " bytes" << std::endl;
    std::cout << "  Height: offset=" << header.heightMapOffset << ", size=" << header.heightMapSize << " bytes" << std::endl;
    std::cout << "  Liquid: offset=" << header.liquidMapOffset << ", size=" << header.liquidMapSize << " bytes" << std::endl;
    std::cout << "  Holes: offset=" << header.holesOffset << ", size=" << header.holesSize << " bytes" << std::endl;

    // Debug: Show what we SHOULD be reading based on raw bytes
    std::cout << "\n[GridMap] === DEBUG: Raw byte interpretation ===" << std::endl;
    uint32_t* raw = reinterpret_cast<uint32_t*>(&header);
    std::cout << "  Field[0] (mapMagic): 0x" << std::hex << raw[0] << std::dec << std::endl;
    std::cout << "  Field[1] (versionMagic): 0x" << std::hex << raw[1] << std::dec << std::endl;
    std::cout << "  Field[2] (areaMapOffset): " << raw[2] << std::endl;
    std::cout << "  Field[3] (areaMapSize): " << raw[3] << std::endl;
    std::cout << "  Field[4] (heightMapOffset): " << raw[4] << std::endl;
    std::cout << "  Field[5] (heightMapSize): " << raw[5] << std::endl;
    std::cout << "  Field[6] (liquidMapOffset): " << raw[6] << std::endl;
    std::cout << "  Field[7] (liquidMapSize): " << raw[7] << std::endl;
    std::cout << "  Field[8] (holesOffset): " << raw[8] << std::endl;
    std::cout << "  Field[9] (holesSize): " << raw[9] << std::endl;

    // Load each section using separate methods (vMaNGOS style)
    bool success = true;

    // Load area data
    if (header.areaMapOffset > 0 && header.areaMapSize > 0)
    {
        if (!loadAreaData(in, header.areaMapOffset, header.areaMapSize))
        {
            std::cout << "[GridMap] ERROR: Failed to load area data!" << std::endl;
            success = false;
        }
    }
    else
    {
        std::cout << "[GridMap] No area data in file" << std::endl;
    }

    // Load holes data
    if (success && header.holesOffset > 0 && header.holesSize > 0)
    {
        if (!loadHolesData(in, header.holesOffset, header.holesSize))
        {
            std::cout << "[GridMap] WARNING: Failed to load holes data (non-fatal)" << std::endl;
            // Don't fail completely for holes
        }
    }
    else
    {
        std::cout << "[GridMap] No holes data specified" << std::endl;
    }

    // Load height data
    if (success && header.heightMapOffset > 0)
    {
        if (header.heightMapSize == 0)
        {
            std::cout << "[GridMap] WARNING: Height offset present but size is 0!" << std::endl;
            std::cout << "[GridMap] This map tile has no height data - using flat terrain" << std::endl;
            // Create a minimal height header for flat terrain
            m_heightHeader = new MapHeightHeader();
            m_heightHeader->fourcc = *reinterpret_cast<const uint32_t*>(MAP_HEIGHT_MAGIC);
            m_heightHeader->flags = MAP_HEIGHT_NO_HEIGHT;
            m_heightHeader->gridHeight = 0.0f;
            m_heightHeader->gridMaxHeight = 0.0f;
            m_gridHeight = 0.0f;
            m_gridGetHeight = &GridMap::getHeightFromFlat;
        }
        else if (!loadHeightData(in, header.heightMapOffset, header.heightMapSize))
        {
            std::cout << "[GridMap] ERROR: Failed to load height data!" << std::endl;
            success = false;
        }
    }
    else
    {
        std::cout << "[GridMap] No height data in file - using flat terrain" << std::endl;
        // Create a minimal height header for flat terrain
        m_heightHeader = new MapHeightHeader();
        m_heightHeader->fourcc = *reinterpret_cast<const uint32_t*>(MAP_HEIGHT_MAGIC);
        m_heightHeader->flags = MAP_HEIGHT_NO_HEIGHT;
        m_heightHeader->gridHeight = 0.0f;
        m_heightHeader->gridMaxHeight = 0.0f;
        m_gridHeight = 0.0f;
        m_gridGetHeight = &GridMap::getHeightFromFlat;
    }

    // Load liquid data
    if (success && header.liquidMapOffset > 0 && header.liquidMapSize > 0)
    {
        if (!loadLiquidData(in, header.liquidMapOffset, header.liquidMapSize))
        {
            std::cout << "[GridMap] WARNING: Failed to load liquid data (non-fatal)" << std::endl;
            // Don't fail completely for liquid
        }
    }
    else
    {
        std::cout << "[GridMap] No liquid data in file" << std::endl;
    }

    fclose(in);

    std::cout << "\n[GridMap] === FINAL LOAD SUMMARY ===" << std::endl;
    std::cout << "[GridMap] Load result: " << (success ? "SUCCESS" : "FAILED") << std::endl;
    std::cout << "[GridMap] Height method pointer: " << (m_gridGetHeight ? "SET" : "NULL") << std::endl;
    std::cout << "[GridMap] Heights loaded: " << (m_V9 || m_uint16_V9 || m_uint8_V9 ? "YES" : "NO") << std::endl;
    std::cout << "[GridMap] Area loaded: " << (m_areaMap != nullptr ? "YES" : "NO") << std::endl;
    std::cout << "[GridMap] Liquid loaded: " << (m_liquidHeader != nullptr ? "YES" : "NO") << std::endl;
    std::cout << "[GridMap] Holes loaded: " << (m_holes != nullptr ? "YES" : "NO") << std::endl;
    std::cout << "========================================================\n" << std::endl;

    return success;
}

void GridMap::unloadData()
{
    std::cout << "[GridMap] Unloading all data..." << std::endl;

    delete m_heightHeader;
    m_heightHeader = nullptr;

    delete m_liquidHeader;
    m_liquidHeader = nullptr;

    delete m_areaHeader;
    m_areaHeader = nullptr;

    delete[] m_V9;
    m_V9 = nullptr;

    delete[] m_V8;
    m_V8 = nullptr;

    delete[] m_liquidHeight;
    m_liquidHeight = nullptr;

    delete[] m_liquidFlags;
    m_liquidFlags = nullptr;

    delete[] m_liquidEntry;
    m_liquidEntry = nullptr;

    delete[] m_areaMap;
    m_areaMap = nullptr;

    delete[] m_holes;
    m_holes = nullptr;

    m_gridGetHeight = nullptr;
}

float GridMap::getHeight(float x, float y) const
{
    std::cout << "\n[GridMap::getHeight] === HEIGHT QUERY ===" << std::endl;
    std::cout << "[GridMap::getHeight] Input coordinates: x=" << x << ", y=" << y << std::endl;

    if (!m_gridGetHeight)
    {
        std::cout << "[GridMap::getHeight] FATAL ERROR: m_gridGetHeight is NULL!" << std::endl;
        return INVALID_HEIGHT;
    }

    std::cout << "[GridMap::getHeight] Calling height method function..." << std::endl;
    float height = (this->*m_gridGetHeight)(x, y);
    std::cout << "[GridMap::getHeight] Result: " << height << std::endl;

    return height;
}

float GridMap::getHeightFromFloat(float x, float y) const
{
    std::cout << "\n[getHeightFromFloat] === DETAILED HEIGHT CALCULATION ===" << std::endl;
    std::cout << "[getHeightFromFloat] Input: x=" << x << ", y=" << y << " (world coordinates)" << std::endl;

    if (!m_V9 || !m_V8)
    {
        std::cout << "[getHeightFromFloat] ERROR: V9=" << (void*)m_V9 << ", V8=" << (void*)m_V8 << std::endl;
        std::cout << "[getHeightFromFloat] Returning INVALID_HEIGHT" << std::endl;
        return INVALID_HEIGHT;
    }

    // vMaNGOS transformation - this expects WORLD coordinates
    float orig_x = x;
    float orig_y = y;
    x = MAP_RESOLUTION * (32 - x / SIZE_OF_GRIDS);
    y = MAP_RESOLUTION * (32 - y / SIZE_OF_GRIDS);

    std::cout << "[getHeightFromFloat] Transform calculation:" << std::endl;
    std::cout << "  x: 128 * (32 - " << orig_x << " / 533.333) = 128 * (32 - " << (orig_x / SIZE_OF_GRIDS)
        << ") = 128 * " << (32 - orig_x / SIZE_OF_GRIDS) << " = " << x << std::endl;
    std::cout << "  y: 128 * (32 - " << orig_y << " / 533.333) = 128 * (32 - " << (orig_y / SIZE_OF_GRIDS)
        << ") = 128 * " << (32 - orig_y / SIZE_OF_GRIDS) << " = " << y << std::endl;

    int x_int = (int)x;
    int y_int = (int)y;
    float x_frac = x - x_int;  // Get fractional part
    float y_frac = y - y_int;  // Get fractional part
    x_int &= (MAP_RESOLUTION - 1);  // Wrap to 0-127 range
    y_int &= (MAP_RESOLUTION - 1);  // Wrap to 0-127 range

    std::cout << "[getHeightFromFloat] Before modulo: x=" << (int)x << ", y=" << (int)y << std::endl;
    std::cout << "[getHeightFromFloat] After modulo (0-127): x_int=" << x_int << ", y_int=" << y_int << std::endl;
    std::cout << "[getHeightFromFloat] Fractional parts: x_frac=" << x_frac << ", y_frac=" << y_frac << std::endl;

    if (isHole(x_int, y_int))
    {
        std::cout << "[getHeightFromFloat] Position is in a hole!" << std::endl;
        return INVALID_HEIGHT;
    }

    // Debug: Show the actual array indices we're about to use
    int v9_idx1 = x_int * 129 + y_int;
    int v9_idx2 = (x_int + 1) * 129 + y_int;
    int v9_idx3 = x_int * 129 + (y_int + 1);
    int v9_idx4 = (x_int + 1) * 129 + (y_int + 1);
    int v8_idx = x_int * 128 + y_int;

    std::cout << "[getHeightFromFloat] Array indices:" << std::endl;
    std::cout << "  V9[" << v9_idx1 << "], V9[" << v9_idx2 << "], V9[" << v9_idx3 << "], V9[" << v9_idx4 << "]" << std::endl;
    std::cout << "  V8[" << v8_idx << "]" << std::endl;

    float a, b, c;

    // Select triangle and calculate coefficients
    if (x_frac + y_frac < 1)
    {
        if (x_frac > y_frac)
        {
            // Triangle 1 (h1, h2, h5 points)
            float h1 = m_V9[v9_idx1];
            float h2 = m_V9[v9_idx2];
            float h5 = 2 * m_V8[v8_idx];
            a = h2 - h1;
            b = h5 - h1 - h2;
            c = h1;
            std::cout << "[getHeightFromFloat] Triangle 1: h1=" << h1 << ", h2=" << h2 << ", h5=" << h5 / 2 << " (*2=" << h5 << ")" << std::endl;
        }
        else
        {
            // Triangle 2 (h1, h3, h5 points)
            float h1 = m_V9[v9_idx1];
            float h3 = m_V9[v9_idx3];
            float h5 = 2 * m_V8[v8_idx];
            a = h5 - h1 - h3;
            b = h3 - h1;
            c = h1;
            std::cout << "[getHeightFromFloat] Triangle 2: h1=" << h1 << ", h3=" << h3 << ", h5=" << h5 / 2 << " (*2=" << h5 << ")" << std::endl;
        }
    }
    else
    {
        if (x_frac > y_frac)
        {
            // Triangle 3 (h2, h4, h5 points)
            float h2 = m_V9[v9_idx2];
            float h4 = m_V9[v9_idx4];
            float h5 = 2 * m_V8[v8_idx];
            a = h2 + h4 - h5;
            b = h4 - h2;
            c = h5 - h4;
            std::cout << "[getHeightFromFloat] Triangle 3: h2=" << h2 << ", h4=" << h4 << ", h5=" << h5 / 2 << " (*2=" << h5 << ")" << std::endl;
        }
        else
        {
            // Triangle 4 (h3, h4, h5 points)
            float h3 = m_V9[v9_idx3];
            float h4 = m_V9[v9_idx4];
            float h5 = 2 * m_V8[v8_idx];
            a = h4 - h3;
            b = h3 + h4 - h5;
            c = h5 - h4;
            std::cout << "[getHeightFromFloat] Triangle 4: h3=" << h3 << ", h4=" << h4 << ", h5=" << h5 / 2 << " (*2=" << h5 << ")" << std::endl;
        }
    }

    // Calculate height
    float result = a * x_frac + b * y_frac + c;

    std::cout << "[getHeightFromFloat] Coefficients: a=" << a << ", b=" << b << ", c=" << c << std::endl;
    std::cout << "[getHeightFromFloat] Final height: " << result << std::endl;
    std::cout << "==========================================" << std::endl;

    return result;
}

float GridMap::getHeightFromUint16(float x, float y) const
{
    std::cout << "\n[getHeightFromUint16] Input: x=" << x << ", y=" << y << std::endl;

    if (!m_uint16_V9 || !m_uint16_V8)
    {
        std::cout << "[getHeightFromUint16] Arrays are NULL, returning grid height: " << m_gridHeight << std::endl;
        return m_gridHeight;
    }

    // FIX: Don't transform coordinates - they're already tile-local
    float grid_x = x / GRID_PART_SIZE;
    float grid_y = y / GRID_PART_SIZE;

    int x_int = (int)grid_x;
    int y_int = (int)grid_y;
    float x_frac = grid_x - x_int;
    float y_frac = grid_y - y_int;

    std::cout << "[getHeightFromUint16] Grid position: x_int=" << x_int << ", y_int=" << y_int << std::endl;

    if (x_int < 0 || x_int >= V8_SIZE || y_int < 0 || y_int >= V8_SIZE)
    {
        std::cout << "[getHeightFromUint16] Out of bounds!" << std::endl;
        return INVALID_HEIGHT;
    }

    if (isHole(x_int, y_int))
    {
        std::cout << "[getHeightFromUint16] Position is in a hole!" << std::endl;
        return INVALID_HEIGHT;
    }

    // FIX: CORRECTED INDEXING - vMaNGOS style
    int idx0 = x_int * V9_SIZE + y_int;
    int idx1 = (x_int + 1) * V9_SIZE + y_int;
    int idx2 = x_int * V9_SIZE + (y_int + 1);
    int idx3 = (x_int + 1) * V9_SIZE + (y_int + 1);

    // Convert uint16 to float heights
    float h0 = m_uint16_V9[idx0] * m_gridIntHeightMultiplier + m_gridHeight;
    float h1 = m_uint16_V9[idx1] * m_gridIntHeightMultiplier + m_gridHeight;
    float h2 = m_uint16_V9[idx2] * m_gridIntHeightMultiplier + m_gridHeight;
    float h3 = m_uint16_V9[idx3] * m_gridIntHeightMultiplier + m_gridHeight;

    std::cout << "[getHeightFromUint16] Raw values: " << m_uint16_V9[idx0] << ", " << m_uint16_V9[idx1]
        << ", " << m_uint16_V9[idx2] << ", " << m_uint16_V9[idx3] << std::endl;
    std::cout << "[getHeightFromUint16] Converted heights: " << h0 << ", " << h1
        << ", " << h2 << ", " << h3 << std::endl;

    // Bilinear interpolation
    float h_top = h0 + (h1 - h0) * x_frac;
    float h_bottom = h2 + (h3 - h2) * x_frac;
    float result = h_top + (h_bottom - h_top) * y_frac;

    std::cout << "[getHeightFromUint16] Interpolated height: " << result << std::endl;
    return result;
}

float GridMap::getHeightFromUint8(float x, float y) const
{
    std::cout << "\n[getHeightFromUint8] Input: x=" << x << ", y=" << y << std::endl;

    if (!m_uint8_V9 || !m_uint8_V8)
    {
        std::cout << "[getHeightFromUint8] Arrays are NULL, returning grid height: " << m_gridHeight << std::endl;
        return m_gridHeight;
    }

    // FIX: Don't transform coordinates - they're already tile-local
    float grid_x = x / GRID_PART_SIZE;
    float grid_y = y / GRID_PART_SIZE;

    int x_int = (int)grid_x;
    int y_int = (int)grid_y;
    float x_frac = grid_x - x_int;
    float y_frac = grid_y - y_int;

    if (x_int < 0 || x_int >= V8_SIZE || y_int < 0 || y_int >= V8_SIZE)
    {
        return INVALID_HEIGHT;
    }

    if (isHole(x_int, y_int))
    {
        std::cout << "[getHeightFromUint8] Position is in a hole!" << std::endl;
        return INVALID_HEIGHT;
    }

    // FIX: CORRECTED INDEXING - vMaNGOS style
    int idx0 = x_int * V9_SIZE + y_int;
    int idx1 = (x_int + 1) * V9_SIZE + y_int;
    int idx2 = x_int * V9_SIZE + (y_int + 1);
    int idx3 = (x_int + 1) * V9_SIZE + (y_int + 1);

    // Convert uint8 to float heights
    float h0 = m_uint8_V9[idx0] * m_gridIntHeightMultiplier + m_gridHeight;
    float h1 = m_uint8_V9[idx1] * m_gridIntHeightMultiplier + m_gridHeight;
    float h2 = m_uint8_V9[idx2] * m_gridIntHeightMultiplier + m_gridHeight;
    float h3 = m_uint8_V9[idx3] * m_gridIntHeightMultiplier + m_gridHeight;

    // Bilinear interpolation
    float h_top = h0 + (h1 - h0) * x_frac;
    float h_bottom = h2 + (h3 - h2) * x_frac;
    float result = h_top + (h_bottom - h_top) * y_frac;

    std::cout << "[getHeightFromUint8] Interpolated height: " << result << std::endl;
    return result;
}

float GridMap::getHeightFromFlat(float /*x*/, float /*y*/) const
{
    std::cout << "[getHeightFromFlat] Returning flat terrain height: " << m_gridHeight << std::endl;
    return m_gridHeight;
}

bool GridMap::isHole(int row, int col) const
{
    if (!m_holes)
        return false;

    // Check if holes data is uninitialized (0xCCCC pattern)
    // If any hole value is 0xCCCC, treat as no holes
    for (int i = 0; i < 8; i++)
    {
        if (m_holes[i] == 0xCCCC || m_holes[i] == 0xCDCD)
        {
            return false; // Uninitialized data, assume no holes
        }
    }

    int cellRow = row / 8;     // 8 squares per cell
    int cellCol = col / 8;
    int holeRow = row % 8 / 2;
    int holeCol = (col - (cellCol * 8)) / 2;

    if (cellRow >= 8 || cellCol >= 8)
        return false;

    // Each m_holes entry represents 2x2 cells
    uint16_t hole = m_holes[cellRow / 2 * 2 + cellCol / 2];
    bool isHolePos = (hole & holetab_h[holeCol] & holetab_v[holeRow]) != 0;

    return isHolePos;
}

float GridMap::getLiquidLevel(float x, float y) const
{
    if (!m_liquidHeader || !m_liquidHeight)
        return INVALID_HEIGHT;

    x = MAP_RESOLUTION * (32 - x / SIZE_OF_GRIDS);
    y = MAP_RESOLUTION * (32 - y / SIZE_OF_GRIDS);

    int cx_int = ((int)x & (MAP_RESOLUTION - 1)) - m_liquidHeader->offsetY;
    int cy_int = ((int)y & (MAP_RESOLUTION - 1)) - m_liquidHeader->offsetX;

    if (cx_int < 0 || cx_int >= m_liquidHeader->height)
        return INVALID_HEIGHT;

    if (cy_int < 0 || cy_int >= m_liquidHeader->width)
        return INVALID_HEIGHT;

    return m_liquidHeight[cx_int * m_liquidHeader->width + cy_int];
}

uint8_t GridMap::getLiquidType(float x, float y) const
{
    if (!m_liquidFlags)
        return m_liquidHeader ? static_cast<uint8_t>(m_liquidHeader->liquidType) : MAP_LIQUID_TYPE_NO_WATER;

    x = 16 * (32 - x / SIZE_OF_GRIDS);
    y = 16 * (32 - y / SIZE_OF_GRIDS);
    int lx = (int)x & 15;
    int ly = (int)y & 15;
    return m_liquidFlags[lx * 16 + ly];
}

uint16_t GridMap::getArea(float x, float y) const
{
    if (!m_areaMap)
        return m_areaHeader ? m_areaHeader->gridArea : 0;

    x = 16 * (32 - x / SIZE_OF_GRIDS);
    y = 16 * (32 - y / SIZE_OF_GRIDS);
    int lx = (int)x & 15;
    int ly = (int)y & 15;
    return m_areaMap[lx * 16 + ly];
}

// ==================== MapLoader Implementation ====================

MapLoader::MapLoader()
{
    std::cout << "[MapLoader] Constructor called" << std::endl;
}

MapLoader::~MapLoader()
{
    std::cout << "[MapLoader] Destructor called" << std::endl;
    Shutdown();
}

bool MapLoader::Initialize(const std::string& dataPath)
{
    std::lock_guard<std::mutex> lock(m_mutex);

    std::cout << "\n[MapLoader] === INITIALIZATION ===" << std::endl;
    std::cout << "[MapLoader] Initializing with path: " << dataPath << std::endl;

    if (m_initialized)
    {
        std::cout << "[MapLoader] Already initialized" << std::endl;
        return true;
    }

    m_dataPath = dataPath;
    if (!m_dataPath.empty() && m_dataPath.back() != '/' && m_dataPath.back() != '\\')
        m_dataPath += '/';

    std::cout << "[MapLoader] Final data path: " << m_dataPath << std::endl;

    // Check if directory exists
    if (std::filesystem::exists(m_dataPath))
    {
        std::cout << "[MapLoader] Data path exists - scanning for map files..." << std::endl;

        // Count and list map files
        int mapCount = 0;
        for (const auto& entry : std::filesystem::directory_iterator(m_dataPath))
        {
            if (entry.path().extension() == ".map")
            {
                mapCount++;
                if (mapCount <= 10)  // Show first 10
                {
                    std::cout << "  Found: " << entry.path().filename() << std::endl;
                }
            }
        }
        std::cout << "[MapLoader] Total map files found: " << mapCount << std::endl;
    }
    else
    {
        std::cout << "[MapLoader] ERROR: Data path does not exist: " << m_dataPath << std::endl;
    }

    m_initialized = true;
    std::cout << "[MapLoader] Initialization complete" << std::endl;
    std::cout << "==================================\n" << std::endl;
    return true;
}

void MapLoader::Shutdown()
{
    std::lock_guard<std::mutex> lock(m_mutex);
    std::cout << "[MapLoader] Shutting down, clearing " << m_loadedTiles.size() << " loaded tiles" << std::endl;
    m_loadedTiles.clear();
    m_initialized = false;
}

std::string MapLoader::getMapFileName(uint32_t mapId, uint32_t x, uint32_t y) const
{
    std::stringstream ss;
    ss << m_dataPath << std::setfill('0') << std::setw(3) << mapId
        << std::setw(2) << x << std::setw(2) << y << ".map";
    return ss.str();
}

uint64_t MapLoader::makeKey(uint32_t mapId, uint32_t x, uint32_t y) const
{
    return ((uint64_t)mapId << 32) | ((uint64_t)x << 16) | (uint64_t)y;
}

void MapLoader::worldToGridCoords(float worldX, float worldY, uint32_t& gridX, uint32_t& gridY) const
{
    // Convert world coordinates to grid tile indices
    // vMaNGOS comment: "Giperion Elysium: It's reversed. That's ok"
    // Grid coordinates are swapped: Y determines gridX, X determines gridY
    gridX = static_cast<uint32_t>((CENTER_GRID_ID - worldY / GRID_SIZE));
    gridY = static_cast<uint32_t>((CENTER_GRID_ID - worldX / GRID_SIZE));

    std::cout << "[MapLoader] World->Grid conversion: (" << worldX << ", " << worldY
        << ") -> Tile [" << gridX << ", " << gridY << "]" << std::endl;

    // Debug: show the actual calculation
    std::cout << "  gridX = (32 - " << worldY << " / 533.333) = " << gridX << std::endl;
    std::cout << "  gridY = (32 - " << worldX << " / 533.333) = " << gridY << std::endl;
}

bool MapLoader::LoadMapTile(uint32_t mapId, uint32_t x, uint32_t y)
{
    std::lock_guard<std::mutex> lock(m_mutex);

    std::cout << "\n[MapLoader::LoadMapTile] Loading Map " << mapId << " Tile [" << x << ", " << y << "]" << std::endl;

    uint64_t key = makeKey(mapId, x, y);
    if (m_loadedTiles.find(key) != m_loadedTiles.end())
    {
        std::cout << "[MapLoader::LoadMapTile] Tile already loaded (cached)" << std::endl;
        return true;
    }

    std::string filename = getMapFileName(mapId, x, y);
    std::cout << "[MapLoader::LoadMapTile] Attempting to load file: " << filename << std::endl;

    if (!std::filesystem::exists(filename))
    {
        std::cout << "[MapLoader::LoadMapTile] ERROR: File does not exist!" << std::endl;
        return false;
    }

    auto gridMap = std::make_unique<GridMap>();
    if (!gridMap->loadData(filename))
    {
        std::cout << "[MapLoader::LoadMapTile] ERROR: Failed to load data from file!" << std::endl;
        return false;
    }

    m_loadedTiles[key] = std::move(gridMap);
    std::cout << "[MapLoader::LoadMapTile] SUCCESS: Tile loaded. Cache now contains "
        << m_loadedTiles.size() << " tiles" << std::endl;
    return true;
}

void MapLoader::UnloadMapTile(uint32_t mapId, uint32_t x, uint32_t y)
{
    std::lock_guard<std::mutex> lock(m_mutex);
    std::cout << "[MapLoader::UnloadMapTile] Unloading Map " << mapId << " Tile [" << x << ", " << y << "]" << std::endl;
    m_loadedTiles.erase(makeKey(mapId, x, y));
}

void MapLoader::UnloadAllTiles()
{
    std::lock_guard<std::mutex> lock(m_mutex);
    std::cout << "[MapLoader::UnloadAllTiles] Clearing all " << m_loadedTiles.size() << " loaded tiles" << std::endl;
    m_loadedTiles.clear();
}

float MapLoader::GetHeight(uint32_t mapId, float x, float y, float z)
{
    std::cout << "\n[MapLoader::GetHeight] ========== MAIN HEIGHT QUERY ==========" << std::endl;
    std::cout << "[MapLoader::GetHeight] Map: " << mapId << ", World Position: ("
        << x << ", " << y << ", " << z << ")" << std::endl;

    // Convert world coordinates to grid tile indices
    uint32_t gridX, gridY;
    worldToGridCoords(x, y, gridX, gridY);

    // Load the tile if not already loaded
    if (!LoadMapTile(mapId, gridY, gridX))
    {
        std::cout << "[MapLoader::GetHeight] ERROR: Failed to load tile!" << std::endl;
        return INVALID_HEIGHT;
    }

    std::lock_guard<std::mutex> lock(m_mutex);

    // Get the tile from cache
    auto it = m_loadedTiles.find(makeKey(mapId, gridY, gridX));
    if (it == m_loadedTiles.end())
    {
        std::cout << "[MapLoader::GetHeight] ERROR: Tile not found in cache after loading!" << std::endl;
        return INVALID_HEIGHT;
    }

    // Pass WORLD coordinates directly to GridMap::getHeight
    // The GridMap will handle the transformation internally
    std::cout << "[MapLoader::GetHeight] Passing world coords to GridMap: (" << x << ", " << y << ")" << std::endl;

    float height = it->second->getHeight(x, y);

    std::cout << "[MapLoader::GetHeight] *** FINAL RESULT: " << height << " ***" << std::endl;
    std::cout << "======================================================\n" << std::endl;

    return height;
}

float MapLoader::GetLiquidLevel(uint32_t mapId, float x, float y, float z)
{
    uint32_t gridX, gridY;
    worldToGridCoords(x, y, gridX, gridY);

    if (!LoadMapTile(mapId, gridX, gridY))
        return INVALID_HEIGHT;

    std::lock_guard<std::mutex> lock(m_mutex);

    auto it = m_loadedTiles.find(makeKey(mapId, gridX, gridY));
    if (it == m_loadedTiles.end())
        return INVALID_HEIGHT;

    float tileX = (CENTER_GRID_ID - gridX) * GRID_SIZE - x;
    float tileY = (CENTER_GRID_ID - gridY) * GRID_SIZE - y;

    return it->second->getLiquidLevel(tileX, tileY);
}

uint8_t MapLoader::GetLiquidType(uint32_t mapId, float x, float y)
{
    uint32_t gridX, gridY;
    worldToGridCoords(x, y, gridX, gridY);

    if (!LoadMapTile(mapId, gridX, gridY))
        return MAP_LIQUID_TYPE_NO_WATER;

    std::lock_guard<std::mutex> lock(m_mutex);

    auto it = m_loadedTiles.find(makeKey(mapId, gridX, gridY));
    if (it == m_loadedTiles.end())
        return MAP_LIQUID_TYPE_NO_WATER;

    float tileX = (CENTER_GRID_ID - gridX) * GRID_SIZE - x;
    float tileY = (CENTER_GRID_ID - gridY) * GRID_SIZE - y;

    return it->second->getLiquidType(tileX, tileY);
}

uint16_t MapLoader::GetAreaId(uint32_t mapId, float x, float y)
{
    uint32_t gridX, gridY;
    worldToGridCoords(x, y, gridX, gridY);

    if (!LoadMapTile(mapId, gridX, gridY))
        return 0;

    std::lock_guard<std::mutex> lock(m_mutex);

    auto it = m_loadedTiles.find(makeKey(mapId, gridX, gridY));
    if (it == m_loadedTiles.end())
        return 0;

    float tileX = (CENTER_GRID_ID - gridX) * GRID_SIZE - x;
    float tileY = (CENTER_GRID_ID - gridY) * GRID_SIZE - y;

    return it->second->getArea(tileX, tileY);
}

size_t MapLoader::GetLoadedTileCount() const
{
    std::lock_guard<std::mutex> lock(m_mutex);
    return m_loadedTiles.size();
}

bool MapLoader::IsTileLoaded(uint32_t mapId, uint32_t x, uint32_t y) const
{
    std::lock_guard<std::mutex> lock(m_mutex);
    return m_loadedTiles.find(makeKey(mapId, x, y)) != m_loadedTiles.end();
}