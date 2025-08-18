// Vector3.cpp
#include "Vector3.h"
#include "Matrix3.h"

namespace G3D
{
    // Vector3 * Matrix3 - treats vector as row vector
    // This is equivalent to transpose(M) * v when v is a column vector
    Vector3 Vector3::operator*(const Matrix3& m) const {
        // This assumes row vector * matrix (row-major)
        return Vector3(
            x * m.get(0, 0) + y * m.get(1, 0) + z * m.get(2, 0),
            x * m.get(0, 1) + y * m.get(1, 1) + z * m.get(2, 1),
            x * m.get(0, 2) + y * m.get(1, 2) + z * m.get(2, 2)
        );
    }

    Vector3 Vector3::cross(const Vector3& v) const {
        return Vector3(
            y * v.z - z * v.y,
            z * v.x - x * v.z,
            x * v.y - y * v.x
        );
    }

    void Vector3::merge(const Vector3& v)
    {
        x = std::max(x, v.x);
        y = std::max(y, v.y);
        z = std::max(z, v.z);
    }

    Vector3 Vector3::min(const Vector3& v) const
    {
        return Vector3(
            std::min(x, v.x),
            std::min(y, v.y),
            std::min(z, v.z)
        );
    }

    Vector3 Vector3::max(const Vector3& v) const
    {
        return Vector3(
            std::max(x, v.x),
            std::max(y, v.y),
            std::max(z, v.z)
        );
    }

    int Vector3::primaryAxis() const
    {
        float ax = std::abs(x);
        float ay = std::abs(y);
        float az = std::abs(z);
        return (ax > ay) ? ((ax > az) ? 0 : 2) : ((ay > az) ? 1 : 2);
    }
}