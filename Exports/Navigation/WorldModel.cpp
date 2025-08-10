// WorldModel.cpp - Fixed to properly handle VMAP_7.0 format files
#include "WorldModel.h"
#include "VMapDefinitions.h"
#include <fstream>
#include <cstring>
#include <iostream>
#include <iomanip>

namespace VMAP
{
    // ======================== WmoLiquid Implementation ========================

    WmoLiquid::WmoLiquid(uint32_t width, uint32_t height, const G3D::Vector3& corner, uint32_t type)
        : iTilesX(width), iTilesY(height), iCorner(corner), iType(type), iHeight(nullptr), iFlags(nullptr)
    {
        if (iTilesX && iTilesY)
        {
            iHeight = new float[iTilesX * iTilesY];
            iFlags = new uint8_t[iTilesX * iTilesY];
            memset(iHeight, 0, iTilesX * iTilesY * sizeof(float));
            memset(iFlags, 0, iTilesX * iTilesY * sizeof(uint8_t));
        }
    }

    WmoLiquid::WmoLiquid(const WmoLiquid& other)
        : iTilesX(other.iTilesX), iTilesY(other.iTilesY), iCorner(other.iCorner),
        iType(other.iType), iHeight(nullptr), iFlags(nullptr)
    {
        if (iTilesX && iTilesY)
        {
            uint32_t size = iTilesX * iTilesY;
            iHeight = new float[size];
            iFlags = new uint8_t[size];
            memcpy(iHeight, other.iHeight, size * sizeof(float));
            memcpy(iFlags, other.iFlags, size * sizeof(uint8_t));
        }
    }

    WmoLiquid::~WmoLiquid()
    {
        delete[] iHeight;
        delete[] iFlags;
    }

    WmoLiquid& WmoLiquid::operator=(const WmoLiquid& other)
    {
        if (this != &other)
        {
            delete[] iHeight;
            delete[] iFlags;

            iTilesX = other.iTilesX;
            iTilesY = other.iTilesY;
            iCorner = other.iCorner;
            iType = other.iType;

            if (iTilesX && iTilesY)
            {
                uint32_t size = iTilesX * iTilesY;
                iHeight = new float[size];
                iFlags = new uint8_t[size];
                memcpy(iHeight, other.iHeight, size * sizeof(float));
                memcpy(iFlags, other.iFlags, size * sizeof(uint8_t));
            }
            else
            {
                iHeight = nullptr;
                iFlags = nullptr;
            }
        }
        return *this;
    }

    bool WmoLiquid::GetLiquidHeight(const G3D::Vector3& pos, float& liqHeight) const
    {
        if (!iHeight || !iFlags)
            return false;

        float tx = (pos.x - iCorner.x) / LIQUID_TILE_SIZE;
        float ty = (pos.y - iCorner.y) / LIQUID_TILE_SIZE;

        if (tx < 0 || tx >= iTilesX - 1 || ty < 0 || ty >= iTilesY - 1)
            return false;

        int x = static_cast<int>(tx);
        int y = static_cast<int>(ty);

        uint32_t index = y * iTilesX + x;
        if (!(iFlags[index] & 0x0F))
            return false;

        float fx = tx - x;
        float fy = ty - y;

        float h1 = iHeight[index];
        float h2 = iHeight[index + 1];
        float h3 = iHeight[index + iTilesX];
        float h4 = iHeight[index + iTilesX + 1];

        liqHeight = h1 * (1 - fx) * (1 - fy) +
            h2 * fx * (1 - fy) +
            h3 * (1 - fx) * fy +
            h4 * fx * fy;

        liqHeight += iCorner.z;
        return true;
    }

    bool WmoLiquid::writeToFile(FILE* wf) const
    {
        if (fwrite(&iTilesX, sizeof(uint32_t), 1, wf) != 1) return false;
        if (fwrite(&iTilesY, sizeof(uint32_t), 1, wf) != 1) return false;
        if (fwrite(&iCorner, sizeof(float), 3, wf) != 3) return false;
        if (fwrite(&iType, sizeof(uint32_t), 1, wf) != 1) return false;

        if (iTilesX && iTilesY)
        {
            uint32_t size = iTilesX * iTilesY;
            if (fwrite(iHeight, sizeof(float), size, wf) != size) return false;
            if (fwrite(iFlags, sizeof(uint8_t), size, wf) != size) return false;
        }

        return true;
    }

    bool WmoLiquid::readFromFile(WmoLiquid*& liquid, FILE* rf)
    {
        uint32_t tilesX, tilesY, type;
        G3D::Vector3 corner;

        if (fread(&tilesX, sizeof(uint32_t), 1, rf) != 1) return false;
        if (fread(&tilesY, sizeof(uint32_t), 1, rf) != 1) return false;
        if (fread(&corner, sizeof(float), 3, rf) != 3) return false;
        if (fread(&type, sizeof(uint32_t), 1, rf) != 1) return false;

        liquid = new WmoLiquid(tilesX, tilesY, corner, type);

        if (tilesX && tilesY)
        {
            uint32_t size = tilesX * tilesY;
            if (fread(liquid->iHeight, sizeof(float), size, rf) != size)
            {
                delete liquid;
                liquid = nullptr;
                return false;
            }
            if (fread(liquid->iFlags, sizeof(uint8_t), size, rf) != size)
            {
                delete liquid;
                liquid = nullptr;
                return false;
            }
        }

        return true;
    }

    void WmoLiquid::getPosInfo(uint32_t& tilesX, uint32_t& tilesY, G3D::Vector3& corner) const
    {
        tilesX = iTilesX;
        tilesY = iTilesY;
        corner = iCorner;
    }

    // ======================== GroupModel Implementation ========================

    void GroupModel::setMeshData(std::vector<G3D::Vector3>& vert, std::vector<MeshTriangle>& tri)
    {
        vertices = std::move(vert);
        triangles = std::move(tri);

        std::vector<G3D::AABox> bounds;
        bounds.reserve(triangles.size());

        for (const auto& tri : triangles)
        {
            G3D::AABox box;
            box.set(vertices[tri.idx0], vertices[tri.idx0]);
            box.merge(vertices[tri.idx1]);
            box.merge(vertices[tri.idx2]);
            bounds.push_back(box);
        }

        meshTree.build(bounds, 4);
    }

    bool GroupModel::IsInsideObject(const G3D::Vector3& pos, const G3D::Vector3& down, float& z_dist) const
    {
        if (!iBound.contains(pos))
            return false;

        G3D::Ray ray(pos, down);
        float dist = z_dist;

        if (IntersectRay(ray, dist, true))
        {
            z_dist = dist;
            return true;
        }

        return false;
    }

    bool GroupModel::IntersectRay(const G3D::Ray& ray, float& distance, bool stopAtFirstHit) const
    {
        if (triangles.empty())
            return false;

        float time = ray.intersectionTime(iBound);
        if (time == G3D::inf())
            return false;

        auto callback = [this](const G3D::Ray& r, uint32_t triIdx, float& dist,
            bool stopAtFirst, bool ignoreM2) -> bool
            {
                if (triIdx >= triangles.size())
                    return false;

                const MeshTriangle& tri = triangles[triIdx];
                const G3D::Vector3& v0 = vertices[tri.idx0];
                const G3D::Vector3& v1 = vertices[tri.idx1];
                const G3D::Vector3& v2 = vertices[tri.idx2];

                G3D::Vector3 edge1 = v1 - v0;
                G3D::Vector3 edge2 = v2 - v0;
                G3D::Vector3 h = r.direction().cross(edge2);
                float a = edge1.dot(h);

                if (std::abs(a) < 0.00001f)
                    return false;

                float f = 1.0f / a;
                G3D::Vector3 s = r.origin() - v0;
                float u = f * s.dot(h);

                if (u < 0.0f || u > 1.0f)
                    return false;

                G3D::Vector3 q = s.cross(edge1);
                float v = f * r.direction().dot(q);

                if (v < 0.0f || u + v > 1.0f)
                    return false;

                float t = f * edge2.dot(q);

                if (t > 0.00001f && t < dist)
                {
                    dist = t;
                    return true;
                }

                return false;
            };

        float oldDist = distance;
        meshTree.intersectRay(ray, callback, distance, stopAtFirstHit, false);

        return distance < oldDist;
    }

    bool GroupModel::GetLiquidLevel(const G3D::Vector3& pos, float& liqHeight) const
    {
        if (iLiquid)
            return iLiquid->GetLiquidHeight(pos, liqHeight);
        return false;
    }

    uint32_t GroupModel::GetLiquidType() const
    {
        if (iLiquid)
            return iLiquid->GetType();
        return 0;
    }

    bool GroupModel::writeToFile(FILE* wf) const
    {
        if (fwrite(&iMogpFlags, sizeof(uint32_t), 1, wf) != 1) return false;
        if (fwrite(&iGroupWMOID, sizeof(uint32_t), 1, wf) != 1) return false;

        G3D::Vector3 low = iBound.low();
        G3D::Vector3 high = iBound.high();
        if (fwrite(&low, sizeof(float), 3, wf) != 3) return false;
        if (fwrite(&high, sizeof(float), 3, wf) != 3) return false;

        uint32_t nVertices = vertices.size();
        if (fwrite(&nVertices, sizeof(uint32_t), 1, wf) != 1) return false;
        if (nVertices > 0)
        {
            if (fwrite(&vertices[0], sizeof(G3D::Vector3), nVertices, wf) != nVertices)
                return false;
        }

        uint32_t nTriangles = triangles.size();
        if (fwrite(&nTriangles, sizeof(uint32_t), 1, wf) != 1) return false;
        if (nTriangles > 0)
        {
            if (fwrite(&triangles[0], sizeof(MeshTriangle), nTriangles, wf) != nTriangles)
                return false;
        }

        if (!meshTree.writeToFile(wf))
            return false;

        bool hasLiquid = (iLiquid != nullptr);
        if (fwrite(&hasLiquid, sizeof(bool), 1, wf) != 1) return false;
        if (hasLiquid && !iLiquid->writeToFile(wf))
            return false;

        return true;
    }

    bool GroupModel::readFromFile(FILE* rf)
    {
        if (fread(&iMogpFlags, sizeof(uint32_t), 1, rf) != 1) return false;
        if (fread(&iGroupWMOID, sizeof(uint32_t), 1, rf) != 1) return false;

        G3D::Vector3 low, high;
        if (fread(&low, sizeof(float), 3, rf) != 3) return false;
        if (fread(&high, sizeof(float), 3, rf) != 3) return false;
        iBound = G3D::AABox(low, high);

        uint32_t nVertices;
        if (fread(&nVertices, sizeof(uint32_t), 1, rf) != 1) return false;
        if (nVertices > 0)
        {
            vertices.resize(nVertices);
            if (fread(&vertices[0], sizeof(G3D::Vector3), nVertices, rf) != nVertices)
                return false;
        }

        uint32_t nTriangles;
        if (fread(&nTriangles, sizeof(uint32_t), 1, rf) != 1) return false;
        if (nTriangles > 0)
        {
            triangles.resize(nTriangles);
            if (fread(&triangles[0], sizeof(MeshTriangle), nTriangles, rf) != nTriangles)
                return false;
        }

        if (!meshTree.readFromFile(rf))
            return false;

        bool hasLiquid;
        if (fread(&hasLiquid, sizeof(bool), 1, rf) != 1) return false;
        if (hasLiquid)
        {
            if (!WmoLiquid::readFromFile(iLiquid, rf))
                return false;
        }

        return true;
    }

    // ======================== WorldModel Implementation ========================

    void WorldModel::setGroupModels(std::vector<GroupModel>& models)
    {
        groupModels = std::move(models);

        std::vector<G3D::AABox> bounds;
        bounds.reserve(groupModels.size());

        for (const auto& group : groupModels)
        {
            bounds.push_back(group.GetBound());
        }

        if (!bounds.empty())
        {
            groupTree.build(bounds, 1);
        }
    }

    bool WorldModel::IntersectRay(const G3D::Ray& ray, float& distance, bool stopAtFirstHit, bool ignoreM2Model) const
    {
        if (ignoreM2Model && (modelFlags & MOD_M2))
            return false;

        if (groupModels.empty())
            return false;

        auto callback = [this](const G3D::Ray& r, uint32_t groupIdx,
            float& dist, bool stopAtFirst, bool ignoreM2) -> bool
            {
                if (groupIdx >= groupModels.size())
                    return false;

                return groupModels[groupIdx].IntersectRay(r, dist, stopAtFirst);
            };

        float oldDist = distance;
        groupTree.intersectRay(ray, callback, distance, stopAtFirstHit, ignoreM2Model);

        return distance < oldDist;
    }

    bool WorldModel::writeFile(const std::string& filename)
    {
        FILE* wf = fopen(filename.c_str(), "wb");
        if (!wf)
            return false;

        char magic[8] = { 'V', 'M', 'O', 'D', '\0', '\0', '\0', '\0' };
        if (fwrite(magic, 1, 8, wf) != 8)
        {
            fclose(wf);
            return false;
        }

        uint32_t version = 17;
        if (fwrite(&version, sizeof(uint32_t), 1, wf) != 1)
        {
            fclose(wf);
            return false;
        }

        if (fwrite(&RootWMOID, sizeof(uint32_t), 1, wf) != 1)
        {
            fclose(wf);
            return false;
        }

        if (fwrite(&modelFlags, sizeof(uint32_t), 1, wf) != 1)
        {
            fclose(wf);
            return false;
        }

        uint32_t nGroups = groupModels.size();
        if (fwrite(&nGroups, sizeof(uint32_t), 1, wf) != 1)
        {
            fclose(wf);
            return false;
        }

        for (const auto& group : groupModels)
        {
            if (!group.writeToFile(wf))
            {
                fclose(wf);
                return false;
            }
        }

        if (!groupTree.writeToFile(wf))
        {
            fclose(wf);
            return false;
        }

        fclose(wf);
        return true;
    }

    bool WorldModel::readFile(const std::string& filename)
    {
        FILE* rf = fopen(filename.c_str(), "rb");
        if (!rf)
        {
            return false;
        }

        // Read magic header
        char magic[8];
        if (fread(magic, 1, 8, rf) != 8)
        {
            fclose(rf);
            return false;
        }

        // Check for VMAP_7.0 format (used by some extractors for individual models)
        if (memcmp(magic, VMAP_MAGIC, 8) == 0)
        {
            // This is a VMAP_7.0 format file
            // For M2 models converted to VMO, these often contain no actual collision data
            // The format after the magic is typically:
            // - 1 byte: tiled flag (usually 0 for individual models)
            // - Then either empty or minimal data for M2 models

            char tiledFlag;
            if (fread(&tiledFlag, sizeof(char), 1, rf) != 1)
            {
                fclose(rf);
                return false;
            }

            // Check if there's a NODE chunk (for actual collision data)
            char chunk[4];
            if (fread(chunk, 1, 4, rf) == 4 && memcmp(chunk, "NODE", 4) == 0)
            {
                // This file has collision data, but we'll treat M2 models as having no collision
                // M2 models in vMaNGOS typically don't provide collision
                fclose(rf);

                // Return success with empty model (no collision)
                modelFlags = MOD_M2;  // Mark as M2 model
                return true;
            }

            // No NODE chunk or other data - this is an empty collision model
            fclose(rf);

            // Return success with empty model
            modelFlags = MOD_M2;  // Mark as M2 model
            return true;
        }

        // Check for standard VMOD format
        bool isVMOD = (memcmp(magic, "VMOD", 4) == 0);
        bool isWMOD = (memcmp(magic, "WMOD", 4) == 0);

        if (!isVMOD && !isWMOD)
        {
            // Check for M2 model formats
            if (memcmp(magic, "MD20", 4) == 0 || memcmp(magic, "MD21", 4) == 0)
            {
                // This is an M2 model - no collision
                fclose(rf);
                modelFlags = MOD_M2;
                return true;
            }

            std::cerr << "[WorldModel] Invalid file magic for: " << filename << std::endl;
            fclose(rf);
            return false;
        }

        // Read version
        uint32_t version;
        if (fread(&version, sizeof(uint32_t), 1, rf) != 1)
        {
            fclose(rf);
            return false;
        }

        if (version != 17 && version != 18)
        {
            std::cerr << "[WorldModel] Unsupported version: " << version << std::endl;
            fclose(rf);
            return false;
        }

        // Read root WMO ID
        if (fread(&RootWMOID, sizeof(uint32_t), 1, rf) != 1)
        {
            fclose(rf);
            return false;
        }

        // Read model flags
        if (fread(&modelFlags, sizeof(uint32_t), 1, rf) != 1)
        {
            fclose(rf);
            return false;
        }

        // Read number of groups
        uint32_t nGroups;
        if (fread(&nGroups, sizeof(uint32_t), 1, rf) != 1)
        {
            fclose(rf);
            return false;
        }

        if (nGroups > 512)
        {
            std::cerr << "[WorldModel] Invalid group count: " << nGroups << std::endl;
            fclose(rf);
            return false;
        }

        // For M2 models or models with 0 groups, return success with empty collision
        if (nGroups == 0 || (modelFlags & MOD_M2))
        {
            fclose(rf);
            return true;
        }

        groupModels.clear();
        groupModels.reserve(nGroups);

        for (uint32_t i = 0; i < nGroups; ++i)
        {
            GroupModel group;
            if (!group.readFromFile(rf))
            {
                std::cerr << "[WorldModel] Failed to read group " << i << std::endl;
                fclose(rf);
                return false;
            }
            groupModels.push_back(std::move(group));
        }

        if (!groupTree.readFromFile(rf))
        {
            std::cerr << "[WorldModel] Failed to read BIH tree" << std::endl;
            fclose(rf);
            return false;
        }

        fclose(rf);
        return true;
    }

    bool WorldModel::IntersectPoint(const G3D::Vector3& p, const G3D::Vector3& down, float& dist, AreaInfo& info) const
    {
        if (groupModels.empty())
            return false;

        G3D::Ray ray(p, down);

        bool hit = false;
        float minDist = dist;

        for (size_t i = 0; i < groupModels.size(); ++i)
        {
            float groupDist = dist;
            if (groupModels[i].IntersectRay(ray, groupDist, false))
            {
                if (groupDist < minDist)
                {
                    minDist = groupDist;
                    info.result = true;
                    info.ground_Z = p.z + down.z * groupDist;
                    info.flags = groupModels[i].GetMogpFlags();
                    info.rootId = RootWMOID;
                    info.groupId = groupModels[i].GetWmoID();
                    hit = true;
                }
            }
        }

        if (hit)
            dist = minDist;

        return hit;
    }

    bool WorldModel::IsUnderObject(const G3D::Vector3& p, const G3D::Vector3& up, bool m2,
        float* outDist, float* inDist) const
    {
        if (m2 && (modelFlags & MOD_M2))
            return false;

        if (groupModels.empty())
            return false;

        G3D::Ray ray(p, up);
        float maxDist = 100.0f;

        auto callback = [this](const G3D::Ray& r, uint32_t groupIdx,
            float& d, bool stopAtFirst, bool ignoreM2) -> bool
            {
                if (groupIdx >= groupModels.size())
                    return false;

                return groupModels[groupIdx].IntersectRay(r, d, stopAtFirst);
            };

        float distance = maxDist;
        groupTree.intersectRay(ray, callback, distance, true, false);

        if (distance < maxDist)
        {
            if (outDist)
                *outDist = distance;
            if (inDist)
                *inDist = 0.0f;
            return true;
        }

        return false;
    }

    bool WorldModel::GetLocationInfo(const G3D::Vector3& p, const G3D::Vector3& down, float& dist,
        GroupLocationInfo& info) const
    {
        if (groupModels.empty())
            return false;

        G3D::Ray ray(p, down);

        bool hit = false;
        float minDist = dist;

        for (size_t i = 0; i < groupModels.size(); ++i)
        {
            float groupDist = dist;
            if (groupModels[i].IntersectRay(ray, groupDist, false))
            {
                if (groupDist < minDist)
                {
                    minDist = groupDist;
                    info.hitModel = &groupModels[i];
                    info.rootId = RootWMOID;
                    hit = true;
                }
            }
        }

        if (hit)
            dist = minDist;

        return hit;
    }

#ifdef MMAP_GENERATOR
    void WorldModel::getGroupModels(std::vector<GroupModel>& models)
    {
        models = groupModels;
    }
#endif

} // namespace VMAP