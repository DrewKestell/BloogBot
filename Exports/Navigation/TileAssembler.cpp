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

#include "TileAssembler.h"
#include "MapTree.h"
#include "BIH.h"
#include "VMapDefinitions.h"
#include "WmoLiquid.h"

#include <set>
#include <iomanip>
#include <sstream>
#include <iostream>
#include "VMapFileUtils.h"


using std::pair;

template<> struct BoundsTrait<VMAP::ModelSpawn*>
{
    static void getBounds(VMAP::ModelSpawn const* const& obj, AABox& out)
    {
        out = obj->getBounds();
    }
};

namespace VMAP
{
    /**
     * @brief Transforms a given vector by the model's position and rotation.
     *
     * @param pIn The input vector to transform.
     * @return Vector3 The transformed vector.
     */
    Vec3 ModelPosition::transform(const Vec3& pIn) const
    {
        Vec3 out = pIn * iScale;
        out = iRotation * out;
        return(out);
    }

    //=================================================================

    /**
     * @brief Constructor to initialize the TileAssembler.
     *
     * @param pSrcDirName The source directory name.
     * @param pDestDirName The destination directory name.
     */
    TileAssembler::TileAssembler(const std::string& pSrcDirName, const std::string& pDestDirName)
    {
        iCurrentUniqueNameId = 0;
        iFilterMethod = NULL;
        iSrcDir = pSrcDirName;
        iDestDir = pDestDirName;
        // mkdir(iDestDir);
        // init();
    }

    /**
     * @brief Destructor to clean up resources.
     */
    TileAssembler::~TileAssembler()
    {
        // delete iCoordModelMapping;
    }

    /**
     * @brief Reads the map spawns from a file.
     *
     * @return bool True if successful, false otherwise.
     */
    bool TileAssembler::readMapSpawns()
    {
        std::string fname = iSrcDir + "/dir_bin";
        FILE* dirf = fopen(fname.c_str(), "rb");
        if (!dirf)
        {
            printf("Could not read dir_bin file!\n");
            return false;
        }
        printf("Read coordinate mapping...\n");
        unsigned int mapID, tileX, tileY;
        ModelSpawn spawn;
        unsigned int numSpawns = 0;

        while (true)
        {
            size_t r = fread(&mapID, sizeof(unsigned int), 1, dirf);
            if (r != 1) break;
            r = fread(&tileX, sizeof(unsigned int), 1, dirf);
            if (r != 1) break;
            r = fread(&tileY, sizeof(unsigned int), 1, dirf);
            if (r != 1) break;

            long spawnPos = ftell(dirf);
            if (!ModelSpawn::ReadFromFile(dirf, spawn))
            {
                std::cout << "[TileAssembler][Error] ModelSpawn parse failed at offset " << spawnPos
                    << " for mapID " << mapID << " tile (" << tileX << "," << tileY << ") in file " << fname << std::endl;
                break;
            }
            numSpawns++;

            MapSpawns* current;
            MapData::iterator map_iter = mapData.find(mapID);
            if (map_iter == mapData.end())
            {
                printf("spawning Map %d\n", mapID);
                mapData[mapID] = current = new MapSpawns();
            }
            else
            {
                current = (*map_iter).second;
            }
            current->UniqueEntries.insert(std::pair<unsigned int, ModelSpawn>(spawn.ID, spawn));
            current->TileEntries.insert(std::pair<unsigned int, unsigned int>(StaticMapTree::packTileID(tileX, tileY), spawn.ID));
        }

        bool success = (ferror(dirf) == 0);
        fclose(dirf);

        std::cout << "[TileAssembler] Parsed " << numSpawns << " ModelSpawns from " << fname << (success ? " [OK]" : " [ERROR]") << std::endl;
        return success;
    }

    /**
     * @brief Calculates the transformed bounding box for a model spawn by
     * reading the raw model data and applying position, rotation, and scale.
     *
     * @param spawn The model spawn to calculate the bounding box for.
     * @param RAW_VMAP_MAGIC The vmap magic string for file validation.
     * @return bool True if successful, false otherwise.
     */
    bool TileAssembler::calculateTransformedBound(ModelSpawn& spawn, const char* RAW_VMAP_MAGIC) const
    {
        // Construct full path to the model file
        std::string modelFilename = iSrcDir + "/" + spawn.name;

        // Initialize ModelPosition with rotation and scale
        ModelPosition modelPosition;
        modelPosition.iDir = spawn.iRot;
        modelPosition.iScale = spawn.iScale;
        modelPosition.init();

        // Load the raw model data from disk
        WorldModel_Raw raw_model;
        if (!raw_model.Read(modelFilename.c_str(), RAW_VMAP_MAGIC))
        {
            return false;
        }

        // If the model has multiple groups, it might indicate it's not an M2
        unsigned int groups = raw_model.groupsArray.size();
        if (groups != 1)
        {
            printf("Warning: '%s' does not seem to be a M2 model!\n", modelFilename.c_str());
        }

        // We'll track the bounding box of the entire model
        AABox modelBound;
        bool boundEmpty = true;

        // Iterate over each group to accumulate vertex data
        // Should be only one for M2 files...
        for (unsigned int g = 0; g < groups; ++g)
        {
            std::vector<Vec3>& vertices = raw_model.groupsArray[g].vertexArray;

            // If no vertices, log an error about missing geometry
            if (vertices.empty())
            {
                std::cout << "error: model '" << spawn.name << "' has no geometry!" << std::endl;
                continue;
            }

            // Transform all vertices by rotation and scale, then update bounding box
            unsigned int nvectors = vertices.size();
            for (unsigned int i = 0; i < nvectors; ++i)
            {
                Vec3 v = modelPosition.transform(vertices[i]);
                if (boundEmpty)
                {
                    modelBound = AABox(v, v);
                    boundEmpty = false;
                }
                else
                {
                    modelBound.merge(v);
                }
            }
        }

        // Add the spawn position to shift the bounding box into world space
        spawn.iBound = modelBound + spawn.iPos;
        spawn.flags |= MOD_HAS_BOUND;
        return true;
    }

    /**
     * @brief Obtain a directory entry name from the model name.
     * This currently returns an empty string as an example stub.
     *
     * @param pMapId The current map ID.
     * @param pModPosName The name of the model position resource.
     * @return std::string Always returns an empty string in this implementation.
     */
    std::string TileAssembler::getDirEntryNameFromModName(unsigned int pMapId, const std::string& pModPosName)
    {
        // Stub function: could transform pModPosName based on pMapId.
        return std::string();
    }

    /**
     * @brief Retrieves the unique name ID for a given model name.
     * Currently returns 0 in this stub implementation.
     *
     * @param pName The model name string.
     * @return unsigned int Always returns 0 in this stub implementation.
     */
    unsigned int TileAssembler::getUniqueNameId(const std::string pName)
    {
        // Stub function: would otherwise track and generate unique IDs per name.
        return 0;
    }

    /**
     * @brief Exports game object models by reading a list of display IDs and model paths,
     * then writes them to the destination folder with bounding box info.
     *
     * @param RAW_VMAP_MAGIC The validation string for reading raw model files.
     */
    void TileAssembler::exportGameobjectModels(const char* RAW_VMAP_MAGIC)
    {
        // Open the file that lists gameobject models
        FILE* model_list = fopen((iSrcDir + "/" + GAMEOBJECT_MODELS).c_str(), "rb");
        if (!model_list)
        {
            return;
        }

        // Open a new file in the destination to copy & enrich the data
        FILE* model_list_copy = fopen((iDestDir + "/" + GAMEOBJECT_MODELS).c_str(), "wb");
        if (!model_list_copy)
        {
            fclose(model_list);
            return;
        }

        // Buffers to read the display ID and model name
        unsigned int name_length, displayId;
        char buff[500];

        // Read until reaching the end of the model list file
        while (!feof(model_list))
        {
            // Attempt to read displayId field
            if (fread(&displayId, sizeof(unsigned int), 1, model_list) <= 0)
            {
                // If we haven't truly reached EOF, the file may be corrupt
                if (!feof(model_list))
                {
                    std::cout << "\nFile '" << GAMEOBJECT_MODELS << "' seems to be corrupted" << std::endl;
                }
                break;
            }

            // Attempt to read the length of the model name
            if (fread(&name_length, sizeof(unsigned int), 1, model_list) <= 0)
            {
                std::cout << "\nFile '" << GAMEOBJECT_MODELS << "' seems to be corrupted" << std::endl;
                break;
            }

            // Check for overly large read sizes
            if (name_length >= sizeof(buff))
            {
                std::cout << "\nFile '" << GAMEOBJECT_MODELS << "' seems to be corrupted" << std::endl;
                break;
            }

            // Read the actual model name
            if (fread(&buff, sizeof(char), name_length, model_list) <= 0)
            {
                std::cout << "\nFile '" << GAMEOBJECT_MODELS << "' seems to be corrupted" << std::endl;
                break;
            }
            std::string model_name(buff, name_length);

            // Load model to help gather bounding box info
            WorldModel_Raw raw_model;
            if (!raw_model.Read((iSrcDir + "/" + model_name).c_str(), RAW_VMAP_MAGIC))
            {
                // If it fails, skip updating bounding box data
                continue;
            }

            // Register this model in the 'spawnedModelFiles' set
            spawnedModelFiles.insert(model_name);

            // Compute bounding box from the vertices in all groups
            AABox bounds;
            bool boundEmpty = true;
            for (unsigned int g = 0; g < raw_model.groupsArray.size(); ++g)
            {
                std::vector<Vec3>& vertices = raw_model.groupsArray[g].vertexArray;

                unsigned int nvectors = vertices.size();
                for (unsigned int i = 0; i < nvectors; ++i)
                {
                    Vec3& v = vertices[i];
                    if (boundEmpty)
                    {
                        bounds = AABox(v, v);
                        boundEmpty = false;
                    }
                    else
                    {
                        bounds.merge(v);
                    }
                }
            }

            // Copy data to the new file and append bounding box info
            fwrite(&displayId, sizeof(unsigned int), 1, model_list_copy);
            fwrite(&name_length, sizeof(unsigned int), 1, model_list_copy);
            fwrite(&buff, sizeof(char), name_length, model_list_copy);
            fwrite(&bounds.low(), sizeof(Vec3), 1, model_list_copy);
            fwrite(&bounds.high(), sizeof(Vec3), 1, model_list_copy);
        }

        // Close the files once done
        fclose(model_list);
        fclose(model_list_copy);
    }

    // Undefine macros used for reading, to avoid scope pollution
#undef READ_OR_RETURN
#undef CMP_OR_RETURN
}