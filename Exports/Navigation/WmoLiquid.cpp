#include "Vec3Ray.h"
#include "WmoLiquid.h"
#include "VMapDefinitions.h"

// ===================== WmoLiquid ==================================

    /**
     * @brief Constructor for WmoLiquid.
     *
     * @param width Width of the liquid area.
     * @param height Height of the liquid area.
     * @param corner The lower corner of the liquid area.
     * @param type The type of the liquid.
     */
WmoLiquid::WmoLiquid(unsigned int width, unsigned int height, const Vec3& corner, unsigned int type) :
    iTilesX(width), iTilesY(height), iCorner(corner), iType(type)
{
    iHeight = new float[(width + 1) * (height + 1)];
    iFlags = new uint8_t[width * height];
}

/**
 * @brief Copy constructor for WmoLiquid.
 *
 * @param other The WmoLiquid to copy from.
 */
WmoLiquid::WmoLiquid(const WmoLiquid& other) : iHeight(NULL), iFlags(NULL)
{
    *this = other;                                      // use assignment operator defined below
}

/**
 * @brief Destructor for WmoLiquid.
 */
WmoLiquid::~WmoLiquid()
{
    delete[] iHeight;
    delete[] iFlags;
}

/**
 * @brief Assignment operator for WmoLiquid.
 *
 * @param other The WmoLiquid to assign from.
 * @return WmoLiquid& Reference to the assigned WmoLiquid.
 */
WmoLiquid& WmoLiquid::operator=(const WmoLiquid& other)
{
    if (this == &other)
    {
        return *this;
    }

    iTilesX = other.iTilesX;
    iTilesY = other.iTilesY;
    iCorner = other.iCorner;
    iType = other.iType;
    delete[] iHeight;
    delete[] iFlags;

    if (other.iHeight)
    {
        iHeight = new float[(iTilesX + 1) * (iTilesY + 1)];
        memcpy(iHeight, other.iHeight, (iTilesX + 1) * (iTilesY + 1) * sizeof(float));
    }
    else
    {
        iHeight = NULL;
    }
    if (other.iFlags)
    {
        iFlags = new uint8_t[iTilesX * iTilesY];
        memcpy(iFlags, other.iFlags, iTilesX * iTilesY * sizeof(uint8_t));
    }
    else
    {
        iFlags = NULL;
    }

    return *this;
}

/**
 * @brief Gets the liquid height at a specific position.
 *
 * @param pos The position to check.
 * @param liqHeight The liquid height at the position.
 * @return bool True if the liquid height was retrieved, false otherwise.
 */
bool WmoLiquid::GetLiquidHeight(const Vec3& pos, float& liqHeight) const
{
    float tx_f = (pos.x - iCorner.x) / LIQUID_TILE_SIZE;
    unsigned int tx = unsigned int(tx_f);
    if (tx_f < 0.0f || tx >= iTilesX)
    {
        return false;
    }
    float ty_f = (pos.y - iCorner.y) / LIQUID_TILE_SIZE;
    unsigned int ty = unsigned int(ty_f);
    if (ty_f < 0.0f || ty >= iTilesY)
    {
        return false;
    }

    // check if tile shall be used for liquid level
    // checking for 0x08 *might* be enough, but disabled tiles always are 0x?F:
    if ((iFlags[tx + ty * iTilesX] & 0x0F) == 0x0F)
    {
        return false;
    }

    // (dx, dy) coordinates inside tile, in [0,1]^2
    float dx = tx_f - (float)tx;
    float dy = ty_f - (float)ty;

    /* Tesselate tile to two triangles (not sure if client does it exactly like this)

        ^ dy
        |
      1 x---------x (1,1)
        | (b)   / |
        |     /   |
        |   /     |
        | /   (a) |
        x---------x---> dx
      0           1
    */
    const unsigned int rowOffset = iTilesX + 1;
    if (dx > dy) // case (a)
    {
        float sx = iHeight[tx + 1 + ty * rowOffset] - iHeight[tx + ty * rowOffset];
        float sy = iHeight[tx + 1 + (ty + 1) * rowOffset] - iHeight[tx + 1 + ty * rowOffset];
        liqHeight = iHeight[tx + ty * rowOffset] + dx * sx + dy * sy;
    }
    else // case (b)
    {
        float sx = iHeight[tx + 1 + (ty + 1) * rowOffset] - iHeight[tx + (ty + 1) * rowOffset];
        float sy = iHeight[tx + (ty + 1) * rowOffset] - iHeight[tx + ty * rowOffset];
        liqHeight = iHeight[tx + ty * rowOffset] + dx * sx + dy * sy;
    }
    return true;
}

/**
 * @brief Gets the file size of the liquid data.
 *
 * @return unsigned int The file size of the liquid data.
 */
unsigned int WmoLiquid::GetFileSize() const
{
    return 2 * sizeof(unsigned int) +
        sizeof(Vec3) +
        (iTilesX + 1) * (iTilesY + 1) * sizeof(float) +
        iTilesX * iTilesY;
}

/**
 * @brief Writes the liquid data to a file.
 *
 * @param wf The file to write to.
 * @return bool True if the write was successful, false otherwise.
 */
bool WmoLiquid::WriteToFile(FILE* wf) const
{
    bool result = true;
    if (result && fwrite(&iTilesX, sizeof(unsigned int), 1, wf) != 1)
    {
        result = false;
    }
    if (result && fwrite(&iTilesY, sizeof(unsigned int), 1, wf) != 1)
    {
        result = false;
    }
    if (result && fwrite(&iCorner, sizeof(Vec3), 1, wf) != 1)
    {
        result = false;
    }
    if (result && fwrite(&iType, sizeof(unsigned int), 1, wf) != 1)
    {
        result = false;
    }
    unsigned int size = (iTilesX + 1) * (iTilesY + 1);
    if (result && fwrite(iHeight, sizeof(float), size, wf) != size)
    {
        result = false;
    }
    size = iTilesX * iTilesY;
    if (result && fwrite(iFlags, sizeof(uint8_t), size, wf) != size)
    {
        result = false;
    }
    return result;
}

/**
 * @brief Reads the liquid data from a file.
 *
 * @param rf The file to read from.
 * @param out The WmoLiquid to read into.
 * @return bool True if the read was successful, false otherwise.
 */
bool WmoLiquid::ReadFromFile(FILE* rf, WmoLiquid*& out)
{
    bool result = true;
    WmoLiquid* liquid = new WmoLiquid();
    if (result && fread(&liquid->iTilesX, sizeof(unsigned int), 1, rf) != 1)
    {
        result = false;
    }
    if (result && fread(&liquid->iTilesY, sizeof(unsigned int), 1, rf) != 1)
    {
        result = false;
    }
    if (result && fread(&liquid->iCorner, sizeof(Vec3), 1, rf) != 1)
    {
        result = false;
    }
    if (result && fread(&liquid->iType, sizeof(unsigned int), 1, rf) != 1)
    {
        result = false;
    }
    unsigned int size = (liquid->iTilesX + 1) * (liquid->iTilesY + 1);
    liquid->iHeight = new float[size];
    if (result && fread(liquid->iHeight, sizeof(float), size, rf) != size)
    {
        result = false;
    }
    size = liquid->iTilesX * liquid->iTilesY;
    liquid->iFlags = new uint8_t[size];
    if (result && fread(liquid->iFlags, sizeof(uint8_t), size, rf) != size)
    {
        result = false;
    }
    if (!result)
    {
        delete liquid;
    }
    else
    {
        out = liquid;
    }
    return result;
}