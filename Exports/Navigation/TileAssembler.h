/**
 * MaNGOS is a full featured server for World of Warcraft, supporting
 * the following clients: 1.12.x, 2.4.3, 3.3.5a, 4.3.4a and 5.4.8
 *
 * Copyright (C) 2005-2025 MaNGOS <https://www.getmangos.eu>
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 * World of Warcraft, and all World of Warcraft or Warcraft art, images,
 * and lore are copyrighted by Blizzard Entertainment, Inc.
 */

#ifndef MANGOS_H_TILEASSEMBLER
#define MANGOS_H_TILEASSEMBLER

#include <map>
#include <set>

#include "Table.h"
#include "ModelInstance.h"
#include "WorldModel.h"

namespace VMAP
{
    /**
     * @brief This Class is used to convert raw vector data into balanced BSP-Trees.
     *
     */
    class ModelPosition
    {
    private:
        Matrix3 iRotation; /**< Rotation matrix for the model */
    public:
        Vec3 iPos; /**< Position of the model */
        Vec3 iDir; /**< Direction of the model */
        float iScale; /**< Scale of the model */

        /**
            * @brief Constructor to initialize member variables.
            *
            * Initializes iPos, iDir, and iScale to default values.
            */
        ModelPosition() : iPos(Vec3::zero()), iDir(Vec3::zero()), iScale(1.0f) {}

        /**
            * @brief Initializes the rotation matrix based on the direction
            */
        void init()
        {
            iRotation = Matrix3::fromEulerAnglesZYX(pi() * iDir.y / 180.f, pi() * iDir.x / 180.f, pi() * iDir.z / 180.f);
        }

        /**
            * @brief Transforms a given vector by the model's position and rotation
            *
            * @param pIn The input vector to transform
            * @return Vec3 The transformed vector
            */
        Vec3 transform(const Vec3& pIn) const;

        /**
            * @brief Moves the model's position to the base position
            *
            * @param pBasePos The base position to move to
            */
        void moveToBasePos(const Vec3& pBasePos) { iPos -= pBasePos; }
    };

    /**
     * @brief Map of unique model entries
     */
    typedef std::map<unsigned int, ModelSpawn> UniqueEntryMap;
    /**
     * @brief Multimap of tile entries
     */
    typedef std::multimap<unsigned int, unsigned int> TileMap;

    /**
     * @brief Structure to hold map spawns
     */
    struct MapSpawns
    {
        UniqueEntryMap UniqueEntries; /**< Unique model entries */
        TileMap TileEntries; /**< Tile entries */
    };

    /**
     * @brief Map of map data
     */
    typedef std::map<unsigned int, MapSpawns*> MapData;
    
    /**
     * @brief Class to assemble tiles from raw data
     */
    class TileAssembler
    {
    private:
        std::string iDestDir; /**< Destination directory */
        std::string iSrcDir; /**< Source directory */
        /**
         * @brief Function pointer for the model name filter method
         *
         * @param pName The name of the model
         * @return bool True if the model name passes the filter, false otherwise
         */
        bool (*iFilterMethod)(char* pName);
        Table<std::string, unsigned int > iUniqueNameIds; /**< Table of unique name IDs */
        unsigned int iCurrentUniqueNameId; /**< Current unique name ID */
        MapData mapData; /**< Map data */
        std::set<std::string> spawnedModelFiles; /**< Set of spawned model files */

    public:
        /**
         * @brief Constructor to initialize the TileAssembler
         *
         * @param pSrcDirName The source directory name
         * @param pDestDirName The destination directory name
         */
        TileAssembler(const std::string& pSrcDirName, const std::string& pDestDirName);
        /**
         * @brief Destructor to clean up resources
         */
        virtual ~TileAssembler();

        /**
         * @brief Converts the world data to a different format
         *
         * @param RAW_VMAP_MAGIC The validation string to verify the file header
         * @return bool True if successful, false otherwise
         */
        bool convertWorld2(const char* RAW_VMAP_MAGIC);
        /**
         * @brief Reads the map spawns from a file
         *
         * @return bool True if successful, false otherwise
         */
        bool readMapSpawns();
        /**
         * @brief Calculates the transformed bounding box for a model spawn
         *
         * @param spawn The model spawn to calculate the bounding box for
         * @param RAW_VMAP_MAGIC The validation string to verify the file header
         * @return bool True if successful, false otherwise
         */
        bool calculateTransformedBound(ModelSpawn& spawn, const char* RAW_VMAP_MAGIC) const;

        /**
         * @brief Exports the game object models
         *
         * @param RAW_VMAP_MAGIC The validation string to verify the file header
         */
        void exportGameobjectModels(const char* RAW_VMAP_MAGIC);
        /**
         * @brief Sets the model name filter method
         *
         * @param pFilterMethod The filter method to set
         */
        void setModelNameFilterMethod(bool (*pFilterMethod)(char* pName)) { iFilterMethod = pFilterMethod; }
        /**
         * @brief Gets the directory entry name from the model name
         *
         * @param pMapId The map ID
         * @param pModPosName The model position name
         * @return std::string The directory entry name
         */
        std::string getDirEntryNameFromModName(unsigned int pMapId, const std::string& pModPosName);
        /**
         * @brief Gets the unique name ID for a given name
         *
         * @param pName The name to get the unique ID for
         * @return unsigned int The unique name ID
         */
        unsigned int getUniqueNameId(const std::string pName);
    };
}
#endif