using PathfindingService;
using System.Collections.Concurrent;
using System.Text;
using TerrainLib;

public class AdtGroundZLoader
{
    private const int CellsPerTile = 16;
    private const float TileScale = 533.33333f;
    private const float ChunkScale = TileScale / CellsPerTile;

    private readonly ConcurrentDictionary<(int, int, int), AdtFile> _adtCache = new();
    private string[] _mpqPaths;

    public AdtGroundZLoader(string[] mpqPaths)
    {
        _mpqPaths = mpqPaths;
        PreloadAdts();
    }

    private void PreloadAdts()
    {
        const uint MPQ_OPEN_FORCE_MPQ_V1 = 0x4;

        foreach (var path in _mpqPaths)
        {
            if (!StormLib.SFileOpenArchive(path, 0, MPQ_OPEN_FORCE_MPQ_V1, out var archive))
            {
                Console.WriteLine($"[AdtGroundZLoader] Failed to open MPQ archive: {path}");
                continue;
            }

            foreach (var (mapId, mapDirName) in MapDirectoryLookup.GetAllMapIds())
            {
                for (int tileX = 0; tileX < 64; tileX++)
                {
                    for (int tileY = 0; tileY < 64; tileY++)
                    {
                        string filePath = $"World\\Maps\\{mapDirName}\\{mapDirName}_{tileX}_{tileY}.adt";
                        //Console.WriteLine($"[AdtGroundZLoader] Attempting to load file: {filePath}");

                        try
                        {
                            if (!StormLib.SFileOpenFileEx(archive, filePath, 0, out var fileHandle))
                            {
                                Console.WriteLine($"[AdtGroundZLoader] Failed to open file: {filePath}");
                                continue;
                            }

                            if (fileHandle == 0)
                                continue;

                            uint len = StormLib.SFileGetFileSize(fileHandle);
                            if (len == 0)
                            {
                                Console.WriteLine($"[AdtGroundZLoader] File size is zero (invalid): {filePath}");
                                StormLib.SFileCloseFile(fileHandle);
                                continue;
                            }

                            byte[] buf = new byte[len];
                            if (!StormLib.SFileReadFile(fileHandle, buf, len, out _, IntPtr.Zero))
                            {
                                Console.WriteLine($"[AdtGroundZLoader] Failed to read file contents: {filePath}");
                                StormLib.SFileCloseFile(fileHandle);
                                continue;
                            }

                            try
                            {
                                var adt = AdtFile.Load(buf);
                                _adtCache[(mapId, tileX, tileY)] = adt;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[AdtGroundZLoader] Failed to parse ADT file {filePath}: {ex.Message} {ex.StackTrace}");
                            }
                            finally
                            {
                                StormLib.SFileCloseFile(fileHandle);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[AdtGroundZLoader] Exception while handling file {filePath}: {ex}");
                        }
                    }
                }
            }

            StormLib.SFileCloseArchive(archive);
        }
    }
    public bool TryGetZ(int mapId, float x, float y, out float z, out float liqZ)
    {
        // Default failure
        z = float.NaN;
        liqZ = float.NegativeInfinity;

        /* ──────────────────────────────────────────────────────────────
           World coordinate system (1.12):
             +X = north, +Y = west
           ADT grid (64×64):
             tileRow (a.k.a. BlockY) increases southwards   ← rows (Y axis)
             tileCol (a.k.a. BlockX) increases eastwards    ← cols (X axis)
        ────────────────────────────────────────────────────────────── */

        // 1) world → 64×64 tile indices
        int tileRow = (int)MathF.Floor(32 - (x / TileScale)); // 0‥63, north→south
        int tileCol = (int)MathF.Floor(32 - (y / TileScale)); // 0‥63, west →east

        if ((uint)tileRow > 63 || (uint)tileCol > 63)
            return false;

        // Key uses (mapId, tileX, tileY) = (col,row)
        if (!_adtCache.TryGetValue((mapId, tileCol, tileRow), out var adt))
            return false;

        // 2) world → local offsets inside that tile (0‥533)
        float originNS = (32 - tileRow) * TileScale;   // world-X at tile’s north edge
        float originEW = (32 - tileCol) * TileScale;   // world-Y at tile’s west  edge

        float localNS = originNS - x;  // south-positive     (0‥533)
        float localEW = originEW - y;  // east-positive      (0‥533)

        // 3) chunk indices (16×16 per tile)
        int chunkRow = (int)(localNS / ChunkScale);    // 0‥15 southward
        int chunkCol = (int)(localEW / ChunkScale);    // 0‥15 eastward

        if ((uint)chunkRow > 15 || (uint)chunkCol > 15)
            return false;

        var chunk = adt.GetChunk(chunkRow, chunkCol);
        if (chunk?.Heights == null) return false;

        if (chunk.HasLiquid)
            liqZ = chunk.LiquidLevel;

        // 4) local offsets inside chunk (0‥33.33)
        float inNS = localNS - chunkRow * ChunkScale;
        float inEW = localEW - chunkCol * ChunkScale;

        // 5) cell indices (8×8 per chunk) and intra-cell fractions
        float normX = inEW / ChunkScale * 8f;     // west→east
        float normY = inNS / ChunkScale * 8f;     // south→north

        int hCol = (int)normX;   // 0‥7
        int hRow = (int)normY;   // 0‥7
        float fx = normX - hCol;
        float fy = normY - hRow;

        if (hCol >= 8 || hRow >= 8) return chunk.HasLiquid; // guard

        // 6) sample the four surrounding outer-grid heights
        float h0 = chunk.Heights[hRow * 9 + hCol]; // top-left
        float h1 = chunk.Heights[hRow * 9 + hCol + 1]; // top-right
        float h2 = chunk.Heights[(hRow + 1) * 9 + hCol + 1]; // bottom-right
        float h3 = chunk.Heights[(hRow + 1) * 9 + hCol]; // bottom-left

        // 7) triangle-based interpolation (vMaNGOS / client logic)
        float relZ = (fx + fy < 1f)
                   ? h0 + (h1 - h0) * fx + (h3 - h0) * fy
                   : h2 + (h3 - h2) * (1f - fx) + (h1 - h2) * (1f - fy);

        // 8) absolute ground height
        z = relZ + chunk.PositionZ;

        return true;
    }
}

public static class MapDirectoryLookup
{
    // ─────────────────────────────────────────────────────────────
    // 1.  MapId → directory (under "World/Maps/")
    // ─────────────────────────────────────────────────────────────
    public static string GetInternalMapName(int mapId) => mapId switch
    {
        // Continents
        0 => "Azeroth",            // Eastern Kingdoms
        1 => "Kalimdor",

        // Battlegrounds
        30 => "AlteracValley",
        37 => "AzsharaCrater",      // never released, but ADTs exist
        489 => "WarsongGulch",
        529 => "ArathiBasin",

        // Unused / dev outdoor worlds (all contain ADTs)
        169 => "EmeraldDream",
        401 => "Kalidar",
        451 => "DevelopmentLand",

        _ => throw new ArgumentOutOfRangeException(
                 $"Unknown (or post-TBC) mapId: {mapId}")
    };

    // ─────────────────────────────────────────────────────────────
    // 2.  directory  → MapId
    // ─────────────────────────────────────────────────────────────
    public static bool TryGetMapId(string mapDirName, out int mapId)
    {
        switch (mapDirName.ToLowerInvariant())
        {
            case "azeroth": mapId = 0; return true;
            case "kalimdor": mapId = 1; return true;
            case "alteracvalley": mapId = 30; return true;
            case "azsharacrater": mapId = 37; return true;
            case "warsonggulch": mapId = 489; return true;
            case "arathibasin": mapId = 529; return true;
            case "emeralddream": mapId = 169; return true;
            case "kalidar": mapId = 401; return true;
            case "developmentland": mapId = 451; return true;
            default: mapId = -1; return false;
        }
    }

    // ─────────────────────────────────────────────────────────────
    // 3.  Convenience list for preload / iteration
    // ─────────────────────────────────────────────────────────────
    public static (int id, string dir)[] GetAllMapIds() =>
    [
        (  0, "Azeroth"        ),
        (  1, "Kalimdor"       ),
        ( 30, "AlteracValley"  ),
        ( 37, "AzsharaCrater"  ),
        (169, "EmeraldDream"   ),
        (401, "Kalidar"        ),
        (451, "DevelopmentLand"),
        (489, "WarsongGulch"   ),
        (529, "ArathiBasin"    )
    ];
}

namespace TerrainLib
{
    public class AdtFile(AdtChunk[,] chunks)
    {
        private readonly AdtChunk[,] _chunks = chunks;

        public AdtChunk GetChunk(int row, int col)
        {
            if (row is < 0 or > 15 || col is < 0 or > 15)
                return null;
            return _chunks[row, col];
        }
        private static string ReadTag(BinaryReader br) =>
        Encoding.ASCII.GetString([.. br.ReadBytes(4).Reverse()]);

        public static AdtFile Load(byte[] data)
        {
            using var reader = new BinaryReader(new MemoryStream(data));
            var chunks = new AdtChunk[16, 16];
            int chunkIndex = 0;

            try
            {
                while (reader.BaseStream.Position + 8 <= reader.BaseStream.Length && chunkIndex < 256)
                {
                    /*── read MCNK header stub ───────────────────────────*/
                    long chunkStart = reader.BaseStream.Position;
                    string tag = ReadTag(reader);               // "KNCM" → "MCNK"
                    int size = reader.ReadInt32();

                    if (tag != "MCNK")
                    {
                        reader.BaseStream.Seek(chunkStart + 8 + size, SeekOrigin.Begin);
                        continue;
                    }

                    long headerPos = chunkStart + 8;                 // start of 128‑byte header
                    long chunkEnd = chunkStart + 8 + size;
                    long payloadPos = headerPos + 128;              // first sub‑chunk

                    /*── header fields ─────────────────────────────────*/
                    reader.BaseStream.Seek(headerPos, SeekOrigin.Begin);
                    uint mcnkFlags = reader.ReadUInt32();             // bit‑field: lq_* etc.

                    // Vanilla layout: liquid_offset @ 0x60, liquid_size @ 0x64
                    reader.BaseStream.Seek(headerPos + 0x60, SeekOrigin.Begin);
                    uint ofsLiquid = reader.ReadUInt32();            // *rel. to chunkStart*
                    uint sizeLiquid = reader.ReadUInt32();

                    /*── liquid extraction ─────────────────────────────*/
                    bool hasLiquid = false;
                    float liquidLevel = float.NaN;

                    if (!(sizeLiquid == 0 && ofsLiquid == 0))
                    {
                        if (ofsLiquid > 0)
                        {
                            long lqPos = chunkStart + ofsLiquid;
                            if (lqPos + 8 <= chunkEnd)
                            {
                                reader.BaseStream.Seek(lqPos, SeekOrigin.Begin);
                                string lqTag = ReadTag(reader);

                                if (lqTag == "MCLQ" && reader.BaseStream.Position + 12 <= reader.BaseStream.Length)
                                {
                                    /*── parse this block ───────────────*/
                                    int length = (int)(reader.BaseStream.Length - reader.BaseStream.Position);
                                    byte[] raw = reader.ReadBytes(length);

                                    if (length >= 12)
                                    {
                                        uint xVerts = BitConverter.ToUInt32(raw, 0);
                                        uint yVerts = BitConverter.ToUInt32(raw, 4);
                                        float baseH = BitConverter.ToSingle(raw, 8);

                                        if (!float.IsNaN(baseH))
                                        {
                                            hasLiquid = true;
                                            liquidLevel = baseH;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    /*── terrain height map ───────────────────────────*/
                    reader.BaseStream.Seek(payloadPos, SeekOrigin.Begin);
                    float[] heights = null;

                    while (reader.BaseStream.Position + 8 <= chunkEnd && heights == null)
                    {
                        long subStart = reader.BaseStream.Position;
                        string subTag = ReadTag(reader);
                        int subSize = reader.ReadInt32();

                        if (subTag == "MCVT" && subSize >= 145 * 4)
                        {
                            heights = new float[145];
                            for (int i = 0; i < 145; i++)
                                heights[i] = reader.ReadSingle();

                            int pad = subSize - 145 * 4;
                            if (pad > 0)
                                reader.BaseStream.Seek(pad, SeekOrigin.Current);
                            break;
                        }

                        reader.BaseStream.Seek(subStart + 8 + subSize, SeekOrigin.Begin);
                    }

                    /*── persist chunk ───────────────────────────────*/
                    if (heights != null)
                    {
                        int row = chunkIndex / 16;
                        int col = chunkIndex % 16;

                        reader.BaseStream.Seek(headerPos + 0x68, SeekOrigin.Begin); // posX/posY/posZ still at 0x68 in vanilla
                        _ = reader.ReadSingle();   // posX (unused)
                        _ = reader.ReadSingle();   // posY (unused)
                        float posZ = reader.ReadSingle();

                        chunks[row, col] = new AdtChunk
                        {
                            Heights = heights,
                            PositionZ = posZ,
                            HasLiquid = hasLiquid,
                            LiquidLevel = liquidLevel
                        };
                    }

                    /*── next MCNK ───────────────────────────────────*/
                    chunkIndex++;
                    reader.BaseStream.Seek(chunkStart + 8 + size, SeekOrigin.Begin);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AdtFile] Error reading ADT: {ex.Message}");
                throw;
            }

            return new AdtFile(chunks);
        }
    }

    public class AdtChunk
    {
        public float PositionZ;

        public float[] Heights = new float[145];
        public bool HasLiquid = false;
        public float LiquidLevel = float.NaN;
    }
}