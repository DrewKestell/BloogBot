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

    Matrix3 Matrix3::operator*(const Matrix3& other) const
    {
        Matrix3 result;
        for (int i = 0; i < 3; ++i)
        {
            for (int j = 0; j < 3; ++j)
            {
                result.m[i][j] = 0;
                for (int k = 0; k < 3; ++k)
                {
                    result.m[i][j] += m[i][k] * other.m[k][j];
                }
            }
        }
        return result;
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
        // Z rotation matrix
        Matrix3 zMat;
        float cz = std::cos(z), sz = std::sin(z);
        zMat.m[0][0] = cz;  zMat.m[0][1] = -sz; zMat.m[0][2] = 0;
        zMat.m[1][0] = sz;  zMat.m[1][1] = cz;  zMat.m[1][2] = 0;
        zMat.m[2][0] = 0;   zMat.m[2][1] = 0;   zMat.m[2][2] = 1;

        // Y rotation matrix  
        Matrix3 yMat;
        float cy = std::cos(y), sy = std::sin(y);
        yMat.m[0][0] = cy;  yMat.m[0][1] = 0;  yMat.m[0][2] = sy;
        yMat.m[1][0] = 0;   yMat.m[1][1] = 1;  yMat.m[1][2] = 0;
        yMat.m[2][0] = -sy; yMat.m[2][1] = 0;  yMat.m[2][2] = cy;

        // X rotation matrix
        Matrix3 xMat;
        float cx = std::cos(x), sx = std::sin(x);
        xMat.m[0][0] = 1;  xMat.m[0][1] = 0;   xMat.m[0][2] = 0;
        xMat.m[1][0] = 0;  xMat.m[1][1] = cx;  xMat.m[1][2] = -sx;
        xMat.m[2][0] = 0;  xMat.m[2][1] = sx;  xMat.m[2][2] = cx;

        // Return Z * (Y * X)
        return zMat * (yMat * xMat);
    }
}