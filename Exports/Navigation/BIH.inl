// BIH.inl - Fixed BIH ray traversal with proper axis-aligned ray handling
#pragma once
#include "VMapDefinitions.h"
#include "VMapLog.h"
#include <algorithm>
#include <iostream>
#include <string>
#include <iomanip>
#include <cmath>

// Build template implementation
template<class BoundsFunc, class PrimArray>
void BIH::build(PrimArray const& primitives, BoundsFunc& getBounds, uint32_t leafSize, bool printStats)
{
    if (primitives.size() == 0)
    {
        init_empty();
        return;
    }

    buildData dat;
    dat.maxPrims = leafSize;
    dat.numPrims = primitives.size();
    dat.indices = new uint32_t[dat.numPrims];
    dat.primBound = new G3D::AABox[dat.numPrims];

    // Initialize bounds with first primitive
    getBounds(primitives[0], bounds);

    // Process all primitives
    for (uint32_t i = 0; i < dat.numPrims; ++i)
    {
        dat.indices[i] = i;
        getBounds(primitives[i], dat.primBound[i]);
        bounds.merge(dat.primBound[i]);
    }

    std::vector<uint32_t> tempTree;
    BuildStats stats;
    buildHierarchy(tempTree, dat, stats);

    if (printStats)
        stats.printStats();

    // Copy indices to objects array
    objects.resize(dat.numPrims);
    for (uint32_t i = 0; i < dat.numPrims; ++i)
        objects[i] = dat.indices[i];

    // Copy temp tree to final tree
    tree = tempTree;

    // Clean up
    delete[] dat.primBound;
    delete[] dat.indices;
}

// Ray intersection template implementation
template<typename RayCallback>
void BIH::intersectRay(const G3D::Ray& r, RayCallback& intersectCallback,
    float& maxDist, bool stopAtFirstHit, bool ignoreM2Model) const
{
    if (tree.empty() || objects.empty())
    {
        return;
    }

    float intervalMin = -1.f;
    float intervalMax = -1.f;
    const G3D::Vector3& org = r.origin();
    const G3D::Vector3& dir = r.direction();
    const G3D::Vector3& invDir = r.invDirection();

    // Calculate initial ray-box intersection with overall bounds
    for (int i = 0; i < 3; ++i)
    {
        if (G3D::fuzzyNe(dir[i], 0.0f))  // Use fuzzyNe like original
        {
            float t1 = (bounds.low()[i] - org[i]) * invDir[i];
            float t2 = (bounds.high()[i] - org[i]) * invDir[i];
            if (t1 > t2)
                std::swap(t1, t2);
            if (t1 > intervalMin)
                intervalMin = t1;
            if (t2 < intervalMax || intervalMax < 0.f)
                intervalMax = t2;

            if (intervalMax <= 0 || intervalMin >= maxDist)
            {
                return;
            }
        }
    }

    if (intervalMin > intervalMax)
    {
        return;
    }

    intervalMin = std::max(intervalMin, 0.f);
    intervalMax = std::min(intervalMax, maxDist);

    // Compute custom offsets from direction sign bit
    uint32_t offsetFront[3], offsetBack[3];
    uint32_t offsetFront3[3], offsetBack3[3];

    for (int i = 0; i < 3; ++i)
    {
        // Use sign bit extraction (original vMaNGOS style)
        offsetFront[i] = VMAP::floatToRawIntBits(dir[i]) >> 31;
        offsetBack[i] = offsetFront[i] ^ 1;
        offsetFront3[i] = offsetFront[i] * 3;
        offsetBack3[i] = offsetBack[i] * 3;

        // Avoid always adding 1 during the inner loop
        ++offsetFront[i];
        ++offsetBack[i];
    }

    // Stack for tree traversal
    StackNode stack[MAX_STACK_SIZE];
    int stackPos = 0;
    int node = 0;

    while (true)
    {
        while (true)
        {
            uint32_t tn = tree[node];
            uint32_t axis = (tn >> 30) & 3;
            bool BVH2 = tn & (1 << 29);
            int offset = tn & ~(7 << 29);

            if (!BVH2)
            {
                if (axis < 3)
                {
                    // "normal" interior node
                    float tf = (VMAP::intBitsToFloat(tree[node + offsetFront[axis]]) - org[axis]) * invDir[axis];
                    float tb = (VMAP::intBitsToFloat(tree[node + offsetBack[axis]]) - org[axis]) * invDir[axis];

                    // ray passes between clip zones
                    if (tf < intervalMin && tb > intervalMax)
                    {
                        break;
                    }

                    int back = offset + offsetBack3[axis];
                    node = back;

                    // ray passes through far node only
                    if (tf < intervalMin)
                    {
                        intervalMin = (tb >= intervalMin) ? tb : intervalMin;
                        continue;
                    }

                    node = offset + offsetFront3[axis]; // front

                    // ray passes through near node only
                    if (tb > intervalMax)
                    {
                        intervalMax = (tf <= intervalMax) ? tf : intervalMax;
                        continue;
                    }

                    // push back node
                    stack[stackPos].node = back;
                    stack[stackPos].tnear = (tb >= intervalMin) ? tb : intervalMin;
                    stack[stackPos].tfar = intervalMax;
                    ++stackPos;

                    // update ray interval for front node
                    intervalMax = (tf <= intervalMax) ? tf : intervalMax;
                    continue;
                }
                else
                {
                    // leaf - test some objects
                    int n = tree[node + 1];

                    while (n > 0)
                    {
                        bool hit = intersectCallback(r, objects[offset], maxDist, stopAtFirstHit, ignoreM2Model);
                        if (stopAtFirstHit && hit)
                        {
                            return;
                        }
                        --n;
                        ++offset;
                    }
                    break;
                }
            }
            else  // BVH2 node
            {
                if (axis > 2)
                    return; // should not happen

                float tf = (VMAP::intBitsToFloat(tree[node + offsetFront[axis]]) - org[axis]) * invDir[axis];
                float tb = (VMAP::intBitsToFloat(tree[node + offsetBack[axis]]) - org[axis]) * invDir[axis];

                node = offset;
                intervalMin = (tf >= intervalMin) ? tf : intervalMin;
                intervalMax = (tb <= intervalMax) ? tb : intervalMax;

                if (intervalMin > intervalMax)
                    break;

                continue;
            }
        } // traversal loop

        do
        {
            // stack is empty?
            if (stackPos == 0)
            {
                return;
            }

            // move back up the stack
            --stackPos;
            intervalMin = stack[stackPos].tnear;

            if (maxDist < intervalMin)
                continue;

            node = stack[stackPos].node;
            intervalMax = stack[stackPos].tfar;
            break;
        } while (true);
    }
}

// Point intersection template implementation
template<typename IsectCallback>
void BIH::intersectPoint(const G3D::Vector3& p, IsectCallback& intersectCallback) const
{
    if (!bounds.contains(p))
    {
        return;
    }

    StackNode stack[MAX_STACK_SIZE];
    int stackPos = 0;
    int node = 0;

    int nodesVisited = 0;
    int leavesChecked = 0;
    int objectsTested = 0;

    while (true)
    {
        while (true)
        {
            uint32_t tn = tree[node];
            uint32_t axis = (tn >> 30) & 3;
            bool const BVH2 = tn & (1 << 29);
            int offset = tn & ~(7 << 29);

            if (!BVH2)
            {
                if (axis < 3)
                {
                    // "normal" interior node
                    float tl = VMAP::intBitsToFloat(tree[node + 1]);
                    float tr = VMAP::intBitsToFloat(tree[node + 2]);

                    // point is between clip zones
                    if (tl < p[axis] && tr > p[axis])
                    {
                        break;
                    }

                    int right = offset + 3;
                    node = right;

                    // point is in right node only
                    if (tl < p[axis])
                    {
                        continue;
                    }

                    node = offset; // left

                    // point is in left node only
                    if (tr > p[axis])
                    {
                        continue;
                    }

                    // push back right node
                    stack[stackPos].node = right;
                    ++stackPos;

                    continue;
                }
                else
                {
                    int n = tree[node + 1];

                    while (n > 0)
                    {
                        uint32_t objIdx = objects[offset];

                        intersectCallback(p, objIdx);

                        --n;
                        ++offset;
                    }
                    break;
                }
            }
            else // BVH2 node (empty space cut off left and right)
            {
                if (axis > 2)
                {
                    return; // should not happen
                }

                float tl = VMAP::intBitsToFloat(tree[node + 1]);
                float tr = VMAP::intBitsToFloat(tree[node + 2]);

                node = offset;

                if (tl > p[axis] || tr < p[axis])
                {
                    break;
                }

                continue;
            }
        } // traversal loop

        // Pop from stack
        // stack is empty?
        if (stackPos == 0)
        {
            return;
        }

        // move back up the stack
        --stackPos;
        node = stack[stackPos].node;
    }
}