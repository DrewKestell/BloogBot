using System;
using System.Collections.Generic;

namespace VMAP
{
    /// <summary>
    /// Represents a dynamic game object in the world with collision. 
    /// Links a GameObject (by display ID) to a ModelInstance (WorldModel).
    /// </summary>
    public class GameObjectModel
    {
        private bool collisionEnabled;
        private AABox iBound;
        private Matrix3 iInvRot;
        private Vector3 iPos;
        private float iInvScale;
        private float iScale;
        private WorldModel? iModel;
        public string name = string.Empty;

        // Static store of all gameobject displayIDs to model filename and bounds, loaded from file
        private static Dictionary<uint, (string name, AABox bound)> modelList = new Dictionary<uint, (string, AABox)>();

        public GameObjectModel()
        {
            collisionEnabled = false;
            iModel = null;
            iScale = 0;
            iInvScale = 0;
            iInvRot = new Matrix3();
            iBound = AABox.Zero;
        }

        /// <summary>
        /// Initialize the GameObjectModel given a game object's data (position, orientation, scale, displayId).
        /// Looks up the model from modelList and loads it.
        /// </summary>
        public bool Initialize(
            float posX, float posY, float posZ,
            float orientation, float scale,
            uint displayId,
            string vmapBasePath,
            VMapManager2 vm)
        {
            if (!modelList.TryGetValue(displayId, out var ent))
                return false;

            WorldModel? wm = vm.AcquireModelInstance(
                Path.Combine(vmapBasePath, "vmaps"), ent.name);

            if (wm is null)
                return false;

            name = ent.name;
            iModel = wm;
            collisionEnabled = true;

            iPos = new Vector3(posX, posY, posZ);
            iScale = scale;
            iInvScale = MathF.Abs(scale) < 1e-6f ? 0f : 1f / scale;

            Matrix3 rot = Matrix3.FromEulerAnglesZYX(orientation, 0, 0);
            iInvRot = rot.Inverse();

            // build world-space AABB
            AABox local = new(ent.bound.Min * scale, ent.bound.Max * scale);
            AABox rotated = AABox.Zero;
            for (int c = 0; c < 8; ++c)
            {
                Vector3 v = new(
                    (c & 1) != 0 ? local.Max.x : local.Min.x,
                    (c & 2) != 0 ? local.Max.y : local.Min.y,
                    (c & 4) != 0 ? local.Max.z : local.Min.z);

                Vector3 worldCorner = rot * v;
                rotated = c == 0 ? new AABox(worldCorner, worldCorner)
                                 : new AABox(rotated.Min.Min(worldCorner),
                                             rotated.Max.Max(worldCorner));
            }
            iBound = rotated + iPos;
            return true;
        }
        /// <summary>
        /// Enables or disables collision for this object.
        /// </summary>
        public void EnableCollision(bool enable)
        {
            collisionEnabled = enable;
        }

        /// <summary>
        /// Performs ray intersection against this dynamic object. 
        /// Similar to ModelInstance.IntersectRay.
        /// </summary>
        public bool IntersectRay(Ray ray, ref float maxDist, bool stopAtFirstHit, bool ignoreM2)
        {
            if (!collisionEnabled || iModel == null)
                return false;
            // Broad phase: AABB
            float t = ray.IntersectionTime(iBound);
            if (float.IsPositiveInfinity(t))
                return false;
            // Transform ray to object local space
            Vector3 p = iInvRot * ((ray.Origin - iPos) * iInvScale);
            Vector3 dir = iInvRot * ray.Direction;
            Ray localRay = new Ray(p, dir, normalizeDir: false);
            float localMax = maxDist * iInvScale;
            bool hit = iModel.IntersectRay(localRay, ref localMax, stopAtFirstHit, ignoreM2);
            if (hit)
            {
                maxDist = localMax * iScale;
            }
            return hit;
        }

        /// <summary>
        /// Relocate the GameObjectModel when the underlying GameObject moves or changes orientation.
        /// Updates position, rotation, and bounding box.
        /// </summary>
        public bool Relocate(float newX, float newY, float newZ, float newOrientation)
        {
            if (iModel == null)
                return false;

            iPos = new Vector3(newX, newY, newZ);
            Matrix3 rot = Matrix3.FromEulerAnglesZYX(newOrientation, 0, 0);
            iInvRot = rot.Inverse();

            // Find the modelData by name
            var entry = modelList.FirstOrDefault(kvp => kvp.Value.name == name);
            if (entry.Key == 0)
                return false;

            AABox baseBound = entry.Value.bound;
            if (baseBound == AABox.Zero)
                return false;

            AABox scaled = new AABox(baseBound.Min * iScale, baseBound.Max * iScale);
            AABox newBounds = AABox.Zero;
            for (int i = 0; i < 8; ++i)
            {
                Vector3 corner = scaled.Min;
                if ((i & 1) != 0) corner.x = scaled.Max.x;
                if ((i & 2) != 0) corner.y = scaled.Max.y;
                if ((i & 4) != 0) corner.z = scaled.Max.z;
                Vector3 rotated = rot * corner;
                if (i == 0)
                    newBounds = new AABox(rotated, rotated);
                else
                    newBounds.Merge(rotated);
            }
            iBound = newBounds + iPos;
            return true;
        }

        /// <summary>
        /// Factory to create a GameObjectModel from game object data.
        /// Checks certain gameobject types that should not have collision (like server-only or non-LoS blocking ones).
        /// Returns null if the object should not have a collision model.
        /// </summary>
        public static GameObjectModel? Construct(
            uint displayId, float x, float y, float z,
            float ori, float sc,
            bool serverOnly, bool losOK,
            string vmapBasePath,
            VMapManager2 vm)
        {
            if (serverOnly || losOK)
                return null;

            GameObjectModel gom = new();
            return gom.Initialize(x, y, z, ori, sc, displayId, vmapBasePath, vm) ? gom : null;
        }

        // Additional accessor if needed
        public Vector3 GetPosition() => iPos;
        public AABox GetBounds() => iBound;
    }
}
