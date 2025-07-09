namespace VMAP
{
    /// <summary>
    /// A triangle defined by indices into a vertex array.
    /// </summary>
    public struct MeshTriangle
    {
        public uint idx0;
        public uint idx1;
        public uint idx2;

        public MeshTriangle(uint a, uint b, uint c)
        {
            idx0 = a; idx1 = b; idx2 = c;
        }
    }
}
