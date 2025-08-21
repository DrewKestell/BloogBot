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