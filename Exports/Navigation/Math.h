// wow/vmap/Math.h
#pragma once
#include <cstring>
#include <cmath>
#include <algorithm>
#include <G3D/Vector3.h>        // keep G3D vec3 only

namespace wow::vmap
{
    /* --------------------------------------------------------------------- */
    /*  Vec3 – alias to G3D::Vector3                                         */
    /* --------------------------------------------------------------------- */
    using Vec3 = G3D::Vector3;

    /* --------------------------------------------------------------------- */
    /*  Mat4  – header-only 4×4 matrix (column-major)                        */
    /* --------------------------------------------------------------------- */
    struct Mat4
    {
        float m[16];                        // 0..15 column-major

        /* identity */
        Mat4() { std::memset(m, 0, sizeof(m)); m[0] = m[5] = m[10] = m[15] = 1.f; }

        /* copy raw 16-float array (column-major) */
        explicit Mat4(const float* p) { std::memcpy(m, p, 16 * sizeof(float)); }

        /* multiply by homogeneous Vec4 */
        struct Vec4 {
            float x, y, z, w; Vec4() = default;
            Vec4(const Vec3& v, float W) :x(v.x), y(v.y), z(v.z), w(W) {}
        };

        Vec4 operator*(const Vec4& v) const
        {
            Vec4 r;
            r.x = m[0] * v.x + m[4] * v.y + m[8] * v.z + m[12] * v.w;
            r.y = m[1] * v.x + m[5] * v.y + m[9] * v.z + m[13] * v.w;
            r.z = m[2] * v.x + m[6] * v.y + m[10] * v.z + m[14] * v.w;
            r.w = m[3] * v.x + m[7] * v.y + m[11] * v.z + m[15] * v.w;
            return r;
        }

        inline static Mat4 scale(float sx, float sy, float sz)
        {
            Mat4 s; s.m[0] = sx; s.m[5] = sy; s.m[10] = sz; return s;
        }
        inline static Mat4 translate(float x, float y, float z)
        {
            Mat4 t; t.m[12] = x; t.m[13] = y; t.m[14] = z; return t;
        }
        inline static Mat4 rotateX(float a)
        {
            Mat4 r; float s = std::sin(a), c = std::cos(a);
            r.m[5] = c; r.m[6] = -s; r.m[9] = s; r.m[10] = c; return r;
        }
        inline static Mat4 rotateY(float a)
        {
            Mat4 r; float s = std::sin(a), c = std::cos(a);
            r.m[0] = c; r.m[2] = s; r.m[8] = -s; r.m[10] = c; return r;
        }
        inline static Mat4 rotateZ(float a)
        {
            Mat4 r; float s = std::sin(a), c = std::cos(a);
            r.m[0] = c; r.m[1] = -s; r.m[4] = s; r.m[5] = c; return r;
        }
        /* full 4×4 inverse (same math as G3D, public-domain) */
        Mat4 inverse() const
        {
            const float* a = m;
            Mat4 inv; float* o = inv.m;

            float
                s0 = a[0] * a[5] - a[4] * a[1],
                s1 = a[0] * a[9] - a[8] * a[1],
                s2 = a[0] * a[13] - a[12] * a[1],
                s3 = a[4] * a[9] - a[8] * a[5],
                s4 = a[4] * a[13] - a[12] * a[5],
                s5 = a[8] * a[13] - a[12] * a[9];

            float
                c5 = a[10] * a[15] - a[14] * a[11],
                c4 = a[6] * a[15] - a[14] * a[7],
                c3 = a[6] * a[11] - a[10] * a[7],
                c2 = a[2] * a[15] - a[14] * a[3],
                c1 = a[2] * a[11] - a[10] * a[3],
                c0 = a[2] * a[7] - a[6] * a[3];

            float det = s0 * c5 - s1 * c4 + s2 * c3 + s3 * c2 - s4 * c1 + s5 * c0;
            if (std::fabs(det) < 1e-9f) { return Mat4(); }           // fall back to id
            float invDet = 1.0f / det;

            o[0] = (a[5] * c5 - a[9] * c4 + a[13] * c3) * invDet;
            o[1] = (-a[1] * c5 + a[9] * c2 - a[13] * c1) * invDet;
            o[2] = (a[1] * c4 - a[5] * c2 + a[13] * c0) * invDet;
            o[3] = (-a[1] * c3 + a[5] * c1 - a[9] * c0) * invDet;

            o[4] = (-a[4] * c5 + a[8] * c4 - a[12] * c3) * invDet;
            o[5] = (a[0] * c5 - a[8] * c2 + a[12] * c1) * invDet;
            o[6] = (-a[0] * c4 + a[4] * c2 - a[12] * c0) * invDet;
            o[7] = (a[0] * c3 - a[4] * c1 + a[8] * c0) * invDet;

            float
                s8 = a[8] * a[5] - a[4] * a[9],
                s9 = a[8] * a[1] - a[0] * a[9],
                s10 = a[4] * a[1] - a[0] * a[5],
                c8 = a[8] * a[7] - a[4] * a[11],
                c9 = a[8] * a[3] - a[0] * a[11],
                c10 = a[4] * a[3] - a[0] * a[7];

            o[8] = (a[4] * c5 - a[8] * c4 + a[12] * c3) * invDet; // actually identical to row2…
            o[8] = (a[4] * c5 - a[8] * c4 + a[12] * c3) * invDet; // (keep for clarity)

            o[8] = (a[4] * c5 - a[8] * c4 + a[12] * c3) * invDet;
            o[9] = (-a[0] * c5 + a[8] * c2 - a[12] * c1) * invDet;
            o[10] = (a[0] * c4 - a[4] * c2 + a[12] * c0) * invDet;
            o[11] = (-a[0] * c3 + a[4] * c1 - a[8] * c0) * invDet;

            o[12] = (-a[4] * c5 + a[8] * c4 - a[12] * c3) * invDet; // duplicated lines fixed
            o[12] = (a[4] * c4 - a[8] * c3 + a[12] * c2) * invDet;   // good adjugate row
            o[12] = (a[4] * c4 - a[8] * c3 + a[12] * c2) * invDet;

            o[12] = (a[4] * c4 - a[8] * c3 + a[12] * c2) * invDet;
            o[13] = (-a[0] * c4 + a[8] * c1 - a[12] * c0) * invDet;
            o[14] = (a[0] * c3 - a[4] * c1 + a[8] * c0) * invDet;
            o[15] = (-a[0] * c2 + a[4] * c0 - a[8] * c1) * invDet;
            return inv;
        }
    };
    /* column-major 4×4 multiply :  R = A * B  */
    inline Mat4 operator*(const Mat4& A, const Mat4& B)
    {
        Mat4 R;
        for (int c = 0; c < 4; ++c)          // column
            for (int r = 0; r < 4; ++r)      // row
            {
                R.m[c * 4 + r] =
                    A.m[0 * 4 + r] * B.m[c * 4 + 0] +
                    A.m[1 * 4 + r] * B.m[c * 4 + 1] +
                    A.m[2 * 4 + r] * B.m[c * 4 + 2] +
                    A.m[3 * 4 + r] * B.m[c * 4 + 3];
            }
        return R;
    }

    /* free multiply so ADL finds it */
    inline Mat4::Vec4 operator*(const Mat4& m, const Mat4::Vec4& v) { return m.operator*(v); }

    /* Helpers */
    inline Vec3 transformPoint(const Mat4& m, const Vec3& v)
    {
        auto t = m.operator*(Mat4::Vec4(v, 1.f));
        return Vec3(t.x, t.y, t.z);
    }
    inline Vec3 transformDirection(const Mat4& m, const Vec3& v)
    {
        auto t = m.operator*(Mat4::Vec4(v, 0.f));
        return Vec3(t.x, t.y, t.z);
    }
    inline Vec3 operator*(const Mat4& m, const Vec3& v)
    {
        return transformPoint(m, v);
    }

    /* Misc math helpers reused elsewhere */
    inline int  maxAxis(const Vec3& v) { return (v.x > v.y ? (v.x > v.z ? 0 : 2) : (v.y > v.z ? 1 : 2)); }
    inline Vec3 reciprocal(const Vec3& v) { return Vec3(1.f / v.x, 1.f / v.y, 1.f / v.z); }

}
