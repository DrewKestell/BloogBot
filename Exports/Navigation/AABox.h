// AABox.h
#pragma once

#include "Vector3.h"

namespace G3D
{
    class AABox
    {
    private:
        Vector3 m_low, m_high;

    public:
        AABox();
        AABox(const Vector3& lo, const Vector3& hi);

        const Vector3& low() const { return m_low; }
        const Vector3& high() const { return m_high; }

        void set(const Vector3& lo, const Vector3& hi);
        void merge(const AABox& box);
        void merge(const Vector3& v);

        bool intersects(const AABox& other) const;
        bool contains(const Vector3& p) const;
        Vector3 corner(int i) const; 

        static AABox zero() { return AABox(Vector3::zero(), Vector3::zero()); }

        bool operator==(const AABox& b) const;
    };
}