#include "DynamicTree.h"
#include "Timer.h"
#include "BIHWrap.h"
#include "RegularGrid.h"
#include "GameObjectModel.h"
#include <limits>
#include <cassert>

template<> struct HashTrait< GameObjectModel>
{
    static size_t hashCode(GameObjectModel const& g)
    {
        return (size_t)(void*)&g;
    }
};

template<> struct PositionTrait< GameObjectModel>
{
    static void getPosition(GameObjectModel const& g, Vec3& p)
    {
        p = g.getPosition();
    }
};

template<> struct BoundsTrait< GameObjectModel>
{
    static void getBounds(GameObjectModel const& g, AABox& out)
    {
        out = g.getBounds();
    }
    static void getBounds2(GameObjectModel const* g, AABox& out)
    {
        out = g->getBounds();
    }
};

int CHECK_TREE_PERIOD = 200;

typedef RegularGrid2D<GameObjectModel, BIHWrap<GameObjectModel>> ParentTree;

struct DynTreeImpl : public ParentTree
{
    typedef GameObjectModel Model;
    typedef ParentTree base;

    DynTreeImpl() :
        rebalance_timer(CHECK_TREE_PERIOD),
        unbalanced_times(0)
    {
    }

    void insert(const Model& mdl)
    {
        base::insert(mdl);
        ++unbalanced_times;
    }

    void remove(const Model& mdl)
    {
        base::remove(mdl);
        ++unbalanced_times;
    }

    void balance()
    {
        base::balance();
        unbalanced_times = 0;
    }

    void update(uint32_t difftime)
    {
        if (!size()) return;

        rebalance_timer.Update(difftime);
        if (rebalance_timer.Passed())
        {
            rebalance_timer.Reset(CHECK_TREE_PERIOD);
            if (unbalanced_times > 0)
                balance();
        }
    }

    TimeTracker rebalance_timer;
    int unbalanced_times;
};

DynamicMapTree::DynamicMapTree() : impl(*new DynTreeImpl())
{
}

DynamicMapTree::~DynamicMapTree()
{
    delete &impl;
}

void DynamicMapTree::insert(const GameObjectModel& mdl)
{
    impl.insert(mdl);
}

void DynamicMapTree::remove(const GameObjectModel& mdl)
{
    impl.remove(mdl);
}

bool DynamicMapTree::contains(const GameObjectModel& mdl) const
{
    return impl.contains(mdl);
}

void DynamicMapTree::balance()
{
    impl.balance();
}

int DynamicMapTree::size() const
{
    return impl.size();
}

void DynamicMapTree::update(unsigned int t_diff)
{
    impl.update(t_diff);
}

struct DynamicTreeIntersectionCallback
{
    bool did_hit;
    DynamicTreeIntersectionCallback() : did_hit(false) {}
    bool operator()(const Ray& r, GameObjectModel const& obj, float& distance, bool stopAtFirst, bool ignoreM2Model)
    {
        did_hit = obj.intersectRay(r, distance, stopAtFirst, ignoreM2Model);
        return did_hit;
    }
    bool didHit() const { return did_hit; }
};


bool DynamicMapTree::getIntersectionTime(Ray const& ray, Vec3 const& endPos, float& pMaxDist) const
{
    float distance = pMaxDist;
    DynamicTreeIntersectionCallback callback;
    impl.intersectRay(ray, callback, distance, endPos, false);
    if (callback.didHit())
        pMaxDist = distance;
    return callback.didHit();
}

bool DynamicMapTree::getObjectHitPos(float x1, float y1, float z1, float x2, float y2, float z2, float& rx, float& ry, float& rz, float pModifyDist) const
{
    Vec3 pos1(x1, y1, z1);
    Vec3 pos2(x2, y2, z2);
    Vec3 resultPos;
    bool result = getObjectHitPos(pos1, pos2, resultPos, pModifyDist);
    rx = resultPos.x;
    ry = resultPos.y;
    rz = resultPos.z;
    return result;
}

bool DynamicMapTree::getObjectHitPos(const Vec3& pPos1, const Vec3& pPos2, Vec3& pResultHitPos, float pModifyDist) const
{
    bool result = false;
    float maxDist = (pPos2 - pPos1).magnitude();
    assert(maxDist < std::numeric_limits<float>::max());
    if (maxDist < 1e-10f)
    {
        pResultHitPos = pPos2;
        return false;
    }
    Vec3 dir = (pPos2 - pPos1) / maxDist;
    Ray ray(pPos1, dir);
    float dist = maxDist;
    if (getIntersectionTime(ray, pPos2, dist))
    {
        pResultHitPos = pPos1 + dir * dist;
        if (pModifyDist < 0)
        {
            if ((pResultHitPos - pPos1).magnitude() > -pModifyDist)
                pResultHitPos = pResultHitPos + dir * pModifyDist;
            else
                pResultHitPos = pPos1;
        }
        else
            pResultHitPos = pResultHitPos + dir * pModifyDist;
        result = true;
    }
    else
    {
        pResultHitPos = pPos2;
        result = false;
    }
    return result;
}

bool DynamicMapTree::isInLineOfSight(float x1, float y1, float z1, float x2, float y2, float z2, bool ignoreM2Model) const
{
    Vec3 v1(x1, y1, z1), v2(x2, y2, z2);

    float maxDist = (v2 - v1).magnitude();

    if (!fuzzyGt(maxDist, 0))
        return true;

    Ray r(v1, (v2 - v1) / maxDist);
    DynamicTreeIntersectionCallback callback;
    impl.intersectRay(r, callback, maxDist, v2, ignoreM2Model);

    return !callback.did_hit;
}

float DynamicMapTree::getHeight(float x, float y, float z, float maxSearchDist) const
{
    Vec3 v(x, y, z);
    Ray r(v, Vec3::down());
    DynamicTreeIntersectionCallback callback;
    impl.intersectZAllignedRay(r, callback, maxSearchDist);

    if (callback.didHit())
        return v.z - maxSearchDist;
    return -finf();
}
