// MapLoader.h - Complete vMaNGOS-style map loader with separate load methods
#pragma once

#include <cstdint>
#include <string>
#include <unordered_map>
#include <memory>
#include <mutex>
#include <cstdio>

// Map file format constants (matching vMaNGOS)
namespace MapFormat
{
    // Grid constants
    constexpr int V9_SIZE = 129;
    constexpr int V9_SIZE_SQ = V9_SIZE * V9_SIZE;
    constexpr int V8_SIZE = 128;
    constexpr int V8_SIZE_SQ = V8_SIZE * V8_SIZE;
    constexpr int MAP_RESOLUTION = 128;

    constexpr float GRID_SIZE = 533.33333f;
    constexpr float GRID_PART_SIZE = GRID_SIZE / float(V8_SIZE);
    constexpr float SIZE_OF_GRIDS = GRID_SIZE;

    // Map file version
    constexpr char MAP_MAGIC[] = "MAPS";
    constexpr char MAP_VERSION_MAGIC[] = "z1.4";
    constexpr char MAP_AREA_MAGIC[] = "AREA";
    constexpr char MAP_HEIGHT_MAGIC[] = "MHGT";
    constexpr char MAP_LIQUID_MAGIC[] = "MLIQ";

    constexpr float INVALID_HEIGHT = -100000.0f;
    constexpr float INVALID_HEIGHT_VALUE = INVALID_HEIGHT;
    constexpr float MAX_HEIGHT = 100000.0f;

    // Height flags
    constexpr uint32_t MAP_HEIGHT_NO_HEIGHT = 0x0001;
    constexpr uint32_t MAP_HEIGHT_AS_INT16 = 0x0002;
    constexpr uint32_t MAP_HEIGHT_AS_INT8 = 0x0004;

    // Area flags
    constexpr uint32_t MAP_AREA_NO_AREA = 0x0001;

    // Liquid flags
    constexpr uint32_t MAP_LIQUID_NO_TYPE = 0x0001;
    constexpr uint32_t MAP_LIQUID_NO_HEIGHT = 0x0002;

#pragma pack(push, 1)
    // Map file header (44 bytes)
    struct MapFileHeader
    {
        uint32_t mapMagic;
        uint32_t versionMagic;
        uint32_t areaMapOffset;    // <-- No buildMagic here!
        uint32_t areaMapSize;
        uint32_t heightMapOffset;
        uint32_t heightMapSize;
        uint32_t liquidMapOffset;
        uint32_t liquidMapSize;
        uint32_t holesOffset;
        uint32_t holesSize;
    };

    // Map height header
    struct MapHeightHeader
    {
        uint32_t fourcc;
        uint32_t flags;
        float gridHeight;
        float gridMaxHeight;
    };

    // Map liquid header
    struct MapLiquidHeader
    {
        uint32_t fourcc;
        uint16_t flags;
        uint16_t liquidType;
        uint8_t offsetX;
        uint8_t offsetY;
        uint8_t width;
        uint8_t height;
        float liquidLevel;
    };

    // Area map header
    struct MapAreaHeader
    {
        uint32_t fourcc;
        uint16_t flags;
        uint16_t gridArea;
    };
#pragma pack(pop)

    // GridMap class - holds one map tile
    class GridMap
    {
    private:
        MapHeightHeader* m_heightHeader = nullptr;
        MapLiquidHeader* m_liquidHeader = nullptr;
        MapAreaHeader* m_areaHeader = nullptr;

        // Height data - V9 for corners, V8 for centers
        union
        {
            float* m_V9 = nullptr;
            uint16_t* m_uint16_V9;
            uint8_t* m_uint8_V9;
        };

        union
        {
            float* m_V8 = nullptr;
            uint16_t* m_uint16_V8;
            uint8_t* m_uint8_V8;
        };

        // Liquid data
        float* m_liquidHeight = nullptr;
        uint8_t* m_liquidFlags = nullptr;
        uint16_t* m_liquidEntry = nullptr;

        // Area data
        uint16_t* m_areaMap = nullptr;

        // Holes in terrain
        uint16_t* m_holes = nullptr;

        // Height calculation
        float m_gridHeight = 0.0f;
        float m_gridIntHeightMultiplier = 0.0f;

        // Function pointer for height method
        typedef float (GridMap::* GetHeightPtr)(float x, float y) const;
        GetHeightPtr m_gridGetHeight = nullptr;

        // Height calculation methods
        float getHeightFromFloat(float x, float y) const;
        float getHeightFromUint16(float x, float y) const;
        float getHeightFromUint8(float x, float y) const;
        float getHeightFromFlat(float x, float y) const;

        // Internal helpers
        bool isHole(int row, int col) const;

        // Separate load methods (vMaNGOS style)
        bool loadAreaData(FILE* in, uint32_t offset, uint32_t size);
        bool loadHeightData(FILE* in, uint32_t offset, uint32_t size);
        bool loadHolesData(FILE* in, uint32_t offset, uint32_t size);
        bool loadLiquidData(FILE* in, uint32_t offset, uint32_t size);

    public:
        GridMap() = default;
        ~GridMap();

        bool loadData(const std::string& filename);
        void unloadData();

        float getHeight(float x, float y) const;
        float getLiquidLevel(float x, float y) const;
        uint8_t getLiquidType(float x, float y) const;
        uint16_t getArea(float x, float y) const;
    };
}

// MapLoader main class
constexpr float CENTER_GRID_ID = 32.0f;

class MapLoader
{
private:
    std::unordered_map<uint64_t, std::unique_ptr<MapFormat::GridMap>> m_loadedTiles;
    std::string m_dataPath;
    mutable std::mutex m_mutex;
    bool m_initialized = false;

    // Helper functions
    std::string getMapFileName(uint32_t mapId, uint32_t x, uint32_t y) const;
    uint64_t makeKey(uint32_t mapId, uint32_t x, uint32_t y) const;
    void worldToGridCoords(float worldX, float worldY, uint32_t& gridX, uint32_t& gridY) const;

public:
    MapLoader();
    ~MapLoader();

    bool Initialize(const std::string& dataPath);
    void Shutdown();

    bool LoadMapTile(uint32_t mapId, uint32_t x, uint32_t y);
    void UnloadMapTile(uint32_t mapId, uint32_t x, uint32_t y);
    void UnloadAllTiles();

    float GetHeight(uint32_t mapId, float x, float y);
    float GetLiquidLevel(uint32_t mapId, float x, float y);
    uint8_t GetLiquidType(uint32_t mapId, float x, float y);
    uint16_t GetAreaId(uint32_t mapId, float x, float y);

    size_t GetLoadedTileCount() const;
    bool IsTileLoaded(uint32_t mapId, uint32_t x, uint32_t y) const;
    bool IsInitialized() const { return m_initialized; }
};