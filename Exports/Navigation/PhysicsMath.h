// PhysicsMath.h - Math utilities for PhysicsEngine
#pragma once

#include <cmath>
#include <algorithm>

namespace PhysicsMath
{
    // Clamp value between min and max
    template<typename T>
    inline T Clamp(T value, T minVal, T maxVal)
    {
        return std::max(minVal, std::min(value, maxVal));
    }

    // Normalize 2D vector
    inline void Normalize2D(float& x, float& y)
    {
        float len = std::sqrt(x * x + y * y);
        if (len > 0.0001f)
        {
            x /= len;
            y /= len;
        }
    }

    // Normalize angle to [-PI, PI]
    inline float NormalizeAngle(float angle)
    {
        while (angle > 3.14159265359f)
            angle -= 2.0f * 3.14159265359f;
        while (angle < -3.14159265359f)
            angle += 2.0f * 3.14159265359f;
        return angle;
    }

    // Calculate 3D distance
    inline float Distance3D(float x1, float y1, float z1, float x2, float y2, float z2)
    {
        float dx = x2 - x1;
        float dy = y2 - y1;
        float dz = z2 - z1;
        return std::sqrt(dx * dx + dy * dy + dz * dz);
    }

    // Calculate 2D distance
    inline float Distance2D(float x1, float y1, float x2, float y2)
    {
        float dx = x2 - x1;
        float dy = y2 - y1;
        return std::sqrt(dx * dx + dy * dy);
    }

    // Linear interpolation
    inline float Lerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }

    // Smooth step interpolation
    inline float SmoothStep(float edge0, float edge1, float x)
    {
        x = Clamp((x - edge0) / (edge1 - edge0), 0.0f, 1.0f);
        return x * x * (3.0f - 2.0f * x);
    }

    // Check if value is approximately equal to target
    inline bool ApproximatelyEqual(float a, float b, float epsilon = 0.0001f)
    {
        return std::abs(a - b) < epsilon;
    }

    // Convert degrees to radians
    inline float DegToRad(float degrees)
    {
        return degrees * (3.14159265359f / 180.0f);
    }

    // Convert radians to degrees
    inline float RadToDeg(float radians)
    {
        return radians * (180.0f / 3.14159265359f);
    }

    // Get sign of value
    template<typename T>
    inline T Sign(T value)
    {
        return (T(0) < value) - (value < T(0));
    }

    // Wrap value to range [0, max)
    inline float Wrap(float value, float max)
    {
        float result = std::fmod(value, max);
        if (result < 0)
            result += max;
        return result;
    }

    // Calculate dot product of 2D vectors
    inline float Dot2D(float x1, float y1, float x2, float y2)
    {
        return x1 * x2 + y1 * y2;
    }

    // Calculate dot product of 3D vectors
    inline float Dot3D(float x1, float y1, float z1, float x2, float y2, float z2)
    {
        return x1 * x2 + y1 * y2 + z1 * z2;
    }

    // Calculate cross product of 3D vectors (Z component only for 2D cross)
    inline float Cross2D(float x1, float y1, float x2, float y2)
    {
        return x1 * y2 - y1 * x2;
    }

    // Project point onto line segment
    inline void ProjectPointOntoLineSegment(float px, float py,
        float x1, float y1,
        float x2, float y2,
        float& projX, float& projY)
    {
        float dx = x2 - x1;
        float dy = y2 - y1;
        float lenSq = dx * dx + dy * dy;

        if (lenSq < 0.0001f)
        {
            projX = x1;
            projY = y1;
            return;
        }

        float t = Clamp(((px - x1) * dx + (py - y1) * dy) / lenSq, 0.0f, 1.0f);
        projX = x1 + t * dx;
        projY = y1 + t * dy;
    }

    // Calculate angle between two 2D vectors
    inline float AngleBetween2D(float x1, float y1, float x2, float y2)
    {
        float dot = Dot2D(x1, y1, x2, y2);
        float len1 = std::sqrt(x1 * x1 + y1 * y1);
        float len2 = std::sqrt(x2 * x2 + y2 * y2);

        if (len1 < 0.0001f || len2 < 0.0001f)
            return 0.0f;

        float cosAngle = Clamp(dot / (len1 * len2), -1.0f, 1.0f);
        return std::acos(cosAngle);
    }

} // namespace PhysicsMath