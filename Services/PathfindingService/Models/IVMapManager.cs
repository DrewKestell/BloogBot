using System;

namespace VMAP
{
    /// <summary>
    /// Outcome codes for loading VMAP data.
    /// </summary>
    public enum VMAPLoadResult
    {
        Error,
        Success,
        Ignored
    }

    /// <summary>
    /// Abstract interface for VMap collision manager.
    /// Provides methods to load/unload maps and perform collision queries.
    /// </summary>
    public abstract class IVMapManager
    {
        protected bool enableLineOfSightCalc = true;
        protected bool enableHeightCalc = true;
        protected bool useManagedModelStorage = true; // whether to unload models when not referenced (weak vs strong refs)

        public virtual void SetEnableLineOfSightCalc(bool val) { enableLineOfSightCalc = val; }
        public virtual void SetEnableHeightCalc(bool val) { enableHeightCalc = val; }
        /// <summary>
        /// Set whether model pointers are managed (unloaded when no references).
        /// If false, models stay loaded until explicitly cleared.
        /// </summary>
        public virtual void SetUseManagedModelStorage(bool val) { useManagedModelStorage = val; }

        public bool IsLineOfSightCalcEnabled() => enableLineOfSightCalc;
        public bool IsHeightCalcEnabled() => enableHeightCalc;
        public bool IsMapLoadingEnabled() => enableLineOfSightCalc || enableHeightCalc;
        public bool IsManagedStorageEnabled() => useManagedModelStorage;

        // Abstract methods to implement
        public abstract VMAPLoadResult LoadMap(string basePath, uint mapId, int tileX, int tileY);
        public abstract void UnloadMap(uint mapId, int tileX, int tileY);
        public abstract void UnloadMap(uint mapId);
        public abstract bool IsInLineOfSight(uint mapId, float x1, float y1, float z1, float x2, float y2, float z2, bool ignoreM2);
        public abstract float GetHeight(uint mapId, float x, float y, float z, float maxSearchDist);
        public abstract bool GetObjectHitPos(uint mapId, float x1, float y1, float z1,
                                             float x2, float y2, float z2, out float rx, out float ry, out float rz, float modifyDist);
        public abstract ModelInstance? FindCollisionModel(uint mapId, float x, float y, float z, float tx, float ty, float tz);
        public abstract bool GetAreaInfo(uint mapId, float x, float y, ref float z, out uint flags, out int adtId, out int rootId, out int groupId);
        public abstract bool IsUnderModel(uint mapId, float x, float y, float z, out float outDist, out float inDist);
        public abstract bool GetLiquidLevel(uint mapId, float x, float y, float z, byte requiredLiquidType, out float level, out float floor, out uint type);
        public abstract string GetDirFileName(uint mapId, int tileX, int tileY);
        public abstract bool ExistsMap(string basePath, uint mapId, int tileX, int tileY);
    }
}
