using System;

namespace VMAP
{
    /// <summary>3 × 3 rotation (row-major) used for model transforms.</summary>
    public struct Matrix3
    {
        public float m00, m01, m02;
        public float m10, m11, m12;
        public float m20, m21, m22;

        public static Matrix3 FromEulerAnglesZYX(float rz, float ry, float rx)
        {
            float cz = MathF.Cos(rz), sz = MathF.Sin(rz);
            float cy = MathF.Cos(ry), sy = MathF.Sin(ry);
            float cx = MathF.Cos(rx), sx = MathF.Sin(rx);

            Matrix3 Rz = new()
            {
                m00 = cz,
                m01 = -sz,
                m02 = 0f,
                m10 = sz,
                m11 = cz,
                m12 = 0f,
                m20 = 0f,
                m21 = 0f,
                m22 = 1f
            };
            Matrix3 Ry = new()
            {
                m00 = cy,
                m01 = 0f,
                m02 = sy,
                m10 = 0f,
                m11 = 1f,
                m12 = 0f,
                m20 = -sy,
                m21 = 0f,
                m22 = cy
            };
            Matrix3 Rx = new()
            {
                m00 = 1f,
                m01 = 0f,
                m02 = 0f,
                m10 = 0f,
                m11 = cx,
                m12 = -sx,
                m20 = 0f,
                m21 = sx,
                m22 = cx
            };
            return Rz * (Ry * Rx);
        }

        public Matrix3 Inverse() => new()
        {
            m00 = m00,
            m01 = m10,
            m02 = m20,
            m10 = m01,
            m11 = m11,
            m12 = m21,
            m20 = m02,
            m21 = m12,
            m22 = m22
        };

        public static Matrix3 operator *(Matrix3 a, Matrix3 b)
        {
            Matrix3 r;
            r.m00 = a.m00 * b.m00 + a.m01 * b.m10 + a.m02 * b.m20;
            r.m01 = a.m00 * b.m01 + a.m01 * b.m11 + a.m02 * b.m21;
            r.m02 = a.m00 * b.m02 + a.m01 * b.m12 + a.m02 * b.m22;

            r.m10 = a.m10 * b.m00 + a.m11 * b.m10 + a.m12 * b.m20;
            r.m11 = a.m10 * b.m01 + a.m11 * b.m11 + a.m12 * b.m21;
            r.m12 = a.m10 * b.m02 + a.m11 * b.m12 + a.m12 * b.m22;

            r.m20 = a.m20 * b.m00 + a.m21 * b.m10 + a.m22 * b.m20;
            r.m21 = a.m20 * b.m01 + a.m21 * b.m11 + a.m22 * b.m21;
            r.m22 = a.m20 * b.m02 + a.m21 * b.m12 + a.m22 * b.m22;
            return r;
        }

        public static Vector3 operator *(Matrix3 m, Vector3 v) =>
            new(m.m00 * v.x + m.m01 * v.y + m.m02 * v.z,
                m.m10 * v.x + m.m11 * v.y + m.m12 * v.z,
                m.m20 * v.x + m.m21 * v.y + m.m22 * v.z);
    }
}
