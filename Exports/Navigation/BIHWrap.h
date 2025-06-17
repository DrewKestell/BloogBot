#pragma once

#include <vector>
#include <unordered_map>
#include <unordered_set>
#include <cassert>
#include "BIH.h"       // Your own BIH tree, not G3D.
#include "Vec3Ray.h"   // Your Vec3, Ray, AABox

template <typename K, typename V>
std::vector<K> getKeys(const std::unordered_map<K, V>& map) {
    std::vector<K> keys;
    keys.reserve(map.size());
    for (const auto& pair : map) {
        keys.push_back(pair.first);
    }
    return keys;
}

// --- BoundsTrait ---
template<class T, class BoundsFunc = BoundsTrait<T> >
class BIHWrap
{
    template<class RayCallback>
    struct MDLCallback
    {
        const T* const* objects;
        RayCallback& cb;
        unsigned int objectsSize;

        MDLCallback(RayCallback& callback, const T* const* objects_array, unsigned int objSize) : objects(objects_array), cb(callback), objectsSize(objSize) {}

        bool operator()(const Ray& r, unsigned int Idx, float& MaxDist, bool stopAtFirst, bool ignoreM2Model)
        {
            if (Idx >= objectsSize)
                return false;

            if (const T* obj = objects[Idx])
                return cb(r, *obj, MaxDist, stopAtFirst, ignoreM2Model);
            return false;
        }

        void operator()(const Vec3& p, unsigned int Idx)
        {
            if (Idx >= objectsSize)
                return;

            if (const T* obj = objects[Idx])
                cb(p, *obj);
        }
    };

    typedef std::vector<const T*> ObjArray;

    BIH m_tree;
    ObjArray m_objects;
    std::unordered_map<const T*, unsigned int> m_obj2Idx;
    std::unordered_set<const T*> m_objects_to_push;
    int unbalanced_times;

public:

    BIHWrap() : unbalanced_times(0) {}

    void insert(const T& obj)
    {
        ++unbalanced_times;
        m_objects_to_push.insert(&obj);
    }

    void remove(const T& obj)
    {
        ++unbalanced_times;
        unsigned int Idx = 0;
        const T* temp = nullptr;

        auto it = m_obj2Idx.find(&obj);
        if (it != m_obj2Idx.end())
        {
            // Get the index
            Idx = it->second;
            temp = it->first; // This is &obj
            m_obj2Idx.erase(it);
            if (Idx < m_objects.size()) // Safety!
                m_objects[Idx] = nullptr;
        }
        else
        {
            m_objects_to_push.erase(&obj); // std::unordered_set erase
        }
    }

    void balance()
    {
        if (unbalanced_times == 0)
            return;

        unbalanced_times = 0;
        m_objects.clear();
        for (const auto& kv : m_obj2Idx)
            m_objects.push_back(kv.first);

        for (const auto& ptr : m_objects_to_push)
            m_objects.push_back(ptr);

        m_tree.build(m_objects, BoundsFunc::getBounds2);
    }

    template<typename RayCallback>
    void intersectRay(const Ray& r, RayCallback& intersectCallback, float& maxDist, bool ignoreM2Model)
    {
        balance();
        MDLCallback<RayCallback> temp_cb(intersectCallback, m_objects.data(), m_objects.size());
        m_tree.intersectRay(r, temp_cb, maxDist, true, ignoreM2Model);
    }

    template<typename IsectCallback>
    void intersectPoint(const Vec3& p, IsectCallback& intersectCallback)
    {
        balance();
        MDLCallback<IsectCallback> temp_cb(intersectCallback, m_objects.data(), m_objects.size());
        m_tree.intersectPoint(p, temp_cb);
    }
};