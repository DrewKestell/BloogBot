using System;
using System.IO;

namespace VMAP
{
    /// <summary>
    /// Represents liquid (water) data in a WMO group. Contains a height map grid and usage flags.
    /// </summary>
    public class WmoLiquid
    {
        private uint iTilesX;   // number of tiles in X direction
        private uint iTilesY;   // number of tiles in Y direction
        private Vector3 iCorner; // lower corner of the liquid grid in model local coordinates
        private uint iType;     // liquid type (water, lava, etc.)
        private float[] iHeight; // height values at grid vertices (size: (TilesX+1)*(TilesY+1))
        private byte[] iFlags;   // usage flags for each tile (size: TilesX*TilesY)

        public WmoLiquid(uint tilesX, uint tilesY, Vector3 corner, uint type)
        {
            iTilesX = tilesX;
            iTilesY = tilesY;
            iCorner = corner;
            iType = type;
            iHeight = new float[(tilesX + 1) * (tilesY + 1)];
            iFlags = new byte[tilesX * tilesY];
        }

        /// <summary>
        /// Get the liquid surface height at a given position (model local coordinates).
        /// Returns true if the position is within liquid and sets liqHeight.
        /// </summary>
        public bool GetLiquidHeight(Vector3 pos, out float liqHeight)
        {
            liqHeight = 0f;
            // Compute tile indices relative to the corner
            float relX = pos.x - iCorner.x;
            float relY = pos.y - iCorner.y;
            float tileSize = VMapDefinitions.LIQUID_TILE_SIZE; // typically 533.333/128
            float txf = relX / tileSize;
            float tyf = relY / tileSize;
            int tx = (int)txf;
            int ty = (int)tyf;
            if (txf < 0f || tyf < 0f || tx >= iTilesX || ty >= iTilesY)
                return false;
            // Check if this tile is actually liquid (some tiles might be "hole" or no liquid if flagged)
            int tileIndex = tx + ty * (int)iTilesX;
            if (tileIndex < 0 || tileIndex >= iFlags.Length)
                return false;
            byte flags = iFlags[tileIndex];
            // In WMO liquid data, 0x0F (all bits =1) often indicates no liquid (unused tile).
            if ((flags & 0x0F) == 0x0F)
                return false;
            // Interpolate height from surrounding vertices if needed (but simplest: use nearest vertex or center height).
            // For simplicity, we'll take the lower-left vertex of that tile as liquid height.
            // (In practice, one might bi-linearly interpolate using the 4 surrounding heights.)
            int vertIndex = tx + ty * ((int)iTilesX + 1);
            if (vertIndex < 0 || vertIndex >= iHeight.Length)
                return false;
            liqHeight = iHeight[vertIndex];
            return true;
        }

        public uint GetType() => iType;

        /// <summary>
        /// Reads WmoLiquid data from a binary stream (called after reading a LIQU chunk).
        /// On success, returns a new WmoLiquid object.
        /// </summary>
        // WmoLiquid.cs
        public static bool ReadFromFile(BinaryReader br, out WmoLiquid? liquid)
        {
            liquid = null;
            try
            {
                Console.WriteLine("WmoLiquid: Reading tilesX, tilesY...");
                uint tilesX = br.ReadUInt32();
                uint tilesY = br.ReadUInt32();
                Console.WriteLine($"WmoLiquid: tilesX={tilesX}, tilesY={tilesY}");

                Console.WriteLine("WmoLiquid: Reading corner...");
                Vector3 corner = new(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                Console.WriteLine($"WmoLiquid: corner={corner}");

                Console.WriteLine("WmoLiquid: Reading type...");
                uint type = br.ReadUInt32();
                Console.WriteLine($"WmoLiquid: type={type}");

                var liq = new WmoLiquid(tilesX, tilesY, corner, type);

                int vertCount = (int)((tilesX + 1) * (tilesY + 1));
                Console.WriteLine($"WmoLiquid: heightmap count = {vertCount}");
                for (int i = 0; i < vertCount; ++i)
                {
                    liq.iHeight[i] = br.ReadSingle();
                }

                int tileCount = (int)(tilesX * tilesY);
                Console.WriteLine($"WmoLiquid: flags count = {tileCount}");
                liq.iFlags = br.ReadBytes(tileCount);

                liquid = liq;
                Console.WriteLine("WmoLiquid: ReadFromFile succeeded");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"WmoLiquid: Exception: {e.Message}");
                return false;
            }
        }

    }
}
