// Ray.cpp
#include "Ray.h"
#include <algorithm>
#include "VMapLog.h"

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

        // Log ray and box details
        LOG_DEBUG("=== Ray::intersectionTime ===");
        LOG_DEBUG("Ray origin: (" << m_origin.x << ", " << m_origin.y << ", " << m_origin.z << ")");
        LOG_DEBUG("Ray direction: (" << m_direction.x << ", " << m_direction.y << ", " << m_direction.z << ")");
        LOG_DEBUG("Box min: (" << box.low().x << ", " << box.low().y << ", " << box.low().z << ")");
        LOG_DEBUG("Box max: (" << box.high().x << ", " << box.high().y << ", " << box.high().z << ")");

        float tmin = -inf();
        float tmax = inf();

        for (int i = 0; i < 3; ++i)
        {
            const char* axisName = (i == 0) ? "X" : (i == 1) ? "Y" : "Z";

            // Check for parallel ray (near-zero direction component)
            if (std::abs(m_direction[i]) < EPSILON)
            {
                LOG_DEBUG("  " << axisName << "-axis: Ray parallel to slab");
                LOG_DEBUG("    Origin[" << i << "]=" << m_origin[i]
                    << " Box range=[" << box.low()[i] << ", " << box.high()[i] << "]");

                // Ray is parallel to slab, check if origin is within slab
                if (m_origin[i] < box.low()[i] || m_origin[i] > box.high()[i])
                {
                    LOG_DEBUG("    Ray origin outside slab bounds - NO INTERSECTION");
                    return inf();
                }
                LOG_DEBUG("    Ray origin within slab bounds - continuing");
            }
            else
            {
                float t1 = (box.low()[i] - m_origin[i]) * m_invDirection[i];
                float t2 = (box.high()[i] - m_origin[i]) * m_invDirection[i];

                LOG_DEBUG("  " << axisName << "-axis calculations:");
                LOG_DEBUG("    invDirection[" << i << "]=" << m_invDirection[i]);
                LOG_DEBUG("    t1 (to low)=" << t1 << " t2 (to high)=" << t2);

                // Handle negative direction
                if (m_invDirection[i] < 0.0f)
                {
                    std::swap(t1, t2);
                    LOG_DEBUG("    Swapped due to negative direction: t1=" << t1 << " t2=" << t2);
                }

                float old_tmin = tmin;
                float old_tmax = tmax;
                tmin = std::max(tmin, t1);
                tmax = std::min(tmax, t2);

                LOG_DEBUG("    Updated: tmin " << old_tmin << " -> " << tmin
                    << ", tmax " << old_tmax << " -> " << tmax);

                // Early exit if no intersection
                if (tmin > tmax)
                {
                    LOG_DEBUG("    tmin > tmax (" << tmin << " > " << tmax << ") - NO INTERSECTION");
                    return inf();
                }
            }
        }

        // Return the entry point (or 0 if ray starts inside)
        float result = tmin > 0 ? tmin : 0;
        LOG_DEBUG("Final result: tmin=" << tmin << " returning " << result
            << (result == 0 ? " (ray starts inside box)" : " (intersection distance)"));

        return result;
    }

    Ray Ray::fromOriginAndDirection(const Vector3& org, const Vector3& dir)
    {
        return Ray(org, dir);
    }
}