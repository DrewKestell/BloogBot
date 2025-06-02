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

#include <iomanip>
#include <string>
#include <sstream>
#include <filesystem>
#include <iostream> // Optional, for debugging output

#include "VMapManager2.h"
#include "Vec3Ray.h"
#include "MapTree.h"
#include "ModelInstance.h"
#include "WorldModel.h"
#include "VMapDefinitions.h"

#define LOG_DEBUG(msg) std::cout << "[DEBUG] " << msg << std::endl
#define LOG_WARN(msg)  std::cerr << "[WARN] " << msg << std::endl
#define LOG_ERROR(msg) std::cerr << "[ERROR] " << msg << std::endl

namespace fs = std::filesystem;

namespace VMAP
{
	bool DefaultIsVMAPDisabledForPtr(unsigned int entry, uint8_t flags)
	{
		return false; // ← Always allow VMAP logic
	}
	/**
	 * @brief Constructor for VMapManager2.
	 */
	VMapManager2::VMapManager2() : IsVMAPDisabledForPtr(nullptr), iLoadedModelFiles(), iInstanceMapTrees()
	{
	}

	VMapManager2::VMapManager2(bool(*IsVMAPDisabledForPtr)(unsigned int, uint8_t))
		: IsVMAPDisabledForPtr(IsVMAPDisabledForPtr), iLoadedModelFiles(), iInstanceMapTrees()
	{
		assert(IsVMAPDisabledForPtr && "IsVMAPDisabledForPtr must not be null");
	}

	/**
	 * @brief Destructor for VMapManager2.
	 * Cleans up all loaded map trees and model files.
	 */
	VMapManager2::~VMapManager2(void)
	{
		for (InstanceTreeMap::iterator i = iInstanceMapTrees.begin(); i != iInstanceMapTrees.end(); ++i)
		{
			delete i->second;
		}
		for (ModelFileMap::iterator i = iLoadedModelFiles.begin(); i != iLoadedModelFiles.end(); ++i)
		{
			delete i->second.getModel();
		}
	}

	VMapManager2* VMapManager2::_instance = nullptr;

	VMapManager2* VMapManager2::Instance()
	{
		if (!_instance)
		{
			_instance = new VMapManager2(&VMAP::DefaultIsVMAPDisabledForPtr);
		}

		return _instance;
	}

	void VMapManager2::Initialize()
	{
		const std::string basePath = "./vmaps";

		for (const auto& entry : fs::directory_iterator(basePath))
		{
			const fs::path& path = entry.path();
			if (!fs::is_regular_file(path))
				continue;

			if (path.extension() == ".vmtree")
			{
				std::string filename = path.stem().string(); // e.g., "451" from "451.vmtree"

				try
				{
					unsigned int mapId = std::stoul(filename);
					for (int tileX = 0; tileX < 64; ++tileX)
					{
						for (int tileY = 0; tileY < 64; ++tileY)
						{
							if (StaticMapTree::CanLoadMap(basePath, mapId, tileX, tileY))
							{
								VMAPLoadResult result = loadMap(basePath.c_str(), mapId, tileX, tileY);
								if (result != VMAP_LOAD_RESULT_OK)
								{
									std::cerr << "Failed to load VMAP tile (" << tileX << "," << tileY << ") for mapId: " << mapId << std::endl;
								}
							}
						}
					}
				}
				catch (const std::exception&)
				{
					std::cerr << "Skipping file (invalid map ID): " << filename << "\n";
				}
			}
		}
	}

	void VMapManager2::Release()
	{
		// Clean up all maps and models
		for (auto& pair : iInstanceMapTrees)
			delete pair.second;
		iInstanceMapTrees.clear();

		iLoadedModelFiles.clear(); // relies on ManagedModel cleanup

		DestroyInstance(); // destroys singleton instance
	}

	void VMapManager2::DestroyInstance()
	{
		delete _instance;
		_instance = nullptr;
	}

	/**
	 * @brief Converts a position from the game world to the internal representation.
	 *
	 * @param x The x-coordinate in the game world.
	 * @param y The y-coordinate in the game world.
	 * @param z The z-coordinate in the game world.
	 * @return Vec3 The converted position.
	 */
	Vec3 VMapManager2::convertPositionToInternalRep(float x, float y, float z) const
	{
		Vec3 pos;
		const float mid = 0.5f * 64.0f * 533.33333333f;
		pos.x = mid - x;
		pos.y = mid - y;
		pos.z = z;

		return pos;
	}

	/**
	 * @brief Generates the map file name based on the map ID.
	 *
	 * @param pMapId The map ID.
	 * @return std::string The generated map file name.
	 */
	std::string VMapManager2::getMapFileName(unsigned int pMapId)
	{
		std::stringstream fname;
		fname.width(3);
		fname << std::setfill('0') << pMapId << std::string(MAP_FILENAME_EXTENSION2);
		return fname.str();
	}

	/**
	 * @brief Loads a map tile.
	 *
	 * @param pBasePath The base path to the map files.
	 * @param pMapId The map ID.
	 * @param x The x-coordinate of the tile.
	 * @param y The y-coordinate of the tile.
	 * @return VMAPLoadResult The result of the load operation.
	 */
	VMAPLoadResult VMapManager2::loadMap(const char* pBasePath, unsigned int pMapId, int x, int y)
	{
		VMAPLoadResult result = VMAP_LOAD_RESULT_IGNORED;
		if (isMapLoadingEnabled())
		{
			if (_loadMap(pMapId, pBasePath, x, y))
			{
				result = VMAP_LOAD_RESULT_OK;
			}
			else
			{
				result = VMAP_LOAD_RESULT_ERROR;
			}
		}
		return result;
	}

	/**
	 * @brief Internal method to load a map tile.
	 *
	 * @param pMapId The map ID.
	 * @param basePath The base path to the map files.
	 * @param tileX The x-coordinate of the tile.
	 * @param tileY The y-coordinate of the tile.
	 * @return bool True if the tile was loaded successfully, false otherwise.
	 */
	bool VMapManager2::_loadMap(unsigned int mapId, const std::string& basePath, unsigned int tileX, unsigned int tileY)
	{
		auto instanceTree = iInstanceMapTrees.find(mapId);
		bool isNewTree = false;

		if (instanceTree == iInstanceMapTrees.end())
		{
			std::string mapFileName = getMapFileName(mapId);

			StaticMapTree* newTree = new StaticMapTree(mapId, basePath);
			if (!newTree->InitMap(mapFileName, this))
			{
				delete newTree;
				return false;
			}

			instanceTree = iInstanceMapTrees.insert({ mapId, newTree }).first;
			isNewTree = true;
		}

		StaticMapTree* tree = instanceTree->second;

		if (!tree)
		{
			std::cerr << "[VMAP][Error] instanceTree->second is NULL after insert for mapId=" << mapId << std::endl;
			return false;
		}

		bool result = tree->LoadMapTile(tileX, tileY, this);

		return result;
	}



	/**
	 * @brief Unloads a map.
	 *
	 * @param pMapId The map ID.
	 */
	void VMapManager2::unloadMap(unsigned int pMapId)
	{
		InstanceTreeMap::iterator instanceTree = iInstanceMapTrees.find(pMapId);
		if (instanceTree != iInstanceMapTrees.end())
		{
			instanceTree->second->UnloadMap(this);
			if (instanceTree->second->numLoadedTiles() == 0)
			{
				delete instanceTree->second;
				iInstanceMapTrees.erase(pMapId);
			}
		}
	}

	/**
	 * @brief Unloads a specific map tile.
	 *
	 * @param pMapId The map ID.
	 * @param x The x-coordinate of the tile.
	 * @param y The y-coordinate of the tile.
	 */
	void VMapManager2::unloadMap(unsigned int pMapId, int x, int y)
	{
		InstanceTreeMap::iterator instanceTree = iInstanceMapTrees.find(pMapId);
		if (instanceTree != iInstanceMapTrees.end())
		{
			instanceTree->second->UnloadMapTile(x, y, this);
			if (instanceTree->second->numLoadedTiles() == 0)
			{
				delete instanceTree->second;
				iInstanceMapTrees.erase(pMapId);
			}
		}
	}

	/**
	 * @brief Checks if there is a line of sight between two points.
	 *
	 * @param pMapId The map ID.
	 * @param x1 The x-coordinate of the first point.
	 * @param y1 The y-coordinate of the first point.
	 * @param z1 The z-coordinate of the first point.
	 * @param x2 The x-coordinate of the second point.
	 * @param y2 The y-coordinate of the second point.
	 * @param z2 The z-coordinate of the second point.
	 * @return bool True if there is a line of sight, false otherwise.
	 */
	bool VMapManager2::isInLineOfSight(unsigned int pMapId, float x1, float y1, float z1, float x2, float y2, float z2)
	{
		if (!isLineOfSightCalcEnabled() || IsVMAPDisabledForPtr(pMapId, VMAP_DISABLE_LOS))
		{
			return true;
		}

		bool result = true;
		InstanceTreeMap::iterator instanceTree = iInstanceMapTrees.find(pMapId);
		if (instanceTree != iInstanceMapTrees.end())
		{
			Vec3 pos1 = convertPositionToInternalRep(x1, y1, z1);
			Vec3 pos2 = convertPositionToInternalRep(x2, y2, z2);
			if (pos1 != pos2)
			{
				result = instanceTree->second->isInLineOfSight(pos1, pos2);
			}
		}
		return result;
	}

	/**
	 * @brief Gets the hit position of an object in the line of sight.
	 *
	 * @param pMapId The map ID.
	 * @param x1 The x-coordinate of the first point.
	 * @param y1 The y-coordinate of the first point.
	 * @param z1 The z-coordinate of the first point.
	 * @param x2 The x-coordinate of the second point.
	 * @param y2 The y-coordinate of the second point.
	 * @param z2 The z-coordinate of the second point.
	 * @param rx The x-coordinate of the hit position.
	 * @param ry The y-coordinate of the hit position.
	 * @param rz The z-coordinate of the hit position.
	 * @param pModifyDist The distance to modify the hit position.
	 * @return bool True if an object was hit, false otherwise.
	 */
	bool VMapManager2::getObjectHitPos(unsigned int mapId,
		float x1, float y1, float z1,
		float x2, float y2, float z2,
		float& outX, float& outY, float& outZ,
		float modifyDist)
	{
		outX = x2;
		outY = y2;
		outZ = z2;

		if (!isLineOfSightCalcEnabled() || (IsVMAPDisabledForPtr && IsVMAPDisabledForPtr(mapId, VMAP_DISABLE_LOS)))
		{
			std::cout << "[VMAP][HitPos] LOS disabled for mapId=" << mapId << std::endl;
			return false;
		}

		auto instanceTree = iInstanceMapTrees.find(mapId);
		if (instanceTree == iInstanceMapTrees.end() || !instanceTree->second)
		{
			std::cout << "[VMAP][HitPos] No tree found for mapId=" << mapId << std::endl;
			return false;
		}

		Vec3 pos1 = convertPositionToInternalRep(x1, y1, z1);
		Vec3 pos2 = convertPositionToInternalRep(x2, y2, z2);

		Vec3 resultPos;
		bool result = instanceTree->second->getObjectHitPos(pos1, pos2, resultPos, modifyDist);

		if (result)
		{
			Vec3 finalPos = convertPositionToInternalRep(resultPos.x, resultPos.y, resultPos.z);
			outX = finalPos.x;
			outY = finalPos.y;
			outZ = finalPos.z;
		}

		return result;
	}

	/**
	 * @brief Gets the height at a specific position.
	 *
	 * @param pMapId The map ID.
	 * @param x The x-coordinate of the position.
	 * @param y The y-coordinate of the position.
	 * @param z The z-coordinate of the position.
	 * @param maxSearchDist The maximum search distance.
	 * @return float The height at the position, or VMAP_INVALID_HEIGHT_VALUE if no height is available.
	 */
	float VMapManager2::getHeight(unsigned int mapId, float x, float y, float z, float maxSearchDist)
	{
		float height = VMAP_INVALID_HEIGHT_VALUE;
		if (isHeightCalcEnabled() && !IsVMAPDisabledForPtr(mapId, VMAP_DISABLE_HEIGHT))
		{
			auto instanceTree = iInstanceMapTrees.find(mapId);
			if (instanceTree != iInstanceMapTrees.end())
			{
				Vec3 pos = convertPositionToInternalRep(x, y, z);
				height = instanceTree->second->getHeight(pos, maxSearchDist);

				if (height >= finf())
					height = VMAP_INVALID_HEIGHT_VALUE;
			}
			else
			{
				std::cout << "[VMAP][Height] No instance tree for mapId=" << mapId << std::endl;
			}
		}
		else
		{
			std::cout << "[VMAP][Height] Height disabled for mapId=" << mapId << std::endl;
		}

		return height;
	}

	/**
	 * @brief Gets area information at a specific position.
	 *
	 * @param pMapId The map ID.
	 * @param x The x-coordinate of the position.
	 * @param y The y-coordinate of the position.
	 * @param z The z-coordinate of the position.
	 * @param flags The area flags.
	 * @param adtId The ADT ID.
	 * @param rootId The root ID.
	 * @param groupId The group ID.
	 * @return bool True if area information was retrieved, false otherwise.
	 */
	bool VMapManager2::getAreaInfo(unsigned int pMapId, float x, float y, float& z, unsigned int& flags, int& adtId, int& rootId, int& groupId) const
	{
		bool result = false;
		if (!IsVMAPDisabledForPtr(pMapId, VMAP_DISABLE_AREAFLAG))
		{
			InstanceTreeMap::const_iterator instanceTree = iInstanceMapTrees.find(pMapId);
			if (instanceTree != iInstanceMapTrees.end())
			{
				Vec3 pos = convertPositionToInternalRep(x, y, z);
				result = instanceTree->second->getAreaInfo(pos, flags, adtId, rootId, groupId);
				// z is not touched by convertPositionToMangosRep(), so just copy
				z = pos.z;
			}
		}
		return result;
	}

	/**
	 * @brief Gets the liquid level at a specific position.
	 *
	 * @param pMapId The map ID.
	 * @param x The x-coordinate of the position.
	 * @param y The y-coordinate of the position.
	 * @param z The z-coordinate of the position.
	 * @param ReqLiquidType The required liquid type.
	 * @param level The liquid level.
	 * @param floor The floor level.
	 * @param type The liquid type.
	 * @return bool True if the liquid level was retrieved, false otherwise.
	 */
	bool VMapManager2::GetLiquidLevel(unsigned int mapId, float x, float y, float z, uint8_t reqLiquidType, float& level, float& floor, unsigned int& type) const
	{
		std::cout << "[VMAP][Liquid] Query at (" << x << ", " << y << ", " << z << ") mapId=" << mapId << " reqType=" << (int)reqLiquidType << std::endl;

		if (!IsVMAPDisabledForPtr(mapId, VMAP_DISABLE_LIQUIDSTATUS))
		{
			auto instanceTree = iInstanceMapTrees.find(mapId);
			if (instanceTree != iInstanceMapTrees.end())
			{
				LocationInfo info;
				Vec3 pos = convertPositionToInternalRep(x, y, z);
				if (instanceTree->second->GetLocationInfo(pos, info))
				{
					floor = info.ground_Z;
					type = info.hitModel->GetLiquidType();

					std::cout << "[VMAP][Liquid] Liquid type = " << type << ", floorZ = " << floor << std::endl;

					if (reqLiquidType && !(type & reqLiquidType))
					{
						std::cout << "[VMAP][Liquid] Liquid type mismatch. Required: " << (int)reqLiquidType << ", Found: " << type << std::endl;
						return false;
					}

					if (info.hitInstance->GetLiquidLevel(pos, info, level))
					{
						std::cout << "[VMAP][Liquid] Liquid level = " << level << std::endl;
						return true;
					}

					std::cout << "[VMAP][Liquid] GetLiquidLevel failed." << std::endl;
				}
				else
				{
					std::cout << "[VMAP][Liquid] GetLocationInfo failed." << std::endl;
				}
			}
			else
			{
				std::cout << "[VMAP][Liquid] No tree found for mapId=" << mapId << std::endl;
			}
		}

		return false;
	}

	/**
	 * @brief Acquires a model instance.
	 *
	 * @param basepath The base path to the model files.
	 * @param filename The name of the model file.
	 * @param flags The flags for the model.
	 * @return WorldModel* The acquired model instance.
	 */
	WorldModel* VMapManager2::acquireModelInstance(const std::string& basepath, const std::string& filename, unsigned int flags/* Only used when creating the model */)
	{
		ModelFileMap::iterator model = iLoadedModelFiles.find(filename);
		if (model == iLoadedModelFiles.end())
		{
			WorldModel* worldmodel = new WorldModel();
			if (!worldmodel->ReadFile(basepath + filename + ".vmo"))
			{
				ERROR_LOG("VMapManager2: could not load '%s%s.vmo'!", basepath.c_str(), filename.c_str());
				delete worldmodel;
				return NULL;
			}
			DEBUG_FILTER_LOG(LOG_FILTER_MAP_LOADING, "VMapManager2: loading file '%s%s'.", basepath.c_str(), filename.c_str());
			worldmodel->Flags = flags;
			model = iLoadedModelFiles.insert(std::pair<std::string, ManagedModel>(filename, ManagedModel())).first;
			model->second.setModel(worldmodel);
		}
		model->second.incRefCount();
		return model->second.getModel();
	}

	/**
	 * @brief Releases a model instance.
	 *
	 * @param filename The name of the model file.
	 */
	void VMapManager2::releaseModelInstance(const std::string& filename)
	{
		ModelFileMap::iterator model = iLoadedModelFiles.find(filename);
		if (model == iLoadedModelFiles.end())
		{
			ERROR_LOG("VMapManager2: trying to unload non-loaded file '%s'!", filename.c_str());
			return;
		}
		if (model->second.decRefCount() == 0)
		{
			DEBUG_FILTER_LOG(LOG_FILTER_MAP_LOADING, "VMapManager2: unloading file '%s'", filename.c_str());
			delete model->second.getModel();
			iLoadedModelFiles.erase(model);
		}
	}

	/**
	 * @brief Checks if a map exists.
	 *
	 * @param pBasePath The base path to the map files.
	 * @param pMapId The map ID.
	 * @param x The x-coordinate of the tile.
	 * @param y The y-coordinate of the tile.
	 * @return bool True if the map exists, false otherwise.
	 */
	bool VMapManager2::existsMap(const char* pBasePath, unsigned int pMapId, int x, int y)
	{
		return StaticMapTree::CanLoadMap(std::string(pBasePath), pMapId, x, y);
	}
} // namespace VMAP