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

#ifndef MANGOS_H_MOVE_MAP
#define MANGOS_H_MOVE_MAP

#include <unordered_map>

#include "DetourAlloc.h"
#include "DetourNavMesh.h"
#include "DetourNavMeshQuery.h"

#include "Utilities/UnorderedMapSet.h"

//  memory management
inline void* dtCustomAlloc(int size, dtAllocHint /*hint*/)
{
	return (void*)new unsigned char[size];
}

inline void dtCustomFree(void* ptr)
{
	delete[](unsigned char*)ptr;
}

//  move map related classes
namespace MMAP
{
	typedef std::unordered_map<unsigned int, dtTileRef> MMapTileSet;
	typedef std::unordered_map<unsigned int, dtNavMeshQuery*> NavMeshQuerySet;

	// dummy struct to hold map's mmap data
	struct MMapData
	{
		MMapData(dtNavMesh* mesh) : navMesh(mesh) {}
		~MMapData()
		{
			for (NavMeshQuerySet::iterator i = navMeshQueries.begin(); i != navMeshQueries.end(); ++i)
			{
				dtFreeNavMeshQuery(i->second);
			}

			if (navMesh)
			{
				dtFreeNavMesh(navMesh);
			}
		}

		dtNavMesh* navMesh;

		// we have to use single dtNavMeshQuery for every instance, since those are not thread safe
		NavMeshQuerySet navMeshQueries;     // instanceId to query
		MMapTileSet mmapLoadedTiles;        // maps [map grid coords] to [dtTile]
	};


	typedef std::unordered_map<unsigned int, MMapData*> MMapDataSet;

	// singelton class
	// holds all all access to mmap loading unloading and meshes
	class MMapManager
	{
	public:
		MMapManager() : loadedTiles(0) {}
		~MMapManager();

		bool loadMap(unsigned int mapId, int x, int y);
		bool unloadMap(unsigned int mapId, int x, int y);
		bool unloadMap(unsigned int mapId);
		bool unloadMapInstance(unsigned int mapId, unsigned int instanceId);
		bool hasLoadedMap(unsigned int mapId, int x, int y);

		// the returned [dtNavMeshQuery const*] is NOT threadsafe
		dtNavMeshQuery const* GetNavMeshQuery(unsigned int mapId, unsigned int instanceId);
		dtNavMesh const* GetNavMesh(unsigned int mapId);

		unsigned int getLoadedTilesCount() const { return loadedTiles; }
		unsigned int getLoadedMapsCount() const { return loadedMMaps.size(); }
	private:
		bool loadMapData(unsigned int mapId);
		unsigned int packTileID(int x, int y);

		MMapDataSet loadedMMaps;
		unsigned int loadedTiles;
	};

	// static class
	// holds all mmap global data
	// access point to MMapManager singelton
	class MMapFactory
	{
	public:
		static MMapManager* createOrGetMMapManager();
		static void clear();
		static void preventPathfindingOnMaps(const char* ignoreMapIds);
		static bool IsPathfindingEnabled(unsigned int mapId);
	};
}

#endif  // _MOVE_MAP_H
