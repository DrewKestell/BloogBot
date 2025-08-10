// Matrix3.cpp
#include "Matrix3.h"
#include <cmath>

namespace G3D
{
    Matrix3::Matrix3()
    {
        for (int i = 0; i < 3; ++i)
            for (int j = 0; j < 3; ++j)
                m[i][j] = (i == j) ? 1.0f : 0.0f;
    }

    Vector3 Matrix3::operator*(const Vector3& v) const
    {
        return Vector3(
            m[0][0] * v.x + m[0][1] * v.y + m[0][2] * v.z,
            m[1][0] * v.x + m[1][1] * v.y + m[1][2] * v.z,
            m[2][0] * v.x + m[2][1] * v.y + m[2][2] * v.z
        );
    }

    Matrix3 Matrix3::inverse() const
    {
        Matrix3 result;
        float det = m[0][0] * (m[1][1] * m[2][2] - m[1][2] * m[2][1]) -
            m[0][1] * (m[1][0] * m[2][2] - m[1][2] * m[2][0]) +
            m[0][2] * (m[1][0] * m[2][1] - m[1][1] * m[2][0]);

        if (std::abs(det) < 1e-6f) return result;

        float invDet = 1.0f / det;

        result.m[0][0] = (m[1][1] * m[2][2] - m[1][2] * m[2][1]) * invDet;
        result.m[0][1] = (m[0][2] * m[2][1] - m[0][1] * m[2][2]) * invDet;
        result.m[0][2] = (m[0][1] * m[1][2] - m[0][2] * m[1][1]) * invDet;

        result.m[1][0] = (m[1][2] * m[2][0] - m[1][0] * m[2][2]) * invDet;
        result.m[1][1] = (m[0][0] * m[2][2] - m[0][2] * m[2][0]) * invDet;
        result.m[1][2] = (m[0][2] * m[1][0] - m[0][0] * m[1][2]) * invDet;

        result.m[2][0] = (m[1][0] * m[2][1] - m[1][1] * m[2][0]) * invDet;
        result.m[2][1] = (m[0][1] * m[2][0] - m[0][0] * m[2][1]) * invDet;
        result.m[2][2] = (m[0][0] * m[1][1] - m[0][1] * m[1][0]) * invDet;

        return result;
    }

    Matrix3 Matrix3::fromEulerAnglesZYX(float z, float y, float x)
    {
        Matrix3 result;
        float cx = std::cos(x), sx = std::sin(x);
        float cy = std::cos(y), sy = std::sin(y);
        float cz = std::cos(z), sz = std::sin(z);

        result.m[0][0] = cy * cz;
        result.m[0][1] = -cy * sz;
        result.m[0][2] = sy;

        result.m[1][0] = sx * sy * cz + cx * sz;
        result.m[1][1] = -sx * sy * sz + cx * cz;
        result.m[1][2] = -sx * cy;

        result.m[2][0] = -cx * sy * cz + sx * sz;
        result.m[2][1] = cx * sy * sz + sx * cz;
        result.m[2][2] = cx * cy;

        return result;
    }
}