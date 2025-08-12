// WorldModel.cpp - Fixed to properly handle VMAP_7.0 format files
#include "WorldModel.h"
#include "VMapDefinitions.h"
#include <fstream>
#include <cstring>
#include <iostream>
#include <iomanip>
#include "VMapLog.h"

namespace VMAP
{
    // ======================== WmoLiquid Implementation ========================

    WmoLiquid::WmoLiquid(uint32_t width, uint32_t height, const G3D::Vector3& corner, uint32_t type)
        : iTilesX(width), iTilesY(height), iCorner(corner), iType(type)
    {
        // Heights array needs an extra row and column for the corners
        iHeight = new float[(width + 1) * (height + 1)];
        iFlags = new uint8_t[width * height];

        // Initialize to zero (optional, server doesn't do this)
        memset(iHeight, 0, (width + 1) * (height + 1) * sizeof(float));
        memset(iFlags, 0, width * height * sizeof(uint8_t));
    }

    WmoLiquid::WmoLiquid(const WmoLiquid& other)
        : iTilesX(other.iTilesX), iTilesY(other.iTilesY), iCorner(other.iCorner),
        iType(other.iType), iHeight(nullptr), iFlags(nullptr)
    {
        if (iTilesX && iTilesY)
        {
            // FIXED: Correct sizes!
            uint32_t heightSize = (iTilesX + 1) * (iTilesY + 1);
            uint32_t flagSize = iTilesX * iTilesY;

            iHeight = new float[heightSize];
            iFlags = new uint8_t[flagSize];

            memcpy(iHeight, other.iHeight, heightSize * sizeof(float));
            memcpy(iFlags, other.iFlags, flagSize * sizeof(uint8_t));
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
                // FIXED: Correct sizes!
                uint32_t heightSize = (iTilesX + 1) * (iTilesY + 1);
                uint32_t flagSize = iTilesX * iTilesY;

                iHeight = new float[heightSize];
                iFlags = new uint8_t[flagSize];

                memcpy(iHeight, other.iHeight, heightSize * sizeof(float));
                memcpy(iFlags, other.iFlags, flagSize * sizeof(uint8_t));
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

        float tx_f = (pos.x - iCorner.x) / LIQUID_TILE_SIZE;
        uint32_t tx = uint32_t(tx_f);
        if (tx_f < 0.0f || tx >= iTilesX)
            return false;

        float ty_f = (pos.y - iCorner.y) / LIQUID_TILE_SIZE;
        uint32_t ty = uint32_t(ty_f);
        if (ty_f < 0.0f || ty >= iTilesY)
            return false;

        // Check if tile is valid for liquid (0x0F = disabled)
        if ((iFlags[tx + ty * iTilesX] & 0x0F) == 0x0F)
            return false;

        float dx = tx_f - float(tx);
        float dy = ty_f - float(ty);

        uint32_t const rowOffset = iTilesX + 1;  // Height array is (width+1) wide

        if (dx > dy)  // Triangle (a)
        {
            float sx = iHeight[tx + 1 + ty * rowOffset] - iHeight[tx + ty * rowOffset];
            float sy = iHeight[tx + 1 + (ty + 1) * rowOffset] - iHeight[tx + 1 + ty * rowOffset];
            liqHeight = iHeight[tx + ty * rowOffset] + dx * sx + dy * sy;
        }
        else  // Triangle (b)
        {
            float sx = iHeight[tx + 1 + (ty + 1) * rowOffset] - iHeight[tx + (ty + 1) * rowOffset];
            float sy = iHeight[tx + (ty + 1) * rowOffset] - iHeight[tx + ty * rowOffset];
            liqHeight = iHeight[tx + ty * rowOffset] + dx * sx + dy * sy;
        }

        // Don't add iCorner.z - height values are already absolute
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

    bool WmoLiquid::readFromFile(FILE* rf, WmoLiquid*& out)
    {
        bool result = true;
        WmoLiquid* liquid = new WmoLiquid();
        if (result && fread(&liquid->iTilesX, sizeof(uint32_t), 1, rf) != 1) result = false;
        if (result && fread(&liquid->iTilesY, sizeof(uint32_t), 1, rf) != 1) result = false;
        if (result && fread(&liquid->iCorner, sizeof(G3D::Vector3), 1, rf) != 1) result = false;
        if (result && fread(&liquid->iType, sizeof(uint32_t), 1, rf) != 1) result = false;
        uint32_t size = (liquid->iTilesX + 1) * (liquid->iTilesY + 1);
        liquid->iHeight = new float[size];
        if (result && fread(liquid->iHeight, sizeof(float), size, rf) != size) result = false;
        size = liquid->iTilesX * liquid->iTilesY;
        liquid->iFlags = new uint8_t[size];
        if (result && fread(liquid->iFlags, sizeof(uint8_t), size, rf) != size) result = false;
        if (!result)
        {
            delete liquid;
            liquid = nullptr;
        }
        out = liquid;
        return result;
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
        LOG_TRACE("GroupModel::IntersectRay ENTER"
            << " GroupWMOID:" << iGroupWMOID
            << " Distance:" << distance
            << " StopAtFirst:" << stopAtFirstHit);

        if (triangles.empty())
        {
            LOG_DEBUG("No triangles in group model");
            return false;
        }

        // Check bounding box first
        float time = ray.intersectionTime(iBound);
        if (time == G3D::inf())
        {
            LOG_DEBUG("Ray misses group bounds");
            LOG_BOUNDS("Group bounds", iBound);
            return false;
        }

        LOG_DEBUG("Ray hits bounds at time " << time);
        LOG_DEBUG("Group has " << triangles.size() << " triangles, " << vertices.size() << " vertices");

        int trianglesTested = 0;
        int trianglesHit = 0;

        auto callback = [this, &trianglesTested, &trianglesHit](const G3D::Ray& r, uint32_t triIdx, float& dist,
            bool stopAtFirst, bool ignoreM2) -> bool
            {
                if (triIdx >= triangles.size())
                {
                    LOG_ERROR("Invalid triangle index: " << triIdx << " (max:" << triangles.size() << ")");
                    return false;
                }

                trianglesTested++;

                const MeshTriangle& tri = triangles[triIdx];
                const G3D::Vector3& v0 = vertices[tri.idx0];
                const G3D::Vector3& v1 = vertices[tri.idx1];
                const G3D::Vector3& v2 = vertices[tri.idx2];

                // Ray-triangle intersection (Möller-Trumbore algorithm)
                G3D::Vector3 edge1 = v1 - v0;
                G3D::Vector3 edge2 = v2 - v0;
                G3D::Vector3 h = r.direction().cross(edge2);
                float a = edge1.dot(h);

                if (std::abs(a) < 0.00001f)
                {
                    return false; // Ray parallel to triangle
                }

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
                    trianglesHit++;
                    dist = t;

                    LOG_DEBUG("Triangle " << triIdx << " HIT at distance " << t
                        << " Verts: (" << tri.idx0 << "," << tri.idx1 << "," << tri.idx2 << ")");

                    return true;
                }

                return false;
            };

        float oldDist = distance;

        LOG_DEBUG("Starting mesh BIH traversal");

        meshTree.intersectRay(ray, callback, distance, stopAtFirstHit, false);

        bool hasHit = distance < oldDist;

        LOG_INFO("GroupModel ray test - TrianglesTested:" << trianglesTested
            << " TrianglesHit:" << trianglesHit
            << " InitialDist:" << oldDist
            << " FinalDist:" << distance
            << " HasHit:" << hasHit);

        LOG_TRACE("GroupModel::IntersectRay EXIT - Hit:" << hasHit);
        return hasHit;
    }


    bool GroupModel::GetLiquidLevel(const G3D::Vector3& pos, float& liqHeight) const
    {
        LOG_TRACE("GroupModel::GetLiquidLevel ENTER"
            << " GroupWMOID:" << iGroupWMOID
            << " Pos:(" << pos.x << "," << pos.y << "," << pos.z << ")");

        if (iLiquid)
        {
            bool result = iLiquid->GetLiquidHeight(pos, liqHeight);

            if (result)
            {
                LOG_INFO("Liquid found at height " << liqHeight);
            }
            else
            {
                LOG_DEBUG("No liquid at position");
            }

            return result;
        }

        LOG_DEBUG("No liquid data in group");
        LOG_TRACE("GroupModel::GetLiquidLevel EXIT - No liquid");
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
        char chunk[8];
        bool result = true;
        uint32_t chunkSize = 0;
        uint32_t count = 0;

        // Clear existing data
        triangles.clear();
        vertices.clear();
        delete iLiquid;
        iLiquid = nullptr;

        // Read bounding box (stored as complete AABox, not two vectors)
        if (fread(&iBound, sizeof(G3D::AABox), 1, rf) != 1)
        {
            std::cerr << "[GroupModel] Failed to read bounding box" << std::endl;
            return false;
        }

        // Read MOGP flags
        if (fread(&iMogpFlags, sizeof(uint32_t), 1, rf) != 1)
        {
            std::cerr << "[GroupModel] Failed to read MOGP flags" << std::endl;
            return false;
        }

        // Read Group WMO ID
        if (fread(&iGroupWMOID, sizeof(uint32_t), 1, rf) != 1)
        {
            std::cerr << "[GroupModel] Failed to read Group WMO ID" << std::endl;
            return false;
        }

        // Read VERT chunk (vertices)
        if (!readChunk(rf, chunk, "VERT", 4))
        {
            std::cerr << "[GroupModel] Missing VERT chunk" << std::endl;
            return false;
        }

        if (fread(&chunkSize, sizeof(uint32_t), 1, rf) != 1)
            return false;

        if (fread(&count, sizeof(uint32_t), 1, rf) != 1)
            return false;

        if (!count) // Models without collision geometry end here
            return true;

        vertices.resize(count);
        if (fread(&vertices[0], sizeof(G3D::Vector3), count, rf) != count)
        {
            std::cerr << "[GroupModel] Failed to read vertices" << std::endl;
            return false;
        }

        // Read TRIM chunk (triangles)
        if (!readChunk(rf, chunk, "TRIM", 4))
        {
            std::cerr << "[GroupModel] Missing TRIM chunk" << std::endl;
            return false;
        }

        if (fread(&chunkSize, sizeof(uint32_t), 1, rf) != 1)
            return false;

        if (fread(&count, sizeof(uint32_t), 1, rf) != 1)
            return false;

        if (count)
        {
            triangles.resize(count);
            if (fread(&triangles[0], sizeof(MeshTriangle), count, rf) != count)
            {
                std::cerr << "[GroupModel] Failed to read triangles" << std::endl;
                return false;
            }
        }

        // Read MBIH chunk (mesh BIH tree)
        if (!readChunk(rf, chunk, "MBIH", 4))
        {
            std::cerr << "[GroupModel] Missing MBIH chunk" << std::endl;
            return false;
        }

        if (!meshTree.readFromFile(rf))
        {
            std::cerr << "[GroupModel] Failed to read mesh BIH tree" << std::endl;
            return false;
        }

        // Read LIQU chunk (liquid data)
        if (!readChunk(rf, chunk, "LIQU", 4))
        {
            std::cerr << "[GroupModel] Missing LIQU chunk" << std::endl;
            return false;
        }

        if (fread(&chunkSize, sizeof(uint32_t), 1, rf) != 1)
            return false;

        if (chunkSize > 0)
        {
            // Note: WmoLiquid::readFromFile signature might be different
            // You may need to adjust this based on your implementation
            if (!WmoLiquid::readFromFile(rf, iLiquid))
            {
                std::cerr << "[GroupModel] Failed to read liquid data" << std::endl;
                return false;
            }
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
        LOG_TRACE("WorldModel::IntersectRay ENTER"
            << " RootWMOID:" << RootWMOID
            << " Distance:" << distance
            << " StopAtFirst:" << stopAtFirstHit
            << " IgnoreM2:" << ignoreM2Model);

        LOG_RAY("Input ray", ray);
        LOG_DEBUG("Model flags: 0x" << std::hex << modelFlags << std::dec
            << " IsM2:" << ((modelFlags & MOD_M2) ? "YES" : "NO"));

        if (ignoreM2Model && (modelFlags & MOD_M2))
        {
            LOG_DEBUG("Ignoring M2 model as requested");
            return false;
        }

        if (groupModels.empty())
        {
            LOG_DEBUG("No group models to test");
            return false;
        }

        LOG_DEBUG("Testing " << groupModels.size() << " group models");

        int groupsTested = 0;
        int groupsHit = 0;

        auto callback = [this, &groupsTested, &groupsHit](const G3D::Ray& r, uint32_t groupIdx,
            float& dist, bool stopAtFirst, bool ignoreM2) -> bool
            {
                if (groupIdx >= groupModels.size())
                {
                    LOG_ERROR("Invalid group index: " << groupIdx << " (max:" << groupModels.size() << ")");
                    return false;
                }

                groupsTested++;
                float oldDist = dist;
                bool hit = groupModels[groupIdx].IntersectRay(r, dist, stopAtFirst);

                if (hit)
                {
                    groupsHit++;
                    LOG_DEBUG("Group " << groupIdx << " HIT at distance " << dist
                        << " (was " << oldDist << ")");
                }

                return hit;
            };

        float oldDist = distance;

        LOG_DEBUG("Starting BIH tree traversal for groups");

        groupTree.intersectRay(ray, callback, distance, stopAtFirstHit, ignoreM2Model);

        bool hasHit = distance < oldDist;

        LOG_INFO("WorldModel ray test - GroupsTested:" << groupsTested
            << " GroupsHit:" << groupsHit
            << " InitialDist:" << oldDist
            << " FinalDist:" << distance
            << " HasHit:" << hasHit);

        LOG_TRACE("WorldModel::IntersectRay EXIT - Hit:" << hasHit);
        return hasHit;
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
            return false;

        bool result = true;
        uint32_t chunkSize = 0;
        uint32_t count = 0;
        char chunk[8];

        // Read VMAP magic header (8 bytes)
        if (!readChunk(rf, chunk, VMAP_MAGIC, 8))
        {
            fclose(rf);
            return false;
        }

        // Read WMOD chunk identifier
        if (!readChunk(rf, chunk, "WMOD", 4))
        {
            fclose(rf);
            return false;
        }

        // Read chunk size
        if (fread(&chunkSize, sizeof(uint32_t), 1, rf) != 1)
        {
            fclose(rf);
            return false;
        }

        // Read RootWMOID
        if (fread(&RootWMOID, sizeof(uint32_t), 1, rf) != 1)
        {
            fclose(rf);
            return false;
        }

        // Read GMOD chunk (group models)
        if (readChunk(rf, chunk, "GMOD", 4))
        {
            // Read count of group models
            if (fread(&count, sizeof(uint32_t), 1, rf) != 1)
            {
                fclose(rf);
                return false;
            }

            groupModels.clear();
            groupModels.reserve(count);

            // Read each group model
            for (uint32_t i = 0; i < count && result; ++i)
            {
                GroupModel group;
                if (!group.readFromFile(rf))
                {
                    std::cerr << "[WorldModel] Failed to read group " << i << std::endl;
                    result = false;
                    break;
                }
                groupModels.push_back(std::move(group));
            }

            // Read GBIH chunk (BIH tree)
            if (result && !readChunk(rf, chunk, "GBIH", 4))
            {
                std::cerr << "[WorldModel] Missing GBIH chunk" << std::endl;
                result = false;
            }

            if (result)
            {
                result = groupTree.readFromFile(rf);
                if (!result)
                {
                    std::cerr << "[WorldModel] Failed to read BIH tree" << std::endl;
                }
            }
        }
        else
        {
            std::cerr << "[WorldModel] Missing GMOD chunk" << std::endl;
            result = false;
        }

        fclose(rf);

        return result;
    }

    bool WorldModel::IntersectPoint(const G3D::Vector3& p, const G3D::Vector3& down, float& dist, AreaInfo& info) const
    {
        LOG_TRACE("WorldModel::IntersectPoint ENTER"
            << " RootWMOID:" << RootWMOID
            << " Point:(" << p.x << "," << p.y << "," << p.z << ")"
            << " Down:(" << down.x << "," << down.y << "," << down.z << ")"
            << " MaxDist:" << dist);

        if (groupModels.empty())
        {
            LOG_DEBUG("No group models for intersection");
            return false;
        }

        G3D::Ray ray(p, down);
        LOG_RAY("Intersection ray", ray);

        bool hit = false;
        float minDist = dist;
        int groupsTested = 0;
        int groupsHit = 0;

        for (size_t i = 0; i < groupModels.size(); ++i)
        {
            groupsTested++;
            float groupDist = dist;

            if (groupModels[i].IntersectRay(ray, groupDist, false))
            {
                groupsHit++;
                LOG_DEBUG("Group " << i << " intersects at distance " << groupDist);

                if (groupDist < minDist)
                {
                    minDist = groupDist;
                    info.result = true;
                    info.ground_Z = p.z + down.z * groupDist;
                    info.flags = groupModels[i].GetMogpFlags();
                    info.rootId = RootWMOID;
                    info.groupId = groupModels[i].GetWmoID();
                    hit = true;

                    LOG_INFO("New closest hit - Group:" << i
                        << " Distance:" << groupDist
                        << " GroundZ:" << info.ground_Z
                        << " Flags:" << std::hex << info.flags << std::dec);
                }
            }
        }

        if (hit)
        {
            dist = minDist;
            LOG_INFO("Final intersection - Distance:" << dist
                << " GroundZ:" << info.ground_Z
                << " Groups tested:" << groupsTested
                << " Groups hit:" << groupsHit);
        }
        else
        {
            LOG_DEBUG("No intersection found after testing " << groupsTested << " groups");
        }

        LOG_TRACE("WorldModel::IntersectPoint EXIT - Hit:" << hit);
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
        LOG_TRACE("WorldModel::GetLocationInfo ENTER"
            << " RootWMOID:" << RootWMOID
            << " Point:(" << p.x << "," << p.y << "," << p.z << ")"
            << " MaxDist:" << dist);

        if (groupModels.empty())
        {
            LOG_DEBUG("No group models for location info");
            return false;
        }

        G3D::Ray ray(p, down);
        LOG_RAY("Location ray", ray);

        bool hit = false;
        float minDist = dist;
        int groupsTested = 0;
        int groupsHit = 0;

        for (size_t i = 0; i < groupModels.size(); ++i)
        {
            groupsTested++;
            float groupDist = dist;

            if (groupModels[i].IntersectRay(ray, groupDist, false))
            {
                groupsHit++;
                LOG_DEBUG("Group " << i << " has location at distance " << groupDist);

                if (groupDist < minDist)
                {
                    minDist = groupDist;
                    info.hitModel = &groupModels[i];
                    info.rootId = RootWMOID;
                    hit = true;

                    LOG_INFO("New location found - Group:" << i
                        << " Distance:" << groupDist
                        << " RootId:" << info.rootId);
                }
            }
        }

        if (hit)
        {
            dist = minDist;
            LOG_INFO("Final location - Distance:" << dist
                << " RootId:" << info.rootId
                << " Groups tested:" << groupsTested
                << " Groups hit:" << groupsHit);
        }
        else
        {
            LOG_DEBUG("No location found after testing " << groupsTested << " groups");
        }

        LOG_TRACE("WorldModel::GetLocationInfo EXIT - Hit:" << hit);
        return hit;
    }

#ifdef MMAP_GENERATOR
    void WorldModel::getGroupModels(std::vector<GroupModel>& models)
    {
        models = groupModels;
    }
#endif

} // namespace VMAP