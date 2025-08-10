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
        int node;
        float tnear;
        float tfar;
    };

private:
    std::vector<uint32_t> tree;
    std::vector<uint32_t> objects;
    G3D::AABox bounds;

    void buildNode(const std::vector<G3D::AABox>& primitives,
        std::vector<uint32_t>& indices,
        uint32_t start, uint32_t end,
        uint32_t maxPrimsPerLeaf, int depth);

public:
    BIH();

    // Default copy and move operations work fine with vectors and AABox
    BIH(const BIH&) = default;
    BIH& operator=(const BIH&) = default;
    BIH(BIH&&) = default;
    BIH& operator=(BIH&&) = default;
    ~BIH() = default;

    void init_empty();
    bool readFromFile(FILE* rf);
    bool writeToFile(FILE* wf) const;

    uint32_t primCount() const { return objects.size(); }
    const G3D::AABox& getBounds() const { return bounds; }

    template<typename RayCallback>
    void intersectRay(const G3D::Ray& r, RayCallback& intersectCallback,
        float& maxDist, bool stopAtFirst = true, bool ignoreM2Model = false) const;

    template<typename IsectCallback>
    void intersectPoint(const G3D::Vector3& p, IsectCallback& intersectCallback) const;

    // Build functions for MMAP generator
    void build(const std::vector<G3D::AABox>& primitives, uint32_t maxPrimsPerLeaf = 4);
};

#include "BIH.inl"