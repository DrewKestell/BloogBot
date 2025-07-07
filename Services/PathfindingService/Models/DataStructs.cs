namespace VMAP
{
    /// <summary>
    /// Results of a point intersection search.
    /// Converted from VMAP::AreaInfo
    /// </summary>
    public struct AreaInfo
    {
        public bool result;
        public float ground_Z;
        public uint flags;
        public int adtId;
        public int rootId;
        public int groupId;

        public AreaInfo()
        {
            result = false;
            ground_Z = float.NegativeInfinity;
            flags = 0;
            adtId = 0;
            rootId = 0;
            groupId = 0;
        }
    }

    /// <summary>
    /// Location information for WMO models.
    /// Converted from VMAP::LocationInfo
    /// </summary>
    public struct LocationInfo
    {
        public ModelInstance? hitInstance;
        public GroupModel? hitModel;
        public float ground_Z;
        public int rootId;

        public LocationInfo()
        {
            hitInstance = null;
            hitModel = null;
            ground_Z = float.NegativeInfinity;
            rootId = -1;
        }
    }
    public struct GroupLocationInfo
    {
        public GroupModel? hitModel;
        public int rootId;
    }

    public struct Triangle<T>
    {
        public T A, B, C;
        public Triangle(T a, T b, T c) { A = a; B = b; C = c; }
    }

    public struct BoundingSphere
    {
        public Vector3 Center; public float Radius;
        public BoundingSphere(Vector3 c, float r) { Center = c; Radius = r; }
        public bool Contains(Vector3 p) => (p - Center).Length() <= Radius;
    }
}