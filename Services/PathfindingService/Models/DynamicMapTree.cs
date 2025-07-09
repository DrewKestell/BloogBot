using System;
using System.Collections.Generic;

namespace VMAP
{
    /// <summary>
    /// Manages dynamic collision models (GameObjectModel) in the world.
    /// Allows insertion and removal of objects and collision queries among them.
    /// </summary>
    public class DynamicMapTree
    {
        private List<GameObjectModel> objects = new List<GameObjectModel>();

        public DynamicMapTree() { }

        public bool IsInLineOfSight(float x1, float y1, float z1, float x2, float y2, float z2, bool ignoreM2Model)
        {
            Vector3 p1 = new Vector3(x1, y1, z1);
            Vector3 p2 = new Vector3(x2, y2, z2);
            float totalDist = (p2 - p1).Length();
            if (totalDist < 1e-6f)
                return true;
            Ray ray = new Ray(p1, p2 - p1);
            float maxDist = totalDist;
            foreach (var obj in objects)
            {
                float testDist = maxDist;
                if (obj.IntersectRay(ray, ref testDist, true, ignoreM2Model))
                {
                    return false; // blocked
                }
            }
            return true;
        }

        public bool GetIntersectionTime(Ray ray, Vector3 endPos, ref float maxDist)
        {
            bool hit = false;
            float bestDist = maxDist;
            foreach (var obj in objects)
            {
                float testDist = bestDist;
                if (obj.IntersectRay(ray, ref testDist, true, ignoreM2: false))
                {
                    if (testDist < bestDist)
                    {
                        bestDist = testDist;
                        hit = true;
                    }
                }
            }
            if (hit)
            {
                maxDist = bestDist;
            }
            return hit;
        }

        public bool GetObjectHitPos(Vector3 p1, Vector3 p2, out Vector3 resultHitPos, float modifyDist)
        {
            resultHitPos = p2;
            float totalDist = (p2 - p1).Length();
            if (totalDist < 1e-6f)
                return false;
            Ray ray = new Ray(p1, p2 - p1);
            float hitDist = totalDist;
            bool hit = false;
            GameObjectModel? hitObj = null;
            foreach (var obj in objects)
            {
                float testDist = hitDist;
                if (obj.IntersectRay(ray, ref testDist, true, ignoreM2: false))
                {
                    if (testDist < hitDist)
                    {
                        hitDist = testDist;
                        hitObj = obj;
                        hit = true;
                    }
                }
            }
            if (!hit || hitObj == null)
                return false;
            // adjust by modifyDist
            if (modifyDist < 0)
            {
                hitDist = MathF.Max(0, hitDist + modifyDist);
            }
            else if (modifyDist > 0)
            {
                hitDist = MathF.Min(totalDist, hitDist + modifyDist);
            }
            resultHitPos = p1 + (ray.Direction * hitDist);
            return true;
        }

        public GameObjectModel? GetObjectHit(Vector3 p1, Vector3 p2)
        {
            Ray ray = new Ray(p1, p2 - p1);
            float bestDist = (p2 - p1).Length();
            GameObjectModel? result = null;
            foreach (var obj in objects)
            {
                float testDist = bestDist;
                if (obj.IntersectRay(ray, ref testDist, true, ignoreM2: false))
                {
                    if (testDist < bestDist)
                    {
                        bestDist = testDist;
                        result = obj;
                    }
                }
            }
            return result;
        }

        public float GetHeight(float x, float y, float z, float maxSearchDist)
        {
            Vector3 start = new Vector3(x, y, z);
            Vector3 dir = Vector3.Down;
            if (maxSearchDist < 0)
            {
                dir = Vector3.Up;
                maxSearchDist = -maxSearchDist;
            }
            Ray ray = new Ray(start, dir);
            float bestDist = maxSearchDist;
            bool found = false;
            foreach (var obj in objects)
            {
                float testDist = bestDist;
                if (obj.IntersectRay(ray, ref testDist, true, ignoreM2: false))
                {
                    if (testDist < bestDist)
                    {
                        bestDist = testDist;
                        found = true;
                    }
                }
            }
            if (!found)
                return float.NegativeInfinity;
            return z + (dir.z < 0 ? -bestDist : bestDist);
        }

        public void Insert(GameObjectModel model)
        {
            objects.Add(model);
        }

        public void Remove(GameObjectModel model)
        {
            objects.Remove(model);
        }

        public bool Contains(GameObjectModel model)
        {
            return objects.Contains(model);
        }

        public int Count => objects.Count;

        public void Update(uint diff)
        {
            // This could trigger periodic rebalancing if we had a tree structure.
            // With a simple list, no periodic update needed.
        }

        public void Balance()
        {
            // If we had deferred insertion in BIHWrap, we'd rebuild tree here. Using list, nothing needed.
        }
    }
}
