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
    // Initialize with an empty leaf node
    tree.push_back(static_cast<uint32_t>(3 << 30)); // axis = 3 (leaf)
    tree.push_back(0); // no objects
    tree.push_back(0); // reserved

    // Set infinite bounds for empty tree
    bounds = G3D::AABox(
        G3D::Vector3(std::numeric_limits<float>::max(),
            std::numeric_limits<float>::max(),
            std::numeric_limits<float>::max()),
        G3D::Vector3(-std::numeric_limits<float>::max(),
            -std::numeric_limits<float>::max(),
            -std::numeric_limits<float>::max())
    );
}

bool BIH::readFromFile(FILE* rf)
{
    if (!rf)
    {
        std::cerr << "[BIH] ERROR: NULL file pointer" << std::endl;
        return false;
    }

    try
    {
        // Read bounding box (6 floats)
        float boundsData[6];
        if (fread(boundsData, sizeof(float), 6, rf) != 6)
        {
            std::cerr << "[BIH] ERROR: Failed to read bounding box" << std::endl;
            return false;
        }

        G3D::Vector3 lo(boundsData[0], boundsData[1], boundsData[2]);
        G3D::Vector3 hi(boundsData[3], boundsData[4], boundsData[5]);

        // Validate bounding box
        if (std::isnan(lo.x) || std::isnan(lo.y) || std::isnan(lo.z) ||
            std::isnan(hi.x) || std::isnan(hi.y) || std::isnan(hi.z))
        {
            std::cerr << "[BIH] ERROR: Invalid bounding box (NaN values)" << std::endl;
            return false;
        }

        bounds = G3D::AABox(lo, hi);

        // FIXED: Read tree size FIRST (matching reference format)
        uint32_t treeSize;
        if (fread(&treeSize, sizeof(uint32_t), 1, rf) != 1)
        {
            std::cerr << "[BIH] ERROR: Failed to read tree size" << std::endl;
            return false;
        }

        if (treeSize > 10000000)  // Sanity check
        {
            std::cerr << "[BIH] ERROR: Tree size too large: " << treeSize << std::endl;
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

        // FIXED: Read object count AFTER tree (matching reference format)
        uint32_t count;
        if (fread(&count, sizeof(uint32_t), 1, rf) != 1)
        {
            std::cerr << "[BIH] ERROR: Failed to read object count" << std::endl;
            return false;
        }

        if (count > 10000000)  // Sanity check
        {
            std::cerr << "[BIH] ERROR: Object count too large: " << count << std::endl;
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
    catch (const std::exception& e)
    {
        std::cerr << "[BIH] ERROR: Exception in readFromFile: " << e.what() << std::endl;
        tree.clear();
        objects.clear();
        return false;
    }
}

bool BIH::writeToFile(FILE* wf) const
{
    // Write bounding box as 6 floats (vMaNGOS format)
    G3D::Vector3 lo = bounds.low();
    G3D::Vector3 hi = bounds.high();
    float boundsData[6] = { lo.x, lo.y, lo.z, hi.x, hi.y, hi.z };

    if (fwrite(boundsData, sizeof(float), 6, wf) != 6)
        return false;

    // Write primitive count
    uint32_t primCount = objects.size();
    if (fwrite(&primCount, sizeof(uint32_t), 1, wf) != 1)
        return false;

    // Note: We don't write node count - vMaNGOS format doesn't have it

    // Write tree nodes
    if (tree.size() > 0 && fwrite(&tree[0], sizeof(uint32_t), tree.size(), wf) != tree.size())
        return false;

    // Write object indices
    if (objects.size() > 0 && fwrite(&objects[0], sizeof(uint32_t), objects.size(), wf) != objects.size())
        return false;

    return true;
}

void BIH::build(const std::vector<G3D::AABox>& primitives, uint32_t maxPrimsPerLeaf)
{
    if (primitives.empty())
    {
        init_empty();
        return;
    }

    // Calculate overall bounds
    bounds = primitives[0];
    for (size_t i = 1; i < primitives.size(); ++i)
    {
        bounds.merge(primitives[i]);
    }

    // Initialize object indices
    objects.resize(primitives.size());
    for (size_t i = 0; i < primitives.size(); ++i)
    {
        objects[i] = static_cast<uint32_t>(i);
    }

    // Build tree recursively
    tree.clear();
    std::vector<uint32_t> tempObjects = objects;
    buildNode(primitives, tempObjects, 0, static_cast<uint32_t>(primitives.size()), maxPrimsPerLeaf, 0);
}

void BIH::buildNode(const std::vector<G3D::AABox>& primitives,
    std::vector<uint32_t>& indices,
    uint32_t start, uint32_t end,
    uint32_t maxPrimsPerLeaf, int depth)
{
    uint32_t numPrims = end - start;

    // Create leaf if we have few enough primitives or max depth reached
    if (numPrims <= maxPrimsPerLeaf || depth > 20)
    {
        // Leaf node
        uint32_t nodeIndex = static_cast<uint32_t>(tree.size());
        tree.push_back((3 << 30) | start); // axis = 3 (leaf), offset to objects
        tree.push_back(numPrims); // number of primitives
        tree.push_back(0); // reserved
        return;
    }

    // Calculate bounds for current primitives
    G3D::AABox nodeBounds = primitives[indices[start]];
    for (uint32_t i = start + 1; i < end; ++i)
    {
        nodeBounds.merge(primitives[indices[i]]);
    }

    // Choose split axis (longest extent)
    G3D::Vector3 extent = nodeBounds.high() - nodeBounds.low();
    int axis = 0;
    if (extent.y > extent.x) axis = 1;
    if (extent.z > extent[axis]) axis = 2;

    // Sort primitives along chosen axis
    float splitPos = (nodeBounds.low()[axis] + nodeBounds.high()[axis]) * 0.5f;

    // Partition primitives
    uint32_t mid = start;
    for (uint32_t i = start; i < end; ++i)
    {
        float center = (primitives[indices[i]].low()[axis] +
            primitives[indices[i]].high()[axis]) * 0.5f;
        if (center < splitPos)
        {
            if (i != mid)
                std::swap(indices[i], indices[mid]);
            ++mid;
        }
    }

    // Ensure we actually split
    if (mid == start || mid == end)
    {
        mid = (start + end) / 2;
    }

    // Create inner node
    uint32_t nodeIndex = static_cast<uint32_t>(tree.size());
    tree.push_back(axis << 30); // axis in top 2 bits
    tree.push_back(VMAP::floatToRawIntBits(nodeBounds.low()[axis]));
    tree.push_back(VMAP::floatToRawIntBits(nodeBounds.high()[axis]));

    // Reserve space for child offset
    uint32_t offsetIndex = static_cast<uint32_t>(tree.size()) - 3;

    // Build left child
    buildNode(primitives, indices, start, mid, maxPrimsPerLeaf, depth + 1);

    // Update offset to right child
    tree[offsetIndex] |= (static_cast<uint32_t>(tree.size()) & 0x1FFFFFFF);

    // Build right child
    buildNode(primitives, indices, mid, end, maxPrimsPerLeaf, depth + 1);
}