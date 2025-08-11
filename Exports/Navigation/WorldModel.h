// WorldModel.h - Complete with all required classes
#pragma once

#include <vector>
#include <memory>
#include <string>
#include "Vector3.h"
#include "AABox.h"
#include "Ray.h"
#include "BIH.h"

namespace VMAP
{
    // Forward declarations
    struct AreaInfo
    {
        bool result;
        float ground_Z;
        uint32_t flags;
        int32_t adtId;
        int32_t rootId;
        int32_t groupId;

        AreaInfo() : result(false), ground_Z(-G3D::inf()), flags(0),
            adtId(-1), rootId(-1), groupId(-1) {
        }
    };

    struct GroupLocationInfo
    {
        const class GroupModel* hitModel;
        int32_t rootId;

        GroupLocationInfo() : hitModel(nullptr), rootId(-1) {}
    };

    // MeshTriangle structure for collision detection
    struct MeshTriangle
    {
        uint32_t idx0;
        uint32_t idx1;
        uint32_t idx2;
    };

    // WmoLiquid class for liquid handling
    class WmoLiquid
    {
    private:
        uint32_t iTilesX;
        uint32_t iTilesY;
        G3D::Vector3 iCorner;
        uint32_t iType;
        float* iHeight;
        uint8_t* iFlags;

    public:
        WmoLiquid(uint32_t width, uint32_t height, const G3D::Vector3& corner, uint32_t type);
        WmoLiquid(const WmoLiquid& other);
        ~WmoLiquid();

        WmoLiquid& operator=(const WmoLiquid& other);

        bool GetLiquidHeight(const G3D::Vector3& pos, float& liqHeight) const;
        uint32_t GetType() const { return iType; }

        bool writeToFile(FILE* wf) const;
        static bool readFromFile(FILE* rf, WmoLiquid*& liquid);

        void getPosInfo(uint32_t& tilesX, uint32_t& tilesY, G3D::Vector3& corner) const;

    private:
        WmoLiquid() : iTilesX(0), iTilesY(0), iCorner(), iType(0), iHeight(nullptr), iFlags(nullptr) {}
    };

    // GroupModel class definition
    class GroupModel
    {
    private:
        G3D::AABox iBound;
        uint32_t iMogpFlags;
        uint32_t iGroupWMOID;
        std::vector<G3D::Vector3> vertices;
        std::vector<MeshTriangle> triangles;
        BIH meshTree;
        WmoLiquid* iLiquid;

        GroupModel(const GroupModel& other) = delete;
        GroupModel& operator=(const GroupModel& other) = delete;

    public:
        GroupModel() : iMogpFlags(0), iGroupWMOID(0), iLiquid(nullptr) {}
        GroupModel(uint32_t mogpFlags, uint32_t groupWMOID, const G3D::AABox& bound)
            : iBound(bound), iMogpFlags(mogpFlags), iGroupWMOID(groupWMOID), iLiquid(nullptr) {
        }

        // Move constructor
        GroupModel(GroupModel&& other) noexcept
            : iBound(std::move(other.iBound)),
            iMogpFlags(other.iMogpFlags),
            iGroupWMOID(other.iGroupWMOID),
            vertices(std::move(other.vertices)),
            triangles(std::move(other.triangles)),
            meshTree(std::move(other.meshTree)),
            iLiquid(other.iLiquid)
        {
            other.iLiquid = nullptr;
        }

        // Move assignment operator
        GroupModel& operator=(GroupModel&& other) noexcept
        {
            if (this != &other)
            {
                delete iLiquid;

                iBound = std::move(other.iBound);
                iMogpFlags = other.iMogpFlags;
                iGroupWMOID = other.iGroupWMOID;
                vertices = std::move(other.vertices);
                triangles = std::move(other.triangles);
                meshTree = std::move(other.meshTree);
                iLiquid = other.iLiquid;
                other.iLiquid = nullptr;
            }
            return *this;
        }

        ~GroupModel() { delete iLiquid; }

        void setMeshData(std::vector<G3D::Vector3>& vert, std::vector<MeshTriangle>& tri);
        void setLiquidData(WmoLiquid* liquid) { iLiquid = liquid; }
        bool IntersectRay(const G3D::Ray& ray, float& distance, bool stopAtFirstHit) const;
        bool IsInsideObject(const G3D::Vector3& pos, const G3D::Vector3& down, float& z_dist) const;
        bool GetLiquidLevel(const G3D::Vector3& pos, float& liqHeight) const;
        uint32_t GetLiquidType() const;
        bool writeToFile(FILE* wf) const;
        bool readFromFile(FILE* rf);
        const G3D::AABox& GetBound() const { return iBound; }
        uint32_t GetMogpFlags() const { return iMogpFlags; }
        uint32_t GetWmoID() const { return iGroupWMOID; }
    };

    // WorldModel class
    class WorldModel
    {
    public:
        WorldModel() : RootWMOID(0), modelFlags(0) {}

        //! pass group models to WorldModel and create BIH. Passed vector is swapped with old geometry!
        void setGroupModels(std::vector<GroupModel>& models);
        void setRootWmoID(uint32_t id) { RootWMOID = id; }
        bool IntersectRay(const G3D::Ray& ray, float& distance, bool stopAtFirstHit, bool ignoreM2Model) const;
        bool IntersectPoint(const G3D::Vector3& p, const G3D::Vector3& down, float& dist, AreaInfo& info) const;
        bool IsUnderObject(const G3D::Vector3& p, const G3D::Vector3& up, bool m2,
            float* outDist = nullptr, float* inDist = nullptr) const;
        bool GetLocationInfo(const G3D::Vector3& p, const G3D::Vector3& down, float& dist,
            GroupLocationInfo& info) const;
        bool writeFile(const std::string& filename);
        bool readFile(const std::string& filename);
        void setModelFlags(uint32_t newFlags) { modelFlags = newFlags; }
        uint32_t getModelFlags() const { return modelFlags; }

    protected:
        uint32_t RootWMOID;
        std::vector<GroupModel> groupModels;
        BIH groupTree;
        uint32_t modelFlags;

#ifdef MMAP_GENERATOR
    public:
        void getGroupModels(std::vector<GroupModel>& models);
#endif
    };
} // namespace VMAP