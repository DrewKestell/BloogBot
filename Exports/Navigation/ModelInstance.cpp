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

        try
        {
            // vMaNGOS ModelSpawn format:
            // uint32 flags
            // uint16 adtId  
            // uint32 ID
            // Vector3 position (3 floats)
            // Vector3 rotation (3 floats)
            // float scale
            // [if flags & MOD_HAS_BOUND:]
            //   AABox bounds (6 floats)
            // uint32 nameLength
            // char name[nameLength]

            // Read flags
            if (fread(&spawn.flags, sizeof(uint32_t), 1, rf) != 1)
            {
                std::cerr << "[ModelSpawn] ERROR: Failed to read flags" << std::endl;
                return false;
            }

            // Read ADT ID
            if (fread(&spawn.adtId, sizeof(uint16_t), 1, rf) != 1)
            {
                std::cerr << "[ModelSpawn] ERROR: Failed to read adtId" << std::endl;
                return false;
            }

            // Read spawn ID
            if (fread(&spawn.ID, sizeof(uint32_t), 1, rf) != 1)
            {
                std::cerr << "[ModelSpawn] ERROR: Failed to read ID" << std::endl;
                return false;
            }

            // Read position (3 floats)
            if (fread(&spawn.iPos.x, sizeof(float), 1, rf) != 1 ||
                fread(&spawn.iPos.y, sizeof(float), 1, rf) != 1 ||
                fread(&spawn.iPos.z, sizeof(float), 1, rf) != 1)
            {
                std::cerr << "[ModelSpawn] ERROR: Failed to read position" << std::endl;
                return false;
            }

            // Validate position
            const float MAX_COORD = 100000.0f;
            if (std::isnan(spawn.iPos.x) || std::isnan(spawn.iPos.y) || std::isnan(spawn.iPos.z) ||
                std::isinf(spawn.iPos.x) || std::isinf(spawn.iPos.y) || std::isinf(spawn.iPos.z))
            {
                std::cerr << "[ModelSpawn] ERROR: Invalid position (NaN or Inf)" << std::endl;
                return false;
            }

            if (std::abs(spawn.iPos.x) > MAX_COORD || std::abs(spawn.iPos.y) > MAX_COORD ||
                std::abs(spawn.iPos.z) > MAX_COORD)
            {
                std::cerr << "[ModelSpawn] Warning: Large position: ("
                    << spawn.iPos.x << ", " << spawn.iPos.y << ", " << spawn.iPos.z << ")" << std::endl;
            }

            // Read rotation (3 floats)
            if (fread(&spawn.iRot.x, sizeof(float), 1, rf) != 1 ||
                fread(&spawn.iRot.y, sizeof(float), 1, rf) != 1 ||
                fread(&spawn.iRot.z, sizeof(float), 1, rf) != 1)
            {
                std::cerr << "[ModelSpawn] ERROR: Failed to read rotation" << std::endl;
                return false;
            }

            // Read scale
            if (fread(&spawn.iScale, sizeof(float), 1, rf) != 1)
            {
                std::cerr << "[ModelSpawn] ERROR: Failed to read scale" << std::endl;
                return false;
            }

            // Validate scale
            if (spawn.iScale <= 0.0f || spawn.iScale > 1000.0f)
            {
                std::cerr << "[ModelSpawn] Warning: Unusual scale: " << spawn.iScale << std::endl;
                if (spawn.iScale <= 0.0f)
                    spawn.iScale = 1.0f;
            }

            // Read bounding box if present
            bool has_bound = (spawn.flags & MOD_HAS_BOUND) != 0;
            if (has_bound)
            {
                // vMaNGOS stores bounds as 6 floats
                float boundsData[6];
                if (fread(boundsData, sizeof(float), 6, rf) != 6)
                {
                    std::cerr << "[ModelSpawn] ERROR: Failed to read bounds" << std::endl;
                    return false;
                }

                G3D::Vector3 bLow(boundsData[0], boundsData[1], boundsData[2]);
                G3D::Vector3 bHigh(boundsData[3], boundsData[4], boundsData[5]);

                // Validate bounds
                if (std::isnan(bLow.x) || std::isnan(bLow.y) || std::isnan(bLow.z) ||
                    std::isnan(bHigh.x) || std::isnan(bHigh.y) || std::isnan(bHigh.z))
                {
                    std::cerr << "[ModelSpawn] Warning: Invalid bounds (NaN), using default" << std::endl;
                    spawn.iBound = G3D::AABox(spawn.iPos - G3D::Vector3(10, 10, 10),
                        spawn.iPos + G3D::Vector3(10, 10, 10));
                }
                else
                {
                    spawn.iBound = G3D::AABox(bLow, bHigh);
                }
            }
            else
            {
                // Create default bounds around position
                spawn.iBound = G3D::AABox(spawn.iPos - G3D::Vector3(10, 10, 10),
                    spawn.iPos + G3D::Vector3(10, 10, 10));
            }

            // Read name length
            uint32_t nameLen = 0;
            if (fread(&nameLen, sizeof(uint32_t), 1, rf) != 1)
            {
                std::cerr << "[ModelSpawn] ERROR: Failed to read name length" << std::endl;
                return false;
            }

            // Sanity check name length
            if (nameLen > 500)
            {
                std::cerr << "[ModelSpawn] ERROR: Name too long: " << nameLen << " bytes" << std::endl;
                return false;
            }

            // Read name
            if (nameLen > 0)
            {
                char* nameBuff = new char[nameLen + 1];
                memset(nameBuff, 0, nameLen + 1);

                if (fread(nameBuff, sizeof(char), nameLen, rf) != nameLen)
                {
                    std::cerr << "[ModelSpawn] ERROR: Failed to read name" << std::endl;
                    delete[] nameBuff;
                    return false;
                }

                spawn.name = std::string(nameBuff, nameLen);
                delete[] nameBuff;

                // Validate name
                if (spawn.name.find('\0') != std::string::npos && spawn.name.find('\0') < spawn.name.length() - 1)
                {
                    std::cerr << "[ModelSpawn] Warning: Name contains embedded null characters" << std::endl;
                    spawn.name = spawn.name.substr(0, spawn.name.find('\0'));
                }
            }
            else
            {
                spawn.name.clear();
            }

            return true;
        }
        catch (const std::exception& e)
        {
            std::cerr << "[ModelSpawn] ERROR: Exception in readFromFile: " << e.what() << std::endl;
            return false;
        }
    }

    bool ModelSpawn::writeToFile(FILE* wf, const ModelSpawn& spawn)
    {
        if (fwrite(&spawn.flags, sizeof(uint32_t), 1, wf) != 1)
            return false;
        if (fwrite(&spawn.adtId, sizeof(uint16_t), 1, wf) != 1)
            return false;
        if (fwrite(&spawn.ID, sizeof(uint32_t), 1, wf) != 1)
            return false;

        // Write position as 3 separate floats
        if (fwrite(&spawn.iPos.x, sizeof(float), 1, wf) != 1 ||
            fwrite(&spawn.iPos.y, sizeof(float), 1, wf) != 1 ||
            fwrite(&spawn.iPos.z, sizeof(float), 1, wf) != 1)
            return false;

        // Write rotation as 3 separate floats
        if (fwrite(&spawn.iRot.x, sizeof(float), 1, wf) != 1 ||
            fwrite(&spawn.iRot.y, sizeof(float), 1, wf) != 1 ||
            fwrite(&spawn.iRot.z, sizeof(float), 1, wf) != 1)
            return false;

        if (fwrite(&spawn.iScale, sizeof(float), 1, wf) != 1)
            return false;

        bool has_bound = (spawn.flags & MOD_HAS_BOUND) != 0;
        if (has_bound)
        {
            // Write bounds as 6 floats
            G3D::Vector3 bLow = spawn.iBound.low();
            G3D::Vector3 bHigh = spawn.iBound.high();
            float boundsData[6] = { bLow.x, bLow.y, bLow.z, bHigh.x, bHigh.y, bHigh.z };
            if (fwrite(boundsData, sizeof(float), 6, wf) != 6)
                return false;
        }

        uint32_t nameLen = spawn.name.length();
        if (fwrite(&nameLen, sizeof(uint32_t), 1, wf) != 1)
            return false;
        if (nameLen > 0 && fwrite(spawn.name.c_str(), sizeof(char), nameLen, wf) != nameLen)
            return false;

        return true;
    }

    // ... Rest of ModelInstance implementation remains the same ...
    ModelInstance::ModelInstance()
        : iInvScale(0), iModel(nullptr)
    {
    }

    ModelInstance::ModelInstance(const ModelSpawn& spawn, std::shared_ptr<WorldModel> model)
        : ModelSpawn(spawn), iModel(model)
    {
        iInvRot = G3D::Matrix3::fromEulerAnglesZYX(
            G3D::pi() * iRot.y / 180.f,
            G3D::pi() * iRot.x / 180.f,
            G3D::pi() * iRot.z / 180.f
        ).inverse();
        iInvScale = 1.f / iScale;
    }

    bool ModelInstance::intersectRay(const G3D::Ray& ray, float& maxDist,
        bool stopAtFirstHit, bool ignoreM2Model) const
    {
        if (!iModel)
            return false;

        float time = ray.intersectionTime(iBound);
        if (time == G3D::inf())
            return false;

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
        if (!iModel || (flags & MOD_M2))
            return;
        if (!iBound.contains(p))
            return;

        G3D::Vector3 pModel = iInvRot * (p - iPos) * iInvScale;
        G3D::Vector3 zDirModel = iInvRot * G3D::Vector3(0, 0, -1);
        float zDist;

        if (iModel->IntersectPoint(pModel, zDirModel, zDist, info))
        {
            G3D::Vector3 modelGround = pModel + zDirModel * zDist;
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
        float zDist;
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
        float zDist;

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