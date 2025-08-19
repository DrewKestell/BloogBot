// MapLoader.cpp - Complete vMaNGOS-style map loader with separate load methods and detailed logging
#include "MapLoader.h"
#include "VMapDefinitions.h"
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
    unloadData();
}

bool GridMap::loadAreaData(FILE* in, uint32_t offset, uint32_t size)
{
    if (fseek(in, offset, SEEK_SET) != 0)
    {
        return false;
    }

    // Check for AREA header or raw data
    char peekBuffer[8];
    if (fread(peekBuffer, 1, 8, in) != 8)
    {
        return false;
    }

    fseek(in, offset, SEEK_SET); // Reset

    uint32_t possibleMagic = *reinterpret_cast<uint32_t*>(peekBuffer);
    uint32_t expectedAreaMagic = *reinterpret_cast<const uint32_t*>(MAP_AREA_MAGIC);

    if (possibleMagic == expectedAreaMagic)
    {
        // Has AREA header
        MapAreaHeader areaHeader;
        if (fread(&areaHeader, sizeof(MapAreaHeader), 1, in) != 1)
        {
            return false;
        }

        m_areaHeader = new MapAreaHeader(areaHeader);

        if (!(areaHeader.flags & MAP_AREA_NO_AREA))
        {
            m_areaMap = new uint16_t[16 * 16];
            if (fread(m_areaMap, sizeof(uint16_t), 16 * 16, in) != 16 * 16)
            {
                delete[] m_areaMap;
                m_areaMap = nullptr;
                return false;
            }
        }
    }
    else
    {
        if (size >= 16 * 16 * sizeof(uint16_t))
        {
            m_areaMap = new uint16_t[16 * 16];
            fseek(in, offset, SEEK_SET); // Reset to offset
            if (fread(m_areaMap, sizeof(uint16_t), 16 * 16, in) != 16 * 16)
            {
                delete[] m_areaMap;
                m_areaMap = nullptr;
                return false;
            }
        }
    }

    return true;
}

bool GridMap::loadHeightData(FILE* in, uint32_t offset, uint32_t size)
{
    if (fseek(in, offset, SEEK_SET) != 0)
    {
        return false;
    }

    // Peek at first 16 bytes to determine format
    char peekBuffer[16];
    if (fread(peekBuffer, 1, 16, in) != 16)
    {
        return false;
    }
    fseek(in, offset, SEEK_SET); // Reset position

    // Check if it's a MHGT header or raw data
    uint32_t possibleMagic = *reinterpret_cast<uint32_t*>(peekBuffer);
    uint32_t expectedHeightMagic = *reinterpret_cast<const uint32_t*>(MAP_HEIGHT_MAGIC);

    bool hasHeader = (possibleMagic == expectedHeightMagic);

    if (hasHeader)
    {
        // Read the header
        MapHeightHeader heightHeader;
        if (fread(&heightHeader, sizeof(MapHeightHeader), 1, in) != 1)
        {
            return false;
        }

        m_heightHeader = new MapHeightHeader(heightHeader);
        m_gridHeight = heightHeader.gridHeight;

        if (!(heightHeader.flags & MAP_HEIGHT_NO_HEIGHT))
        {
            // Read data based on flags
            if (heightHeader.flags & MAP_HEIGHT_AS_INT16)
            {
                m_uint16_V9 = new uint16_t[V9_SIZE_SQ];
                m_uint16_V8 = new uint16_t[V8_SIZE_SQ];

                if (fread(m_uint16_V9, sizeof(uint16_t), V9_SIZE_SQ, in) != V9_SIZE_SQ ||
                    fread(m_uint16_V8, sizeof(uint16_t), V8_SIZE_SQ, in) != V8_SIZE_SQ)
                {
                    delete[] m_uint16_V9;
                    delete[] m_uint16_V8;
                    m_uint16_V9 = nullptr;
                    m_uint16_V8 = nullptr;
                    return false;
                }

                m_gridIntHeightMultiplier = (heightHeader.gridMaxHeight - heightHeader.gridHeight) / 65535;
                m_gridGetHeight = &GridMap::getHeightFromUint16;
            }
            else if (heightHeader.flags & MAP_HEIGHT_AS_INT8)
            {
                m_uint8_V9 = new uint8_t[V9_SIZE_SQ];
                m_uint8_V8 = new uint8_t[V8_SIZE_SQ];

                if (fread(m_uint8_V9, sizeof(uint8_t), V9_SIZE_SQ, in) != V9_SIZE_SQ ||
                    fread(m_uint8_V8, sizeof(uint8_t), V8_SIZE_SQ, in) != V8_SIZE_SQ)
                {
                    delete[] m_uint8_V9;
                    delete[] m_uint8_V8;
                    m_uint8_V9 = nullptr;
                    m_uint8_V8 = nullptr;
                    return false;
                }

                m_gridIntHeightMultiplier = (heightHeader.gridMaxHeight - heightHeader.gridHeight) / 255;
                m_gridGetHeight = &GridMap::getHeightFromUint8;
            }
            else
            {
                m_V9 = new float[V9_SIZE_SQ];
                m_V8 = new float[V8_SIZE_SQ];

                if (fread(m_V9, sizeof(float), V9_SIZE_SQ, in) != V9_SIZE_SQ ||
                    fread(m_V8, sizeof(float), V8_SIZE_SQ, in) != V8_SIZE_SQ)
                {
                    delete[] m_V9;
                    delete[] m_V8;
                    m_V9 = nullptr;
                    m_V8 = nullptr;
                    return false;
                }

                m_gridGetHeight = &GridMap::getHeightFromFloat;
            }
        }
        else
        {
            m_gridGetHeight = &GridMap::getHeightFromFlat;
        }
    }
    else
    {
        size_t expectedV9Size = V9_SIZE_SQ * sizeof(float);
        size_t expectedV8Size = V8_SIZE_SQ * sizeof(float);
        size_t expectedTotalSize = expectedV9Size + expectedV8Size;

        // Allocate arrays
        m_V9 = new float[V9_SIZE_SQ];
        m_V8 = new float[V8_SIZE_SQ];

        for (int i = 0; i < V9_SIZE_SQ; i++)
            m_V9[i] = INVALID_HEIGHT;
        for (int i = 0; i < V8_SIZE_SQ; i++)
            m_V8[i] = INVALID_HEIGHT;

        // Read as much data as available
        fseek(in, offset, SEEK_SET); // Reset to offset
        size_t v9BytesToRead = std::min((size_t)size, expectedV9Size);
        size_t v9FloatsRead = fread(m_V9, sizeof(float), v9BytesToRead / sizeof(float), in);

        if (size > expectedV9Size)
        {
            size_t v8BytesToRead = std::min((size_t)(size - expectedV9Size), expectedV8Size);
            size_t v8FloatsRead = fread(m_V8, sizeof(float), v8BytesToRead / sizeof(float), in);
        }

        m_gridGetHeight = &GridMap::getHeightFromFloat;

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
    }

    return true;
}

bool GridMap::loadHolesData(FILE* in, uint32_t offset, uint32_t size)
{
    if (fseek(in, offset, SEEK_SET) != 0)
    {
        return false;
    }

    // FIX: Holes should be 64 uint16 values (8x8 grid), not 8
    const int HOLES_COUNT = 64;  // 8x8 grid
    if (size < HOLES_COUNT * sizeof(uint16_t))
    {
        return false;
    }

    m_holes = new uint16_t[HOLES_COUNT];
    if (fread(m_holes, sizeof(uint16_t), HOLES_COUNT, in) != HOLES_COUNT)
    {
        delete[] m_holes;
        m_holes = nullptr;
        return false;
    }

    return true;
}

bool GridMap::loadLiquidData(FILE* in, uint32_t offset, uint32_t size)
{
    if (fseek(in, offset, SEEK_SET) != 0)
    {
        return false;
    }

    // Check for MLIQ header or raw data
    char peekBuffer[8];
    if (fread(peekBuffer, 1, 8, in) != 8)
    {
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
            return false;
        }

        m_liquidHeader = new MapLiquidHeader(liquidHeader);

        if (!(liquidHeader.flags & MAP_LIQUID_NO_TYPE))
        {
            uint32_t liquidSize = liquidHeader.width * liquidHeader.height;
            if (liquidSize > 0 && liquidSize < 1000000)  // Sanity check
            {
                m_liquidEntry = new uint16_t[16 * 16];
                m_liquidFlags = new uint8_t[16 * 16];

                if (fread(m_liquidEntry, sizeof(uint16_t), 16 * 16, in) != 16 * 16 ||
                    fread(m_liquidFlags, sizeof(uint8_t), 16 * 16, in) != 16 * 16)
                {
                    delete[] m_liquidEntry;
                    delete[] m_liquidFlags;
                    m_liquidEntry = nullptr;
                    m_liquidFlags = nullptr;
                    return false;
                }
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
                    delete[] m_liquidHeight;
                    m_liquidHeight = nullptr;
                    return false;
                }
            }
        }
    }

    return true;
}

bool GridMap::loadData(const std::string& filename)
{
    // Unload any existing data
    unloadData();

    // Check if file exists
    if (!std::filesystem::exists(filename))
    {
        return false;
    }

    // Open file using FILE* (vMaNGOS style)
    FILE* in = fopen(filename.c_str(), "rb");
    if (!in)
    {
        return false;
    }

    // Get file size
    fseek(in, 0, SEEK_END);
    long fileSize = ftell(in);
    fseek(in, 0, SEEK_SET);

    // Read header
    MapFileHeader header;
    if (fread(&header, sizeof(MapFileHeader), 1, in) != 1)
    {
        fclose(in);
        return false;
    }

    uint8_t* headerBytes = reinterpret_cast<uint8_t*>(&header);

    // Verify magic
    uint32_t expectedMapMagic = *reinterpret_cast<const uint32_t*>(MAP_MAGIC);
    uint32_t expectedVersionMagic = *reinterpret_cast<const uint32_t*>(MAP_VERSION_MAGIC);

    if (header.mapMagic != expectedMapMagic || header.versionMagic != expectedVersionMagic)
    {
        fclose(in);
        return false;
    }

    // Load each section using separate methods (vMaNGOS style)
    bool success = true;

    // Load area data
    if (header.areaMapOffset > 0 && header.areaMapSize > 0)
    {
        if (!loadAreaData(in, header.areaMapOffset, header.areaMapSize))
        {
            success = false;
        }
    }

    // Load holes data
    if (success && header.holesOffset > 0 && header.holesSize > 0)
    {
        if (!loadHolesData(in, header.holesOffset, header.holesSize))
        {
            std::cout << "[GridMap] ERROR: Failed to load holes data!" << std::endl;
        }
    }

    // Load height data
    if (success && header.heightMapOffset > 0)
    {
        if (header.heightMapSize == 0)
        {
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
        }
    }

    fclose(in);

    return success;
}

void GridMap::unloadData()
{
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
    if (!m_gridGetHeight)
    {
        return INVALID_HEIGHT;
    }

    float height = (this->*m_gridGetHeight)(x, y);

    return height;
}

float GridMap::getHeightFromFloat(float x, float y) const
{
    if (!m_V9 || !m_V8)
    {
        return INVALID_HEIGHT;
    }

    // vMaNGOS transformation - this expects WORLD coordinates
    float orig_x = x;
    float orig_y = y;
    x = MAP_RESOLUTION * (32 - x / SIZE_OF_GRIDS);
    y = MAP_RESOLUTION * (32 - y / SIZE_OF_GRIDS);

    int x_int = (int)x;
    int y_int = (int)y;
    float x_frac = x - x_int;  // Get fractional part
    float y_frac = y - y_int;  // Get fractional part
    x_int &= (MAP_RESOLUTION - 1);  // Wrap to 0-127 range
    y_int &= (MAP_RESOLUTION - 1);  // Wrap to 0-127 range

    if (isHole(x_int, y_int))
    {
        return INVALID_HEIGHT;
    }

    // Debug: Show the actual array indices we're about to use
    int v9_idx1 = x_int * 129 + y_int;
    int v9_idx2 = (x_int + 1) * 129 + y_int;
    int v9_idx3 = x_int * 129 + (y_int + 1);
    int v9_idx4 = (x_int + 1) * 129 + (y_int + 1);
    int v8_idx = x_int * 128 + y_int;

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
        }
    }

    // Calculate height
    float result = a * x_frac + b * y_frac + c;

    return result;
}

float GridMap::getHeightFromUint16(float x, float y) const
{
    if (!m_uint16_V9 || !m_uint16_V8)
    {
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

    // Bilinear interpolation
    float h_top = h0 + (h1 - h0) * x_frac;
    float h_bottom = h2 + (h3 - h2) * x_frac;
    float result = h_top + (h_bottom - h_top) * y_frac;

    return result;
}

float GridMap::getHeightFromUint8(float x, float y) const
{
    if (!m_uint8_V9 || !m_uint8_V8)
    {
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

    return result;
}

float GridMap::getHeightFromFlat(float /*x*/, float /*y*/) const
{
    return m_gridHeight;
}

bool GridMap::isHole(int row, int col) const
{
    if (!m_holes)
        return false;

    int cellRow = row / 8;     // 8 squares per cell
    int cellCol = col / 8;
    int holeRow = row % 8 / 2;
    int holeCol = (col - (cellCol * 8)) / 2;

    if (cellRow >= 8 || cellCol >= 8)
        return false;

    uint16_t hole = m_holes[cellRow * 8 + cellCol];  // Row-major order
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
        return m_liquidHeader ? static_cast<uint8_t>(m_liquidHeader->liquidType) : VMAP::MAP_LIQUID_TYPE_NO_WATER;

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

}

MapLoader::~MapLoader()
{
    Shutdown();
}

bool MapLoader::Initialize(const std::string& dataPath)
{
    std::lock_guard<std::mutex> lock(m_mutex);

    if (m_initialized)
    {
        return true;
    }

    m_dataPath = dataPath;
    if (!m_dataPath.empty() && m_dataPath.back() != '/' && m_dataPath.back() != '\\')
        m_dataPath += '/';

    m_initialized = true;

    return true;
}

void MapLoader::Shutdown()
{
    std::lock_guard<std::mutex> lock(m_mutex);

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
    gridX = static_cast<uint32_t>((CENTER_GRID_ID - worldY / GRID_SIZE));
    gridY = static_cast<uint32_t>((CENTER_GRID_ID - worldX / GRID_SIZE));
}

bool MapLoader::LoadMapTile(uint32_t mapId, uint32_t x, uint32_t y)
{
    std::lock_guard<std::mutex> lock(m_mutex);

    uint64_t key = makeKey(mapId, x, y);
    if (m_loadedTiles.find(key) != m_loadedTiles.end())
    {
        return true;
    }

    std::string filename = getMapFileName(mapId, x, y);

    if (!std::filesystem::exists(filename))
    {
        return false;
    }

    auto gridMap = std::make_unique<GridMap>();
    if (!gridMap->loadData(filename))
    {
        return false;
    }

    m_loadedTiles[key] = std::move(gridMap);

    return true;
}

void MapLoader::UnloadMapTile(uint32_t mapId, uint32_t x, uint32_t y)
{
    std::lock_guard<std::mutex> lock(m_mutex);

    m_loadedTiles.erase(makeKey(mapId, x, y));
}

void MapLoader::UnloadAllTiles()
{
    std::lock_guard<std::mutex> lock(m_mutex);

    m_loadedTiles.clear();
}

float MapLoader::GetHeight(uint32_t mapId, float x, float y)
{
    uint32_t gridX, gridY;
    worldToGridCoords(x, y, gridX, gridY);

    // Load the tile if not already loaded
    if (!LoadMapTile(mapId, gridY, gridX))
    {
        return INVALID_HEIGHT;
    }

    std::lock_guard<std::mutex> lock(m_mutex);

    // Get the tile from cache
    auto it = m_loadedTiles.find(makeKey(mapId, gridY, gridX));
    if (it == m_loadedTiles.end())
    {
        return INVALID_HEIGHT;
    }

    float height = it->second->getHeight(x, y);

    return height;
}

float MapLoader::GetLiquidLevel(uint32_t mapId, float x, float y)
{
    uint32_t gridX, gridY;
    worldToGridCoords(x, y, gridX, gridY);

    if (!LoadMapTile(mapId, gridY, gridX))
    {
        return INVALID_HEIGHT;
    }

    std::lock_guard<std::mutex> lock(m_mutex);

    auto it = m_loadedTiles.find(makeKey(mapId, gridY, gridX));
    if (it == m_loadedTiles.end())
    {
        return INVALID_HEIGHT;
    }

    float liquidLevel = it->second->getLiquidLevel(x, y);

    return liquidLevel;
}

uint8_t MapLoader::GetLiquidType(uint32_t mapId, float x, float y)
{
    uint32_t gridX, gridY;
    worldToGridCoords(x, y, gridX, gridY);

    if (!LoadMapTile(mapId, gridY, gridX))
    {
        return VMAP::MAP_LIQUID_TYPE_NO_WATER;
    }

    std::lock_guard<std::mutex> lock(m_mutex);

    auto it = m_loadedTiles.find(makeKey(mapId, gridY, gridX));
    if (it == m_loadedTiles.end())
    {
        return VMAP::MAP_LIQUID_TYPE_NO_WATER;
    }

    uint8_t liquidType = it->second->getLiquidType(x, y);

    return liquidType;
}

uint16_t MapLoader::GetAreaId(uint32_t mapId, float x, float y)
{
    uint32_t gridX, gridY;
    worldToGridCoords(x, y, gridX, gridY);

    if (!LoadMapTile(mapId, gridY, gridX))
    {
        return 0;
    }

    std::lock_guard<std::mutex> lock(m_mutex);

    auto it = m_loadedTiles.find(makeKey(mapId, gridY, gridX));
    if (it == m_loadedTiles.end())
    {
        return 0;
    }

    uint16_t areaId = it->second->getArea(x, y);

    return areaId;
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