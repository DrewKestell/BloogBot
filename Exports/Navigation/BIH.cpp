// BIH.cpp - Fixed for vMaNGOS format
#include "BIH.h"
#include "VMapDefinitions.h"
#include <cstdio>
#include <algorithm>
#include <limits>
#include <iostream>

BIH::BIH()
{
    init_empty();
}

void BIH::init_empty()
{
    tree.clear();
    objects.clear();
    // create space for the first node
    tree.push_back(static_cast<uint32_t>(3 << 30)); // dummy leaf
    tree.insert(tree.end(), 2, 0);
}

bool BIH::readFromFile(FILE* rf)
{
    if (!rf)
    {
        std::cerr << "[BIH] ERROR: NULL file pointer" << std::endl;
        return false;
    }

    // Read bounding box (6 floats)
    float boundsData[6];
    if (fread(boundsData, sizeof(float), 6, rf) != 6)
    {
        std::cerr << "[BIH] ERROR: Failed to read bounding box" << std::endl;
        return false;
    }

    G3D::Vector3 lo(boundsData[0], boundsData[1], boundsData[2]);
    G3D::Vector3 hi(boundsData[3], boundsData[4], boundsData[5]);

    bounds = G3D::AABox(lo, hi);

    uint32_t treeSize;
    if (fread(&treeSize, sizeof(uint32_t), 1, rf) != 1)
    {
        std::cerr << "[BIH] ERROR: Failed to read tree size" << std::endl;
        return false;
    }

    // Read tree data
    tree.clear();
    tree.resize(treeSize);
    if (treeSize > 0 && fread(&tree[0], sizeof(uint32_t), treeSize, rf) != treeSize)
    {
        std::cerr << "[BIH] ERROR: Failed to read tree data" << std::endl;
        return false;
    }

    uint32_t count;
    if (fread(&count, sizeof(uint32_t), 1, rf) != 1)
    {
        std::cerr << "[BIH] ERROR: Failed to read object count" << std::endl;
        return false;
    }

    // Read object indices
    objects.clear();
    objects.resize(count);
    if (count > 0 && fread(&objects[0], sizeof(uint32_t), count, rf) != count)
    {
        std::cerr << "[BIH] ERROR: Failed to read object indices" << std::endl;
        return false;
    }

    return true;
}

void BIH::BuildStats::updateLeaf(int depth, int n)
{
    ++numLeaves;
    sumDepth += depth;
    if (depth < minDepth)
        minDepth = depth;
    if (depth > maxDepth)
        maxDepth = depth;

    sumObjects += n;
    if (n < minObjects)
        minObjects = n;
    if (n > maxObjects)
        maxObjects = n;

    if (n <= 5)
        ++numLeavesN[n];
}

void BIH::BuildStats::printStats()
{
    std::cout << "BIH Build Statistics:" << std::endl;
    std::cout << "  Nodes: " << numNodes << std::endl;
    std::cout << "  Leaves: " << numLeaves << std::endl;
    std::cout << "  BVH2 nodes: " << numBVH2 << std::endl;
    std::cout << "  Objects: Total=" << sumObjects
        << " Min=" << minObjects
        << " Max=" << maxObjects
        << " Avg=" << (float)sumObjects / numLeaves << std::endl;
    std::cout << "  Depth: Min=" << minDepth
        << " Max=" << maxDepth
        << " Avg=" << (float)sumDepth / numLeaves << std::endl;
    std::cout << "  Leaf distribution:";
    for (int i = 0; i <= 5; ++i)
        if (numLeavesN[i] > 0)
            std::cout << " [" << i << "]=" << numLeavesN[i];
    std::cout << std::endl;
}

void BIH::buildHierarchy(std::vector<uint32_t>& tempTree, buildData& dat, BuildStats& stats)
{
    // Implementation would go here - this is typically a complex recursive build
    // that creates the spatial hierarchy from the primitives
    // The actual implementation depends on your specific BIH variant

    // Reserve space for the tree
    tempTree.reserve(dat.numPrims * 2);

    // Create root node
    tempTree.push_back(0);
    tempTree.push_back(0);
    tempTree.push_back(0);

    // Start subdivision
    G3D::AABox gridBox = bounds;
    G3D::AABox nodeBox = bounds;

    subdivide(0, dat.numPrims - 1, tempTree, dat, gridBox, nodeBox, 0, 0, stats);
}

void BIH::subdivide(int left, int right, std::vector<uint32_t>& tempTree, buildData& dat,
    G3D::AABox& gridBox, G3D::AABox& nodeBox, int nodeIndex, int depth, BuildStats& stats)
{
    // This is where the actual BIH construction algorithm would be implemented
    // It would:
    // 1. Check if we should create a leaf (few enough primitives)
    // 2. Choose split axis and position
    // 3. Partition primitives
    // 4. Recursively build children
    // 5. Update node in tempTree with child pointers

    // For now, this is a placeholder that creates a simple leaf
    if ((right - left + 1) <= dat.maxPrims || depth > 40)
    {
        // Create leaf node
        createNode(tempTree, nodeIndex, left, right);
        stats.updateLeaf(depth, right - left + 1);
        return;
    }

    // The actual implementation would continue with splitting logic here
    stats.updateInner();

    // Placeholder: would normally partition and recurse
}