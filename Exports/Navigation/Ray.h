// Ray.h
#pragma once

#include "Vector3.h"
#include "AABox.h"

namespace G3D
{
    class Ray
    {
    private:
        Vector3 m_origin;
        Vector3 m_direction;
        Vector3 m_invDirection;

    public:
        Ray();
        Ray(const Vector3& org, const Vector3& dir);

        const Vector3& origin() const { return m_origin; }
        const Vector3& direction() const { return m_direction; }
        const Vector3& invDirection() const { return m_invDirection; }

        float intersectionTime(const AABox& box) const;

        static Ray fromOriginAndDirection(const Vector3& org, const Vector3& dir);
    };
}