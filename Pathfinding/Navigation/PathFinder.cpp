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

#include "DetourCommon.h"

#include "MoveMap.h"
#include "PathFinder.h"

////////////////// PathFinder //////////////////
PathFinder::PathFinder(unsigned int mapId, unsigned int instanceId) :
m_polyLength(0), m_type(PATHFIND_BLANK),
m_useStraightPath(false), m_forceDestination(false), m_pointPathLimit(MAX_POINT_PATH_LENGTH),
m_mapId(mapId), m_instanceId(instanceId), m_navMesh(NULL), m_navMeshQuery(NULL)
{
	//printf("++ PathFinder::PathInfo for ME \n");

	if (MMAP::MMapFactory::IsPathfindingEnabled(m_mapId))
	{
		MMAP::MMapManager* mmap = MMAP::MMapFactory::createOrGetMMapManager();
		m_navMesh = mmap->GetNavMesh(m_mapId);
		m_navMeshQuery = mmap->GetNavMeshQuery(m_mapId, m_instanceId);
	}

	createFilter();
}

PathFinder::~PathFinder()
{
	//printf("++ PathFinder::~PathInfo() for ME \n");
}

bool PathFinder::calculate(float originX, float originY, float originZ, float destX, float destY, float destZ, bool forceDest, bool isSwimming)
{
	Vector3 start(originX, originY, originZ);
	setStartPosition(start);

	Vector3 dest(destX, destY, destZ);
	setEndPosition(dest);

	m_forceDestination = forceDest;

	//printf("++ PathFinder::calculate() for Me \n");

	// make sure navMesh works - we can run on map w/o mmap
	// check if the start and end point have a .mmtile loaded (can we pass via not loaded tile on the way?)
	if (!m_navMesh || !m_navMeshQuery || !HaveTile(start) || !HaveTile(dest))
	{
		BuildShortcut();

		//printf("!!!!!!! 1 !!!!!!!\n");

		//printf("1 %i\n", !m_navMesh);
		//printf("2 %i\n", !m_navMeshQuery);
		//printf("3 %i\n", !HaveTile(start));
		//printf("4 %i\n", !HaveTile(dest));


		//printf("!!!!!!! 1 !!!!!!!\n");

		m_type = PathType(PATHFIND_NORMAL | PATHFIND_NOT_USING_PATH);
		return true;
	}

	updateFilter(isSwimming, originX, originY, originZ);

	BuildPolyPath(start, dest);
	return true;
}

dtPolyRef PathFinder::getPathPolyByPosition(const dtPolyRef* polyPath, unsigned int polyPathSize, const float* point, float* distance) const
{
	if (!polyPath || !polyPathSize)
	{
		return INVALID_POLYREF;
	}

	dtPolyRef nearestPoly = INVALID_POLYREF;
	float minDist2d = FLT_MAX;
	float minDist3d = 0.0f;

	for (unsigned int i = 0; i < polyPathSize; i++)
	{
		float closestPoint[VERTEX_SIZE];
		dtStatus dtResult = m_navMeshQuery->closestPointOnPoly(polyPath[i], point, closestPoint, NULL);
		if (dtStatusFailed(dtResult))
			continue;

		float d = dtVdist2DSqr(point, closestPoint);
		if (d < minDist2d)
		{
			minDist2d = d;
			nearestPoly = polyPath[i];
			minDist3d = dtVdistSqr(point, closestPoint);
		}

		if (minDist2d < 1.0f) // shortcut out - close enough for us
		{
			break;
		}
	}

	if (distance)
	{
		*distance = dtSqrt(minDist3d);
	}

	return (minDist2d < 3.0f) ? nearestPoly : INVALID_POLYREF;
}

dtPolyRef PathFinder::getPolyByLocation(const float* point, float* distance) const
{
	// first we check the current path
	// if the current path doesn't contain the current poly,
	// we need to use the expensive navMesh.findNearestPoly
	dtPolyRef polyRef = getPathPolyByPosition(m_pathPolyRefs, m_polyLength, point, distance);
	if (polyRef != INVALID_POLYREF)
	{
		return polyRef;
	}

	// we don't have it in our old path
	// try to get it by findNearestPoly()
	// first try with low search box
	float extents[VERTEX_SIZE] = { 3.0f, 5.0f, 3.0f };    // bounds of poly search area
	float closestPoint[VERTEX_SIZE] = { 0.0f, 0.0f, 0.0f };
	dtStatus dtResult = m_navMeshQuery->findNearestPoly(point, extents, &m_filter, &polyRef, closestPoint);
	if (dtStatusSucceed(dtResult) && polyRef != INVALID_POLYREF)
	{
		*distance = dtVdist(closestPoint, point);
		return polyRef;
	}

	// still nothing ..
	// try with bigger search box
	extents[1] = 200.0f;
	dtResult = m_navMeshQuery->findNearestPoly(point, extents, &m_filter, &polyRef, closestPoint);
	if (dtStatusSucceed(dtResult) && polyRef != INVALID_POLYREF)
	{
		*distance = dtVdist(closestPoint, point);
		return polyRef;
	}

	return INVALID_POLYREF;
}

void PathFinder::BuildPolyPath(const Vector3& startPos, const Vector3& endPos)
{
	// *** getting start/end poly logic ***

	float distToStartPoly, distToEndPoly;
	float startPoint[VERTEX_SIZE] = { startPos.y, startPos.z, startPos.x };
	float endPoint[VERTEX_SIZE] = { endPos.y, endPos.z, endPos.x };

	dtPolyRef startPoly = getPolyByLocation(startPoint, &distToStartPoly);
	dtPolyRef endPoly = getPolyByLocation(endPoint, &distToEndPoly);

	dtStatus dtResult;

	// we have a hole in our mesh
	// make shortcut path and mark it as NOPATH ( with flying exception )
	// its up to caller how he will use this info
	if (startPoly == INVALID_POLYREF || endPoly == INVALID_POLYREF)
	{
		//printf("++ BuildPolyPath :: (startPoly == 0 || endPoly == 0)\n");
		BuildShortcut();
		//printf("!!!!!!! 2 !!!!!!!\n");
		m_type = PathType(PATHFIND_NORMAL | PATHFIND_NOT_USING_PATH);

		return;
	}

	// we may need a better number here
	bool farFromPoly = (distToStartPoly > 7.0f || distToEndPoly > 7.0f);
	if (farFromPoly)
	{
		//printf("++ BuildPolyPath :: farFromPoly distToStartPoly=%.3f distToEndPoly=%.3f\n", distToStartPoly, distToEndPoly);
		
		bool isSwimming = false;

		if (isSwimming)
		{
			BuildShortcut();
			//printf("!!!!!!! 3 !!!!!!!\n");
			m_type = PathType(PATHFIND_NORMAL | PATHFIND_NOT_USING_PATH);
			return;
		}
		else
		{
			float closestPoint[VERTEX_SIZE];
			// we may want to use closestPointOnPolyBoundary instead
			dtResult = m_navMeshQuery->closestPointOnPoly(endPoly, endPoint, closestPoint, NULL);
			if (dtStatusSucceed(dtResult))
			{
				dtVcopy(endPoint, closestPoint);
				setActualEndPosition(Vector3(endPoint[2], endPoint[0], endPoint[1]));
			}

			m_type = PATHFIND_INCOMPLETE;
		}
	}

	// *** poly path generating logic ***

	// start and end are on same polygon
	// just need to move in straight line
	if (startPoly == endPoly)
	{
		//printf("++ BuildPolyPath :: (startPoly == endPoly)\n");

		BuildShortcut();

		m_pathPolyRefs[0] = startPoly;
		m_polyLength = 1;

		m_type = farFromPoly ? PATHFIND_INCOMPLETE : PATHFIND_NORMAL;
		//printf("++ BuildPolyPath :: path type %d\n", m_type);
		return;
	}

	// look for startPoly/endPoly in current path
	// TODO: we can merge it with getPathPolyByPosition() loop
	bool startPolyFound = false;
	bool endPolyFound = false;
	unsigned int pathStartIndex, pathEndIndex;

	if (m_polyLength)
	{
		for (pathStartIndex = 0; pathStartIndex < m_polyLength; ++pathStartIndex)
		{
			// here to catch few bugs
			//MANGOS_ASSERT(m_pathPolyRefs[pathStartIndex] != INVALID_POLYREF || m_sourceUnit->PrintEntryError("PathFinder::BuildPolyPath"));

			if (m_pathPolyRefs[pathStartIndex] == startPoly)
			{
				startPolyFound = true;
				break;
			}
		}

		for (pathEndIndex = m_polyLength - 1; pathEndIndex > pathStartIndex; --pathEndIndex)
		if (m_pathPolyRefs[pathEndIndex] == endPoly)
		{
			endPolyFound = true;
			break;
		}
	}

	if (startPolyFound && endPolyFound)
	{
		//printf("++ BuildPolyPath :: (startPolyFound && endPolyFound)\n");

		// we moved along the path and the target did not move out of our old poly-path
		// our path is a simple subpath case, we have all the data we need
		// just "cut" it out

		m_polyLength = pathEndIndex - pathStartIndex + 1;
		memmove(m_pathPolyRefs, m_pathPolyRefs + pathStartIndex, m_polyLength * sizeof(dtPolyRef));
	}
	else if (startPolyFound && !endPolyFound)
	{
		//printf("++ BuildPolyPath :: (startPolyFound && !endPolyFound)\n");

		// we are moving on the old path but target moved out
		// so we have atleast part of poly-path ready

		m_polyLength -= pathStartIndex;

		// try to adjust the suffix of the path instead of recalculating entire length
		// at given interval the target can not get too far from its last location
		// thus we have less poly to cover
		// sub-path of optimal path is optimal

		// take ~80% of the original length
		// TODO : play with the values here
		unsigned int prefixPolyLength = unsigned int(m_polyLength * 0.8f + 0.5f);
		memmove(m_pathPolyRefs, m_pathPolyRefs + pathStartIndex, prefixPolyLength * sizeof(dtPolyRef));

		dtPolyRef suffixStartPoly = m_pathPolyRefs[prefixPolyLength - 1];

		// we need any point on our suffix start poly to generate poly-path, so we need last poly in prefix data
		float suffixEndPoint[VERTEX_SIZE];
		dtResult = m_navMeshQuery->closestPointOnPoly(suffixStartPoly, endPoint, suffixEndPoint, NULL);
		if (dtStatusFailed(dtResult))
		{
			// we can hit offmesh connection as last poly - closestPointOnPoly() don't like that
			// try to recover by using prev polyref
			--prefixPolyLength;
			suffixStartPoly = m_pathPolyRefs[prefixPolyLength - 1];
			dtResult = m_navMeshQuery->closestPointOnPoly(suffixStartPoly, endPoint, suffixEndPoint, NULL);
			if (dtStatusFailed(dtResult))
			{
				// suffixStartPoly is still invalid, error state
				BuildShortcut();
				m_type = PATHFIND_NOPATH;
				return;
			}
		}

		// generate suffix
		unsigned int suffixPolyLength = 0;
		dtResult = m_navMeshQuery->findPath(
			suffixStartPoly,    // start polygon
			endPoly,            // end polygon
			suffixEndPoint,     // start position
			endPoint,           // end position
			&m_filter,            // polygon search filter
			m_pathPolyRefs + prefixPolyLength - 1,    // [out] path
			(int*)&suffixPolyLength,
			MAX_PATH_LENGTH - prefixPolyLength); // max number of polygons in output path

		if (!suffixPolyLength || dtStatusFailed(dtResult))
		{
			// this is probably an error state, but we'll leave it
			// and hopefully recover on the next Update
			// we still need to copy our preffix
			//sLog.outError("%u's Path Build failed: 0 length path", m_sourceUnit->GetGUIDLow());
		}

		//printf("++  m_polyLength=%u prefixPolyLength=%u suffixPolyLength=%u \n", m_polyLength, prefixPolyLength, suffixPolyLength);

		// new path = prefix + suffix - overlap
		m_polyLength = prefixPolyLength + suffixPolyLength - 1;
	}
	else
	{
		//printf("++ BuildPolyPath :: (!startPolyFound && !endPolyFound)\n");

		// either we have no path at all -> first run
		// or something went really wrong -> we aren't moving along the path to the target
		// just generate new path

		// free and invalidate old path data
		clear();

		dtResult = m_navMeshQuery->findPath(
			startPoly,          // start polygon
			endPoly,            // end polygon
			startPoint,         // start position
			endPoint,           // end position
			&m_filter,           // polygon search filter
			m_pathPolyRefs,     // [out] path
			(int*)&m_polyLength,
			MAX_PATH_LENGTH);   // max number of polygons in output path

		if (!m_polyLength || dtStatusFailed(dtResult))
		{
			// only happens if we passed bad data to findPath(), or navmesh is messed up
			//sLog.outError("%u's Path Build failed: 0 length path", m_sourceUnit->GetGUIDLow());
			BuildShortcut();
			m_type = PATHFIND_NOPATH;
			return;
		}
	}

	// by now we know what type of path we can get
	if (m_pathPolyRefs[m_polyLength - 1] == endPoly && !(m_type & PATHFIND_INCOMPLETE))
	{
		m_type = PATHFIND_NORMAL;
	}
	else
	{
		m_type = PATHFIND_INCOMPLETE;
	}

	// generate the point-path out of our up-to-date poly-path
	BuildPointPath(startPoint, endPoint);
}

void PathFinder::BuildPointPath(const float* startPoint, const float* endPoint)
{
	float pathPoints[MAX_POINT_PATH_LENGTH * VERTEX_SIZE];
	unsigned int pointCount = 0;
	dtStatus dtResult = DT_FAILURE;
	if (m_useStraightPath)
	{
		dtResult = m_navMeshQuery->findStraightPath(
			startPoint,         // start position
			endPoint,           // end position
			m_pathPolyRefs,     // current path
			m_polyLength,       // lenth of current path
			pathPoints,         // [out] path corner points
			NULL,               // [out] flags
			NULL,               // [out] shortened path
			(int*)&pointCount,
			m_pointPathLimit);   // maximum number of points/polygons to use
	}
	else
	{
		dtResult = findSmoothPath(
			startPoint,         // start position
			endPoint,           // end position
			m_pathPolyRefs,     // current path
			m_polyLength,       // length of current path
			pathPoints,         // [out] path corner points
			(int*)&pointCount,
			m_pointPathLimit);    // maximum number of points
	}

	if (pointCount < 2 || dtStatusFailed(dtResult))
	{
		// only happens if pass bad data to findStraightPath or navmesh is broken
		// single point paths can be generated here
		// TODO : check the exact cases
		//printf("++ PathFinder::BuildPointPath FAILED! path sized %d returned\n", pointCount);
		BuildShortcut();
		m_type = PATHFIND_NOPATH;
		return;
	}

	m_pathPoints.resize(pointCount);
	for (unsigned int i = 0; i < pointCount; ++i)
	{
		m_pathPoints[i] = Vector3(pathPoints[i * VERTEX_SIZE + 2], pathPoints[i * VERTEX_SIZE], pathPoints[i * VERTEX_SIZE + 1]);
	}

	// first point is always our current location - we need the next one
	setActualEndPosition(m_pathPoints[pointCount - 1]);

	// force the given destination, if needed
	if (m_forceDestination &&
		(!(m_type & PATHFIND_NORMAL) || !inRange(getEndPosition(), getActualEndPosition(), 1.0f, 1.0f)))
	{
		// we may want to keep partial subpath
		if (dist3DSqr(getActualEndPosition(), getEndPosition()) <
			0.3f * dist3DSqr(getStartPosition(), getEndPosition()))
		{
			setActualEndPosition(getEndPosition());
			m_pathPoints[m_pathPoints.size() - 1] = getEndPosition();
		}
		else
		{
			setActualEndPosition(getEndPosition());
			BuildShortcut();
		}

		//printf("!!!!!!! 4 !!!!!!!\n");
		m_type = PathType(PATHFIND_NORMAL | PATHFIND_NOT_USING_PATH);
	}

	//printf("++ PathFinder::BuildPointPath path type %d size %d poly-size %d\n", m_type, pointCount, m_polyLength);
}

void PathFinder::BuildShortcut()
{
	//printf("++ PathFinder::BuildShortcut :: making shortcut\n");

	clear();

	// make two point path, our curr pos is the start, and dest is the end
	m_pathPoints.resize(2);

	// set start and a default next position
	m_pathPoints[0] = getStartPosition();
	m_pathPoints[1] = getActualEndPosition();

	m_type = PATHFIND_SHORTCUT;
}

void PathFinder::createFilter()
{
	unsigned short includeFlags = 0;
	unsigned short excludeFlags = 0;
	includeFlags |= (NAV_GROUND | NAV_WATER);
	
	m_filter.setIncludeFlags(includeFlags);
	m_filter.setExcludeFlags(excludeFlags);

	// TODO
	updateFilter(false, 0, 0, 0);
}

void PathFinder::updateFilter(bool isSwimming, float x, float y, float z)
{
	// allow creatures to cheat and use different movement types if they are moved
	// forcefully into terrain they can't normally move in
	if (isSwimming)
	{
		unsigned short includedFlags = m_filter.getIncludeFlags();
		includedFlags |= getNavTerrain(x, y, z);

		m_filter.setIncludeFlags(includedFlags);
	}
}

NavTerrain PathFinder::getNavTerrain(float x, float y, float z)
{
	GridMapLiquidData data;
	//m_sourceUnit->GetTerrain()->getLiquidStatus(x, y, z, MAP_ALL_LIQUIDS, &data);

	// TODO !!!
	return NAV_GROUND;

	switch (data.type_flags)
	{
	case MAP_LIQUID_TYPE_WATER:
	case MAP_LIQUID_TYPE_OCEAN:
		return NAV_WATER;
	case MAP_LIQUID_TYPE_MAGMA:
		return NAV_MAGMA;
	case MAP_LIQUID_TYPE_SLIME:
		return NAV_SLIME;
	default:
		return NAV_GROUND;
	}
}

bool PathFinder::HaveTile(const Vector3& p) const
{
	int tx, ty;
	float point[VERTEX_SIZE] = { p.y, p.z, p.x };

	m_navMesh->calcTileLoc(point, &tx, &ty);
	return (m_navMesh->getTileAt(tx, ty, 0) != NULL);
}

unsigned int PathFinder::fixupCorridor(dtPolyRef* path, unsigned int npath, unsigned int maxPath,
	const dtPolyRef* visited, unsigned int nvisited)
{
	int furthestPath = -1;
	int furthestVisited = -1;

	// Find furthest common polygon.
	for (int i = npath - 1; i >= 0; --i)
	{
		bool found = false;
		for (int j = nvisited - 1; j >= 0; --j)
		{
			if (path[i] == visited[j])
			{
				furthestPath = i;
				furthestVisited = j;
				found = true;
			}
		}
		if (found)
		{
			break;
		}
	}

	// If no intersection found just return current path.
	if (furthestPath == -1 || furthestVisited == -1)
	{
		return npath;
	}

	// Concatenate paths.

	// Adjust beginning of the buffer to include the visited.
	unsigned int req = nvisited - furthestVisited;
	unsigned int orig = unsigned int(furthestPath + 1) < npath ? furthestPath + 1 : npath;
	unsigned int size = npath > orig ? npath - orig : 0;
	if (req + size > maxPath)
	{
		size = maxPath - req;
	}

	if (size)
	{
		memmove(path + req, path + orig, size * sizeof(dtPolyRef));
	}

	// Store visited
	for (unsigned int i = 0; i < req; ++i)
	{
		path[i] = visited[(nvisited - 1) - i];
	}

	return req + size;
}

bool PathFinder::getSteerTarget(const float* startPos, const float* endPos,
	float minTargetDist, const dtPolyRef* path, unsigned int pathSize,
	float* steerPos, unsigned char& steerPosFlag, dtPolyRef& steerPosRef)
{
	// Find steer target.
	static const unsigned int MAX_STEER_POINTS = 3;
	float steerPath[MAX_STEER_POINTS * VERTEX_SIZE];
	unsigned char steerPathFlags[MAX_STEER_POINTS];
	dtPolyRef steerPathPolys[MAX_STEER_POINTS];
	unsigned int nsteerPath = 0;
	dtStatus dtResult = m_navMeshQuery->findStraightPath(startPos, endPos, path, pathSize,
		steerPath, steerPathFlags, steerPathPolys, (int*)&nsteerPath, MAX_STEER_POINTS);
	if (!nsteerPath || dtStatusFailed(dtResult))
	{
		return false;
	}

	// Find vertex far enough to steer to.
	unsigned int ns = 0;
	while (ns < nsteerPath)
	{
		// Stop at Off-Mesh link or when point is further than slop away.
		if ((steerPathFlags[ns] & DT_STRAIGHTPATH_OFFMESH_CONNECTION) ||
			!inRangeYZX(&steerPath[ns * VERTEX_SIZE], startPos, minTargetDist, 1000.0f))
		{
			break;
		}
		++ns;
	}
	// Failed to find good point to steer to.
	if (ns >= nsteerPath)
	{
		return false;
	}

	dtVcopy(steerPos, &steerPath[ns * VERTEX_SIZE]);
	steerPos[1] = startPos[1];  // keep Z value
	steerPosFlag = steerPathFlags[ns];
	steerPosRef = steerPathPolys[ns];

	return true;
}

dtStatus PathFinder::findSmoothPath(const float* startPos, const float* endPos,
	const dtPolyRef* polyPath, unsigned int polyPathSize,
	float* smoothPath, int* smoothPathSize, unsigned int maxSmoothPathSize)
{
	*smoothPathSize = 0;
	unsigned int nsmoothPath = 0;

	dtPolyRef polys[MAX_PATH_LENGTH];
	memcpy(polys, polyPath, sizeof(dtPolyRef)*polyPathSize);
	unsigned int npolys = polyPathSize;

	float iterPos[VERTEX_SIZE], targetPos[VERTEX_SIZE];
	dtStatus dtResult = m_navMeshQuery->closestPointOnPolyBoundary(polys[0], startPos, iterPos);
	if (dtStatusFailed(dtResult))
	{
		return DT_FAILURE;
	}

	dtResult = m_navMeshQuery->closestPointOnPolyBoundary(polys[npolys - 1], endPos, targetPos);
	if (dtStatusFailed(dtResult))
	{
		return DT_FAILURE;
	}

	dtVcopy(&smoothPath[nsmoothPath * VERTEX_SIZE], iterPos);
	++nsmoothPath;

	// Move towards target a small advancement at a time until target reached or
	// when ran out of memory to store the path.
	while (npolys && nsmoothPath < maxSmoothPathSize)
	{
		// Find location to steer towards.
		float steerPos[VERTEX_SIZE];
		unsigned char steerPosFlag;
		dtPolyRef steerPosRef = INVALID_POLYREF;

		if (!getSteerTarget(iterPos, targetPos, SMOOTH_PATH_SLOP, polys, npolys, steerPos, steerPosFlag, steerPosRef))
		{
			break;
		}

		bool endOfPath = (steerPosFlag & DT_STRAIGHTPATH_END);
		bool offMeshConnection = (steerPosFlag & DT_STRAIGHTPATH_OFFMESH_CONNECTION);

		// Find movement delta.
		float delta[VERTEX_SIZE];
		dtVsub(delta, steerPos, iterPos);
		float len = dtSqrt(dtVdot(delta, delta));
		// If the steer target is end of path or off-mesh link, do not move past the location.
		if ((endOfPath || offMeshConnection) && len < SMOOTH_PATH_STEP_SIZE)
		{
			len = 1.0f;
		}
		else
		{
			len = SMOOTH_PATH_STEP_SIZE / len;
		}

		float moveTgt[VERTEX_SIZE];
		dtVmad(moveTgt, iterPos, delta, len);

		// Move
		float result[VERTEX_SIZE];
		const static unsigned int MAX_VISIT_POLY = 16;
		dtPolyRef visited[MAX_VISIT_POLY];

		unsigned int nvisited = 0;
		m_navMeshQuery->moveAlongSurface(polys[0], iterPos, moveTgt, &m_filter, result, visited, (int*)&nvisited, MAX_VISIT_POLY);
		npolys = fixupCorridor(polys, npolys, MAX_PATH_LENGTH, visited, nvisited);

		m_navMeshQuery->getPolyHeight(polys[0], result, &result[1]);
		result[1] += 0.5f;
		dtVcopy(iterPos, result);

		// Handle end of path and off-mesh links when close enough.
		if (endOfPath && inRangeYZX(iterPos, steerPos, SMOOTH_PATH_SLOP, 1.0f))
		{
			// Reached end of path.
			dtVcopy(iterPos, targetPos);
			if (nsmoothPath < maxSmoothPathSize)
			{
				dtVcopy(&smoothPath[nsmoothPath * VERTEX_SIZE], iterPos);
				++nsmoothPath;
			}
			break;
		}
		else if (offMeshConnection && inRangeYZX(iterPos, steerPos, SMOOTH_PATH_SLOP, 1.0f))
		{
			// Advance the path up to and over the off-mesh connection.
			dtPolyRef prevRef = INVALID_POLYREF;
			dtPolyRef polyRef = polys[0];
			unsigned int npos = 0;
			while (npos < npolys && polyRef != steerPosRef)
			{
				prevRef = polyRef;
				polyRef = polys[npos];
				++npos;
			}

			for (unsigned int i = npos; i < npolys; ++i)
			{
				polys[i - npos] = polys[i];
			}

			npolys -= npos;

			// Handle the connection.
			float startPos[VERTEX_SIZE], endPos[VERTEX_SIZE];
			dtResult = m_navMesh->getOffMeshConnectionPolyEndPoints(prevRef, polyRef, startPos, endPos);
			if (dtStatusSucceed(dtResult))
			{
				if (nsmoothPath < maxSmoothPathSize)
				{
					dtVcopy(&smoothPath[nsmoothPath * VERTEX_SIZE], startPos);
					++nsmoothPath;
				}
				// Move position at the other side of the off-mesh link.
				dtVcopy(iterPos, endPos);

				m_navMeshQuery->getPolyHeight(polys[0], iterPos, &iterPos[1]);
				iterPos[1] += 0.5f;
			}
		}

		// Store results.
		if (nsmoothPath < maxSmoothPathSize)
		{
			dtVcopy(&smoothPath[nsmoothPath * VERTEX_SIZE], iterPos);
			++nsmoothPath;
		}
	}

	*smoothPathSize = nsmoothPath;

	// this is most likely a loop
	return nsmoothPath < MAX_POINT_PATH_LENGTH ? DT_SUCCESS : DT_FAILURE;
}

bool PathFinder::inRangeYZX(const float* v1, const float* v2, float r, float h) const
{
	const float dx = v2[0] - v1[0];
	const float dy = v2[1] - v1[1]; // elevation
	const float dz = v2[2] - v1[2];
	return (dx * dx + dz * dz) < r * r && fabsf(dy) < h;
}

bool PathFinder::inRange(const Vector3& p1, const Vector3& p2, float r, float h) const
{
	Vector3 d = p1 - p2;
	return (d.x * d.x + d.y * d.y) < r * r && fabsf(d.z) < h;
}

float PathFinder::dist3DSqr(const Vector3& p1, const Vector3& p2) const
{
	return (p1 - p2).squaredLength();
}
