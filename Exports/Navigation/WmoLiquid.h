#ifndef WMOLIQUID_H
#define WMOLIQUID_H

#include "Vec3Ray.h"


struct WMOLiquidHeader
{
	int xverts, yverts, xtiles, ytiles;
	float pos_x;
	float pos_y;
	float pos_z;
	short type;
};

class WmoLiquid
{
public:
	/**
	 * @brief Constructor for WmoLiquid.
	 *
	 * @param width Width of the liquid area.
	 * @param height Height of the liquid area.
	 * @param corner The lower corner of the liquid area.
	 * @param type The type of the liquid.
	 */
	WmoLiquid(unsigned int width, unsigned int height, const Vec3& corner, unsigned int type);
	/**
	 * @brief Copy constructor for WmoLiquid.
	 *
	 * @param other The WmoLiquid to copy from.
	 */
	WmoLiquid(const WmoLiquid& other);
	/**
	 * @brief Destructor for WmoLiquid.
	 */
	~WmoLiquid();
	/**
	 * @brief Assignment operator for WmoLiquid.
	 *
	 * @param other The WmoLiquid to assign from.
	 * @return WmoLiquid& Reference to the assigned WmoLiquid.
	 */
	WmoLiquid& operator=(const WmoLiquid& other);
	/**
	 * @brief Gets the liquid height at a specific position.
	 *
	 * @param pos The position to check.
	 * @param liqHeight The liquid height at the position.
	 * @return bool True if the liquid height was retrieved, false otherwise.
	 */
	bool GetLiquidHeight(const Vec3& pos, float& liqHeight) const;
	/**
	 * @brief Gets the type of the liquid.
	 *
	 * @return unsigned int The type of the liquid.
	 */
	unsigned int GetType() const { return iType; }
	/**
	 * @brief Gets the height storage array.
	 *
	 * @return float* Pointer to the height storage array.
	 */
	float* GetHeightStorage() { return iHeight; }
	/**
	 * @brief Gets the flags storage array.
	 *
	 * @return uint8_t* Pointer to the flags storage array.
	 */
	uint8_t* GetFlagsStorage() { return iFlags; }
	/**
	 * @brief Gets the file size of the liquid data.
	 *
	 * @return unsigned int The file size of the liquid data.
	 */
	unsigned int GetFileSize() const;
	/**
	 * @brief Writes the liquid data to a file.
	 *
	 * @param wf The file to write to.
	 * @return bool True if the write was successful, false otherwise.
	 */
	bool WriteToFile(FILE* wf) const;
	/**
	 * @brief Reads the liquid data from a file.
	 *
	 * @param rf The file to read from.
	 * @param liquid The WmoLiquid to read into.
	 * @return bool True if the read was successful, false otherwise.
	 */
	static bool ReadFromFile(FILE* rf, WmoLiquid*& liquid);
private:
	/**
	 * @brief Default constructor for WmoLiquid.
	 */
	WmoLiquid() : iTilesX(0), iTilesY(0), iCorner(Vec3::zero()), iType(0), iHeight(0), iFlags(0) {}

	unsigned int iTilesX;  /**< Number of tiles in x direction. */
	unsigned int iTilesY;  /**< Number of tiles in y direction. */
	Vec3 iCorner; /**< The lower corner of the liquid area. */
	unsigned int iType;    /**< The type of the liquid. */
	float* iHeight;  /**< Height values for the liquid area. (tilesX + 1)*(tilesY + 1) */
	uint8_t* iFlags;   /**< Flags indicating if a liquid tile is used. */
#ifdef MMAP_GENERATOR
public:
	/**
	 * @brief Gets the position information of the liquid.
	 *
	 * @param tilesX The number of tiles in x direction.
	 * @param tilesY The number of tiles in y direction.
	 * @param corner The lower corner of the liquid area.
	 */
	void getPosInfo(unsigned int& tilesX, unsigned int& tilesY, Vec3& corner) const;
#endif
};
#endif