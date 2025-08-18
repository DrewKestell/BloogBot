// Ray.cpp
#include "Ray.h"
#include <algorithm>

namespace G3D
{
    Ray::Ray()
    {
    }

    Ray::Ray(const Vector3& org, const Vector3& dir)
        : m_origin(org), m_direction(dir)
    {
        m_invDirection = Vector3(
            dir.x != 0 ? 1.0f / dir.x : inf(),
            dir.y != 0 ? 1.0f / dir.y : inf(),
            dir.z != 0 ? 1.0f / dir.z : inf()
        );
    }

    float Ray::intersectionTime(const AABox& box) const
    {
        const float EPSILON = 1e-6f;  // Add epsilon for stability
        float tmin = -inf();
        float tmax = inf();

        for (int i = 0; i < 3; ++i)
        {
            // Check for parallel ray (near-zero direction component)
            if (std::abs(m_direction[i]) < EPSILON)
            {
                // Ray is parallel to slab, check if origin is within slab
                if (m_origin[i] < box.low()[i] || m_origin[i] > box.high()[i])
                    return inf();
            }
            else
            {
                float t1 = (box.low()[i] - m_origin[i]) * m_invDirection[i];
                float t2 = (box.high()[i] - m_origin[i]) * m_invDirection[i];

                // Handle negative direction
                if (m_invDirection[i] < 0.0f)
                    std::swap(t1, t2);

                tmin = std::max(tmin, t1);
                tmax = std::min(tmax, t2);

                // Early exit if no intersection
                if (tmin > tmax)
                    return inf();
            }
        }

        // Return the entry point (or 0 if ray starts inside)
        return tmin > 0 ? tmin : 0;
    }

    Ray Ray::fromOriginAndDirection(const Vector3& org, const Vector3& dir)
    {
        return Ray(org, dir);
    }
}