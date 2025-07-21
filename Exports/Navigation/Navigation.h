#ifndef NAVIGATION_H
#define NAVIGATION_H

#include "MoveMap.h"
#include <vector>
#include <string>

class XYZ
{
public:
    float X;
    float Y;
    float Z;

    XYZ() : X(0), Y(0), Z(0) {}

    XYZ(double X, double Y, double Z)
    {
        this->X = static_cast<float>(X);
        this->Y = static_cast<float>(Y);
        this->Z = static_cast<float>(Z);
    }
};

struct NavPoly            // <-- goes below XYZ definition
{
    uint64_t refId;       // Detour poly reference
    uint32_t area;        // 0‑ground, 1‑water, 2‑lava, …
    uint32_t flags;       // walk / swim / door / etc.
    uint32_t vertCount;   // 3‑6
    XYZ      verts[6];    // world‑space verts (WoW axis)
};

class Navigation
{
public:
    static Navigation* GetInstance();
    void Initialize();
    void Release(); 
    bool RaycastToWmoMesh(unsigned int mapId, float startX, float startY, float startZ, float endX, float endY, float endZ, float* hitX, float* hitY, float* hitZ);
    std::vector<NavPoly> CapsuleOverlapSweep(uint32_t mapId,
        const XYZ& p0,
        const XYZ& p1,
        float r, float h,
        float step /* =0.3f */);
    XYZ* CalculatePath(unsigned int mapId, XYZ start, XYZ end, bool straightPath, int* length);
    void FreePathArr(XYZ* length);
    std::string GetMmapsPath();
    bool IsLineOfSight(uint32_t mapId, const XYZ& a, const XYZ& b);
    std::vector<NavPoly> CapsuleOverlap(uint32_t mapId, const XYZ& pos, float radius, float height);
    float GetLiquidHeight(uint32_t mapId, float x, float y, float z, uint32_t liquidTypeMask);
    const dtNavMeshQuery* GetQueryForMap(uint32_t mapId);
private:
    void InitializeMapsForContinent(MMAP::MMapManager* manager, unsigned int mapId);
    static Navigation* s_singletonInstance;
    XYZ* currentPath;
};

#endif
