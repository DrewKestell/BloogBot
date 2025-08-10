// AABox.cpp
#include "AABox.h"

namespace G3D
{
    AABox::AABox()
        : m_low(inf(), inf(), inf()),
        m_high(-inf(), -inf(), -inf())
    {
    }

    AABox::AABox(const Vector3& lo, const Vector3& hi)
        : m_low(lo), m_high(hi)
    {
    }

    void AABox::set(const Vector3& lo, const Vector3& hi)
    {
        m_low = lo;
        m_high = hi;
    }

    void AABox::merge(const AABox& box)
    {
        m_low = m_low.min(box.m_low);
        m_high = m_high.max(box.m_high);
    }

    void AABox::merge(const Vector3& v)
    {
        m_low = m_low.min(v);
        m_high = m_high.max(v);
    }

    bool AABox::contains(const Vector3& p) const
    {
        return p.x >= m_low.x && p.x <= m_high.x &&
            p.y >= m_low.y && p.y <= m_high.y &&
            p.z >= m_low.z && p.z <= m_high.z;
    }

    Vector3 AABox::corner(int i) const
    {
        return Vector3(
            (i & 1) ? m_high.x : m_low.x,
            (i & 2) ? m_high.y : m_low.y,
            (i & 4) ? m_high.z : m_low.z
        );
    }

    bool AABox::operator==(const AABox& b) const
    {
        return m_low == b.m_low && m_high == b.m_high;
    }
}