// Matrix3.h
#pragma once

#include "Vector3.h"

namespace G3D
{
    class Matrix3
    {
    private:
        float m[3][3];

    public:
        Matrix3();

        // Access to matrix elements for Vector3 * Matrix3 operation
        float get(int i, int j) const { return m[i][j]; }

        Matrix3 operator*(const Matrix3& other) const;
        Vector3 operator*(const Vector3& v) const;
        Matrix3 inverse() const;

        static Matrix3 fromEulerAnglesZYX(float z, float y, float x);
    };
}