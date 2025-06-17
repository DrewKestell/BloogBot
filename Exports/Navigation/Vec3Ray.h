// Vec3Ray.h
#pragma once

#include <cmath>
#include <limits>
#include <algorithm>
#include <string>
#include <ostream>
#include <cstdio>
#include <cstring>

#ifdef min
#undef min
#endif
#ifdef max
#undef max
#endif

// ---------- Helpers ----------
inline float pi() { return 3.14159265358979323846f; }
inline float fnan() { return std::numeric_limits<float>::quiet_NaN(); }
inline float finf() { return std::numeric_limits<float>::infinity(); }
inline bool fuzzyEq(float a, float b, float epsilon = 1e-6f) { return std::fabs(a - b) < epsilon; }
inline bool fuzzyNe(float a, float b, float epsilon = 1e-6f) { return std::fabs(a - b) >= epsilon; }
inline bool fuzzyGt(float a, float b, float epsilon = 1e-6f) { return (a - b) > epsilon; }

// ---------- Vec3 ----------
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
    static Vec3 up();
    static Vec3 down();
    bool isWithin(const Vec3& min, const Vec3& max) const;

    friend std::ostream& operator<<(std::ostream& os, const Vec3& v);
    friend Vec3 operator*(float scalar, const Vec3& vec);
};

inline std::ostream& operator<<(std::ostream& os, const Vec3& v)
{
    os << "(" << v.x << ", " << v.y << ", " << v.z << ")";
    return os;
}

// ---------- Matrix3 ----------
struct Matrix3 {
    float m[3][3];

    Matrix3();
    static Matrix3 identity();
    static Matrix3 fromEulerAnglesZYX(float x, float y, float z);
    Matrix3 inverse() const;

    Vec3 operator*(const Vec3& v) const;
    friend Vec3 operator*(const Vec3& vec, const Matrix3& mat);
};

// ---------- Forward declare AABox ----------
struct AABox;

// ---------- Quat ----------
struct Quat {
    float w, x, y, z;

    Quat() : w(1), x(0), y(0), z(0) {}
    Quat(float w_, float x_, float y_, float z_) : w(w_), x(x_), y(y_), z(z_) {}
    Quat(float angleRadians, const Vec3& axis); // axis-angle ctor
    Quat(const Vec3& v) : w(0), x(v.x), y(v.y), z(v.z) {}

    static Quat identity() { return Quat(1, 0, 0, 0); }
    Quat operator*(const Quat& rhs) const;
    Vec3 operator*(const Vec3& v) const;
    Quat normalized() const;
    Quat inverse() const;
    Quat conjugate() const { return Quat(w, -x, -y, -z); }
    float dot(const Quat& rhs) const { return w * rhs.w + x * rhs.x + y * rhs.y + z * rhs.z; }
    static Quat slerp(const Quat& a, const Quat& b, float t);
    static Quat fromEuler(float pitch, float yaw, float roll);
    Matrix3 toMatrix3() const;

    friend std::ostream& operator<<(std::ostream& os, const Quat& q);
    Quat operator*(float s) const { return Quat(w * s, x * s, y * s, z * s); }
    friend Quat operator*(float s, const Quat& q) { return q * s; }
    Quat operator+(const Quat& rhs) const { return Quat(w + rhs.w, x + rhs.x, y + rhs.y, z + rhs.z); }
    Quat operator-(const Quat& rhs) const { return Quat(w - rhs.w, x - rhs.x, y - rhs.y, z - rhs.z); }
};

// ---------- Ray ----------
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

// ---------- AABox ----------
struct AABox {
    Vec3 min, max;

    AABox();
    AABox(const Vec3& min_, const Vec3& max_);

    void set(const Vec3& min_, const Vec3& max_);

    Vec3 center() const;
    Vec3 extent() const;

    Vec3& low() { return min; }
    const Vec3& low() const { return min; }
    Vec3& high() { return max; }
    const Vec3& high() const { return max; }

    bool contains(const Vec3& point) const;
    bool intersects(const Ray& ray, float& tminOut) const;
    void merge(const Vec3& point);
    void merge(const AABox& other);

    AABox operator+(const Vec3& offset) const;
    AABox& operator+=(const Vec3& offset);

    // G3D-style: get the i-th corner (bitmask: 0..7)
    Vec3 corner(int i) const {
        return Vec3(
            (i & 1) ? max.x : min.x,
            (i & 2) ? max.y : min.y,
            (i & 4) ? max.z : min.z
        );
    }
};

// ---------- MeshTriangle ----------
struct MeshTriangle {
    unsigned int idx0, idx1, idx2;
    MeshTriangle() : idx0(0), idx1(0), idx2(0) {}
    MeshTriangle(unsigned int a, unsigned int b, unsigned int c)
        : idx0(a), idx1(b), idx2(c) {
    }
};

// ---------- BoundsTrait: Used for BIH spatial indexing ----------
template<typename T>
struct BoundsTrait {
    static void getBounds(const T& obj, AABox& out) { out = obj.GetBound(); }
    static void getBounds2(const T* obj, AABox& out) { out = obj->GetBound(); }
};

// --- Specializations for core types ---
template<> struct BoundsTrait<Vec3> {
    static void getBounds(const Vec3& v, AABox& out) { out = AABox(v, v); }
    static void getBounds2(const Vec3* v, AABox& out) { out = AABox(*v, *v); }
};
template<> struct BoundsTrait<AABox> {
    static void getBounds(const AABox& b, AABox& out) { out = b; }
    static void getBounds2(const AABox* b, AABox& out) { out = *b; }
};

