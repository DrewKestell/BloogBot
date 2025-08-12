// BIH.inl - Reverted to stable version
#pragma once
#include "VMapDefinitions.h"
#include <algorithm>
#include <iostream>
#include <string>
#include <iomanip>

template<typename RayCallback>
void BIH::intersectRay(const G3D::Ray& r, RayCallback& intersectCallback,
    float& maxDist, bool stopAtFirst, bool ignoreM2Model) const
{
    int nodesVisited = 0;
    int leavesChecked = 0;
    int objectsTested = 0;
    int objectsHit = 0;

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
            // intervalMax can only become smaller for other axis,
            // and intervalMin only larger respectively, so stop early
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

    // Prepare offset arrays for traversal
    uint32_t offsetFront[3], offsetBack[3];
    uint32_t offsetFront3[3], offsetBack3[3];

    // compute custom offsets from direction sign bit
    for (int i = 0; i < 3; ++i)
    {
        offsetFront[i] = VMAP::floatToRawIntBits(dir[i]) >> 31;
        offsetBack[i] = offsetFront[i] ^ 1;
        offsetFront3[i] = offsetFront[i] * 3;
        offsetBack3[i] = offsetBack[i] * 3;

        // avoid always adding 1 during the inner loop
        ++offsetFront[i];
        ++offsetBack[i];
    }

    // Stack for tree traversal
    struct StackNode
    {
        int node;
        float tnear;
        float tfar;
    };
    StackNode stack[MAX_STACK_SIZE];
    int stackPos = 0;
    int node = 0;

    // Tree traversal
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
                if (axis < 3) // Inner node
                {
                    // "normal" interior node
                    float tf = (VMAP::intBitsToFloat(tree[node + offsetFront[axis]]) - org[axis]) * invDir[axis];
                    float tb = (VMAP::intBitsToFloat(tree[node + offsetBack[axis]]) - org[axis]) * invDir[axis];

                    // ray passes between clip zones
                    if (tf < intervalMin && tb > intervalMax)
                        break;

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

                    // ray passes through both nodes
                    // push back node
                    if (stackPos < MAX_STACK_SIZE)
                    {
                        stack[stackPos].node = back;
                        stack[stackPos].tnear = (tb >= intervalMin) ? tb : intervalMin;
                        stack[stackPos].tfar = intervalMax;
                        ++stackPos;
                    }

                    // update ray interval for front node
                    intervalMax = (tf <= intervalMax) ? tf : intervalMax;
                    continue;
                }
                else // Leaf node
                {
                    leavesChecked++;

                    // leaf - test some objects
                    int n = tree[node + 1];

                    while (n > 0)
                    {
                        uint32_t objIdx = objects[offset];

                        if (objIdx >= 100000000)  // Sanity check for corrupted data
                        {
                            --n;
                            ++offset;
                            continue;
                        }

                        objectsTested++;
                        float oldDist = maxDist;

                        bool hit = intersectCallback(r, objIdx, maxDist, stopAtFirst, ignoreM2Model);

                        if (hit)
                        {
                            objectsHit++;
                        }

                        if (stopAtFirst && hit)
                        {
                            return;
                        }

                        --n;
                        ++offset;
                    }
                    break;
                }
            }
            else
            {
                if (axis > 2)
                {
                    return; // should not happen
                }
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

        // Pop from stack
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

template<typename IsectCallback>
void BIH::intersectPoint(const G3D::Vector3& p, IsectCallback& intersectCallback) const
{
    if (!bounds.contains(p) || tree.empty() || objects.empty())
        return;

    struct StackNode
    {
        int node;
    };
    StackNode stack[MAX_STACK_SIZE];
    int stackPos = 0;
    int node = 0;

    while (true)
    {
        while (true)
        {
            uint32_t tn = tree[node];
            uint32_t axis = (tn >> 30) & 3;
            uint32_t offset = tn & 0x1FFFFFFF;

            if (axis == 3) // Leaf node
            {
                // Process all objects in leaf
                uint32_t count = tree[node + 1];

                // Validate count and offset
                if (count > objects.size() || offset >= objects.size() || offset + count > objects.size())
                {
                    break;
                }

                for (uint32_t i = 0; i < count; ++i)
                {
                    uint32_t objIdx = objects[offset + i];
                    if (objIdx < 100000000)  // Sanity check
                    {
                        // Point callback only takes 2 parameters (point and index)
                        intersectCallback(p, objIdx);
                    }
                }
                break;
            }
            else // Inner node
            {
                float splitMin = VMAP::intBitsToFloat(tree[node + 1]);
                float splitMax = VMAP::intBitsToFloat(tree[node + 2]);

                int first = node + 3;  // Left child
                int second = offset;   // Right child

                // Check which child contains the point
                if (p[axis] <= splitMin)
                {
                    node = first;
                }
                else if (p[axis] >= splitMax)
                {
                    node = second;
                }
                else
                {
                    // Point is in overlap region, must check both
                    if (stackPos < MAX_STACK_SIZE)
                    {
                        stack[stackPos].node = second;
                        ++stackPos;
                    }
                    node = first;
                }
                continue;
            }
        }

        // Pop from stack
        if (stackPos == 0)
            return;
        --stackPos;
        node = stack[stackPos].node;
    }
}