using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PathfindingService.Repository
{
    public class AdtGroundZLoader
    {
        private readonly List<string> _mpqPaths;
        private readonly Dictionary<int, string> _mapIdToDir;

        private const float TileSize = 533.33333f;
        private const float InvalidHeight = -99999.0f;
        private const int ExpectedVersion = 18;

        public AdtGroundZLoader()
        {
            string basePath = Path.Combine(AppContext.BaseDirectory, "Data");
            _mpqPaths = [Path.Combine(basePath, "terrain.MPQ")];
            _mapIdToDir = new Dictionary<int, string>
            {
                { 0, "Azeroth" },
                { 1, "Kalimdor" },
                { 530, "Expansion01" },
                { 571, "Northrend" }
            };
        }

        public float GetGroundZ(int mapId, float x, float y, float fallbackZ)
        {
            if (!_mapIdToDir.TryGetValue(mapId, out var mapDir))
                return fallbackZ;

            float adjustedX = x + 32 * TileSize;
            float adjustedY = y + 32 * TileSize;
            int tileX = (int)Math.Floor(adjustedX / TileSize);
            int tileY = (int)Math.Floor(adjustedY / TileSize);

            if (tileX < 0 || tileX >= 64 || tileY < 0 || tileY >= 64)
                return fallbackZ;

            string adtPath = $"World\\Maps\\{mapDir}\\{mapDir}_{tileX}_{tileY}.adt";
            byte[] data = ReadFromMPQ(adtPath);
            if (data == null) return fallbackZ;

            using var ms = new MemoryStream(data);
            using var reader = new BinaryReader(ms);

            bool versionOk = false;
            while (ms.Position + 8 <= ms.Length)
            {
                string tag = new string(reader.ReadChars(4));
                uint size = reader.ReadUInt32();
                long start = ms.Position;

                if (tag == "REVM")
                {
                    int version = reader.ReadInt32();
                    versionOk = version == ExpectedVersion;
                    break;
                }

                ms.Position = start + size;
            }

            if (!versionOk)
                return fallbackZ;

            float localX = adjustedX - tileX * TileSize;
            float localY = adjustedY - tileY * TileSize;
            int chunkX = (int)(localX / (TileSize / 16));
            int chunkY = (int)(localY / (TileSize / 16));

            Console.WriteLine($"X:{x}, Y:{y} -> chunkX:{chunkX}, chunkY:{chunkY}");


            float cellX = (localX % (TileSize / 16)) / (TileSize / 16);
            float cellY = (localY % (TileSize / 16)) / (TileSize / 16);
            ms.Position = 0;
            while (ms.Position + 8 <= ms.Length)
            {
                string tag = new string(reader.ReadChars(4));
                uint size = reader.ReadUInt32();
                long chunkStart = ms.Position;

                if (tag == "KNCM")
                {
                    ms.Position = chunkStart + 4;
                    int ix = reader.ReadInt32();
                    int iy = reader.ReadInt32();

                    if (ix == chunkX && iy == chunkY)
                    {
                        ms.Position = chunkStart + 0x14;
                        uint ofsMCVT = reader.ReadUInt32();

                        if (ofsMCVT > 0 && ofsMCVT < size)
                        {
                            ms.Position = chunkStart + ofsMCVT;
                            float[] h = new float[145];
                            for (int i = 0; i < 145; i++)
                                h[i] = reader.ReadSingle();

                            int row = (int)(cellX * 8);
                            int col = (int)(cellY * 8);

                            row = Math.Clamp(row, 0, 7);
                            col = Math.Clamp(col, 0, 7);

                            int i00 = row * 9 + col;     // h1
                            int i10 = (row + 1) * 9 + col; // h3
                            int i01 = row * 9 + col + 1;   // h2
                            int i11 = (row + 1) * 9 + col + 1; // h4
                            int im = 81 + row * 8 + col;   // center (V8 index), offset by 81

                            float fx = cellX * 8 - row;
                            float fy = cellY * 8 - col;

                            Console.WriteLine($"h5 = 2 * h[{im}], row={row}, col={col}");

                            float z;
                            if (fx + fy < 1.0f)
                            {
                                if (fx > fy)
                                {
                                    float h1 = h[i00];
                                    float h2 = h[i01];
                                    float h5 = 2 * h[im];
                                    z = h2 - h1;
                                    z = z * fx + (h5 - h1 - h2) * fy + h1;
                                }
                                else
                                {
                                    float h1 = h[i00];
                                    float h3 = h[i10];
                                    float h5 = 2 * h[im];
                                    z = (h5 - h1 - h3) * fx + (h3 - h1) * fy + h1;
                                }
                            }
                            else
                            {
                                if (fx > fy)
                                {
                                    float h2 = h[i01];
                                    float h4 = h[i11];
                                    float h5 = 2 * h[im];
                                    z = (h2 + h4 - h5) * fx + (h4 - h2) * fy + h5 - h4;
                                }
                                else
                                {
                                    float h3 = h[i10];
                                    float h4 = h[i11];
                                    float h5 = 2 * h[im];
                                    z = (h4 - h3) * fx + (h3 + h4 - h5) * fy + h5 - h4;
                                }
                            }

                            return z + 0.05f;
                        }
                    }
                }

                ms.Position = chunkStart + size;
            }

            return fallbackZ;
        }

        private byte[] ReadFromMPQ(string internalPath)
        {
            const uint MPQ_OPEN_FORCE_MPQ_V1 = 0x00000004;

            foreach (var path in _mpqPaths)
            {
                if (!StormLib.SFileOpenArchive(path, 0, MPQ_OPEN_FORCE_MPQ_V1, out IntPtr archive))
                    continue;

                if (!StormLib.SFileHasFile(archive, internalPath))
                {
                    StormLib.SFileCloseArchive(archive);
                    continue;
                }

                if (StormLib.SFileOpenFileEx(archive, internalPath, 0, out IntPtr file))
                {
                    uint fileSize = StormLib.SFileGetFileSize(file);
                    byte[] buffer = new byte[fileSize];

                    if (StormLib.SFileReadFile(file, buffer, fileSize, out uint bytesRead, IntPtr.Zero))
                    {
                        StormLib.SFileCloseFile(file);
                        StormLib.SFileCloseArchive(archive);
                        return bytesRead != fileSize ? buffer[..(int)bytesRead] : buffer;
                    }

                    StormLib.SFileCloseFile(file);
                }
                StormLib.SFileCloseArchive(archive);
            }

            return null;
        }
    }
}