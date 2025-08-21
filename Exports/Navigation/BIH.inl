// BIH.inl - Updated with extensive logging
#pragma once
#include "VMapDefinitions.h"
#include "VMapLog.h"
#include <algorithm>
#include <iostream>
#include <string>
#include <iomanip>
#include <cmath>

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
        if (G3D::fuzzyNe(dir[i], 0.0f))
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
        offsetFront[i] = VMAP::floatToRawIntBits(dir[i]) >> 31;
        offsetBack[i] = offsetFront[i] ^ 1;
        offsetFront3[i] = offsetFront[i] * 3;
        offsetBack3[i] = offsetBack[i] * 3;
        ++offsetFront[i];
        ++offsetBack[i];
    }

    // Stack for tree traversal
    StackNode stack[MAX_STACK_SIZE];
    int stackPos = 0;
    int node = 0;
    int nodesVisited = 0;
    int leavesProcessed = 0;
    int objectsTested = 0;

    while (true)
    {
        while (true)
        {
            nodesVisited++;
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
                    leavesProcessed++;
                    int n = tree[node + 1];

                    while (n > 0)
                    {
                        objectsTested++;
                        uint32_t objIdx = objects[offset];

                        bool hit = intersectCallback(r, objIdx, maxDist, stopAtFirstHit, ignoreM2Model);

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
                {
                    return;
                }

                float tf = (VMAP::intBitsToFloat(tree[node + offsetFront[axis]]) - org[axis]) * invDir[axis];
                float tb = (VMAP::intBitsToFloat(tree[node + offsetBack[axis]]) - org[axis]) * invDir[axis];

                node = offset;
                intervalMin = (tf >= intervalMin) ? tf : intervalMin;
                intervalMax = (tb <= intervalMax) ? tb : intervalMax;

                if (intervalMin > intervalMax)
                {
                    break;
                }

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
            {
                continue;
            }

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
            nodesVisited++;
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
                    leavesChecked++;
                    int n = tree[node + 1];

                    while (n > 0)
                    {
                        objectsTested++;
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
                    return;
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
        if (stackPos == 0)
        {
            return;
        }

        --stackPos;
        node = stack[stackPos].node;
    }
}