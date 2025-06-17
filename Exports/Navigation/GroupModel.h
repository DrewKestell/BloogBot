#pragma once

#include <vector>
#include "Vec3Ray.h"
#include "BIH.h"
#include "VMapFileUtils.h"
#include "WmoLiquid.h"

namespace VMAP {
	class GroupModel {
	public:
		GroupModel();
		GroupModel(const GroupModel& other);
		GroupModel(unsigned int mogpFlags, unsigned int groupWMOID, const AABox& bound);
		~GroupModel();

		void SetMeshData(std::vector<Vec3>& vert, std::vector<MeshTriangle>& tri);

		bool IntersectRay(Ray const& ray, float& distance, bool stopAtFirstHit, bool ignoreM2Model) const;
		bool IsInsideObject(const Vec3& pos, const Vec3& down, float& z_dist) const;

		// Liquid functions stubbed
		bool GetLiquidLevel(const Vec3& pos, float& liqHeight) const { return false; }
		unsigned int GetLiquidType() const { return 0; }

		// Not implemented: serialization
		bool WriteToFile(FILE*) { return false; }
		bool ReadFromFile(FILE* rf);
		bool ReadFromRaw(const GroupModel_Raw& raw);

		const AABox& GetBound() const { return iBound; }
		unsigned int GetMogpFlags() const { return iMogpFlags; }
		unsigned int GetWmoID() const { return iGroupWMOID; }

	protected:
		AABox iBound;
		unsigned int iMogpFlags;// 0x8 outdor; 0x2000 indoor
		unsigned int iGroupWMOID;
		std::vector<Vec3> vertices;
		std::vector<MeshTriangle> triangles;
		BIH meshTree;
		WmoLiquid* iLiquid;
	};
}