#include "WorldModel.h"
#include <cmath>
#include <limits>
#include <algorithm>
#include <iostream>
#include <memory>
#include <iomanip>
#include <sstream>
#include <vector>
#include "PhysicsQuery.h"
#include "TileAssembler.h"
#include "VMapDefinitions.h"
#include "MapTree.h"
#include "GroupModel.h"
#include "WmoLiquid.h"

template<> struct BoundsTrait<VMAP::GroupModel>
{
    static void getBounds(VMAP::GroupModel const& obj, AABox& out)
    {
        out = obj.GetBound();
    }
};

namespace VMAP {
    bool IntersectTriangle(MeshTriangle const& tri, std::vector<Vec3>::const_iterator points, Ray const& ray, float& distance)
    {
        static float const EPS = 1e-5f;

        // See RTR2 ch. 13.7 for the algorithm.

        Vec3 const e1 = points[tri.idx1] - points[tri.idx0];
        Vec3 const e2 = points[tri.idx2] - points[tri.idx0];
        Vec3 const p(ray.direction().cross(e2));
        float const a = e1.dot(p);

        if (fabs(a) < EPS)
        {
            // Determinant is ill-conditioned; abort early
            return false;
        }

        float const f = 1.0f / a;
        Vec3 const s(ray.origin() - points[tri.idx0]);
        float const u = f * s.dot(p);

        if ((u < 0.0f) || (u > 1.0f))
        {
            // We hit the plane of the m_geometry, but outside the m_geometry
            return false;
        }

        Vec3 const q(s.cross(e1));
        float const v = f * ray.direction().dot(q);

        if ((v < 0.0f) || ((u + v) > 1.0f))
        {
            // We hit the plane of the triangle, but outside the triangle
            return false;
        }

        float const t = f * e2.dot(q);

        if ((t > 0.0f) && (t < distance))
        {
            // This is a new hit, closer than the previous one
            distance = t;

            /* baryCoord[0] = 1.0 - u - v;
            baryCoord[1] = u;
            baryCoord[2] = v; */

            return true;
        }
        // This hit is after the previous hit, so ignore it
        return false;
    }
    // --- Model implementation ---

    WorldModel::WorldModel() : RootWMOID(0), modelFlags(0) {}

    WorldModel::WorldModel(const std::vector<Vec3>& verts, const std::vector<MeshTriangle>& tris)
        : vertices(verts), triangles(tris) {
        computeBounds();
    }

    void WorldModel::computeBounds() {
        if (vertices.empty()) {
            bound = AABox(Vec3(0, 0, 0), Vec3(0, 0, 0));
            return;
        }
        Vec3 vmin = vertices[0], vmax = vertices[0];
        for (const auto& v : vertices) {
            vmin = vmin.min(v);
            vmax = vmax.max(v);
        }
        bound = AABox(vmin, vmax);
    }

    bool WorldModel::RayIntersectsTriangle(const Vec3& orig, const Vec3& dir,
        const Vec3& v0, const Vec3& v1, const Vec3& v2, float& t)
    {
        const float EPSILON = 1e-6f;
        Vec3 edge1 = v1 - v0;
        Vec3 edge2 = v2 - v0;
        Vec3 h = dir.cross(edge2);
        float a = edge1.dot(h);
        if (std::fabs(a) < EPSILON)
            return false;
        float f = 1.0f / a;
        Vec3 s = orig - v0;
        float u = f * s.dot(h);
        if (u < 0.0f || u > 1.0f)
            return false;
        Vec3 q = s.cross(edge1);
        float v = f * dir.dot(q);
        if (v < 0.0f || u + v > 1.0f)
            return false;
        float tTmp = f * edge2.dot(q);
        if (tTmp > EPSILON) {
            t = tTmp;
            return true;
        }
        return false;
    }

    bool WorldModel::RayIntersectsTriangleWithBarycentric(
        const Vec3& orig, const Vec3& dir,
        const Vec3& v0, const Vec3& v1, const Vec3& v2,
        float& t, float& u, float& v)
    {
        const float EPSILON = 1e-6f;
        Vec3 edge1 = v1 - v0;
        Vec3 edge2 = v2 - v0;
        Vec3 h = dir.cross(edge2);
        float a = edge1.dot(h);
        if (std::fabs(a) < EPSILON)
            return false;
        float f = 1.0f / a;
        Vec3 s = orig - v0;
        u = f * s.dot(h);
        if (u < 0.0f || u > 1.0f)
            return false;
        Vec3 q = s.cross(edge1);
        v = f * dir.dot(q);
        if (v < 0.0f || u + v > 1.0f)
            return false;
        float tTmp = f * edge2.dot(q);
        if (tTmp > EPSILON) {
            t = tTmp;
            return true;
        }
        return false;
    }
    
    struct WModelRayCallBack
    {
        WModelRayCallBack(std::vector<GroupModel> const& mod) : models(mod.begin()), hit(false) {}
        bool operator()(Ray const& ray, unsigned int entry, float& distance, bool pStopAtFirstHit, bool ignoreM2Model)
        {
            bool result = models[entry].IntersectRay(ray, distance, pStopAtFirstHit, ignoreM2Model);
            if (result)  hit = true;
            return hit;
        }
        std::vector<GroupModel>::const_iterator models;
        bool hit;
    };

    bool WorldModel::IntersectRay(Ray const& ray, float& distance, bool stopAtFirstHit, bool ignoreM2Model) const
    {
        if (ignoreM2Model && (modelFlags & MOD_M2))
            return false;

        // small M2 workaround, maybe better make separate class with virtual intersection funcs
        // in any case, there's no need to use a bound tree if we only have one submodel
        if (groupModels.size() == 1)
            return groupModels[0].IntersectRay(ray, distance, stopAtFirstHit, ignoreM2Model);

        WModelRayCallBack isc(groupModels);
        groupTree.intersectRay(ray, isc, distance, stopAtFirstHit, ignoreM2Model);
        return isc.hit;
    }


    class WModelAreaCallback
    {
    public:
        WModelAreaCallback(std::vector<GroupModel> const& vals, Vec3 const& down) :
            prims(vals.begin()), hit(vals.end()), minVol(finf()), zDist(finf()), zVec(down) {
        }
        std::vector<GroupModel>::const_iterator prims;
        std::vector<GroupModel>::const_iterator hit;
        float minVol;
        float zDist;
        Vec3 zVec;
        void operator()(Vec3 const& point, unsigned int entry)
        {
            float group_Z;
            // float pVol = prims[entry].GetBound().volume();
            // if (pVol < minVol)
            //{
            /* if (prims[entry].iBound.contains(point)) */
            if (prims[entry].IsInsideObject(point, zVec, group_Z))
            {
                // minVol = pVol;
                // hit = prims + entry;
                if (group_Z < zDist)
                {
                    zDist = group_Z;
                    hit = prims + entry;
                }
#ifdef VMAP_DEBUG
                GroupModel const& gm = prims[entry];
                printf("%10u %8X %7.3f,%7.3f,%7.3f | %7.3f,%7.3f,%7.3f | z=%f, p_z=%f\n", gm.GetWmoID(), gm.GetMogpFlags(),
                    gm.GetBound().low().x, gm.GetBound().low().y, gm.GetBound().low().z,
                    gm.GetBound().high().x, gm.GetBound().high().y, gm.GetBound().high().z, group_Z, point.z);
#endif
            }
            //}
            // std::cout << "trying to intersect '" << prims[entry].name << "'\n";
        }
    };

    bool WorldModel::GetAreaInfo(const Vec3& p, const Vec3& dir, float& dist, struct AreaInfo& info) const {
        float closest = std::numeric_limits<float>::infinity();
        int hitTri = -1;
        float ground_Z = -std::numeric_limits<float>::infinity();
        for (size_t i = 0; i < triangles.size(); ++i) {
            const auto& tri = triangles[i];
            float t, u, v;
            if (RayIntersectsTriangleWithBarycentric(p, dir,
                vertices[tri.idx0], vertices[tri.idx1], vertices[tri.idx2], t, u, v)) {
                if (t < closest) {
                    closest = t;
                    hitTri = static_cast<int>(i);
                    Vec3 hit = p + dir * t;
                    if (hit.z > ground_Z)
                        ground_Z = hit.z;
                }
            }
        }
        if (hitTri != -1) {
            dist = closest;
            info.ground_Z = ground_Z;
            return true;
        }
        return false;
    }

    bool WorldModel::GetLocationInfo(const Vec3& p, const Vec3& dir, float& dist, struct LocationInfo& info) const {
        float closest = std::numeric_limits<float>::infinity();
        const MeshTriangle* hitTri = nullptr;
        Vec3 hitPoint;
        float ground_Z = -std::numeric_limits<float>::infinity();
        for (const auto& tri : triangles) {
            float t, u, v;
            if (RayIntersectsTriangleWithBarycentric(p, dir,
                vertices[tri.idx0], vertices[tri.idx1], vertices[tri.idx2], t, u, v)) {
                if (t < closest) {
                    closest = t;
                    hitTri = &tri;
                    hitPoint = p + dir * t;
                    if (hitPoint.z > ground_Z)
                        ground_Z = hitPoint.z;
                }
            }
        }
        if (hitTri) {
            dist = closest;
            info.ground_Z = ground_Z;
            return true;
        }
        return false;
    }

    bool WorldModel::ReadFile(const std::string& filename)
    {
        FileHandle rf(fopen(filename.c_str(), "rb"));
        if (!rf) return false;

        char magic[8] = { 0 };
        if (fread(magic, 1, 8, rf.get()) != 8) return false;
        if (strncmp(magic, RAW_VMAP_MAGIC, 8) != 0) return false;

        // Try to read as M2 (single mesh/doodad) first:
        long start = ftell(rf.get());
        unsigned int nVertices = 0, nofgroups = 0;
        if (fread(&nVertices, sizeof(unsigned int), 1, rf.get()) != 1) return false;
        if (fread(&nofgroups, sizeof(unsigned int), 1, rf.get()) != 1) return false;

        if (nofgroups == 1) // Treat as M2 doodad (extractor always sets 1 group for m2)
        {
            // Skip 3x unsigned int: rootwmoid, flags, groupid
            unsigned int unused[3];
            if (fread(unused, sizeof(unsigned int), 3, rf.get()) != 3) return false;

            // Skip 6x float: bounding box
            float bbox[6];
            if (fread(bbox, sizeof(float), 6, rf.get()) != 6) return false;
            bound = AABox(Vec3(bbox[0], bbox[1], bbox[2]), Vec3(bbox[3], bbox[4], bbox[5]));

            // Skip liquidflags
            unsigned int liquidflags;
            if (fread(&liquidflags, sizeof(unsigned int), 1, rf.get()) != 1) return false;

            // "GRP "
            char grpChunk[5] = { 0 };
            if (fread(grpChunk, 1, 4, rf.get()) != 4) return false;
            if (strncmp(grpChunk, "GRP ", 4) != 0) return false;

            int groupChunkSize = 0;
            unsigned int branches = 0;
            if (fread(&groupChunkSize, sizeof(int), 1, rf.get()) != 1) return false;
            if (fread(&branches, sizeof(unsigned int), 1, rf.get()) != 1) return false;
            for (unsigned int b = 0; b < branches; ++b) {
                unsigned int indexes = 0;
                if (fread(&indexes, sizeof(unsigned int), 1, rf.get()) != 1) return false;
            }

            // Read INDX chunk
            unsigned int nIndexes = 0;
            if (fread(&nIndexes, sizeof(unsigned int), 1, rf.get()) != 1) return false;
            char indxChunk[5] = { 0 };
            if (fread(indxChunk, 1, 4, rf.get()) != 4) return false;
            if (strncmp(indxChunk, "INDX", 4) != 0) return false;
            int indxChunkSize = 0;
            if (fread(&indxChunkSize, sizeof(int), 1, rf.get()) != 1) return false;
            unsigned int nIndexesCheck = 0;
            if (fread(&nIndexesCheck, sizeof(unsigned int), 1, rf.get()) != 1) return false;
            if (nIndexesCheck != nIndexes) return false;

            std::vector<unsigned short> indices(nIndexes);
            if (nIndexes > 0 && fread(indices.data(), sizeof(unsigned short), nIndexes, rf.get()) != nIndexes) return false;

            // Read VERT chunk
            char vertChunk[5] = { 0 };
            if (fread(vertChunk, 1, 4, rf.get()) != 4) return false;
            if (strncmp(vertChunk, "VERT", 4) != 0) return false;
            int vertChunkSize = 0;
            if (fread(&vertChunkSize, sizeof(int), 1, rf.get()) != 1) return false;
            unsigned int nVerticesCheck = 0;
            if (fread(&nVerticesCheck, sizeof(unsigned int), 1, rf.get()) != 1) return false;
            if (nVerticesCheck != nVertices) return false;

            std::vector<Vec3> vertexArray(nVertices);
            if (nVertices > 0 && fread(vertexArray.data(), sizeof(float) * 3, nVertices, rf.get()) != nVertices) return false;

            // Store mesh data
            vertices = vertexArray;
            triangles.clear();
            for (unsigned int i = 0; i + 2 < nIndexes; i += 3)
                triangles.emplace_back(indices[i], indices[i + 1], indices[i + 2]);

            // Build mesh BIH
            if (!triangles.empty()) {
                auto getTriangleBounds = [this](const MeshTriangle& tri, AABox& out) {
                    const Vec3& v0 = vertices[tri.idx0];
                    const Vec3& v1 = vertices[tri.idx1];
                    const Vec3& v2 = vertices[tri.idx2];
                    Vec3 minV = v0.min(v1).min(v2);
                    Vec3 maxV = v0.max(v1).max(v2);
                    out = AABox(minV, maxV);
                    };
                groupTree.build(triangles, getTriangleBounds);
            }
            else {
                groupTree = BIH();
            }
            return true;
        }

        // Fallback: WMO multi-group loader (call your raw group loader)
        fseek(rf.get(), start, SEEK_SET);
        VMAP::WorldModel_Raw raw;
        if (!raw.Read(filename.c_str(), RAW_VMAP_MAGIC)) return false;
        RootWMOID = raw.RootWMOID;
        groupModels.clear();
        groupModels.reserve(raw.groupsArray.size());
        for (const auto& rawGroup : raw.groupsArray) {
            GroupModel group;
            if (!group.ReadFromRaw(rawGroup)) return false;
            groupModels.push_back(std::move(group));
        }
        return true;
    }

    bool WorldModel::ReadM2VmoFile(const std::string& filename)
    {
        FileHandle rf(fopen(filename.c_str(), "rb"));
        if (!rf) return false;

        char magic[8] = { 0 };
        CHUNK_FAIL_CHECK(fread(magic, 1, 8, rf.get()) == 8, "magic header");
        CHUNK_FAIL_CHECK(strncmp(magic, RAW_VMAP_MAGIC, 8) == 0, "not RAW vmap");

        unsigned int nVertices = 0, nofGroups = 0;
        CHUNK_FAIL_CHECK(fread(&nVertices, sizeof(unsigned int), 1, rf.get()) == 1, "nVertices");
        CHUNK_FAIL_CHECK(fread(&nofGroups, sizeof(unsigned int), 1, rf.get()) == 1, "nofGroups");

        // skip unused header fields (rootwmoid, flags, groupid, bbox, liquidflags)
        CHUNK_FAIL_CHECK(fseek(rf.get(), 3 * sizeof(unsigned int) + 6 * sizeof(float) + sizeof(unsigned int), SEEK_CUR) == 0, "header skip");

        // "GRP "
        char grpChunk[5] = { 0 };
        CHUNK_FAIL_CHECK(fread(grpChunk, 1, 4, rf.get()) == 4, "GRP chunk id");
        CHUNK_FAIL_CHECK(strncmp(grpChunk, "GRP ", 4) == 0, "GRP chunk");

        int groupChunkSize = 0; unsigned int branches = 0;
        CHUNK_FAIL_CHECK(fread(&groupChunkSize, sizeof(int), 1, rf.get()) == 1, "groupChunkSize");
        CHUNK_FAIL_CHECK(fread(&branches, sizeof(unsigned int), 1, rf.get()) == 1, "branches");
        for (unsigned int b = 0; b < branches; ++b) {
            unsigned int indexes = 0;
            CHUNK_FAIL_CHECK(fread(&indexes, sizeof(unsigned int), 1, rf.get()) == 1, "branch indexes");
        }

        // "INDX" chunk
        char indxChunk[5] = { 0 };
        CHUNK_FAIL_CHECK(fread(indxChunk, 1, 4, rf.get()) == 4, "INDX chunk id");
        CHUNK_FAIL_CHECK(strncmp(indxChunk, "INDX", 4) == 0, "INDX chunk");

        int indxChunkSize = 0; unsigned int nIndexes = 0;
        CHUNK_FAIL_CHECK(fread(&indxChunkSize, sizeof(int), 1, rf.get()) == 1, "indxChunkSize");
        CHUNK_FAIL_CHECK(fread(&nIndexes, sizeof(unsigned int), 1, rf.get()) == 1, "nIndexes");

        std::vector<unsigned short> indices(nIndexes);
        if (nIndexes > 0)
            CHUNK_FAIL_CHECK(fread(indices.data(), sizeof(unsigned short), nIndexes, rf.get()) == nIndexes, "indices");

        // "VERT" chunk
        char vertChunk[5] = { 0 };
        CHUNK_FAIL_CHECK(fread(vertChunk, 1, 4, rf.get()) == 4, "VERT chunk id");
        CHUNK_FAIL_CHECK(strncmp(vertChunk, "VERT", 4) == 0, "VERT chunk");

        int vertChunkSize = 0; unsigned int nVerticesCheck = 0;
        CHUNK_FAIL_CHECK(fread(&vertChunkSize, sizeof(int), 1, rf.get()) == 1, "vertChunkSize");
        CHUNK_FAIL_CHECK(fread(&nVerticesCheck, sizeof(unsigned int), 1, rf.get()) == 1, "nVerticesCheck");
        CHUNK_FAIL_CHECK(nVerticesCheck == nVertices, "vertex count mismatch");

        std::vector<Vec3> vertexArray(nVertices);
        if (nVertices > 0)
            CHUNK_FAIL_CHECK(fread(vertexArray.data(), sizeof(float) * 3, nVertices, rf.get()) == nVertices, "vertices");

        vertices = vertexArray;
        triangles.clear();
        for (unsigned int i = 0; i + 2 < nIndexes; i += 3)
            triangles.emplace_back(indices[i], indices[i + 1], indices[i + 2]);

        // Build BIH if needed
        if (!triangles.empty()) {
            auto getTriangleBounds = [this](const MeshTriangle& tri, AABox& out) {
                const Vec3& v0 = vertices[tri.idx0];
                const Vec3& v1 = vertices[tri.idx1];
                const Vec3& v2 = vertices[tri.idx2];
                Vec3 minV = v0.min(v1).min(v2);
                Vec3 maxV = v0.max(v1).max(v2);
                out = AABox(minV, maxV);
                };
            groupTree.build(triangles, getTriangleBounds);
        }
        else {
            groupTree = BIH();
        }
        return true;
    }

    // --- GroupModel implementation ---

    GroupModel::GroupModel() : iMogpFlags(0), iGroupWMOID(0) {}
    GroupModel::GroupModel(const GroupModel& other)
        : iBound(other.iBound), iMogpFlags(other.iMogpFlags), iGroupWMOID(other.iGroupWMOID),
        vertices(other.vertices), triangles(other.triangles) {
    }
    GroupModel::GroupModel(unsigned int mogpFlags, unsigned int groupWMOID, const AABox& bound)
        : iBound(bound), iMogpFlags(mogpFlags), iGroupWMOID(groupWMOID) {
    }


    GroupModel::~GroupModel() { delete iLiquid; }

    void GroupModel::SetMeshData(std::vector<Vec3>& vert, std::vector<MeshTriangle>& tri) {
        vertices.swap(vert);
        triangles.swap(tri);
        // Optionally recompute bounds here if needed
    }
    struct GModelRayCallback
    {
        GModelRayCallback(std::vector<MeshTriangle> const& tris, std::vector<Vec3> const& vert) :
            vertices(vert.begin()), triangles(tris.begin()), hit(0) {
        }
        bool operator()(Ray const& ray, unsigned int entry, float& distance, bool /*pStopAtFirstHit*/, bool /*ignoreM2Model*/)
        {
            bool result = IntersectTriangle(triangles[entry], vertices, ray, distance);
            if (result)  ++hit;
            return hit;
        }
        std::vector<Vec3>::const_iterator vertices;
        std::vector<MeshTriangle>::const_iterator triangles;
        unsigned int hit;
    };

    bool GroupModel::IntersectRay(Ray const& ray, float& distance, bool stopAtFirstHit, bool ignoreM2Model) const
    {
        if (triangles.empty())
            return false;
        GModelRayCallback callback(triangles, vertices);
        meshTree.intersectRay(ray, callback, distance, stopAtFirstHit, ignoreM2Model);
        return callback.hit;
    }

    bool GroupModel::IsInsideObject(const Vec3& pos, const Vec3& down, float& z_dist) const {
        float minDist = std::numeric_limits<float>::infinity();
        bool found = false;
        for (const auto& tri : triangles) {
            float t, u, v;
            if (WorldModel::RayIntersectsTriangleWithBarycentric(
                pos, down,
                vertices[tri.idx0], vertices[tri.idx1], vertices[tri.idx2], t, u, v)) {
                if (t < minDist) {
                    minDist = t;
                    found = true;
                }
            }
        }
        if (found) {
            z_dist = minDist;
            return true;
        }
        return false;
    }
    
    bool GroupModel::ReadFromFile(FILE* rf)
    {
        char chunk[5] = { 0 };
        unsigned int chunkSize = 0;
        unsigned int count = 0;
        bool result = true;

        triangles.clear();
        vertices.clear();
        delete iLiquid; iLiquid = nullptr;

        // Bounding box, flags, group id
        CHUNK_FAIL_CHECK(fread(&iBound, sizeof(AABox), 1, rf) == 1, "AABox");
        CHUNK_FAIL_CHECK(fread(&iMogpFlags, sizeof(unsigned int), 1, rf) == 1, "MOGP flags");
        CHUNK_FAIL_CHECK(fread(&iGroupWMOID, sizeof(unsigned int), 1, rf) == 1, "GroupWMOID");

        // "VERT" chunk
        CHUNK_FAIL_CHECK(fread(chunk, 1, 4, rf) == 4, "VERT chunk id");
        CHUNK_FAIL_CHECK(strncmp(chunk, "VERT", 4) == 0, "VERT chunk");
        CHUNK_FAIL_CHECK(fread(&chunkSize, sizeof(unsigned int), 1, rf) == 1, "VERT chunk size");
        CHUNK_FAIL_CHECK(fread(&count, sizeof(unsigned int), 1, rf) == 1, "VERT count");
        if (count > 0) {
            vertices.resize(count);
            CHUNK_FAIL_CHECK(fread(vertices.data(), sizeof(Vec3), count, rf) == count, "VERT data");
        }

        // "TRIM" chunk
        CHUNK_FAIL_CHECK(fread(chunk, 1, 4, rf) == 4, "TRIM chunk id");
        CHUNK_FAIL_CHECK(strncmp(chunk, "TRIM", 4) == 0, "TRIM chunk");
        CHUNK_FAIL_CHECK(fread(&chunkSize, sizeof(unsigned int), 1, rf) == 1, "TRIM chunk size");
        CHUNK_FAIL_CHECK(fread(&count, sizeof(unsigned int), 1, rf) == 1, "TRIM count");
        if (count > 0) {
            triangles.resize(count);
            CHUNK_FAIL_CHECK(fread(triangles.data(), sizeof(MeshTriangle), count, rf) == count, "TRIM data");
        }

        // "MBIH" chunk
        CHUNK_FAIL_CHECK(fread(chunk, 1, 4, rf) == 4, "MBIH chunk id");
        CHUNK_FAIL_CHECK(strncmp(chunk, "MBIH", 4) == 0, "MBIH chunk");
        CHUNK_FAIL_CHECK(meshTree.ReadFromFile(rf), "MBIH data");

        // Optional "LIQU" chunk
        if (fread(chunk, 1, 4, rf) == 4 && strncmp(chunk, "LIQU", 4) == 0) {
            CHUNK_FAIL_CHECK(fread(&chunkSize, sizeof(unsigned int), 1, rf) == 1, "LIQU chunk size");
            if (chunkSize > 0)
                CHUNK_FAIL_CHECK(WmoLiquid::ReadFromFile(rf, iLiquid), "LIQU data");
        }
        // else: LIQU is absent, no problem.

        return true;
    }

    bool GroupModel::ReadFromRaw(const VMAP::GroupModel_Raw& raw)
    {
        iBound = raw.bounds;
        iMogpFlags = raw.mogpflags;
        iGroupWMOID = raw.GroupWMOID;
        vertices = raw.vertexArray;
        triangles = raw.triangles;

        // Clean up any previous liquid
        if (iLiquid) {
            delete iLiquid;
            iLiquid = nullptr;
        }

        // Copy liquid data if present
        if (raw.liquid) {
            // Deep copy via copy constructor (ensure WmoLiquid has proper copy ctor)
            iLiquid = new WmoLiquid(*raw.liquid);
        }

        // Rebuild BIH over the group mesh triangles (if non-empty)
        if (!vertices.empty() && !triangles.empty()) {
            // Lambda or functor for bounds:
            auto getTriangleBounds = [this](const MeshTriangle& tri, AABox& out) {
                const Vec3& v0 = vertices[tri.idx0];
                const Vec3& v1 = vertices[tri.idx1];
                const Vec3& v2 = vertices[tri.idx2];
                Vec3 minV = v0.min(v1).min(v2);
                Vec3 maxV = v0.max(v1).max(v2);
                out = AABox(minV, maxV);
                };
            meshTree.build(triangles, getTriangleBounds);
        }
        else {
            // If no geometry, clear tree
            meshTree = BIH();
        }

        // All done
        return true;
    }

} // namespace VMAP
