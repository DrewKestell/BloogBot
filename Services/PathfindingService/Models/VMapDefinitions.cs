using System.Text;

namespace VMAP
{
    /// <summary>
    /// Constants and utility methods for VMAP binary file formats.
    /// </summary>
    public static class VMapDefinitions
    {
        // Magic headers for vmap files
        public const string VMAP_MAGIC = "VMAP_7.0";        // Header in final vmap files (.vmtree, .vmtile, .vmo)
        public const string RAW_VMAP_MAGIC = "VMAPs05";     // Header in raw extracted vmap files (not used here)
        public const string GAMEOBJECT_MODELS = "temp_gameobject_models"; // Filename for gameobject models data
        public const float LIQUID_TILE_SIZE = 533.333f / 128;
        public const float INVALID_HEIGHT = float.NegativeInfinity;
        public const float INVALID_HEIGHT_VALUE = float.NegativeInfinity;

        /// <summary>
        /// Reads a chunk tag from the binary stream and verifies it matches the expected tag.
        /// This ensures we are reading the correct section of the file.
        /// </summary>
        /// <param name="br">BinaryReader for the stream (little-endian)</param>
        /// <param name="expectedTag">The expected 4- or 8-character tag string</param>
        /// <param name="length">Length of tag in bytes (e.g. 4 or 8)</param>
        /// <returns>true if the tag was read and matched, false otherwise</returns>
        public static bool ReadChunk(BinaryReader br, string expectedTag, int length)
        {
            byte[] tagBytes = br.ReadBytes(length);
            if (tagBytes.Length < length)
                return false; // Unexpected EOF
            string tag = Encoding.ASCII.GetString(tagBytes);
            return tag == expectedTag;
        }
    }
}
