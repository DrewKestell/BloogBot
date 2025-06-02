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

#include "MoveMap.h"
#include "MoveMapSharedDefines.h"

#include <set>
#include <windows.h>
#include <sstream>
#include <iostream>
#include <fstream>

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

	void str_replace(string& s, const string& search, const string& replace)
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

	void getMapName(unsigned int mapId, string& result)
	{
		string mapIdStr = "";
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

		WCHAR DllPath[MAX_PATH] = { 0 };
		GetModuleFileNameW((HINSTANCE)&__ImageBase, DllPath, _countof(DllPath));
		wstring ws(DllPath);
		string pathAndFile(ws.begin(), ws.end());
		char* c = const_cast<char*>(pathAndFile.c_str());
		int strLength = strlen(c);
		int lastOccur = 0;
		for (int i = 0; i < strLength; i++)
		{
			if (c[i] == '\\') lastOccur = i;
		}
		string pathToMmap = pathAndFile.substr(0, lastOccur + 1);
		pathToMmap = pathToMmap.append("mmaps\\");
		string pathToMmapFile = pathToMmap.append(mapIdStr);
		c = const_cast<char*>(pathToMmapFile.c_str());
		strLength = strlen(c);

		str_replace(pathToMmapFile, "\\", "\\\\");

		result = pathToMmapFile;
	}

	void getTileName(unsigned int mapId, int x, int y, string& result)
	{
		string tileName = "";
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
		}
		tileName.append(NumberToString(x));

		if (y < 10)
		{
			tileName = tileName.append("0");
		}
		tileName.append(NumberToString(y));

		tileName.append(".mmtile");

		WCHAR DllPath[MAX_PATH] = { 0 };
		GetModuleFileNameW((HINSTANCE)&__ImageBase, DllPath, _countof(DllPath));
		wstring ws(DllPath);
		string pathAndFile(ws.begin(), ws.end());

		char* c = const_cast<char*>(pathAndFile.c_str());
		int strLength = strlen(c);
		int lastOccur = 0;
		for (int i = 0; i < strLength; i++)
		{
			if (c[i] == '\\') lastOccur = i;
		}
		string pathToMmap = pathAndFile.substr(0, lastOccur + 1);
		pathToMmap = pathToMmap.append("mmaps\\");
		string pathToMmapFile = pathToMmap.append(tileName);

		str_replace(pathToMmapFile, "\\", "\\\\");

		result = pathToMmapFile;
	}

	// ######################## MMapFactory ########################
	// our global singelton copy
	MMapManager* g_MMapManager = NULL;

	MMapManager* MMapFactory::createOrGetMMapManager()
	{
		if (g_MMapManager == NULL)
		{
			g_MMapManager = new MMapManager();
		}

		return g_MMapManager;
	}

	// ######################## MMapManager ########################
	MMapManager::~MMapManager()
	{
		for (MMapDataSet::iterator i = loadedMMaps.begin(); i != loadedMMaps.end(); ++i)
		{
			delete i->second;
		}
	}

	bool MMapManager::loadMapData(unsigned int mapId)
	{
		if (loadedMMaps.find(mapId) != loadedMMaps.end())
			return true;

		string fileName = "";
		getMapName(mapId, fileName);
		FILE* file = fopen(fileName.c_str(), "rb");
		dtNavMeshParams params;
		size_t file_read = fread(&params, sizeof(dtNavMeshParams), 1, file);
		fclose(file);

		dtNavMesh* mesh = dtAllocNavMesh();
		dtStatus dtResult = mesh->init(&params);
		if (dtStatusFailed(dtResult))
		{
			dtFreeNavMesh(mesh);
			return false;
		}

		MMapData* mmap_data = new MMapData(mesh);
		mmap_data->mmapLoadedTiles.clear();

		loadedMMaps.insert(std::pair<unsigned int, MMapData*>(mapId, mmap_data));
		return true;
	}

	unsigned int MMapManager::packTileID(int x, int y)
	{
		return unsigned int(x << 16 | y);
	}

	bool MMapManager::loadMap(unsigned int mapId, int x, int y)
	{
		loadMapData(mapId);

		MMapData* mmap = loadedMMaps[mapId];

		unsigned int packedGridPos = packTileID(x, y);
		if (mmap->mmapLoadedTiles.find(packedGridPos) != mmap->mmapLoadedTiles.end())
			return true;

		string fileName = "";
		getTileName(mapId, x, y, fileName);

		FILE* file = fopen(fileName.c_str(), "rb");
		MmapTileHeader fileHeader;
		size_t file_read = fread(&fileHeader, sizeof(MmapTileHeader), 1, file);
		unsigned char* data = (unsigned char*)dtAlloc(fileHeader.size, DT_ALLOC_PERM);
		size_t result = fread(data, fileHeader.size, 1, file);
		fclose(file);

		dtMeshHeader* header = (dtMeshHeader*)data;
		dtTileRef tileRef = 0;
		dtStatus dtResult = mmap->navMesh->addTile(data, fileHeader.size, DT_TILE_FREE_DATA, 0, &tileRef);
		if (dtStatusFailed(dtResult))
		{
			dtFree(data);
			return false;
		}

		mmap->mmapLoadedTiles.insert(std::pair<unsigned int, dtTileRef>(packedGridPos, tileRef));

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
			dtNavMeshQuery* query = dtAllocNavMeshQuery();
			dtStatus dtResult = query->init(mmap->navMesh, 65535);
			if (dtStatusFailed(dtResult))
			{
				dtFreeNavMeshQuery(query);
				return NULL;
			}

			mmap->navMeshQueries.insert(std::pair<unsigned int, dtNavMeshQuery*>(instanceId, query));
		}

		return mmap->navMeshQueries[instanceId];
	}

	bool hasLoadedWesternContinent()
	{
		return hasLoadedWesternContinent;
	}
}
