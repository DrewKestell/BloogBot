// BIH.h
#pragma once

#include <vector>
#include <cstdint>
#include "AABox.h"
#include "Ray.h"

class BIH
{
public:
    static const int MAX_STACK_SIZE = 64;

    struct StackNode
    {
        uint32_t node;
        float tnear;
        float tfar;
    };

public:
    BIH();

    std::vector<uint32_t> tree;
    std::vector<uint32_t> objects;
    G3D::AABox bounds;

    // Default copy and move operations work fine with vectors and AABox
    BIH(const BIH&) = default;
    BIH& operator=(const BIH&) = default;
    BIH(BIH&&) = default;
    BIH& operator=(BIH&&) = default;
    ~BIH() = default;

    // Build the BIH from primitives
    template<class BoundsFunc, class PrimArray>
    void build(PrimArray const& primitives, BoundsFunc& getBounds, uint32_t leafSize = 3, bool printStats = false);

    // File I/O
    bool readFromFile(FILE* rf);

    // Query methods
    uint32_t primCount() const { return objects.size(); }
    const G3D::AABox& getBounds() const { return bounds; }

    // Ray intersection
    template<typename RayCallback>
    void intersectRay(const G3D::Ray& r, RayCallback& intersectCallback,
        float& maxDist, bool stopAtFirst = true, bool ignoreM2Model = false) const;

    // Point intersection
    template<typename IsectCallback>
    void intersectPoint(const G3D::Vector3& p, IsectCallback& intersectCallback) const;

private:
    void init_empty();
};

#include "BIH.inl"