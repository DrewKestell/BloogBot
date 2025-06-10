#pragma once

#include <string>
#include "Vec3Ray.h"         // For Vec3, Quat, AABox, Ray

namespace VMAP { class WorldModel; }

struct GameObjectModel
{
    // === Members ===
    bool          isCollidable = true;
    std::string   name;
    AABox         iBound;         // World-space bounds (transformed)
    Matrix3       iInvRot;
    AABox         iModelBound;    // Model-local bounds (unrotated, unscaled)
    Vec3          iPos;
    Quat          iQuat;          // Rotation as a quaternion
    float         iScale = 1.0f;
    float         iInvScale = 1.0f;
    VMAP::WorldModel* iModel = nullptr;

    // === API ===

public:
    const AABox& getBounds() const { return iBound; }
    const Vec3& getPosition() const { return iPos; }
    const std::string& GetName() const { return name; }
    void SetName(const std::string& n) { name = n; }
    void SetCollidable(bool enabled) { isCollidable = enabled; }

    // Build from data (displayId, position, orientation, scale, bounds, model pointer)
    void Initialize(const std::string& modelName, const AABox& modelLocalBox, const Vec3& pos, const Quat& rotation, float scale, VMAP::WorldModel* model);

    // Ray-model intersection test (in world-space)
    bool intersectRay(Ray const& ray, float& MaxDist, bool StopAtFirstHit, bool ignoreM2Model) const;

    // For compatibility with BoundsTrait (BIH): GetBound(s)
    const AABox& GetBound() const { return iBound; } // For BIH by value

    // Optional: Utility to update position/rotation/scale and recompute iBound
    void Relocate(const Vec3& newPos, const Quat& newQuat, float newScale, const AABox& modelLocalBox);
};
