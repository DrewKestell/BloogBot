// WorldModel.cpp - Fixed to properly handle VMAP_7.0 format files
#include "WorldModel.h"
#include "VMapDefinitions.h"
#include "G3D/BoundsTrait.h"
#include <fstream>
#include <cstring>
#include <iostream>
#include <iomanip>
#include "VMapLog.h"

namespace VMAP
{
    class WModelRayCallBack
    {
    public:
        WModelRayCallBack(const std::vector<GroupModel>& models) : groupModels(models), hit(false) {}

        bool operator()(const G3D::Ray& ray, uint32_t entry, float& distance, bool stopAtFirstHit, bool ignoreM2Model)
        {
            if (entry >= groupModels.size())
                return false;

            bool result = groupModels[entry].IntersectRay(ray, distance, stopAtFirstHit, ignoreM2Model);
            if (result)
                hit = true;
            return result;
        }

        const std::vector<GroupModel>& groupModels;
        bool hit;
    };
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

    void GroupModel::setMeshData(std::vector<G3D::Vector3>& vert, std::vector<MeshTriangle>& triangles)
    {
        vertices = std::move(vert);
        this->triangles = std::move(triangles);

        // Calculate the overall bounds for the group
        if (!vertices.empty())
        {
            iBound = G3D::AABox(vertices[0], vertices[0]);
            for (const auto& vertex : vertices)
            {
                iBound.merge(vertex);
            }
        }

        // Build bounds for each triangle
        std::vector<G3D::AABox> bounds;
        bounds.reserve(this->triangles.size());

        for (const auto& tri : this->triangles)
        {
            // Create bounds from all three vertices of the triangle
            G3D::Vector3 lo = vertices[tri.idx0];
            G3D::Vector3 hi = lo;

            // Properly expand bounds to include all three vertices
            lo = lo.min(vertices[tri.idx1]).min(vertices[tri.idx2]);
            hi = hi.max(vertices[tri.idx1]).max(vertices[tri.idx2]);

            bounds.push_back(G3D::AABox(lo, hi));
        }
    }

    bool GroupModel::IsInsideObject(const G3D::Vector3& pos, const G3D::Vector3& down, float& z_dist) const
    {
        if (!iBound.contains(pos))
            return false;

        G3D::Ray ray(pos, down);
        float dist = z_dist;

        if (IntersectRay(ray, dist, true, false))
        {
            z_dist = dist;
            return true;
        }

        return false;
    }

    // Also add logging to the IntersectTriangle method:
    uint32_t GroupModel::IntersectRay(const G3D::Ray& ray, float& distance, bool stopAtFirstHit, bool ignoreM2Model) const
    {
        if (triangles.empty())
            return 0;  // Note: server returns false but should return 0 for uint32

        GModelRayCallback callback(triangles, vertices);
        meshTree.intersectRay(ray, callback, distance, stopAtFirstHit, ignoreM2Model);
        return callback.hit;
    }

    bool GroupModel::IntersectTriangle(const MeshTriangle& tri, std::vector<G3D::Vector3>::const_iterator vertices,
        const G3D::Ray& ray, float& distance)
    {
        const G3D::Vector3& v0 = vertices[tri.idx0];
        const G3D::Vector3& v1 = vertices[tri.idx1];
        const G3D::Vector3& v2 = vertices[tri.idx2];

        // Ray-triangle intersection (Möller-Trumbore algorithm)
        G3D::Vector3 edge1 = v1 - v0;
        G3D::Vector3 edge2 = v2 - v0;

        G3D::Vector3 h = ray.direction().cross(edge2);
        float a = edge1.dot(h);

        if (std::abs(a) < 0.00001f)
        {
            return false;
        }

        float f = 1.0f / a;
        G3D::Vector3 s = ray.origin() - v0;
        float u = f * s.dot(h);

        if (u < 0.0f || u > 1.0f)
        {
            return false;
        }

        G3D::Vector3 q = s.cross(edge1);
        float v = f * ray.direction().dot(q);

        if (v < 0.0f || u + v > 1.0f)
        {
            return false;
        }

        float t = f * edge2.dot(q);

        if (t > 0.00001f && t < distance)
        {
            distance = t;
            return true;
        }

        return false;
    }

    bool GroupModel::GetLiquidLevel(const G3D::Vector3& pos, float& liqHeight) const
    {
        if (iLiquid)
        {
            return iLiquid->GetLiquidHeight(pos, liqHeight);
        }

        return false;
    }

    uint32_t GroupModel::GetLiquidType() const
    {
        if (iLiquid)
            return iLiquid->GetType();
        return 0;
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

    bool WorldModel::IntersectRay(const G3D::Ray& ray, float& distance, bool stopAtFirstHit, bool ignoreM2Model) const
    {
        if (ignoreM2Model && (modelFlags & MOD_M2))
            return false;

        // Small M2 workaround, maybe better make separate class with virtual intersection funcs
        // In any case, there's no need to use a bound tree if we only have one submodel
        if (groupModels.size() == 1)
            return groupModels[0].IntersectRay(ray, distance, stopAtFirstHit, ignoreM2Model);

        WModelRayCallBack isc(groupModels);
        groupTree.intersectRay(ray, isc, distance, stopAtFirstHit, ignoreM2Model);
        return isc.hit;
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
            groupModels.resize(count);

            // Read each group model
            for (uint32_t i = 0; i < count && result; ++i)
                result = groupModels[i].readFromFile(rf);

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
        LOG_TRACE("[WorldModel::IntersectPoint] ENTER - Point:(" << p.x << "," << p.y << "," << p.z
            << ") Down:(" << down.x << "," << down.y << "," << down.z
            << ") MaxDist:" << dist
            << " GroupModels:" << groupModels.size());

        if (groupModels.empty())
        {
            LOG_WARN("[WorldModel::IntersectPoint] No group models available");
            return false;
        }

        G3D::Ray ray(p, down);
        LOG_DEBUG("[WorldModel::IntersectPoint] Created ray - Origin:(" << ray.origin().x << ","
            << ray.origin().y << "," << ray.origin().z << ") Dir:("
            << ray.direction().x << "," << ray.direction().y << "," << ray.direction().z << ")");

        float minDist = dist;
        bool hit = false;
        int groupsTested = 0;
        int groupsHit = 0;

        // Define callback for BIH traversal
        auto callback = [this, &ray, &minDist, &info, &hit, &p, &down, &groupsTested, &groupsHit]
        (const G3D::Ray& r, uint32_t groupIdx, float& currentDist, bool stopAtFirst, bool ignoreM2) -> bool
            {
                if (groupIdx >= groupModels.size())
                {
                    LOG_ERROR("[WorldModel::IntersectPoint] Invalid group index: " << groupIdx
                        << " (max: " << groupModels.size() << ")");
                    return false;
                }

                groupsTested++;
                LOG_DEBUG("[WorldModel::IntersectPoint] Testing group " << groupIdx
                    << " with currentDist=" << currentDist);

                float groupDist = currentDist;

                if (groupModels[groupIdx].IntersectRay(r, groupDist, stopAtFirst, ignoreM2))
                {
                    groupsHit++;
                    LOG_DEBUG("[WorldModel::IntersectPoint] Group " << groupIdx
                        << " intersects at distance " << groupDist);

                    if (groupDist < minDist)
                    {
                        minDist = groupDist;
                        info.result = true;
                        info.ground_Z = p.z + down.z * groupDist;
                        info.flags = groupModels[groupIdx].GetMogpFlags();
                        info.rootId = RootWMOID;
                        info.groupId = groupModels[groupIdx].GetWmoID();
                        hit = true;

                        LOG_INFO("[WorldModel::IntersectPoint] New closest hit - Group:" << groupIdx
                            << " Distance:" << groupDist
                            << " GroundZ:" << info.ground_Z
                            << " Flags:" << std::hex << info.flags << std::dec);

                        // Update the distance for subsequent checks
                        currentDist = groupDist;
                        return true;
                    }
                }
                else
                {
                    LOG_TRACE("[WorldModel::IntersectPoint] Group " << groupIdx << " - no intersection");
                }
                return false;
            };

        // Use BIH tree to efficiently find intersections
        LOG_DEBUG("[WorldModel::IntersectPoint] Starting BIH tree traversal");
        groupTree.intersectRay(ray, callback, dist, false, false);

        if (hit)
        {
            dist = minDist;
            LOG_INFO("[WorldModel::IntersectPoint] Final intersection - Distance:" << dist
                << " GroundZ:" << info.ground_Z
                << " Groups tested:" << groupsTested
                << " Groups hit:" << groupsHit);
        }
        else
        {
            LOG_DEBUG("[WorldModel::IntersectPoint] No intersection found after testing "
                << groupsTested << " groups");
        }

        LOG_TRACE("[WorldModel::IntersectPoint] EXIT - Hit:" << hit);
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

                return groupModels[groupIdx].IntersectRay(r, d, stopAtFirst, ignoreM2);
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
        LOG_TRACE("[WorldModel::GetLocationInfo] ENTER - RootWMOID:" << RootWMOID
            << " Point:(" << p.x << "," << p.y << "," << p.z << ")"
            << " Down:(" << down.x << "," << down.y << "," << down.z << ")"
            << " MaxDist:" << dist);

        if (groupModels.empty())
        {
            LOG_DEBUG("[WorldModel::GetLocationInfo] No group models for location info");
            return false;
        }

        G3D::Ray ray(p, down);
        LOG_DEBUG("[WorldModel::GetLocationInfo] Ray created");

        float minDist = dist;
        bool hit = false;
        int groupsTested = 0;
        int groupsHit = 0;

        // Define callback for BIH traversal
        auto callback = [this, &minDist, &info, &hit, &groupsTested, &groupsHit]
        (const G3D::Ray& r, uint32_t groupIdx, float& currentDist, bool stopAtFirst, bool ignoreM2) -> bool
            {
                if (groupIdx >= groupModels.size())
                {
                    LOG_ERROR("[WorldModel::GetLocationInfo] Invalid group index: " << groupIdx);
                    return false;
                }

                groupsTested++;
                float groupDist = currentDist;

                if (groupModels[groupIdx].IntersectRay(r, groupDist, stopAtFirst, ignoreM2))
                {
                    groupsHit++;
                    LOG_DEBUG("[WorldModel::GetLocationInfo] Group " << groupIdx
                        << " has location at distance " << groupDist);

                    if (groupDist < minDist)
                    {
                        minDist = groupDist;
                        info.hitModel = &groupModels[groupIdx];
                        info.rootId = RootWMOID;
                        hit = true;

                        LOG_INFO("[WorldModel::GetLocationInfo] New location found - Group:" << groupIdx
                            << " Distance:" << groupDist
                            << " RootId:" << info.rootId);

                        // Update the distance for subsequent checks
                        currentDist = groupDist;
                        return true;
                    }
                }
                return false;
            };

        // Use BIH tree to efficiently find intersections
        groupTree.intersectRay(ray, callback, dist, false, false);

        if (hit)
        {
            dist = minDist;
            LOG_INFO("[WorldModel::GetLocationInfo] Final location - Distance:" << dist
                << " RootId:" << info.rootId
                << " Groups tested:" << groupsTested
                << " Groups hit:" << groupsHit);
        }
        else
        {
            LOG_DEBUG("[WorldModel::GetLocationInfo] No location found after testing "
                << groupsTested << " groups");
        }

        LOG_TRACE("[WorldModel::GetLocationInfo] EXIT - Hit:" << hit);
        return hit;
    }
} // namespace VMAP