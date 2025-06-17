#pragma once

#include <vector>
#include <cstddef>
#include <limits>
#include <cstdio>
#include "PhysicsQuery.h"
#include "BIH.h"

namespace VMAP {
	
	// Core geometry mesh, e.g. for WMO/M2 models
	class WorldModel {
	public:
		unsigned int Flags = 0;
		AABox bound;
		std::vector<Vec3> vertices;
		std::vector<MeshTriangle> triangles;

		WorldModel();
		WorldModel(const std::vector<Vec3>& verts, const std::vector<MeshTriangle>& tris);

		bool ReadFile(const std::string& filename); // Loads mesh from file
		bool ReadM2VmoFile(const std::string& filename);
		void computeBounds();

		// Ray-mesh intersection: modifies maxDist if hit
		bool IntersectRay(Ray const& ray, float& distance, bool stopAtFirstHit, bool ignoreM2Model) const;

		// Returns true if hit; fills AreaInfo with ground_Z and adtId if provided
		bool GetAreaInfo(const Vec3& p, const Vec3& dir, float& dist, struct AreaInfo& info) const;

		// Returns true if hit; fills LocationInfo with ground_Z, etc
		bool GetLocationInfo(const Vec3& p, const Vec3& dir, float& dist, struct LocationInfo& info) const;
		static bool RayIntersectsTriangle(const Vec3& orig, const Vec3& dir,
			const Vec3& v0, const Vec3& v1, const Vec3& v2, float& t);

		static bool RayIntersectsTriangleWithBarycentric(
			const Vec3& orig, const Vec3& dir,
			const Vec3& v0, const Vec3& v1, const Vec3& v2,
			float& t, float& u, float& v);
	protected:
		unsigned int RootWMOID;
		std::vector<GroupModel> groupModels;
		BIH groupTree;
		unsigned int modelFlags;
	};
} // namespace VMAP
