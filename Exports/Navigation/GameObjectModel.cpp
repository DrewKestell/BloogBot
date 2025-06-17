#include "GameObjectModel.h"
#include "WorldModel.h"

void GameObjectModel::Initialize(const std::string& modelName, const AABox& modelLocalBox, const Vec3& pos, const Quat& rotation, float scale, VMAP::WorldModel* model)
{
    name = modelName;
    iModelBound = modelLocalBox;
    iPos = pos;
    iQuat = rotation;
    iScale = scale;
    iInvScale = 1.0f / scale;
    iModel = model;

    // Transform bounding box: scale, rotate, then translate to world-space
    AABox scaledBox(iModelBound.low() * iScale, iModelBound.high() * iScale);
    AABox rotatedBox;
    for (int i = 0; i < 8; ++i)
        rotatedBox.merge(iQuat * scaledBox.corner(i));
    iBound = rotatedBox + iPos;
}

bool GameObjectModel::intersectRay(Ray const& ray, float& MaxDist, bool StopAtFirstHit, bool ignoreM2Model) const
{    float time = ray.intersectionTime(iBound);
    if (time == finf())
        return false;

    // child bounds are defined in object space:
    Vec3 p = iInvRot * (ray.origin() - iPos) * iInvScale;
    Ray modRay(p, iInvRot * ray.direction());
    float distance = MaxDist * iInvScale;
    bool hit = iModel->IntersectRay(modRay, distance, StopAtFirstHit, ignoreM2Model);
    if (hit)
    {
        distance *= iScale;
        MaxDist = distance;
    }
    return hit;
}

void GameObjectModel::Relocate(const Vec3& newPos, const Quat& newQuat, float newScale, const AABox& modelLocalBox)
{
    iPos = newPos;
    iQuat = newQuat;
    iScale = newScale;
    iInvScale = 1.0f / newScale;
    iModelBound = modelLocalBox;

    AABox scaledBox(iModelBound.low() * iScale, iModelBound.high() * iScale);
    AABox rotatedBox;
    for (int i = 0; i < 8; ++i)
        rotatedBox.merge(iQuat * scaledBox.corner(i));
    iBound = rotatedBox + iPos;
}
