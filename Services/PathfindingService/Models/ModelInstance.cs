using G3D;
using System;

namespace VMAP
{
    /// <summary>
    /// An instance of a model in the world (inherits ModelSpawn data and adds runtime model reference).
    /// Provides collision and query functions in world coordinates.
    /// </summary>
    public class ModelInstance : ModelSpawn
    {
        // Precomputed inverse rotation matrix and inverse scale for transforming points/rays to model local space.
        private Matrix3 iInvRot;
        private float iInvScale;
        private readonly WorldModel? iModel;  // The geometry of the model (shared among instances of the same model file)

        // Default constructor for array initialization
        public ModelInstance()
        {
            iModel = null;
            iInvScale = 0;
            iInvRot = new Matrix3(); // zero matrix (not used until set)
        }

        /// <summary>
        /// Creates a ModelInstance from a spawn definition and a loaded WorldModel.
        /// Also precomputes inverse rotation and scale for use in intersections.
        /// </summary>
        public ModelInstance(ModelSpawn s, WorldModel? model)
        {
            flags = s.flags;
            adtId = s.adtId;
            ID = s.ID;
            iPos = s.iPos;
            iRot = s.iRot;
            iScale = s.iScale;
            iBound = s.iBound;
            name = s.name;
            iModel = model;

            // 1) generate the eight corners of the model‐space bound:
            var lo = s.iBound.Min;
            var hi = s.iBound.Max;
            var corners = new[]
            {
                new Vector3(lo.x, lo.y, lo.z),
                new Vector3(lo.x, lo.y, hi.z),
                new Vector3(lo.x, hi.y, lo.z),
                new Vector3(lo.x, hi.y, hi.z),
                new Vector3(hi.x, lo.y, lo.z),
                new Vector3(hi.x, lo.y, hi.z),
                new Vector3(hi.x, hi.y, lo.z),
                new Vector3(hi.x, hi.y, hi.z),
            };

            // 2) build the world‐space transform:
            var rot = Matrix3.FromEulerAnglesZYX(iRot.z, iRot.y, iRot.x);
            var scale = iScale;

            iInvRot = rot.Inverse();
            iInvScale = MathF.Abs(iScale) < 1e-6f ? 0f : 1f / iScale;

            // 3) transform each corner and expand a VMAP‐internal AABB:
            var worldMin = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
            var worldMax = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

            foreach (var c in corners)
            {
                var v = rot * (c * scale);  // scale then rotate in model space
                                            // now translate into internal‐space position:
                v += iPos;                  // iPos is already in internal coords
                worldMin = worldMin.Min(v);
                worldMax = worldMax.Max(v);
            }

            // 4) override iBound so that the **broad‐phase** test is correct:
            iBound = new AABox(worldMin, worldMax);
        }

        /// <summary>
        /// Mark this instance as unloaded (drop the reference to its WorldModel).
        /// </summary>
        public void SetUnloaded()
        {
            // iModel is a readonly reference in this design (set in constructor), 
            // so to "unload" we would rely on clearing references elsewhere or GC.
            // Here we might simply do nothing or log.
            // (In C++ this set iModel = nullptr to indicate an unused slot.)
        }

        /// <summary>
        /// Check if a ray intersects this model instance. 
        /// If an intersection is found, pMaxDist is reduced to the distance of the hit.
        /// If pStopAtFirstHit is true, returns as soon as any hit is found.
        /// If ignoreM2Model is true, M2 models (doodads) will be ignored for collision.
        /// </summary>
        public bool IntersectRay(Ray ray, ref float pMaxDist, bool pStopAtFirstHit, bool ignoreM2Model = false)
        {
            if (iModel == null)
            {
                // Model not loaded (should not happen if properly managed)
                return false;
            }
            // If ignoring M2 and this instance is an M2 model, skip it.
            if (ignoreM2Model && flags.HasFlag(ModelFlags.MOD_M2))
                return false;
            // First test: ray vs the model's overall bounding box in world space
            float t = ray.IntersectionTime(this.iBound);
            if (float.IsPositiveInfinity(t))
            {
                // Ray does not intersect the broad-phase bounding box
                return false;
            }
            // Transform the ray into the model's local coordinate space:
            // p' = iInvRot * ((p - iPos) * iInvScale)
            Vector3 originLocal = iInvRot * (ray.Origin - iPos) * iInvScale;
            Vector3 dirLocal = iInvRot * ray.Direction;
            Ray localRay = new Ray(originLocal, dirLocal, normalizeDir: false);
            // Scale the maximum distance to local space
            float localMaxDist = pMaxDist * iInvScale;
            // Delegate to the WorldModel's intersection (tests against actual geometry triangles)
            bool hit = iModel.IntersectRay(localRay, ref localMaxDist, pStopAtFirstHit, ignoreM2Model);
            if (hit)
            {
                // Transform distance back to world scale
                pMaxDist = localMaxDist * iScale;
            }
            return hit;
        }

        /// <summary>
        /// Determine if a point lies inside this model (for area queries).
        /// If the model is a World (WMO) model, and the point is within the model's geometry,
        /// this updates the AreaInfo structure with ground height and identifiers.
        /// (M2 models do not provide area info.)
        /// </summary>
        public void IntersectPoint(Vector3 p, ref AreaInfo info)
        {
            if (iModel == null)
                return;
            // Only WMO models contain area information (M2 models have no interior data for area IDs)
            if (flags.HasFlag(ModelFlags.MOD_M2))
                return;
            // Quick BB check:
            if (!iBound.Contains(p))
                return;
            // Transform point to model local space
            Vector3 pModel = iInvRot * ((p - iPos) * iInvScale);
            Vector3 downDirModel = iInvRot * Vector3.Down;
            float zDist;
            if (iModel.IntersectPoint(pModel, downDirModel, out zDist, out var locInfo))
            {
                // If there's an intersection with model geometry below the point, compute world Z of that surface
                Vector3 modelGround = pModel + zDist * downDirModel;
                float worldZ = (modelGround * iInvRot.Inverse() * iScale + iPos).z;
                if (worldZ > info.ground_Z)
                {
                    // Update the highest ground point below p
                    info.ground_Z = worldZ;
                    info.adtId = this.adtId;
                }
            }
        }

        /// <summary>
        /// Checks if the given point is under the model (e.g., under a roof or inside a structure).
        /// Returns true if so, and optionally provides distances.
        /// </summary>
        public bool IsUnderModel(Vector3 p, out float outDist, out float inDist)
        {
            outDist = inDist = 0f;
            if (iModel == null)
                return false;
            // M2 models typically have no interior (they often don't define volume), but we still can check geometry
            // For WMO (MOD_HAS_BOUND), ensure point is within bounding box
            if (!flags.HasFlag(ModelFlags.MOD_M2) && !iBound.Contains(p))
                return false;
            // Transform point and upward direction into model space
            Vector3 pModel = iInvRot * ((p - iPos) * iInvScale);
            Vector3 upModel = iInvRot * Vector3.Up; // direction upward in model space
            // Query the WorldModel for under-object (this uses ray casting to find in/out intersections)
            return iModel.IsUnderObject(pModel, upModel, flags.HasFlag(ModelFlags.MOD_M2), outDist, inDist);
        }

        /// <summary>
        /// Retrieves location information (WMO group and root IDs) for a point in or on this model.
        /// If the point is inside a WMO, this returns true and fills the LocationInfo (with ground height and group details).
        /// For M2, returns false (no interior location info).
        /// </summary>
        public bool GetLocationInfo(Vector3 p, out LocationInfo info)
        {
            info = new LocationInfo();
            if (iModel == null)
                return false;
            if (flags.HasFlag(ModelFlags.MOD_M2))
                return false; // M2 models have no location data like area ID
            if (!iBound.Contains(p))
                return false;
            Vector3 pModel = iInvRot * ((p - iPos) * iInvScale);
            Vector3 downDirModel = iInvRot * Vector3.Down;
            float zDist;
            GroupLocationInfo groupInfo;
            if (iModel.GetLocationInfo(pModel, downDirModel, out zDist, out groupInfo))
            {
                // We hit something inside the model
                Vector3 modelGround = pModel + zDist * downDirModel;
                float worldZ = ((modelGround * iInvRot.Inverse()) * iScale + iPos).z;
                if (worldZ > info.ground_Z)
                {
                    info.hitInstance = this;
                    info.hitModel = groupInfo.hitModel;
                    info.rootId = groupInfo.rootId;
                    info.ground_Z = worldZ;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if there is liquid at the given point within this model and returns the liquid surface height if so.
        /// </summary>
        public bool GetLiquidLevel(Vector3 p, out float liquidHeight, ref LocationInfo info)
        {
            liquidHeight = 0f;
            if (iModel == null)
                return false;
            // Transform point to model space
            Vector3 pModel = iInvRot * ((p - iPos) * iInvScale);
            // Check for liquid level in model (down direction in model space)
            bool result = iModel.GetLiquidLevel(pModel, out liquidHeight);
            if (result)
            {
                // Transform liquid height back to world coordinates
                float worldZ = (liquidHeight * iScale) + iPos.z;
                if (worldZ > info.ground_Z)
                {
                    info.ground_Z = worldZ;
                }
            }
            return result;
        }

        // Provide access to the internal WorldModel (if needed externally)
        public WorldModel? GetWorldModel() => iModel;
        public float GetScale() => iScale;        // Note: originally iInvScale stored, but returning actual scale
        public Matrix3 GetInvRot() => iInvRot;
        /// <summary>
        /// Reads an M2‐based .vmo into this instance (matches C++ ModelInstance::readFromFile).
        /// </summary>
        public bool ReadFromFile(BinaryReader br)
        {
            try
            {
                //Console.WriteLine("ModelInstance: Starting ReadFromFile");

                // flags, ADT id, instance ID
                //Console.WriteLine("ModelInstance: Reading flags, ADT id, and instance ID...");
                flags = (ModelFlags)br.ReadUInt32();
                adtId = br.ReadUInt16();
                ID = br.ReadUInt32();
                //Console.WriteLine($"ModelInstance: flags={flags}, adtId={adtId}, ID={ID}");

                // position, rotation, scale
                //Console.WriteLine("ModelInstance: Reading position...");
                iPos = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                //Console.WriteLine($"ModelInstance: position={iPos}");

                //Console.WriteLine("ModelInstance: Reading rotation...");
                iRot = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                //Console.WriteLine($"ModelInstance: rotation={iRot}");

                //Console.WriteLine("ModelInstance: Reading scale...");
                iScale = br.ReadSingle();
                //Console.WriteLine($"ModelInstance: scale={iScale}");

                // optional bounding box
                if (flags.HasFlag(ModelFlags.MOD_HAS_BOUND))
                {
                    //Console.WriteLine("ModelInstance: Reading bounding box...");
                    var lo = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                    var hi = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                    iBound = new AABox(lo, hi);
                    //Console.WriteLine($"ModelInstance: bound={lo}→{hi}");
                }
                else
                {
                    //Console.WriteLine("ModelInstance: No bounding box present");
                }

                // optional name
                //Console.WriteLine("ModelInstance: Reading name length...");
                uint nameLen = br.ReadUInt32();
                //Console.WriteLine($"ModelInstance: nameLen={nameLen}");
                if (nameLen > 0)
                {
                    name = System.Text.Encoding.UTF8.GetString(br.ReadBytes((int)nameLen));
                    //Console.WriteLine($"ModelInstance: name='{name}'");
                }
                else
                {
                    //Console.WriteLine("ModelInstance: No name present");
                }

                //Console.WriteLine("ModelInstance: ReadFromFile completed successfully");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ModelInstance: Exception in ReadFromFile: {ex.Message}");
                return false;
            }
        }
    }
}
