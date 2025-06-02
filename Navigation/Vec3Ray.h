// Vec3Ray.h
#pragma once

#include <cmath>
#include <limits>
#include <algorithm>
#ifdef min
#undef min
#endif
#ifdef max
#undef max
#endif
#include <string>
#include <ostream>  // Required for std::ostream

inline float pi() { return 3.14159265358979323846f; }
inline float fnan() { return std::numeric_limits<float>::quiet_NaN(); }
inline float finf() { return std::numeric_limits<float>::infinity(); }
inline bool fuzzyEq(float a, float b, float epsilon = 1e-6f) { return std::fabs(a - b) < epsilon; }
inline bool fuzzyNe(float a, float b, float epsilon = 1e-6f) { return std::fabs(a - b) >= epsilon; }

struct Vec3 {
    float x, y, z;

    Vec3(); 
    Vec3(const float* arr);
    Vec3(float x_, float y_, float z_);

    bool operator==(const Vec3& rhs) const;
    bool operator!=(const Vec3& rhs) const;
    Vec3 operator*(const Vec3& rhs) const;
    Vec3 operator+(const Vec3& rhs) const;
    Vec3 operator-(const Vec3& rhs) const;
    Vec3 operator*(float s) const;
    Vec3 operator/(float scalar) const;
    Vec3& operator+=(const Vec3& rhs);
    Vec3& operator-=(const Vec3& rhs);
    Vec3& operator*=(float scalar);
    Vec3& operator/=(float scalar);

    float& operator[](int i);
    const float& operator[](int i) const; 
    

    float dot(const Vec3& rhs) const;
    Vec3 cross(const Vec3& rhs) const;
    float length() const;
    float magnitude() const;
    Vec3 normalized() const;

    int primaryAxis() const;

    Vec3 min(const Vec3& other) const;
    Vec3 max(const Vec3& other) const;
    static Vec3 zero();
    bool isWithin(const Vec3& min, const Vec3& max) const
    {
        return (x >= min.x && x <= max.x) &&
            (y >= min.y && y <= max.y) &&
            (z >= min.z && z <= max.z);
    }
    friend std::ostream& operator<<(std::ostream& os, const Vec3& v);
    friend Vec3 operator*(float scalar, const Vec3& vec);
};


inline std::ostream& operator<<(std::ostream& os, const Vec3& v)
{
    os << "(" << v.x << ", " << v.y << ", " << v.z << ")";
    return os;
}

struct Matrix3 {
    float m[3][3];

    Matrix3();
    static Matrix3 identity();
    static Matrix3 fromEulerAnglesZYX(float x, float y, float z);
    Matrix3 inverse() const;

    Vec3 operator*(const Vec3& v) const;
    friend Vec3 operator*(const Vec3& vec, const Matrix3& mat);
};

struct AABox;

struct Ray {
    Ray();
    Ray(const Vec3& o, const Vec3& d);

    static Ray fromOriginAndDirection(const Vec3& origin, const Vec3& direction);

    Vec3 origin() const;
    Vec3 direction() const;
    Vec3 pointAt(float t) const;
    Vec3 invDirection() const;

    bool intersectsAABB(const Vec3& min, const Vec3& max, float& tminOut) const;
    float intersectionTime(const AABox& box) const;

private:
    Vec3 _origin;
    Vec3 _direction;
};

struct AABox {
    Vec3 min;
    Vec3 max;

    AABox();
    AABox(const Vec3& min_, const Vec3& max_);

    void set(const Vec3& min_, const Vec3& max_);

    Vec3 center() const;
    Vec3 extent() const;

    Vec3& low();
    const Vec3& low() const;
    Vec3& high();
    const Vec3& high() const;

    bool contains(const Vec3& point) const;
    bool intersects(const Ray& ray, float& tminOut) const;
    void merge(const Vec3& point);
    void merge(const AABox& other);

    AABox operator+(const Vec3& offset) const;
    AABox& operator+=(const Vec3& offset);
};
namespace VMAP {
    inline bool readChunk(FILE* rf, char* dest, const char* compare, unsigned int len) {
        if (fread(dest, sizeof(char), len, rf) != len)
            return false;
        return memcmp(dest, compare, len) == 0;
    }
}