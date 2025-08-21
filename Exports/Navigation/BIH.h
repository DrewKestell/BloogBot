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

protected:
    struct buildData
    {
        uint32_t* indices;
        G3D::AABox* primBound;
        uint32_t numPrims;
        int maxPrims;
    };

    class BuildStats
    {
    private:
        int numNodes;
        int numLeaves;
        int sumObjects;
        int minObjects;
        int maxObjects;
        int sumDepth;
        int minDepth;
        int maxDepth;
        int numLeavesN[6];
        int numBVH2;

    public:
        BuildStats() :
            numNodes(0), numLeaves(0), sumObjects(0), minObjects(0x0FFFFFFF),
            maxObjects(0xFFFFFFFF), sumDepth(0), minDepth(0x0FFFFFFF),
            maxDepth(0xFFFFFFFF), numBVH2(0)
        {
            for (int& i : numLeavesN) i = 0;
        }

        void updateInner() { ++numNodes; }
        void updateBVH2() { ++numBVH2; }
        void updateLeaf(int depth, int n);
        void printStats();
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
    void buildHierarchy(std::vector<uint32_t>& tempTree, buildData& dat, BuildStats& stats);

    static void createNode(std::vector<uint32_t>& tempTree, int nodeIndex, uint32_t left, uint32_t right)
    {
        // write leaf node
        tempTree[nodeIndex + 0] = (3 << 30) | left;
        tempTree[nodeIndex + 1] = right - left + 1;
    }

    void subdivide(int left, int right, std::vector<uint32_t>& tempTree, buildData& dat,
        G3D::AABox& gridBox, G3D::AABox& nodeBox, int nodeIndex, int depth, BuildStats& stats);
};

#include "BIH.inl"