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

    Vector3 AABox::corner(int index) const
    {
        Vector3 v;
        switch (index)
        {
        case 0: v = Vector3(m_low.x, m_low.y, m_high.z); break;
        case 1: v = Vector3(m_high.x, m_low.y, m_high.z); break;
        case 2: v = Vector3(m_high.x, m_high.y, m_high.z); break;
        case 3: v = Vector3(m_low.x, m_high.y, m_high.z); break;
        case 4: v = Vector3(m_low.x, m_low.y, m_low.z);  break;
        case 5: v = Vector3(m_high.x, m_low.y, m_low.z);  break;
        case 6: v = Vector3(m_high.x, m_high.y, m_low.z);  break;
        case 7: v = Vector3(m_low.x, m_high.y, m_low.z);  break;
        default:
            // Invalid corner index
            return Vector3::zero();
        }
        return v;
    }

    bool AABox::intersects(const AABox& other) const
    {
        // Must be overlap along all three axes
        for (int a = 0; a < 3; ++a) {
            if ((m_low[a] > other.m_high[a]) || (m_high[a] < other.m_low[a])) {
                return false;
            }
        }
        return true;
    }

    bool AABox::operator==(const AABox& b) const
    {
        return m_low == b.m_low && m_high == b.m_high;
    }
}