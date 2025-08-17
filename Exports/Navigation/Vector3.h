// Vector3.h
#pragma once

#include <cmath>
#include <algorithm>
#include <limits>

namespace G3D
{
    // Forward declaration for Matrix3
    class Matrix3;

    class Vector3
    {
    public:
        float x, y, z;

        Vector3() : x(0), y(0), z(0) {}
        Vector3(float x_, float y_, float z_) : x(x_), y(y_), z(z_) {}
        Vector3(const float* v) : x(v[0]), y(v[1]), z(v[2]) {}

        static Vector3 zero() { return Vector3(0, 0, 0); }
        static Vector3 up() { return Vector3(0, 0, 1); }
        static Vector3 down() { return Vector3(0, 0, -1); }

        Vector3 operator+(const Vector3& v) const { return Vector3(x + v.x, y + v.y, z + v.z); }
        Vector3 operator-(const Vector3& v) const { return Vector3(x - v.x, y - v.y, z - v.z); }
        Vector3 operator*(float s) const { return Vector3(x * s, y * s, z * s); }
        Vector3 operator/(float s) const
        {
            float inv = 1.0f / s;
            return Vector3(x * inv, y * inv, z * inv);
        }
        Vector3 operator-() const { return Vector3(-x, -y, -z); }

        // Right multiplication by matrix (defined in Vector3.cpp to avoid circular dependency)
        Vector3 operator*(const Matrix3& m) const;

        // Division assignment operator
        Vector3& operator/=(float s)
        {
            float inv = 1.0f / s;
            x *= inv;
            y *= inv;
            z *= inv;
            return *this;
        }
        
        float length() const
        {
            return sqrt(x * x + y * y + z * z);
        }

        bool operator==(const Vector3& v) const { return x == v.x && y == v.y && z == v.z; }
        bool operator!=(const Vector3& v) const { return !(*this == v); }

        float dot(const Vector3& v) const { return x * v.x + y * v.y + z * v.z; }
        Vector3 cross(const Vector3& v) const;

        float magnitude() const { return std::sqrt(x * x + y * y + z * z); }
        float squaredMagnitude() const { return x * x + y * y + z * z; }

        void merge(const Vector3& v);

        Vector3 min(const Vector3& v) const;
        Vector3 max(const Vector3& v) const;

        float& operator[](int i) { return (&x)[i]; }
        const float& operator[](int i) const { return (&x)[i]; }

        int primaryAxis() const;
    };

    // Utility functions
    inline float inf() { return std::numeric_limits<float>::infinity(); }
    inline float fnan() { return std::numeric_limits<float>::quiet_NaN(); }
    inline bool fuzzyEq(float a, float b) { return std::abs(a - b) < 1e-6f; }
    inline bool fuzzyNe(float a, float b) { return !fuzzyEq(a, b); }
    inline bool fuzzyGt(float a, float b) { return a > b + 1e-6f; }
    inline float pi() { return 3.14159265358979323846f; }
}