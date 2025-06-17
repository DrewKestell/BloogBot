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

#include "ModelInstance.h"
#include "PhysicsQuery.h"
#include "GroupModel.h"
#include "WorldModel.h"
#include "MapTree.h"
#include "VMapDefinitions.h"
#include <cstdio>

namespace VMAP
{
    /**
     * @brief Constructor for ModelInstance.
     *
     * @param spawn The model spawn data.
     * @param model The world model.
     */
    ModelInstance::ModelInstance(const ModelSpawn& spawn, WorldModel* model) : ModelSpawn(spawn), iModel(model)
    {
        iInvRot = Matrix3::fromEulerAnglesZYX(pi() * iRot.y / 180.f, pi() * iRot.x / 180.f, pi() * iRot.z / 180.f).inverse();
        iInvScale = 1.f / iScale;
    }

    /**
     * @brief Intersects a ray with the model instance.
     *
     * @param pRay The ray to intersect.
     * @param pMaxDist The maximum distance to check.
     * @param pStopAtFirstHit Whether to stop at the first hit.
     * @return true if an intersection is found, false otherwise.
     */
    bool ModelInstance::intersectRay(Ray const& pRay, float& pMaxDist, bool pStopAtFirstHit, bool ignoreM2Model) const
    {
        if (!iModel)
        {
            return false;
        }
        float time = pRay.intersectionTime(iBound);
        if (time == finf())
        {
            return false;
        }
        // child bounds are defined in object space:
        Vec3 p = iInvRot * (pRay.origin() - iPos) * iInvScale;
        Ray modRay(p, iInvRot * pRay.direction());
        float distance = pMaxDist * iInvScale;
        bool hit = iModel->IntersectRay(modRay, distance, pStopAtFirstHit, ignoreM2Model);
        if (hit)
        {
            distance *= iScale;
            pMaxDist = distance;
            //printf("LoS HIT ! Flags 0x%x (%s)", flags, name.c_str());
        }
        return hit;
    }

    /**
     * @brief Retrieves area information for a given position.
     *
     * @param p The position to check.
     * @param info The area information.
     */
    void ModelInstance::GetAreaInfo(const Vec3& p, AreaInfo& info) const
    {
        if (!iModel)
        {
            return;
        }

        // M2 files don't contain area info, only WMO files
        if (flags & MOD_M2)
        {
            return;
        }

        if (!iBound.contains(p))
        {
            return;
        }

        // Child bounds are defined in object space:
        Vec3 pModel = iInvRot * (p - iPos) * iInvScale;
        Vec3 zDirModel = iInvRot * Vec3(0.f, 0.f, -1.f);
        float zDist;
        if (iModel->GetAreaInfo(pModel, zDirModel, zDist, info))
        {
            Vec3 modelGround = pModel + zDist * zDirModel;
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

    /**
     * @brief Retrieves location information for a given position.
     *
     * @param p The position to check.
     * @param info The location information.
     * @return true if location information was found, false otherwise.
     */
    bool ModelInstance::GetLocationInfo(const Vec3& p, LocationInfo& info) const
    {
        if (!iModel)
        {
            return false;
        }

        // M2 files don't contain area info, only WMO files
        if (flags & MOD_M2)
        {
            return false;
        }

        if (!iBound.contains(p))
        {
            return false;
        }

        // Child bounds are defined in object space:
        Vec3 pModel = iInvRot * (p - iPos) * iInvScale;
        Vec3 zDirModel = iInvRot * Vec3(0.f, 0.f, -1.f);
        float zDist;
        if (iModel->GetLocationInfo(pModel, zDirModel, zDist, info))
        {
            Vec3 modelGround = pModel + zDist * zDirModel;
            // Transform back to world space. Note that:
            // Mat * vec == vec * Mat.transpose()
            // and for rotation matrices: Mat.inverse() == Mat.transpose()
            float world_Z = ((modelGround * iInvRot) * iScale + iPos).z;
            if (info.ground_Z < world_Z) // hm...could it be handled automatically with zDist at intersection?
            {
                info.ground_Z = world_Z;
                info.hitInstance = this;
                return true;
            }
        }
        return false;
    }

    /**
     * @brief Retrieves the liquid level at a given position.
     *
     * @param p The position to check.
     * @param info The location information.
     * @param liqHeight The liquid height.
     * @return true if the liquid level was found, false otherwise.
     */
    bool ModelInstance::GetLiquidLevel(const Vec3& p, LocationInfo& info, float& liqHeight) const
    {
        // Child bounds are defined in object space:
        Vec3 pModel = iInvRot * (p - iPos) * iInvScale;
        // Vec3 zDirModel = iInvRot * Vec3(0.f, 0.f, -1.f);
        float zLevel;
        if (info.hitModel->GetLiquidLevel(pModel, zLevel))
        {
            // Calculate world height (zDist in model coords):
            // Despite making little sense, there ARE some (slightly) tilted WMOs...
            // We can only determine liquid height in LOCAL z-direction (heightmap data),
            // so with increasing tilt, liquid calculation gets increasingly wrong...not my fault, really :p
            liqHeight = (zLevel - pModel.z) * iScale + p.z;
            return true;
        }
        return false;
    }

    /**
     * @brief Reads a ModelSpawn from a file.
     *
     * @param rf The file to read from.
     * @param spawn The ModelSpawn to read into.
     * @return true if the read was successful, false otherwise.
     */
    bool ModelSpawn::ReadFromFile(FILE* rf, ModelSpawn& spawn)
    {
        unsigned int flags = 0;
        unsigned short adtId = 0;
        unsigned int ID = 0;
        Vec3 iPos, iRot;
        float iScale = 0;
        AABox iBound;

        if (fread(&flags, sizeof(unsigned int), 1, rf) != 1) {
            std::cout << "[ModelSpawn] Failed to read flags" << std::endl;
            return false;
        }

        if (fread(&adtId, sizeof(unsigned short), 1, rf) != 1) {
            std::cout << "[ModelSpawn] Failed to read adtId" << std::endl;
            return false;
        }

        if (fread(&ID, sizeof(unsigned int), 1, rf) != 1) {
            std::cout << "[ModelSpawn] Failed to read ID" << std::endl;
            return false;
        }

        if (fread(&iPos, sizeof(float), 3, rf) != 3) {
            std::cout << "[ModelSpawn] Failed to read position" << std::endl;
            return false;
        }

        if (fread(&iRot, sizeof(float), 3, rf) != 3) {
            std::cout << "[ModelSpawn] Failed to read rotation" << std::endl;
            return false;
        }

        if (fread(&iScale, sizeof(float), 1, rf) != 1) {
            std::cout << "[ModelSpawn] Failed to read scale" << std::endl;
            return false;
        }

        bool has_bound = (flags & MOD_HAS_BOUND);
        if (has_bound) {
            Vec3 bLow, bHigh;
            if (fread(&bLow, sizeof(float), 3, rf) != 3) {
                std::cout << "[ModelSpawn] Failed to read bound low" << std::endl;
                return false;
            }
            if (fread(&bHigh, sizeof(float), 3, rf) != 3) {
                std::cout << "[ModelSpawn] Failed to read bound high" << std::endl;
                return false;
            }
            iBound = AABox(bLow, bHigh);
        }
        else {
            iBound = AABox();
        }

        unsigned int nameLen = 0;
        if (fread(&nameLen, sizeof(unsigned int), 1, rf) != 1) {
            std::cout << "[ModelSpawn] Failed to read nameLen" << std::endl;
            return false;
        }

        if (nameLen == 0 || nameLen > 4096) {
            std::cout << "[ModelSpawn] Error: suspicious file name length: " << nameLen << std::endl;
            return false;
        }

        std::vector<char> nameBuff(nameLen);
        if (fread(nameBuff.data(), sizeof(char), nameLen, rf) != nameLen) {
            std::cout << "[ModelSpawn] Error reading model name string!" << std::endl;
            return false;
        }
        std::string modelName(nameBuff.begin(), nameBuff.end());

        // Assign fields only after full read
        spawn.flags = flags;
        spawn.adtId = adtId;
        spawn.ID = ID;
        spawn.iPos = iPos;
        spawn.iRot = iRot;
        spawn.iScale = iScale;
        spawn.iBound = iBound;
        spawn.name = modelName;

        return true;
    }

    /**
     * @brief Writes a ModelSpawn to a file.
     *
     * @param wf The file to write to.
     * @param spawn The ModelSpawn to write.
     * @return true if the write was successful, false otherwise.
     */
    bool ModelSpawn::WriteToFile(FILE* wf, const ModelSpawn& spawn)
    {
        unsigned int check = 0;
        check += fwrite(&spawn.flags, sizeof(unsigned int), 1, wf);
        check += fwrite(&spawn.adtId, sizeof(unsigned short), 1, wf);
        check += fwrite(&spawn.ID, sizeof(unsigned int), 1, wf);
        check += fwrite(&spawn.iPos, sizeof(float), 3, wf);
        check += fwrite(&spawn.iRot, sizeof(float), 3, wf);
        check += fwrite(&spawn.iScale, sizeof(float), 1, wf);
        bool has_bound = (spawn.flags & MOD_HAS_BOUND);
        if (has_bound) // Only WMOs have bound in MPQ, only available after computation
        {
            check += fwrite(&spawn.iBound.low(), sizeof(float), 3, wf);
            check += fwrite(&spawn.iBound.high(), sizeof(float), 3, wf);
        }
        unsigned int nameLen = spawn.name.length();
        check += fwrite(&nameLen, sizeof(unsigned int), 1, wf);
        if (check != unsigned int(has_bound ? 17 : 11))
        {
            return false;
        }
        check = fwrite(spawn.name.c_str(), sizeof(char), nameLen, wf);
        if (check != nameLen)
        {
            return false;
        }
        return true;
    }
}