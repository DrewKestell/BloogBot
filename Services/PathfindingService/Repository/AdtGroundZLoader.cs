using PathfindingService;
using System.Collections.Concurrent;
using TerrainLib;

public static class AdtGroundZLoader
{
    private const int CellsPerTile = 16;
    private const float TileScale = 533.33333f;
    private const float ChunkScale = TileScale / CellsPerTile;

    private static readonly ConcurrentDictionary<(int, int, int), AdtFile> _adtCache = new();
    private static string[] _mpqPaths;

    public static void SetMPQPaths(string[] mpqPaths)
    {
        _mpqPaths = mpqPaths;
        PreloadAdts();
    }

    private static void PreloadAdts()
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
                                //Console.WriteLine($"[AdtGroundZLoader] Loading ADT tile {mapId}_{tileX}_{tileY}...");
                                var adt = AdtFile.Load(buf);
                                _adtCache[(mapId, tileX, tileY)] = adt;
                                //Console.WriteLine($"[AdtGroundZLoader] Successfully cached tile {mapId}_{tileX}_{tileY}");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[AdtGroundZLoader] Failed to parse ADT file {filePath}: {ex.Message}");
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
    public static bool TryGetZ(int mapId, float x, float y, out float z)
    {
        // Default failure
        z = float.NaN;

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

        if (hCol >= 8 || hRow >= 8) return false; // guard

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

        public static AdtFile Load(byte[] data)
        {
            using var reader = new BinaryReader(new MemoryStream(data));
            var chunks = new AdtChunk[16, 16];
            int chunkIndex = 0;

            try
            {
                while (reader.BaseStream.Position + 8 < reader.BaseStream.Length && chunkIndex < 256)
                {
                    long chunkStart = reader.BaseStream.Position;
                    string magic = new(reader.ReadChars(4).Reverse().ToArray());
                    int size = reader.ReadInt32();

                    if (magic != "MCNK")
                    {
                        reader.BaseStream.Seek(chunkStart + 8 + size, SeekOrigin.Begin);
                        continue;
                    }

                    reader.BaseStream.Seek(chunkStart + 8 + 128, SeekOrigin.Begin); // skip MCNK header

                    // scan sub-chunks within MCNK
                    long mcnkEnd = chunkStart + 8 + size;
                    float[] heights = null;
                    while (reader.BaseStream.Position + 8 < mcnkEnd)
                    {
                        string subMagic = new(reader.ReadChars(4).Reverse().ToArray());
                        int subSize = reader.ReadInt32();

                        if (subMagic == "MCVT")
                        {
                            if (subSize < 145 * 4)
                            {
                                Console.WriteLine($"[AdtFile] MCVT size too small at chunk {chunkIndex}, skipping.");
                                break;
                            }
                            heights = new float[145];
                            for (int i = 0; i < 145; i++)
                                heights[i] = reader.ReadSingle();
                            break;
                        }
                        else
                        {
                            reader.BaseStream.Seek(reader.BaseStream.Position + subSize, SeekOrigin.Begin);
                        }
                    }

                    if (heights != null)
                    {
                        int row = chunkIndex / 16;
                        int col = chunkIndex % 16;

                        reader.BaseStream.Seek(chunkStart + 8 + 0x68, SeekOrigin.Begin);
                        float posX = reader.ReadSingle();
                        float posY = reader.ReadSingle();
                        float posZ = reader.ReadSingle();

                        // Optional: World-space debug position
                        if (posZ > 34 && posZ < 42)
                        {
                            float worldX = (col + posX) * 33.33333f;
                            float worldY = (row + posY) * 33.33333f;
                            //Console.WriteLine($"[AdtFile] MCNK Pos: ChunkIndex={chunkIndex} WorldX={worldX:F2} WorldY={worldY:F2} Z={posZ} row={row} col={col}");
                        }

                        chunks[row, col] = new AdtChunk   // ✅ row first, col second
{
    Heights    = heights,
    PositionZ  = posZ            // posZ is the *vertical* component
};
                        //Console.WriteLine($"[AdtFile] Stored chunkIndex={chunkIndex} at row={row}, col={col}, height[0]={heights[0]}");
                    }
                    else
                    {
                        Console.WriteLine($"[AdtFile] No MCVT found for chunk {chunkIndex}, skipping.");
                    }

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
    }
}
