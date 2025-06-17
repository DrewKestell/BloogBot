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

#include "MapTree.h"
#include "ModelInstance.h"
#include "VMapManager2.h"
#include "VMapDefinitions.h"

#include <string>
#include <sstream>
#include <iomanip>
#include <limits>
#include <iostream>
#include "VMapFileUtils.h"

namespace VMAP
{
	/**
	 * @brief Callback class for ray intersection with models.
	 */
	class MapRayCallback
	{
	public:
		MapRayCallback(ModelInstance* val) : prims(val), hit(false) {}
		bool operator()(Ray const& ray, unsigned int entry, float& distance, bool pStopAtFirstHit = true, bool ignoreM2Model = false)
		{
			bool result = prims[entry].intersectRay(ray, distance, pStopAtFirstHit, ignoreM2Model);
			if (result)
				hit = true;
			return result;
		}
		bool didHit() const
		{
			return hit;
		}
	protected:
		ModelInstance* prims;
		bool hit;
		bool los;
	};

	/**
	 * @brief Callback class for area information.
	 */
	class AreaInfoCallback
	{
	public:
		AreaInfoCallback(ModelInstance* val) : prims(val) {}

		/**
		 * @brief Operator to handle area information retrieval.
		 *
		 * @param point The point to check.
		 * @param entry The entry index.
		 */
		void operator()(const Vec3& point, unsigned int entry)
		{
#ifdef VMAP_DEBUG
			DEBUG_LOG("trying to intersect '%s'", prims[entry].name.c_str());
#endif
			prims[entry].GetAreaInfo(point, aInfo);
		}

		ModelInstance* prims; /**< Pointer to model instances. */
		AreaInfo aInfo; /**< Area information. */
	};

	/**
	 * @brief Callback class for location information.
	 */
	class LocationInfoCallback
	{
	public:
		LocationInfoCallback(ModelInstance* val, LocationInfo& info) : prims(val), locInfo(info), result(false) {}

		/**
		 * @brief Operator to handle location information retrieval.
		 *
		 * @param point The point to check.
		 * @param entry The entry index.
		 */
		void operator()(const Vec3& point, unsigned int entry)
		{
#ifdef VMAP_DEBUG
			DEBUG_LOG("trying to intersect '%s'", prims[entry].name.c_str());
#endif
			if (prims[entry].GetLocationInfo(point, locInfo))
			{
				result = true;
			}
		}

		ModelInstance* prims; /**< Pointer to model instances. */
		LocationInfo& locInfo; /**< Location information. */
		bool result; /**< Flag indicating if location information was found. */
	};

	/**
	 * @brief Generates the tile file name based on map ID, tile X, and tile Y.
	 *
	 * @param mapID The map ID.
	 * @param tileX The tile X coordinate.
	 * @param tileY The tile Y coordinate.
	 * @return std::string The generated tile file name.
	 */
	std::string StaticMapTree::getTileFileName(unsigned int mapID, unsigned int tileX, unsigned int tileY)
	{
		std::stringstream tilefilename;
		tilefilename.fill('0');
		tilefilename << std::setw(3) << mapID << "_";
		tilefilename << std::setw(2) << tileY << "_" << std::setw(2) << tileX << ".vmtile";
		return tilefilename.str();
	}

	/**
	 * @brief Retrieves area information for a given position.
	 *
	 * @param pos The position to check.
	 * @param flags The area flags.
	 * @param adtId The ADT ID.
	 * @param rootId The root ID.
	 * @param groupId The group ID.
	 * @return true if area information was found, false otherwise.
	 */
	bool StaticMapTree::getAreaInfo(Vec3& pos, unsigned int& flags, int& adtId, int& rootId, int& groupId) const
	{
		AreaInfoCallback intersectionCallBack(iTreeValues);
		iTree.intersectPoint(pos, intersectionCallBack);
		if (intersectionCallBack.aInfo.result)
		{
			flags = intersectionCallBack.aInfo.flags;
			adtId = intersectionCallBack.aInfo.adtId;
			rootId = intersectionCallBack.aInfo.rootId;
			groupId = intersectionCallBack.aInfo.groupId;
			pos.z = intersectionCallBack.aInfo.ground_Z;
			return true;
		}
		return false;
	}

	/**
	 * @brief Retrieves location information for a given position.
	 *
	 * @param pos The position to check.
	 * @param info The location information.
	 * @return true if location information was found, false otherwise.
	 */
	bool StaticMapTree::GetLocationInfo(const Vec3& pos, LocationInfo& info) const
	{
		LocationInfoCallback intersectionCallBack(iTreeValues, info);
		iTree.intersectPoint(pos, intersectionCallBack);
		return intersectionCallBack.result;
	}

	/**
	 * @brief Constructor for StaticMapTree.
	 *
	 * @param mapID The map ID.
	 * @param basePath The base path for map files.
	 */
	StaticMapTree::StaticMapTree(unsigned int mapID, const std::string& basePath) :
		iMapID(mapID), iTreeValues(nullptr), iBasePath(basePath), iIsTiled(false), iNTreeValues(0)
	{
		if (iBasePath.length() > 0 && (iBasePath[iBasePath.length() - 1] != '/' || iBasePath[iBasePath.length() - 1] != '\\'))
		{
			iBasePath.append("/");
		}
	}

	/**
	 * @brief Destructor for StaticMapTree.
	 *
	 * Make sure to call unloadMap() to unregister acquired model references before destroying.
	 */
	StaticMapTree::~StaticMapTree()
	{
		delete[] iTreeValues;
	}

	/**
	 * @brief Checks if there is an intersection within the maximum distance.
	 *
	 * @param pRay The ray to check.
	 * @param pMaxDist The maximum distance to check.
	 * @param pStopAtFirstHit Whether to stop at the first hit.
	 * @return true if an intersection is found, false otherwise.
	 */
	bool StaticMapTree::getIntersectionTime(Ray const& pRay, float& pMaxDist, bool pStopAtFirstHit, bool ignoreM2Model) const
	{
		float distance = pMaxDist;
		MapRayCallback intersectionCallBack(iTreeValues);
		iTree.intersectRay(pRay, intersectionCallBack, distance, pStopAtFirstHit, ignoreM2Model);
		if (intersectionCallBack.didHit())
			pMaxDist = distance;
		return intersectionCallBack.didHit();
	}

	/**
	 * @brief Checks if there is a line of sight between two positions.
	 *
	 * @param pos1 The starting position.
	 * @param pos2 The ending position.
	 * @return true if there is a line of sight, false otherwise.
	 */
	bool StaticMapTree::isInLineOfSight(Vec3 const& pos1, Vec3 const& pos2, bool ignoreM2Model) const
	{
		float maxDist = (pos2 - pos1).magnitude();
		// valid map coords should *never ever* produce float overflow, but this would produce NaNs too:
		MANGOS_ASSERT(maxDist < std::numeric_limits<float>::max());
		// prevent NaN values which can cause BIH intersection to enter infinite loop
		if (maxDist < 1e-10f)
			return true;
		// direction with length of 1
		Ray ray = Ray::fromOriginAndDirection(pos1, (pos2 - pos1) / maxDist);
		return !getIntersectionTime(ray, maxDist, true, ignoreM2Model);
	}

	/**
	 * @brief Checks if an object is hit when moving from pos1 to pos2.
	 *
	 * @param pPos1 The starting position.
	 * @param pPos2 The ending position.
	 * @param pResultHitPos The resulting hit position.
	 * @param pModifyDist The distance to modify the hit position.
	 * @return true if an object is hit, false otherwise.
	 */
	bool StaticMapTree::getObjectHitPos(Vec3 const& pPos1, Vec3 const& pPos2, Vec3& pResultHitPos, float pModifyDist) const
	{
		float maxDist = (pPos2 - pPos1).magnitude();
		// valid map coords should *never ever* produce float overflow, but this would produce NaNs too:
		MANGOS_ASSERT(maxDist < std::numeric_limits<float>::max());
		// prevent NaN values which can cause BIH intersection to enter infinite loop
		if (maxDist < 1e-10f)
		{
			pResultHitPos = pPos2;
			return false;
		}
		Vec3 dir = (pPos2 - pPos1) / maxDist;            // direction with length of 1
		Ray ray(pPos1, dir);
		float dist = maxDist;
		if (getIntersectionTime(ray, dist, false, false))
		{
			pResultHitPos = pPos1 + dir * dist;
			if (pModifyDist < 0)
			{
				if ((pResultHitPos - pPos1).magnitude() > -pModifyDist)
					pResultHitPos = pResultHitPos + dir * pModifyDist;
				else
					pResultHitPos = pPos1;
			}
			else
				pResultHitPos = pResultHitPos + dir * pModifyDist;
			return true;
		}
		pResultHitPos = pPos2;
		return false;
	}

	/**
	 * @brief Retrieves the height at a given position.
	 *
	 * @param pPos The position to check.
	 * @param maxSearchDist The maximum search distance.
	 * @return float The height at the position.
	 */
	float StaticMapTree::getHeight(Vec3 const& pPos, float maxSearchDist) const
	{
		float height = finf();
		Vec3 dir;
		if (maxSearchDist >= 0.f)
			dir = Vec3::down();
		else
			dir = Vec3::up();
		Ray ray(pPos, dir); // direction with length of 1
		float maxDist = std::abs(maxSearchDist);
		if (getIntersectionTime(ray, maxDist, false, false))
		{
			if (maxSearchDist >= 0.f)
				height = pPos.z - maxDist;
			else
				height = pPos.z + maxDist;
		}
		return height;
	}
	/**
	 * @brief Checks if a map can be loaded.
	 *
	 * @param vmapPath The path to the VMAP files.
	 * @param mapID The map ID.
	 * @param tileX The tile X coordinate.
	 * @param tileY The tile Y coordinate.
	 * @return true if the map can be loaded, false otherwise.
	 */
	bool StaticMapTree::CanLoadMap(const std::string& vmapPath, unsigned int mapID, unsigned int tileX, unsigned int tileY)
	{
		std::string basePath = vmapPath;
		if (basePath.length() > 0 && (basePath[basePath.length() - 1] != '/' || basePath[basePath.length() - 1] != '\\'))
		{
			basePath.append("/");
		}
		std::string fullname = basePath + VMapManager2::getMapFileName(mapID);
		bool success = true;
		FILE* rf = fopen(fullname.c_str(), "rb");
		if (!rf)
		{
			return false;
		}
		// TODO: check magic number when implemented...
		char tiled;
		char chunk[8];
		if (!readChunk(rf, chunk, VMAP_MAGIC, 8) || fread(&tiled, sizeof(char), 1, rf) != 1)
		{
			fclose(rf);
			return false;
		}
		if (tiled)
		{
			std::string tilefile = basePath + getTileFileName(mapID, tileX, tileY);
			FILE* tf = fopen(tilefile.c_str(), "rb");
			if (!tf)
			{
				success = false;
			}
			else
			{
				if (!readChunk(tf, chunk, VMAP_MAGIC, 8))
				{
					success = false;
				}
				fclose(tf);
			}
		}
		fclose(rf);
		return success;
	}

	/**
	 * @brief Initializes the map.
	 *
	 * @param fname The file name of the map.
	 * @param vm The VMap manager.
	 * @return true if the map was successfully initialized, false otherwise.
	 */
	bool StaticMapTree::InitMap(const std::string& fname, VMapManager2* vm)
	{
		std::string fullname = iBasePath + fname;
		FILE* rf = fopen(fullname.c_str(), "rb");
		if (!rf)
			return false;
		bool success = true;
		char chunk[8];
		if (!readChunk(rf, chunk, VMAP_MAGIC, 8))
			success = false;
		char tiled = 0;
		if (success && fread(&tiled, sizeof(char), 1, rf) != 1)
			success = false;
		iIsTiled = bool(tiled);
		// Read BIH ("NODE" section)
		if (success && !readChunk(rf, chunk, "NODE", 4))
			success = false;
		if (success) {
			success = iTree.ReadFromFile(rf); // Read BIH nodes from file
		}
		if (success) {
			iNTreeValues = iTree.primCount();
			iTreeValues = new ModelInstance[iNTreeValues];
		}
		// Read "GOBJ" (global objects)
		if (success && !readChunk(rf, chunk, "GOBJ", 4))
			success = false;
		// Only non-tiled maps have a single global spawn
		ModelSpawn spawn;
		if (!iIsTiled && ModelSpawn::ReadFromFile(rf, spawn)) {
			WorldModel* model = vm->acquireUniversalModelInstance(iBasePath, spawn.name, spawn.flags);
			if (model) {
				iTreeValues[0] = ModelInstance(spawn, model);
				iLoadedSpawns[0] = 1;
			}
			else {
				success = false;
			}
		}
		fclose(rf);
		return success;
	}

	/**
	 * @brief Unloads the map.
	 *
	 * @param vm The VMap manager.
	 */
	void StaticMapTree::UnloadMap(VMapManager2* vm)
	{
		for (loadedSpawnMap::iterator i = iLoadedSpawns.begin(); i != iLoadedSpawns.end(); ++i)
		{
			iTreeValues[i->first].setUnloaded();
			for (unsigned int refCount = 0; refCount < i->second; ++refCount)
			{
				vm->releaseModelInstance(iTreeValues[i->first].name);
			}
		}
		iLoadedSpawns.clear();
		iLoadedTiles.clear();
	}

	bool isStaticModel(const ModelSpawn& spawn) {
		// Insert your logic to distinguish static/dynamic:
		// - Could be a flag on spawn
		// - Or: return spawn.name ends with .wmo or is known static
		// - Or: lookup by type/db
		// Example (basic):
		std::string n = spawn.name;
		return n.find(".wmo") != std::string::npos;
	}

	/**
	 * @brief Loads a map tile.
	 *
	 * @param tileX The tile X coordinate.
	 * @param tileY The tile Y coordinate.
	 * @param vm The VMap manager.
	 * @return true if the tile was successfully loaded, false otherwise.
	 */
	bool StaticMapTree::LoadMapTile(unsigned int tileX, unsigned int tileY, VMapManager2* vm)
	{
		if (!iIsTiled) {
			iLoadedTiles[packTileID(tileX, tileY)] = false;
			return true;
		}
		if (!iTreeValues) {
			return false;
		}
		std::string tilefile = iBasePath + getTileFileName(iMapID, tileX, tileY);
		//std::cout << "[TileLoader] Attempting to open tile: " << tilefile << std::endl;
		FILE* tf = fopen(tilefile.c_str(), "rb");
		if (!tf) {
			std::cout << "[TileLoader] File does not exist: " << tilefile << std::endl;
			iLoadedTiles[packTileID(tileX, tileY)] = false;
			return false;
		}

		bool result = true;
		char chunk[8];
		if (!readChunk(tf, chunk, VMAP_MAGIC, 8))
			result = false;
		unsigned int numSpawns = 0;
		fread(&numSpawns, sizeof(unsigned int), 1, tf); // Still read it, but use as a hint
		unsigned int i = 0;
		for (unsigned int i = 0; i < numSpawns && result; ++i) {
			ModelSpawn spawn;
			long spawnOffset = ftell(tf);
			result = ModelSpawn::ReadFromFile(tf, spawn);
			if (!result) {
				// log error
				break;
			}
			// Always read referencedVal, even if skipping spawn!
			unsigned int referencedVal;
			if (fread(&referencedVal, sizeof(unsigned int), 1, tf) != 1) {
				// log error
				break;
			}
			if (!isStaticModel(spawn)) continue;

			WorldModel* model = vm->acquireUniversalModelInstance(iBasePath, spawn.name, spawn.flags);
			if (!model) { ++i; continue; }

			if (referencedVal > iNTreeValues) { ++i; continue; }
			if (!iLoadedSpawns.count(referencedVal)) {
				iTreeValues[referencedVal] = ModelInstance(spawn, model);
				iLoadedSpawns[referencedVal] = 1;
			}
			else {
				++iLoadedSpawns[referencedVal];
			}
			++i;
		}
		iLoadedTiles[packTileID(tileX, tileY)] = result;
		fclose(tf);
		return result;
	}

	//=========================================================

	/**
	 * @brief Unloads a map tile.
	 *
	 * @param tileX The tile X coordinate.
	 * @param tileY The tile Y coordinate.
	 * @param vm The VMap manager.
	 */
	void StaticMapTree::UnloadMapTile(unsigned int tileX, unsigned int tileY, VMapManager2* vm)
	{
		unsigned int tileID = packTileID(tileX, tileY);
		loadedTileMap::iterator tile = iLoadedTiles.find(tileID);
		if (tile == iLoadedTiles.end())
		{
			ERROR_LOG("StaticMapTree::UnloadMapTile(): Trying to unload non-loaded tile. Map:%u X:%u Y:%u", iMapID, tileX, tileY);
			return;
		}
		if (tile->second) // File associated with tile
		{
			std::string tilefile = iBasePath + getTileFileName(iMapID, tileX, tileY);
			FILE* tf = fopen(tilefile.c_str(), "rb");
			if (tf)
			{
				bool result = true;
				char chunk[8];
				if (!readChunk(tf, chunk, VMAP_MAGIC, 8))
				{
					result = false;
				}
				unsigned int numSpawns;
				if (fread(&numSpawns, sizeof(unsigned int), 1, tf) != 1)
				{
					result = false;
				}
				for (unsigned int i = 0; i < numSpawns && result; ++i)
				{
					// Read model spawns
					ModelSpawn spawn;
					result = ModelSpawn::ReadFromFile(tf, spawn);
					if (result)
					{
						// Release model instance
						vm->releaseModelInstance(spawn.name);

						// Update tree
						unsigned int referencedNode;

						size_t fileRead = fread(&referencedNode, sizeof(unsigned int), 1, tf);
						if (!iLoadedSpawns.count(referencedNode) || fileRead <= 0)
						{
							ERROR_LOG("Trying to unload non-referenced model '%s' (ID:%u)", spawn.name.c_str(), spawn.ID);
						}
						else if (--iLoadedSpawns[referencedNode] == 0)
						{
							iTreeValues[referencedNode].setUnloaded();
							iLoadedSpawns.erase(referencedNode);
						}
					}
				}
				fclose(tf);
			}
		}
		iLoadedTiles.erase(tile);
	}
}
