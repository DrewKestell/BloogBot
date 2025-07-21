// wow/vmap/MapTree.cpp
#include "MapTree.h"

using namespace wow::vmap;
using wow::vmap::transformPoint;
using wow::vmap::transformDirection;

MapTree::MapTree()
    : _bih(_bihTris, 4)         // empty vector, leaf size 4
{
}

void MapTree::addModel(const WorldModel* wm, const Mat4& placement)
{
    GroupModel_Raw g;
    g.model = wm;
    g.world = placement;

    // world-space bbox
    Vec3 wsMin = transformPoint(placement, wm->bounds().min);
    Vec3 wsMax = transformPoint(placement, wm->bounds().max);
    g.boundsWS.min = wsMin.min(wsMax);
    g.boundsWS.max = wsMin.max(wsMax);

    _bounds.expand(g.boundsWS);
    _models.push_back(g);
}

void MapTree::finalize()
{
    if (_finalized) return;
    _bihTris.reserve(_models.size());
    for (const auto& g : _models)
    {
        // one degenerate tri per model bbox for coarse BIH
        BIH::Triangle t;
        t.v[0] = g.boundsWS.min;
        t.v[1] = Vec3(g.boundsWS.max.x, g.boundsWS.min.y, g.boundsWS.min.z);
        t.v[2] = g.boundsWS.max;
        t.id = static_cast<uint32>(_bihTris.size());
        _bihTris.push_back(t);
    }
    _bih = BIH(_bihTris, 4);
    _finalized = true;
}

/* ------------------------------------------------------------------------- */

bool MapTree::isInLineOfSight(const Vec3& p, const Vec3& q) const
{
    Vec3 dir = q - p;
    float len = dir.length();
    if (len < 1e-3f) return true;
    dir /= len;

    float tHit; Vec3 n;
    if (!_bih.intersectRay(p, dir, tHit, n) || tHit > len) return true;

    // tHit hits a model bbox – refine against that model
    const uint32 idx = static_cast<uint32>(tHit); // id stored earlier
    const auto& gm = _models[idx];

    // transform ray into model space
    Mat4 inv = gm.world.inverse();
    Vec3 mp = transformPoint(inv, p);
    Vec3 md = transformDirection(inv, dir).unit();
    md = md.unit();

    float t; Vec3 n2;
    if (!gm.model->bih().intersectRay(mp, md, t, n2)) return true;
    return (t > len);                 // clear if farther than dest
}

/* ------------------------------------------------------------------------- */

float MapTree::getHeight(const Vec3& pos, float maxSearchDist) const
{
    Vec3 start = pos + Vec3(0, 0, maxSearchDist * 0.5f);
    Vec3 end = pos - Vec3(0, 0, maxSearchDist);

    Vec3 dir = (end - start).unit();
    float len = maxSearchDist * 1.5f;

    float tMin = VMAP_INVALID_HEIGHT;
    for (const auto& gm : _models)
    {
        Mat4 inv = gm.world.inverse();
        Vec3 pms = transformPoint(inv, start);
        Vec3 dms = transformDirection(inv, dir);

        float t; Vec3 n;
        if (gm.model->bih().intersectRay(pms, dms, t, n))
            if (t < len && t > 0) { len = t; tMin = (dir * t + start).z; }
    }
    return tMin;
}

/* ------------------------------------------------------------------------- */

bool MapTree::getObjectHitPos(const Vec3& p, const Vec3& q,
    Vec3& out, float padding) const
{
    Vec3 dir = q - p;
    float len = dir.length();
    if (len < 1e-3f) return false;
    dir /= len;

    float bestT = len;
    bool hit = false;

    for (const auto& gm : _models)
    {
        Mat4 inv = gm.world.inverse();
        Vec3 ps = transformPoint(inv, p);
        Vec3 ds = transformDirection(inv, dir);

        float t; Vec3 n;
        if (gm.model->bih().intersectRay(ps, ds, t, n) && t < bestT)
        {
            bestT = t;
            hit = true;
        }
    }

    if (hit)
    {
        out = p + dir * (bestT - padding);
        return true;
    }
    return false;
}
