#include "Vec3Ray.h"

Vec3::Vec3() : x(0), y(0), z(0) {}
Vec3::Vec3(float x_, float y_, float z_) : x(x_), y(y_), z(z_) {}
Vec3::Vec3(const float* arr)
    : x(arr[0]), y(arr[1]), z(arr[2]) {
}


bool Vec3::operator==(const Vec3& rhs) const { return fuzzyEq(x, rhs.x) && fuzzyEq(y, rhs.y) && fuzzyEq(z, rhs.z); }
bool Vec3::operator!=(const Vec3& rhs) const { return !(*this == rhs); }
Vec3 Vec3::operator*(const Vec3& rhs) const { return Vec3(x * rhs.x, y * rhs.y, z * rhs.z); }
Vec3 Vec3::operator+(const Vec3& rhs) const { return Vec3(x + rhs.x, y + rhs.y, z + rhs.z); }
Vec3 Vec3::operator-(const Vec3& rhs) const { return Vec3(x - rhs.x, y - rhs.y, z - rhs.z); }
Vec3 Vec3::operator*(float s) const { return Vec3(x * s, y * s, z * s); }
Vec3 Vec3::operator/(float s) const { return Vec3(x / s, y / s, z / s); }

Vec3& Vec3::operator+=(const Vec3& rhs) { x += rhs.x; y += rhs.y; z += rhs.z; return *this; }
Vec3& Vec3::operator-=(const Vec3& rhs) { x -= rhs.x; y -= rhs.y; z -= rhs.z; return *this; }
Vec3& Vec3::operator*=(float s) { x *= s; y *= s; z *= s; return *this; }
Vec3& Vec3::operator/=(float s) { x /= s; y /= s; z /= s; return *this; }

float& Vec3::operator[](int i) { return *((&x) + i); }
const float& Vec3::operator[](int i) const { return *((&x) + i); }

float Vec3::dot(const Vec3& rhs) const { return x * rhs.x + y * rhs.y + z * rhs.z; }
Vec3 Vec3::cross(const Vec3& rhs) const {
    return Vec3(y * rhs.z - z * rhs.y, z * rhs.x - x * rhs.z, x * rhs.y - y * rhs.x);
}
float Vec3::length() const { return std::sqrt(x * x + y * y + z * z); }
float Vec3::magnitude() const { return length(); }
Vec3 Vec3::normalized() const {
    float len = length();
    return (len > std::numeric_limits<float>::epsilon()) ? (*this) * (1.0f / len) : Vec3(0, 0, 0);
}
int Vec3::primaryAxis() const {
    if (x >= y && x >= z) return 0;
    if (y >= z) return 1;
    return 2;
}
Vec3 Vec3::min(const Vec3& other) const { return Vec3(std::min(x, other.x), std::min(y, other.y), std::min(z, other.z)); }
Vec3 Vec3::max(const Vec3& other) const { return Vec3(std::max(x, other.x), std::max(y, other.y), std::max(z, other.z)); }
Vec3 Vec3::zero() { return Vec3(0, 0, 0); }
Vec3 operator*(float s, const Vec3& v) { return Vec3(s * v.x, s * v.y, s * v.z); }

Matrix3::Matrix3() {
    for (int i = 0; i < 3; ++i)
        for (int j = 0; j < 3; ++j)
            m[i][j] = (i == j) ? 1.0f : 0.0f;
}
Matrix3 Matrix3::identity() { return Matrix3(); }
Matrix3 Matrix3::fromEulerAnglesZYX(float x, float y, float z) {
    float cx = std::cos(x), sx = std::sin(x);
    float cy = std::cos(y), sy = std::sin(y);
    float cz = std::cos(z), sz = std::sin(z);
    Matrix3 m;
    m.m[0][0] = cz * cy;              m.m[0][1] = cz * sy * sx - sz * cx;    m.m[0][2] = cz * sy * cx + sz * sx;
    m.m[1][0] = sz * cy;              m.m[1][1] = sz * sy * sx + cz * cx;    m.m[1][2] = sz * sy * cx - cz * sx;
    m.m[2][0] = -sy;                  m.m[2][1] = cy * sx;                   m.m[2][2] = cy * cx;
    return m;
}
Matrix3 Matrix3::inverse() const {
    Matrix3 inv;
    float det = m[0][0] * (m[1][1] * m[2][2] - m[1][2] * m[2][1]) -
        m[0][1] * (m[1][0] * m[2][2] - m[1][2] * m[2][0]) +
        m[0][2] * (m[1][0] * m[2][1] - m[1][1] * m[2][0]);

    if (std::abs(det) < std::numeric_limits<float>::epsilon()) return Matrix3();

    float invDet = 1.0f / det;
    inv.m[0][0] = (m[1][1] * m[2][2] - m[1][2] * m[2][1]) * invDet;
    inv.m[0][1] = -(m[0][1] * m[2][2] - m[0][2] * m[2][1]) * invDet;
    inv.m[0][2] = (m[0][1] * m[1][2] - m[0][2] * m[1][1]) * invDet;
    inv.m[1][0] = -(m[1][0] * m[2][2] - m[1][2] * m[2][0]) * invDet;
    inv.m[1][1] = (m[0][0] * m[2][2] - m[0][2] * m[2][0]) * invDet;
    inv.m[1][2] = -(m[0][0] * m[1][2] - m[0][2] * m[1][0]) * invDet;
    inv.m[2][0] = (m[1][0] * m[2][1] - m[1][1] * m[2][0]) * invDet;
    inv.m[2][1] = -(m[0][0] * m[2][1] - m[0][1] * m[2][0]) * invDet;
    inv.m[2][2] = (m[0][0] * m[1][1] - m[0][1] * m[1][0]) * invDet;
    return inv;
}
Vec3 Matrix3::operator*(const Vec3& v) const {
    return Vec3(
        m[0][0] * v.x + m[0][1] * v.y + m[0][2] * v.z,
        m[1][0] * v.x + m[1][1] * v.y + m[1][2] * v.z,
        m[2][0] * v.x + m[2][1] * v.y + m[2][2] * v.z
    );
}
Vec3 operator*(const Vec3& vec, const Matrix3& mat) {
    return Vec3(
        vec.x * mat.m[0][0] + vec.y * mat.m[1][0] + vec.z * mat.m[2][0],
        vec.x * mat.m[0][1] + vec.y * mat.m[1][1] + vec.z * mat.m[2][1],
        vec.x * mat.m[0][2] + vec.y * mat.m[1][2] + vec.z * mat.m[2][2]
    );
}

Ray::Ray() {}
Ray::Ray(const Vec3& o, const Vec3& d) : _origin(o), _direction(d.normalized()) {}
Ray Ray::fromOriginAndDirection(const Vec3& origin, const Vec3& direction) {
    return Ray(origin, direction.normalized());
}
Vec3 Ray::origin() const { return _origin; }
Vec3 Ray::direction() const { return _direction; }
Vec3 Ray::pointAt(float t) const { return _origin + _direction * t; }
Vec3 Ray::invDirection() const {
    return Vec3(
        _direction.x != 0.0f ? 1.0f / _direction.x : finf(),
        _direction.y != 0.0f ? 1.0f / _direction.y : finf(),
        _direction.z != 0.0f ? 1.0f / _direction.z : finf()
    );
}
bool Ray::intersectsAABB(const Vec3& min, const Vec3& max, float& tminOut) const {
    float tmin = (min.x - _origin.x) / _direction.x;
    float tmax = (max.x - _origin.x) / _direction.x;
    if (tmin > tmax) std::swap(tmin, tmax);

    float tymin = (min.y - _origin.y) / _direction.y;
    float tymax = (max.y - _origin.y) / _direction.y;
    if (tymin > tymax) std::swap(tymin, tymax);

    if ((tmin > tymax) || (tymin > tmax)) return false;
    if (tymin > tmin) tmin = tymin;
    if (tymax < tmax) tmax = tymax;

    float tzmin = (min.z - _origin.z) / _direction.z;
    float tzmax = (max.z - _origin.z) / _direction.z;
    if (tzmin > tzmax) std::swap(tzmin, tzmax);

    if ((tmin > tzmax) || (tzmin > tmax)) return false;
    if (tzmin > tmin) tmin = tzmin;
    if (tzmax < tmax) tmax = tzmax;

    tminOut = tmin;
    return true;
}
float Ray::intersectionTime(const AABox& box) const {
    float t;
    return intersectsAABB(box.min, box.max, t) ? t : finf();
}

AABox::AABox() : min(), max() {}
AABox::AABox(const Vec3& min_, const Vec3& max_) : min(min_), max(max_) {}
void AABox::set(const Vec3& min_, const Vec3& max_) { min = min_; max = max_; }
Vec3 AABox::center() const { return (min + max) * 0.5f; }
Vec3 AABox::extent() const { return (max - min) * 0.5f; }
Vec3& AABox::low() { return min; }
const Vec3& AABox::low() const { return min; }
Vec3& AABox::high() { return max; }
const Vec3& AABox::high() const { return max; }
bool AABox::contains(const Vec3& p) const {
    return (p.x >= min.x && p.x <= max.x &&
        p.y >= min.y && p.y <= max.y &&
        p.z >= min.z && p.z <= max.z);
}

bool AABox::intersects(const Ray& ray, float& tminOut) const {
    return ray.intersectsAABB(min, max, tminOut);
}

void AABox::merge(const Vec3& point)
{
    min.x = std::min(min.x, point.x);
    min.y = std::min(min.y, point.y);
    min.z = std::min(min.z, point.z);

    max.x = std::max(max.x, point.x);
    max.y = std::max(max.y, point.y);
    max.z = std::max(max.z, point.z);
}

void AABox::merge(const AABox& other) {
    min = min.min(other.min);
    max = max.max(other.max);
}

AABox AABox::operator+(const Vec3& offset) const {
    return AABox(min + offset, max + offset);
}

AABox& AABox::operator+=(const Vec3& offset) {
    min += offset;
    max += offset;
    return *this;
}