// BIH.inl - Aligned with Server_BIH.h implementation with full logging
#pragma once
#include "VMapDefinitions.h"
#include "VMapLog.h"
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
    int stackDepth = 0;
    int maxStackDepth = 0;

    LOG_TRACE("BIH::intersectRay START TreeSize:" << tree.size()
        << " ObjectCount:" << objects.size()
        << " MaxDist:" << maxDist
        << " StopAtFirst:" << (stopAtFirst ? "YES" : "NO")
        << " IgnoreM2:" << (ignoreM2Model ? "YES" : "NO"));

    if (tree.empty())
    {
        LOG_DEBUG("BIH tree is empty - no traversal possible");
        return;
    }

    if (objects.empty())
    {
        LOG_DEBUG("BIH objects array is empty - no objects to test");
        return;
    }

    float intervalMin = -1.f;
    float intervalMax = -1.f;
    const G3D::Vector3& org = r.origin();
    const G3D::Vector3& dir = r.direction();
    const G3D::Vector3& invDir = r.invDirection();

    LOG_DEBUG("Ray parameters Origin(" << org.x << ", " << org.y << ", " << org.z
        << ") Dir(" << dir.x << ", " << dir.y << ", " << dir.z << ")");
    LOG_DEBUG("BIH bounds Low(" << bounds.low().x << ", " << bounds.low().y << ", " << bounds.low().z
        << ") High(" << bounds.high().x << ", " << bounds.high().y << ", " << bounds.high().z << ")");

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

            LOG_TRACE("Axis " << i << ": t1=" << t1 << " t2=" << t2
                << " intervalMin=" << intervalMin << " intervalMax=" << intervalMax);

            // intervalMax can only become smaller for other axis,
            // and intervalMin only larger respectively, so stop early
            if (intervalMax <= 0 || intervalMin >= maxDist)
            {
                LOG_DEBUG("Early exit: Ray misses BIH bounds (intervalMax=" << intervalMax
                    << " intervalMin=" << intervalMin << " maxDist=" << maxDist << ")");
                return;
            }
        }
    }

    if (intervalMin > intervalMax)
    {
        LOG_DEBUG("Ray misses BIH bounds: intervalMin(" << intervalMin << ") > intervalMax(" << intervalMax << ")");
        return;
    }

    intervalMin = std::max(intervalMin, 0.f);
    intervalMax = std::min(intervalMax, maxDist);

    LOG_DEBUG("Initial interval: [" << intervalMin << ", " << intervalMax << "]");

    // Compute custom offsets from direction sign bit (Server_BIH.h optimization)
    uint32_t offsetFront[3], offsetBack[3];
    uint32_t offsetFront3[3], offsetBack3[3];

    for (int i = 0; i < 3; ++i)
    {
        offsetFront[i] = VMAP::floatToRawIntBits(dir[i]) >> 31;
        offsetBack[i] = offsetFront[i] ^ 1;
        offsetFront3[i] = offsetFront[i] * 3;
        offsetBack3[i] = offsetBack[i] * 3;

        // avoid always adding 1 during the inner loop
        ++offsetFront[i];
        ++offsetBack[i];

        LOG_TRACE("Axis " << i << " offsets: front=" << offsetFront[i]
            << " back=" << offsetBack[i]
            << " front3=" << offsetFront3[i]
            << " back3=" << offsetBack3[i]);
    }

    // Stack for tree traversal
    StackNode stack[MAX_STACK_SIZE];
    int stackPos = 0;
    int node = 0;

    // Main traversal loop
    while (true)
    {
        while (true)
        {
            nodesVisited++;

            if (nodesVisited % 100 == 0) // Log every 100 nodes to avoid spam
            {
                LOG_TRACE("Nodes visited: " << nodesVisited << " Current node: " << node
                    << " Stack depth: " << stackPos);
            }

            uint32_t tn = tree[node];
            uint32_t axis = (tn >> 30) & 3;
            bool BVH2 = tn & (1 << 29);
            int offset = tn & ~(7 << 29);

            LOG_TRACE("Node " << node << ": axis=" << axis
                << " BVH2=" << (BVH2 ? "YES" : "NO")
                << " offset=" << offset);

            if (!BVH2)
            {
                if (axis < 3)
                {
                    // "normal" interior node
                    float tf = (VMAP::intBitsToFloat(tree[node + offsetFront[axis]]) - org[axis]) * invDir[axis];
                    float tb = (VMAP::intBitsToFloat(tree[node + offsetBack[axis]]) - org[axis]) * invDir[axis];

                    LOG_TRACE("Interior node " << node << ": Axis=" << axis
                        << " tf=" << tf << " tb=" << tb
                        << " interval=[" << intervalMin << "," << intervalMax << "]");

                    // ray passes between clip zones
                    if (tf < intervalMin && tb > intervalMax)
                    {
                        LOG_TRACE("Ray passes between clip zones - skipping subtree");
                        break;
                    }

                    int back = offset + offsetBack3[axis];
                    node = back;

                    // ray passes through far node only
                    if (tf < intervalMin)
                    {
                        LOG_TRACE("Ray passes through far node only");
                        intervalMin = (tb >= intervalMin) ? tb : intervalMin;
                        continue;
                    }

                    node = offset + offsetFront3[axis]; // front

                    // ray passes through near node only
                    if (tb > intervalMax)
                    {
                        LOG_TRACE("Ray passes through near node only");
                        intervalMax = (tf <= intervalMax) ? tf : intervalMax;
                        continue;
                    }

                    // ray passes through both nodes
                    LOG_TRACE("Ray passes through both children");

                    // push back node
                    stack[stackPos].node = back;
                    stack[stackPos].tnear = (tb >= intervalMin) ? tb : intervalMin;
                    stack[stackPos].tfar = intervalMax;
                    ++stackPos;
                    maxStackDepth = std::max(maxStackDepth, stackPos);

                    LOG_TRACE("Pushed back child to stack pos " << (stackPos - 1)
                        << " node=" << back
                        << " interval=[" << stack[stackPos - 1].tnear
                        << "," << stack[stackPos - 1].tfar << "]");

                    // update ray interval for front node
                    intervalMax = (tf <= intervalMax) ? tf : intervalMax;
                    continue;
                }
                else
                {
                    // leaf - test some objects
                    leavesChecked++;
                    int n = tree[node + 1];

                    LOG_TRACE("Leaf node " << node << ": ObjectCount=" << n
                        << " FirstObjIdx=" << offset);

                    while (n > 0)
                    {
                        objectsTested++;
                        uint32_t objIdx = objects[offset];

                        LOG_TRACE("Testing object " << objIdx << " at offset " << offset);

                        float oldDist = maxDist;
                        bool hit = intersectCallback(r, objIdx, maxDist, stopAtFirst, ignoreM2Model);

                        if (hit)
                        {
                            objectsHit++;
                            LOG_DEBUG("Object " << objIdx << " HIT at distance " << maxDist
                                << " (was " << oldDist << ")");

                            if (stopAtFirst)
                            {
                                LOG_DEBUG("Stopping at first hit");
                                LOG_INFO("BIH traversal summary: NodesVisited:" << nodesVisited
                                    << " LeavesChecked:" << leavesChecked
                                    << " ObjectsTested:" << objectsTested
                                    << " ObjectsHit:" << objectsHit
                                    << " MaxStackDepth:" << maxStackDepth);
                                return;
                            }
                        }
                        else
                        {
                            LOG_TRACE("Object " << objIdx << " MISS");
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
                    LOG_ERROR("Invalid BVH2 node with axis=" << axis << " at node " << node);
                    return; // should not happen
                }

                LOG_TRACE("BVH2 node " << node << ": Axis=" << axis);

                float tf = (VMAP::intBitsToFloat(tree[node + offsetFront[axis]]) - org[axis]) * invDir[axis];
                float tb = (VMAP::intBitsToFloat(tree[node + offsetBack[axis]]) - org[axis]) * invDir[axis];

                LOG_TRACE("BVH2 empty space cutoff: tf=" << tf << " tb=" << tb);

                node = offset;
                intervalMin = (tf >= intervalMin) ? tf : intervalMin;
                intervalMax = (tb <= intervalMax) ? tb : intervalMax;

                if (intervalMin > intervalMax)
                {
                    LOG_TRACE("BVH2 cutoff resulted in empty interval - backtracking");
                    break;
                }

                LOG_TRACE("BVH2 updated interval: [" << intervalMin << "," << intervalMax << "]");
                continue;
            }
        } // traversal loop

        // Pop from stack
        do
        {
            // stack is empty?
            if (stackPos == 0)
            {
                LOG_DEBUG("Stack empty - traversal complete");
                LOG_INFO("BIH traversal summary: NodesVisited:" << nodesVisited
                    << " LeavesChecked:" << leavesChecked
                    << " ObjectsTested:" << objectsTested
                    << " ObjectsHit:" << objectsHit
                    << " MaxStackDepth:" << maxStackDepth);
                return;
            }

            // move back up the stack
            --stackPos;
            intervalMin = stack[stackPos].tnear;

            if (maxDist < intervalMin)
            {
                LOG_TRACE("Skipping stack entry " << stackPos
                    << " - beyond maxDist (intervalMin=" << intervalMin
                    << " maxDist=" << maxDist << ")");
                continue;
            }

            node = stack[stackPos].node;
            intervalMax = stack[stackPos].tfar;

            LOG_TRACE("Popped from stack pos " << stackPos << " node:" << node
                << " interval:[" << intervalMin << "," << intervalMax << "]");
            break;
        } while (true);
    }
}

template<typename IsectCallback>
void BIH::intersectPoint(const G3D::Vector3& p, IsectCallback& intersectCallback) const
{
    LOG_TRACE("BIH::intersectPoint START Point:(" << p.x << "," << p.y << "," << p.z << ")");

    if (!bounds.contains(p))
    {
        LOG_DEBUG("Point is outside BIH bounds");
        return;
    }

    if (tree.empty() || objects.empty())
    {
        LOG_DEBUG("BIH tree or objects array is empty");
        return;
    }

    struct StackNode
    {
        int node;
    };

    StackNode stack[MAX_STACK_SIZE];
    int stackPos = 0;
    int node = 0;

    int nodesVisited = 0;
    int leavesChecked = 0;
    int objectsTested = 0;

    LOG_DEBUG("Starting point intersection traversal");

    while (true)
    {
        while (true)
        {
            nodesVisited++;

            if (nodesVisited % 50 == 0)
            {
                LOG_TRACE("Point traversal: Nodes visited=" << nodesVisited
                    << " Current node=" << node
                    << " Stack depth=" << stackPos);
            }

            uint32_t tn = tree[node];
            uint32_t axis = (tn >> 30) & 3;
            bool BVH2 = tn & (1 << 29);
            int offset = tn & ~(7 << 29);

            LOG_TRACE("Node " << node << ": axis=" << axis
                << " BVH2=" << (BVH2 ? "YES" : "NO")
                << " offset=" << offset);

            if (!BVH2)
            {
                if (axis < 3)
                {
                    // "normal" interior node
                    float tl = VMAP::intBitsToFloat(tree[node + 1]);
                    float tr = VMAP::intBitsToFloat(tree[node + 2]);

                    LOG_TRACE("Interior node " << node
                        << ": Split planes tl=" << tl << " tr=" << tr
                        << " point[axis]=" << p[axis]);

                    // point is between clip zones
                    if (tl < p[axis] && tr > p[axis])
                    {
                        LOG_TRACE("Point is between clip zones - skipping subtree");
                        break;
                    }

                    int right = offset + 3;
                    node = right;

                    // point is in right node only
                    if (tl < p[axis])
                    {
                        LOG_TRACE("Point is in right child only");
                        continue;
                    }

                    node = offset; // left

                    // point is in left node only
                    if (tr > p[axis])
                    {
                        LOG_TRACE("Point is in left child only");
                        continue;
                    }

                    // point is in both nodes
                    LOG_TRACE("Point is in both children - pushing right to stack");

                    // push back right node
                    stack[stackPos].node = right;
                    ++stackPos;

                    LOG_TRACE("Pushed right child to stack pos " << (stackPos - 1));
                    continue;
                }
                else
                {
                    // leaf - test some objects
                    leavesChecked++;
                    int n = tree[node + 1];

                    LOG_TRACE("Leaf node " << node << ": ObjectCount=" << n
                        << " FirstObjIdx=" << offset);

                    // Validate count and offset
                    if (n > (int)objects.size() || offset >= (int)objects.size() || offset + n > (int)objects.size())
                    {
                        LOG_ERROR("Invalid leaf data: count=" << n
                            << " offset=" << offset
                            << " objects.size()=" << objects.size());
                        break;
                    }

                    while (n > 0)
                    {
                        objectsTested++;
                        uint32_t objIdx = objects[offset];

                        LOG_TRACE("Testing point against object " << objIdx);

                        // Point callback only takes 2 parameters (point and index)
                        intersectCallback(p, objIdx);

                        LOG_TRACE("Object " << objIdx << " processed");

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
                    LOG_ERROR("Invalid BVH2 node with axis=" << axis << " at node " << node);
                    return; // should not happen
                }

                LOG_TRACE("BVH2 node " << node << ": Axis=" << axis);

                float tl = VMAP::intBitsToFloat(tree[node + 1]);
                float tr = VMAP::intBitsToFloat(tree[node + 2]);

                LOG_TRACE("BVH2 empty space bounds: tl=" << tl << " tr=" << tr
                    << " point[axis]=" << p[axis]);

                node = offset;

                if (tl > p[axis] || tr < p[axis])
                {
                    LOG_TRACE("Point is outside BVH2 bounds - skipping");
                    break;
                }

                LOG_TRACE("Point is within BVH2 bounds - continuing to child");
                continue;
            }
        } // traversal loop

        // Pop from stack
        // stack is empty?
        if (stackPos == 0)
        {
            LOG_DEBUG("Stack empty - point traversal complete");
            LOG_INFO("Point traversal summary: NodesVisited:" << nodesVisited
                << " LeavesChecked:" << leavesChecked
                << " ObjectsTested:" << objectsTested);
            return;
        }

        // move back up the stack
        --stackPos;
        node = stack[stackPos].node;

        LOG_TRACE("Popped from stack pos " << stackPos << " node:" << node);
    }
}