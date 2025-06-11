#pragma once

#include "Vec3Ray.h"

class GameObjectModel;

class DynamicMapTree
{
public:
    DynamicMapTree();
    ~DynamicMapTree();

    bool isInLineOfSight(float x1, float y1, float z1, float x2, float y2, float z2, bool ignoreM2Model) const;
    bool getIntersectionTime(const Ray& ray, const Vec3& endPos, float& maxDist) const;
    bool getObjectHitPos(const Vec3& pPos1, const Vec3& pPos2, Vec3& pResultHitPos, float pModifyDist) const;
    bool getObjectHitPos(float x1, float y1, float z1, float x2, float y2, float z2, float& rx, float& ry, float& rz, float pModifyDist) const;
    float getHeight(float x, float y, float z, float maxSearchDist) const;

    void insert(const GameObjectModel&);
    void remove(const GameObjectModel&);
    bool contains(const GameObjectModel&) const;
    int size() const;
    void balance();
    void update(unsigned int diff);
private:
    struct DynTreeImpl& impl;
};
