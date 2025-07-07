using VMAP;

namespace G3D
{
    public static class MathUtil
    {
        public const float inf = float.PositiveInfinity;
        public static float fnan() => float.NaN;

        public static bool fuzzyEq(float a, float b, float eps = 1e-6f) => MathF.Abs(a - b) <= eps;
        public static bool fuzzyNe(float a, float b, float eps = 1e-6f) => !fuzzyEq(a, b, eps);


        /// <summary>Reads 4 bytes and verifies the tag equals <paramref name="id"/>
        /// or its byte-reversed form (older little-endian writers).</summary>
        public static bool ReadChunk(BinaryReader br, string id)
        {
            Span<byte> buf = stackalloc byte[4];
            if (br.Read(buf) != 4) return false;
            string tag = System.Text.Encoding.ASCII.GetString(buf);
            string rev = new string(id.Reverse().ToArray());
            return tag == id || tag == rev;
        }
    }
    public static class ModelFlagUtil
    {
        /// <summary>Bit test helper to silence “Operator & cannot be applied…”</summary>
        public static bool HasAny(this ModelFlags f, uint mask) =>
            ((uint)f & mask) != 0;
    }
}
