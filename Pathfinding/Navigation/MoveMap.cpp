/**
* MaNGOS is a full featured server for World of Warcraft, supporting
* the following clients: 1.12.x, 2.4.3, 3.3.5a, 4.3.4a and 5.4.8
*
* Copyright (C) 2005-2015  MaNGOS project <http://getmangos.eu>
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

#include <set>

#include "MoveMap.h"
#include "MoveMapSharedDefines.h"

#include <windows.h>
#include <sstream>

using namespace std;
namespace MMAP
{
	template <typename T>
	string NumberToString(T Number)
	{
		ostringstream ss;
		ss << Number;
		return ss.str();
	}

	void str_replace(string &s, const string &search, const string &replace)
	{
		for (size_t pos = 0;; pos += replace.length())
		{
			pos = s.find(search, pos);
			if (pos == string::npos) break;

			s.erase(pos, search.length());
			s.insert(pos, replace);
		}
	}

	EXTERN_C IMAGE_DOS_HEADER __ImageBase;
	void log(char* str)
	{
		/*FILE * pFile;

		WCHAR   DllPath[MAX_PATH] = { 0 };
		GetModuleFileNameW((HINSTANCE)&__ImageBase, DllPath, _countof(DllPath));
		wstring ws(DllPath);
		string pathAndFile(ws.begin(), ws.end());
		char *c = const_cast<char*>(pathAndFile.c_str());
		int strLength = strlen(c);
		int lastOccur = 0;
		for (int i = 0; i < strLength; i++)
		{
			if (c[i] == '\\') lastOccur = i;
		}
		string pathToLogFile = pathAndFile.substr(0, lastOccur + 1);
		pathToLogFile = pathToLogFile.append("Pathfinding.log");

		str_replace(pathToLogFile, "\\", "\\\\");

		pFile = fopen(pathToLogFile.c_str(), "a");
		fputs(str, pFile);
		fputs("\n", pFile);
		fclose(pFile);*/
	}

	void getMapName(unsigned int mapId, string& result)
	{
#if NDEBUG
		string mapIdStr = "mmaps\\";
#else
		string mapIdStr = "";
#endif
		if (mapId < 10)
		{
			mapIdStr = mapIdStr.append("00");
		}
		else if (mapId < 100)
		{
			mapIdStr = mapIdStr.append("0");
		}
		string mapIdStr2 = NumberToString(mapId);
		mapIdStr = mapIdStr.append(mapIdStr2);
		mapIdStr = mapIdStr.append(".mmap");

		WCHAR   DllPath[MAX_PATH] = { 0 };
		GetModuleFileNameW((HINSTANCE)&__ImageBase, DllPath, _countof(DllPath));
		wstring ws(DllPath);
		string pathAndFile(ws.begin(), ws.end());
		char *c = const_cast<char*>(pathAndFile.c_str());
		int strLength = strlen(c);
		int lastOccur = 0;
		for (int i = 0; i < strLength; i++)
		{
			if (c[i] == '\\') lastOccur = i;
		}
		string pathToMmap = pathAndFile.substr(0, lastOccur + 1);
		string pathToMmapFile = pathToMmap.append(mapIdStr);
		c = const_cast<char*>(pathToMmapFile.c_str());
		strLength = strlen(c);

		str_replace(pathToMmapFile, "\\", "\\\\");
		result = pathToMmapFile;
	}

	void getTileName(unsigned int mapId, int x, int y, string& result)
	{
#if NDEBUG
		string tileName = "mmaps\\";
#else
		string tileName = ""; //"mmaps\\";
#endif
		if (mapId < 10)
		{
			tileName = tileName.append("00");
		}
		else if (mapId < 100)
		{
			tileName = tileName.append("0");
		}
		tileName.append(NumberToString(mapId));

		if (x < 10)
		{
			tileName = tileName.append("0");
		}tileName.append(NumberToString(x));

		if (y < 10)
		{
			tileName = tileName.append("0");
		}tileName.append(NumberToString(y));
		tileName.append(".mmtile");

		WCHAR   DllPath[MAX_PATH] = { 0 };
		GetModuleFileNameW((HINSTANCE)&__ImageBase, DllPath, _countof(DllPath));
		wstring ws(DllPath);
		string pathAndFile(ws.begin(), ws.end());
		char *c = const_cast<char*>(pathAndFile.c_str());
		int strLength = strlen(c);
		int lastOccur = 0;
		for (int i = 0; i < strLength; i++)
		{
			if (c[i] == '\\') lastOccur = i;
		}
		string pathToMmap = pathAndFile.substr(0, lastOccur + 1);
		string pathToMmapFile = pathToMmap.append(tileName);

		str_replace(pathToMmapFile, "\\", "\\\\");
		result = pathToMmapFile;
	}

	// ######################## MMapFactory ########################
	// our global singelton copy
	MMapManager* g_MMapManager = NULL;

	// stores list of mapids which do not use pathfinding
	std::set<unsigned int>* g_mmapDisabledIds = NULL;

	MMapManager* MMapFactory::createOrGetMMapManager()
	{
		if (g_MMapManager == NULL)
		{
			g_MMapManager = new MMapManager();
		}

		return g_MMapManager;
	}

	void MMapFactory::preventPathfindingOnMaps(const char* ignoreMapIds)
	{
		if (!g_mmapDisabledIds)
		{
			g_mmapDisabledIds = new std::set<unsigned int>();
		}

		unsigned int strLenght = strlen(ignoreMapIds) + 1;
		char* mapList = new char[strLenght];
		memcpy(mapList, ignoreMapIds, sizeof(char)*strLenght);

		char* idstr = strtok(mapList, ",");
		while (idstr)
		{
			g_mmapDisabledIds->insert(unsigned int(atoi(idstr)));
			idstr = strtok(NULL, ",");
		}

		delete[] mapList;
	}

	bool MMapFactory::IsPathfindingEnabled(unsigned int mapId)
	{
		// todo remove
		return true;
	}

	void MMapFactory::clear()
	{
		delete g_mmapDisabledIds;
		delete g_MMapManager;

		g_mmapDisabledIds = NULL;
		g_MMapManager = NULL;
	}

	// ######################## MMapManager ########################
	MMapManager::~MMapManager()
	{
		for (MMapDataSet::iterator i = loadedMMaps.begin(); i != loadedMMaps.end(); ++i)
		{
			delete i->second;
		}

		// by now we should not have maps loaded
		// if we had, tiles in MMapData->mmapLoadedTiles, their actual data is lost!
	}

	bool MMapManager::loadMapData(unsigned int mapId)
	{
		// we already have this map loaded?
		if (loadedMMaps.find(mapId) != loadedMMaps.end())
		{
			return true;
		}

		string fileName = "";
		getMapName(mapId, fileName);
		FILE* file = fopen(fileName.c_str(), "rb");
		if (!file)
		{
			if (MMapFactory::IsPathfindingEnabled(mapId))
			{
				char* b = "MMAP:loadMapData: Error: Could not open mmap file '%s'"
					; int l = strlen(b);
				char* c = new char[l];
				//_snprintf(c, l, b, fileName);
				log(c);
			}
			return false;
		}

		dtNavMeshParams params;
		size_t file_read = fread(&params, sizeof(dtNavMeshParams), 1, file);
		if (file_read <= 0)
		{
			char* b = "MMAP:loadMapData: Failed to load mmap %03u from file %s"
				; int l = strlen(b);
			char* c = new char[l];
			//_snprintf(c, l, b, mapId, fileName);
			log(c);
			fclose(file);
			return false;
		}
		fclose(file);

		dtNavMesh* mesh = dtAllocNavMesh();
		//MANGOS_ASSERT(mesh);
		dtStatus dtResult = mesh->init(&params);
		char* b = "dtResult: %x\n"
			; int l = strlen(b);
		char* c = new char[l];
		//_snprintf(c, l, b, dtResult);
		log(c);
		b = "maxPolys: %u\n"
			; l = strlen(b);
		c = new char[l];
		//_snprintf(c, l, b, params.maxPolys);
		log(c);
		b = "maxTiles: %u\n"
			; l = strlen(b);
		c = new char[l];
		//_snprintf(c, l, b, params.maxTiles);
		log(c);
		b = "orig[0]: %f\n"
			; l = strlen(b);
		c = new char[l];
		//_snprintf(c, l, b, params.orig[0]);
		log(c);
		b = "orig[1]: %f\n"
			; l = strlen(b);
		c = new char[l];
		//_snprintf(c, l, b, params.orig[1]);
		log(c);
		b = "orig[2]: %f\n"
			; l = strlen(b);
		c = new char[l];
		//_snprintf(c, l, b, params.orig[2]);
		log(c);
		b = "tileHeight: %u\n"
			; l = strlen(b);
		c = new char[l];
		//_snprintf(c, l, b, params.tileHeight);
		log(c);
		b = "tileWidth: %u\n"
			; l = strlen(b);
		c = new char[l];
		//_snprintf(c, l, b, params.tileWidth);
		log(c);
		if (dtStatusFailed(dtResult))
		{
			dtFreeNavMesh(mesh);
			char* b = "MMAP:loadMapData: Failed to initialize dtNavMesh for mmap %03u from file %s"
				; int l = strlen(b);
			char* c = new char[l];
			//_snprintf(c, l, b, mapId, fileName);
			log(c);
			return false;
		}
		b = "MMAP:loadMapData: Loaded %03i.mmap"
			; l = strlen(b);
		c = new char[l];
		//_snprintf(c, l, b, mapId);
		log(c);

		// store inside our map list
		MMapData* mmap_data = new MMapData(mesh);
		mmap_data->mmapLoadedTiles.clear();

		loadedMMaps.insert(std::pair<unsigned int, MMapData*>(mapId, mmap_data));
		return true;
	}

	unsigned int MMapManager::packTileID(int x, int y)
	{
		return unsigned int(x << 16 | y);
	}

	bool MMapManager::hasLoadedMap(unsigned int mapId, int x, int y) {
		MMapData* mmap = loadedMMaps[mapId];
		unsigned int packedGridPos = packTileID(x, y);
		if (mmap->mmapLoadedTiles.find(packedGridPos) != mmap->mmapLoadedTiles.end())
		{
			char* b = "MMAP:loadMap: Asked to load already loaded navmesh tile. %03u%02i%02i.mmtile"
				; int l = strlen(b);
			char* c = new char[l];
			//_snprintf(c, l, b, mapId, x, y);
			log(c);
			//return false;
			return true; // return true -> already loaded
		}
		return false;
	}

	bool MMapManager::loadMap(unsigned int mapId, int x, int y)
	{
		// make sure the mmap is loaded and ready to load tiles
		if (!loadMapData(mapId))
		{
			return false;
		}

		// get this mmap data
		MMapData* mmap = loadedMMaps[mapId];
		//MANGOS_ASSERT(mmap->navMesh);

		// check if we already have this tile loaded
		unsigned int packedGridPos = packTileID(x, y);
		if (mmap->mmapLoadedTiles.find(packedGridPos) != mmap->mmapLoadedTiles.end())
		{
			char* b = "MMAP:loadMap: Asked to load already loaded navmesh tile. %03u%02i%02i.mmtile"
				; int l = strlen(b);
			char* c = new char[l];
			//_snprintf(c, l, b, mapId, x, y);
			log(c);
			//return false;
			return true; // return true -> already loaded
		}

		string fileName = "";
		getTileName(mapId, x, y, fileName);
		FILE* file = fopen(fileName.c_str(), "rb");
		if (!file)
		{
			char* b = "ERROR: MMAP:loadMap: Could not open mmtile file '%s'"
				; int l = strlen(b);
			char* c = new char[l];
			//_snprintf(c, l, b, fileName);
			log(c);
			return false;
		}
		// read header
		MmapTileHeader fileHeader;
		size_t file_read = fread(&fileHeader, sizeof(MmapTileHeader), 1, file);

		if (file_read <= 0)
		{
			char* b = "MMAP:loadMap: Could not load mmap %03u%02i%02i.mmtile"
				; int l = strlen(b);
			char* c = new char[l];
			//_snprintf(c, l, b, mapId, x, y);
			log(c);
			fclose(file);
			return false;
		}

		if (fileHeader.mmapMagic != MMAP_MAGIC)
		{
			char* b = "MMAP:loadMap: Bad header in mmap %03u%02i%02i.mmtile"
				; int l = strlen(b);
			char* c = new char[l];
			//_snprintf(c, l, b, mapId, x, y);
			log(c);
			fclose(file);
			return false;
		}

		if (fileHeader.mmapVersion != MMAP_VERSION)
		{
			char* b = "MMAP:loadMap: %03u%02i%02i.mmtile was built with generator v%i, expected v%i"
				; int l = strlen(b);
			char* c = new char[l];
			//_snprintf(c, l, b, mapId, x, y, fileHeader.mmapVersion, MMAP_VERSION);
			log(c);
			//	mapId, x, y, fileHeader.mmapVersion, MMAP_VERSION);
			fclose(file);
			return false;
		}

		unsigned char* data = (unsigned char*)dtAlloc(fileHeader.size, DT_ALLOC_PERM);
		//MANGOS_ASSERT(data);

		size_t result = fread(data, fileHeader.size, 1, file);
		if (!result)
		{
			char* b = "MMAP:loadMap: Bad header or data in mmap %03u%02i%02i.mmtile"
				; int l = strlen(b);
			char* c = new char[l];
			//_snprintf(c, l, b, mapId, x, y);
			log(c);
			fclose(file);
			return false;
		}

		fclose(file);

		dtMeshHeader* header = (dtMeshHeader*)data;
		dtTileRef tileRef = 0;

		// memory allocated for data is now managed by detour, and will be deallocated when the tile is removed
		dtStatus dtResult = mmap->navMesh->addTile(data, fileHeader.size, DT_TILE_FREE_DATA, 0, &tileRef);
		if (dtStatusFailed(dtResult))
		{
			char* b = "MMAP:loadMap: Could not load %03u%02i%02i.mmtile into navmesh"
				; int l = strlen(b);
			char* c = new char[l];
			//_snprintf(c, l, b, mapId, x, y);
			log(c);
			dtFree(data);
			return false;
		}

		mmap->mmapLoadedTiles.insert(std::pair<unsigned int, dtTileRef>(packedGridPos, tileRef));
		++loadedTiles;
		char* b = "MMAP:loadMap: Loaded mmtile %03i[%02i,%02i] into %03i[%02i,%02i]"
			; int l = strlen(b);
		char* c = new char[l];
		//_snprintf(c, l, b, mapId, x, y, mapId, header->x, header->y);
		log(c);
		return true;
	}

	bool MMapManager::unloadMap(unsigned int mapId, int x, int y)
	{
		// check if we have this map loaded
		if (loadedMMaps.find(mapId) == loadedMMaps.end())
		{
			// file may not exist, therefore not loaded
			char* b = "MMAP:unloadMap: Asked to unload not loaded navmesh map. %03u%02i%02i.mmtile"
				; int l = strlen(b);
			char* c = new char[l];
			//_snprintf(c, l, b, mapId, x, y);
			log(c);
			return false;
		}

		MMapData* mmap = loadedMMaps[mapId];

		// check if we have this tile loaded
		unsigned int packedGridPos = packTileID(x, y);
		if (mmap->mmapLoadedTiles.find(packedGridPos) == mmap->mmapLoadedTiles.end())
		{
			// file may not exist, therefore not loaded
			char* b = "MMAP:unloadMap: Asked to unload not loaded navmesh tile. %03u%02i%02i.mmtile"
				; int l = strlen(b);
			char* c = new char[l];
			//_snprintf(c, l, b, mapId, x, y);
			log(c);
			return false;
		}

		dtTileRef tileRef = mmap->mmapLoadedTiles[packedGridPos];

		// unload, and mark as non loaded
		dtStatus dtResult = mmap->navMesh->removeTile(tileRef, NULL, NULL);
		if (dtStatusFailed(dtResult))
		{
			// this is technically a memory leak
			// if the grid is later reloaded, dtNavMesh::addTile will return error but no extra memory is used
			// we can not recover from this error - assert out
			char* b = "MMAP:unloadMap: Could not unload %03u%02i%02i.mmtile from navmesh"
				; int l = strlen(b);
			char* c = new char[l];
			//_snprintf(c, l, b, mapId, x, y);
			log(c);
			//MANGOS_ASSERT(false);
		}
		else
		{
			mmap->mmapLoadedTiles.erase(packedGridPos);
			--loadedTiles;
			char* b = "MMAP:unloadMap: Unloaded mmtile %03i[%02i,%02i] from %03i"
				; int l = strlen(b);
			char* c = new char[l];
			//_snprintf(c, l, b, mapId, x, y, mapId);
			log(c);
			return true;
		}

		return false;
	}

	bool MMapManager::unloadMap(unsigned int mapId)
	{
		if (loadedMMaps.find(mapId) == loadedMMaps.end())
		{
			// file may not exist, therefore not loaded
			char* b = "MMAP:unloadMap: Asked to unload not loaded navmesh map %03u"
				; int l = strlen(b);
			char* c = new char[l];
			//_snprintf(c, l, b, mapId);
			log(c);
			return false;
		}

		// unload all tiles from given map
		MMapData* mmap = loadedMMaps[mapId];
		for (MMapTileSet::iterator i = mmap->mmapLoadedTiles.begin(); i != mmap->mmapLoadedTiles.end(); ++i)
		{
			unsigned int x = (i->first >> 16);
			unsigned int y = (i->first & 0x0000FFFF);
			dtStatus dtResult = mmap->navMesh->removeTile(i->second, NULL, NULL);
			if (dtStatusFailed(dtResult))
			{
				char* b = "MMAP:unloadMap: Could not unload %03u%02i%02i.mmtile from navmesh"
					; int l = strlen(b);
				char* c = new char[l];
				//_snprintf(c, l, b, mapId, x, y);
				log(c);
			}
			else
			{
				--loadedTiles;
				char* b = "MMAP:unloadMap: Unloaded mmtile %03i[%02i,%02i] from %03i"
					; int l = strlen(b);
				char* c = new char[l];
				//_snprintf(c, l, b, mapId, x, y, mapId);
				log(c);
			}
		}

		delete mmap;
		loadedMMaps.erase(mapId);
		char* b = "MMAP:unloadMap: Unloaded %03i.mmap"
			; int l = strlen(b);
		char* c = new char[l];
		//_snprintf(c, l, b, mapId);
		log(c);

		return true;
	}

	bool MMapManager::unloadMapInstance(unsigned int mapId, unsigned int instanceId)
	{
		// check if we have this map loaded
		if (loadedMMaps.find(mapId) == loadedMMaps.end())
		{
			// file may not exist, therefore not loaded
			char* b = "MMAP:unloadMapInstance: Asked to unload not loaded navmesh map %03u"
				; int l = strlen(b);
			char* c = new char[l];
			//_snprintf(c, l, b, mapId);
			log(c);
			return false;
		}

		MMapData* mmap = loadedMMaps[mapId];
		if (mmap->navMeshQueries.find(instanceId) == mmap->navMeshQueries.end())
		{
			char* b = "MMAP:unloadMapInstance: Asked to unload not loaded dtNavMeshQuery mapId %03u instanceId %u"
				; int l = strlen(b);
			char* c = new char[l];
			//_snprintf(c, l, b, mapId, instanceId);
			log(c);
			return false;
		}

		dtNavMeshQuery* query = mmap->navMeshQueries[instanceId];

		dtFreeNavMeshQuery(query);
		mmap->navMeshQueries.erase(instanceId);
		char* b = "MMAP:unloadMapInstance: Unloaded mapId %03u instanceId %u"
			; int l = strlen(b);
		char* c = new char[l];
		//_snprintf(c, l, b, mapId, instanceId);
		log(c);

		return true;
	}

	dtNavMesh const* MMapManager::GetNavMesh(unsigned int mapId)
	{
		if (loadedMMaps.find(mapId) == loadedMMaps.end())
		{
			return NULL;
		}

		return loadedMMaps[mapId]->navMesh;
	}

	dtNavMeshQuery const* MMapManager::GetNavMeshQuery(unsigned int mapId, unsigned int instanceId)
	{
		if (loadedMMaps.find(mapId) == loadedMMaps.end())
		{
			return NULL;
		}

		MMapData* mmap = loadedMMaps[mapId];
		if (mmap->navMeshQueries.find(instanceId) == mmap->navMeshQueries.end())
		{
			// allocate mesh query
			dtNavMeshQuery* query = dtAllocNavMeshQuery();
			//MANGOS_ASSERT(query);
			dtStatus dtResult = query->init(mmap->navMesh, 65535);
			if (dtStatusFailed(dtResult))
			{
				dtFreeNavMeshQuery(query);
				char* b = "MMAP:GetNavMeshQuery: Failed to initialize dtNavMeshQuery for mapId %03u instanceId %u"
					; int l = strlen(b);
				char* c = new char[l];
				//_snprintf(c, l, b, mapId, instanceId);
				log(c);
				return NULL;
			}

			char* b = "MMAP:GetNavMeshQuery: created dtNavMeshQuery for mapId %03u instanceId %u"
				; int l = strlen(b);
			char* c = new char[l];
			//_snprintf(c, l, b, mapId, instanceId);
			log(c);
			mmap->navMeshQueries.insert(std::pair<unsigned int, dtNavMeshQuery*>(instanceId, query));
		}

		return mmap->navMeshQueries[instanceId];
	}
}
