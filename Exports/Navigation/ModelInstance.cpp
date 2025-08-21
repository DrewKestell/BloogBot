// ModelInstance.cpp - Fixed ModelSpawn::readFromFile for vMaNGOS format
#include "ModelInstance.h"
#include "WorldModel.h"
#include "VMapDefinitions.h"

namespace VMAP
{
	bool ModelSpawn::readFromFile(FILE* rf, ModelSpawn& spawn)
	{
		if (!rf)
		{
			std::cerr << "[ModelSpawn] ERROR: NULL file pointer" << std::endl;
			return false;
		}
		uint32_t check = 0;

		// Read flags
		check += fread(&spawn.flags, sizeof(uint32_t), 1, rf);

		// EoF check
		if (!check)
		{
			if (ferror(rf))
				std::cerr << "[ModelSpawn] Error reading ModelSpawn!" << std::endl;
			return false;
		}

		// Read basic data
		check += fread(&spawn.adtId, sizeof(uint16_t), 1, rf);
		check += fread(&spawn.ID, sizeof(uint32_t), 1, rf);

		// FIXED: Read position as 3 floats in ONE call (matching reference)
		check += fread(&spawn.iPos, sizeof(float), 3, rf);

		// FIXED: Read rotation as 3 floats in ONE call (matching reference)
		check += fread(&spawn.iRot, sizeof(float), 3, rf);

		check += fread(&spawn.iScale, sizeof(float), 1, rf);

		// Read bounding box if present
		bool has_bound = (spawn.flags & MOD_HAS_BOUND) != 0;
		if (has_bound)
		{
			// FIXED: Read as two Vector3 directly (matching reference)
			G3D::Vector3 bLow, bHigh;
			check += fread(&bLow, sizeof(float), 3, rf);
			check += fread(&bHigh, sizeof(float), 3, rf);
			spawn.iBound = G3D::AABox(bLow, bHigh);
		}

		// Read name length
		uint32_t nameLen;
		check += fread(&nameLen, sizeof(uint32_t), 1, rf);

		// FIXED: Validate read count (matching reference)
		if (check != uint32_t(has_bound ? 17 : 11))
		{
			std::cerr << "[ModelSpawn] Error reading ModelSpawn!" << std::endl;
			return false;
		}

		// Sanity check name length
		if (nameLen > 500)
		{
			std::cerr << "[ModelSpawn] Error: Name too long: " << nameLen << std::endl;
			return false;
		}

		// FIXED: Use fixed buffer like reference (avoids dynamic allocation)
		char nameBuff[500];
		check = fread(nameBuff, sizeof(char), nameLen, rf);
		if (check != nameLen)
		{
			std::cerr << "[ModelSpawn] Error reading name string!" << std::endl;
			return false;
		}

		spawn.name = std::string(nameBuff, nameLen);

		return true;
	}

	ModelInstance::ModelInstance()
		: iInvScale(0), iModel(nullptr)
	{
	}

	ModelInstance::ModelInstance(const ModelSpawn& spawn, std::shared_ptr<WorldModel> model)
		: ModelSpawn(spawn), iModel(model)
	{
		iInvRot = G3D::Matrix3::fromEulerAnglesZYX(
			G3D::pi() * iRot.y / 180.f,  // z rotation
			G3D::pi() * iRot.x / 180.f,  // y rotation  
			G3D::pi() * iRot.z / 180.f   // x rotation
		).inverse();
		iInvScale = 1.f / iScale;
	}

	bool ModelInstance::intersectRay(const G3D::Ray& ray, float& maxDist,
		bool stopAtFirstHit, bool ignoreM2Model) const
	{
		if (!iModel)
		{
			return false;
		}

		float time = ray.intersectionTime(iBound);
		if (time == G3D::inf())
		{
			return false;
		}

		// child bounds are defined in object space:
		G3D::Vector3 p = iInvRot * (ray.origin() - iPos) * iInvScale;
		G3D::Ray modRay(p, iInvRot * ray.direction());
		float distance = maxDist * iInvScale;

		bool hit = iModel->IntersectRay(modRay, distance, stopAtFirstHit, ignoreM2Model);

		if (hit)
		{
			distance *= iScale;
			maxDist = distance;
		}

		return hit;
	}

	void ModelInstance::intersectPoint(const G3D::Vector3& p, AreaInfo& info) const
	{
		if (!iModel)
		{
			return;
		}

		// M2 files don't contain area info, only WMO files
		if (flags & MOD_M2)
			return;

		if (!iBound.contains(p))
			return;

		// child bounds are defined in object space:
		G3D::Vector3 pModel = iInvRot * (p - iPos) * iInvScale;
		G3D::Vector3 zDirModel = iInvRot * G3D::Vector3(0, 0, -1);  // Vector3::down()
		float zDist = 10000.0f;;

		if (iModel->IntersectPoint(pModel, zDirModel, zDist, info))
		{
			G3D::Vector3 modelGround = pModel + zDirModel * zDist;
			// Transform back to world space. Note that:
			// Mat * vec == vec * Mat.transpose()
			// and for rotation matrices: Mat.inverse() == Mat.transpose()
			float world_Z = ((modelGround * iInvRot) * iScale + iPos).z;
			if (info.ground_Z < world_Z)
			{
				info.ground_Z = world_Z;
				info.adtId = adtId;
			}
		}
	}

	bool ModelInstance::GetLocationInfo(const G3D::Vector3& p, LocationInfo& info) const
	{
		if (!iModel || (flags & MOD_M2))
			return false;
		if (!iBound.contains(p))
			return false;

		G3D::Vector3 pModel = iInvRot * (p - iPos) * iInvScale;
		G3D::Vector3 zDirModel = iInvRot * G3D::Vector3(0, 0, -1);
		float zDist = 10000.0f;;
		GroupLocationInfo groupInfo;

		if (iModel->GetLocationInfo(pModel, zDirModel, zDist, groupInfo))
		{
			G3D::Vector3 modelGround = pModel + zDirModel * zDist;
			float world_Z = ((modelGround * iInvRot) * iScale + iPos).z;
			if (info.ground_Z < world_Z)
			{
				info.rootId = groupInfo.rootId;
				info.hitModel = groupInfo.hitModel;
				info.ground_Z = world_Z;
				info.hitInstance = this;
				return true;
			}
		}
		return false;
	}

	bool ModelInstance::GetLiquidLevel(const G3D::Vector3& p, LocationInfo& info,
		float& liqHeight) const
	{
		if (!info.hitModel)
			return false;

		G3D::Vector3 pModel = iInvRot * (p - iPos) * iInvScale;
		if (info.hitModel->GetLiquidLevel(pModel, liqHeight))
		{
			liqHeight = (G3D::Vector3(pModel.x, pModel.y, liqHeight) * iInvRot * iScale + iPos).z;
			return true;
		}
		return false;
	}

	void ModelInstance::getAreaInfo(G3D::Vector3& pos, uint32_t& flags,
		int32_t& adtId, int32_t& rootId, int32_t& groupId) const
	{
		if (!iModel || (this->flags & MOD_M2))
			return;

		if (!iBound.contains(pos))
			return;

		G3D::Vector3 pModel = iInvRot * (pos - iPos) * iInvScale;
		G3D::Vector3 zDirModel = iInvRot * G3D::Vector3(0, 0, -1);
		float zDist = 10000.0f;

		AreaInfo info;
		if (iModel->IntersectPoint(pModel, zDirModel, zDist, info))
		{
			flags = info.flags;
			adtId = this->adtId;
			rootId = info.rootId;
			groupId = info.groupId;

			G3D::Vector3 modelGround = pModel + zDirModel * zDist;
			float world_Z = ((modelGround * iInvRot) * iScale + iPos).z;
			if (pos.z > world_Z)
				pos.z = world_Z;
		}
	}
}