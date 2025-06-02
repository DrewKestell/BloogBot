using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace PathfindingService.Repository
{
    public class AdtGroundZLoader
    {
        private readonly List<string> _mpqPaths;
        private readonly Dictionary<int, string> _mapIdToDir;

        private const float TileSize = 533.33333f;
        private const float ZeroPoint = 32.0f * TileSize;
        private const float InvalidHeight = -99999.0f;

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

        public float GetGroundZ(int mapId, float x, float y, float currentZ)
        {
            var z = SampleTerrainZ(mapId, x, y);
            return z != InvalidHeight ? z : currentZ;
        }

        private float SampleTerrainZ(int mapId, float x, float y)
        {
            if (!_mapIdToDir.TryGetValue(mapId, out var mapDir))
                return InvalidHeight;

            float adjustedX = x + ZeroPoint;
            float adjustedY = y + ZeroPoint;

            int tileX = (int)Math.Floor(adjustedX / TileSize);
            int tileY = (int)Math.Floor(adjustedY / TileSize);

            if (tileX < 0 || tileX >= 64 || tileY < 0 || tileY >= 64)
                return InvalidHeight;

            string adtPath = $"World\\Maps\\{mapDir}\\{mapDir}_{tileX}_{tileY}.adt";

            byte[] adtData = ReadFromMPQ(adtPath);
            if (adtData == null)
                return InvalidHeight;

            return ParseZFromAdt(adtData, adjustedX % TileSize, adjustedY % TileSize);
        }

        private float ParseZFromAdt(byte[] data, float localX, float localY)
        {
            const int HEADER_SIZE = 8;

            using var stream = new MemoryStream(data);
            using var reader = new BinaryReader(stream);

            long fileSize = stream.Length;
            long mcinOffset = -1;

            while (stream.Position + HEADER_SIZE < fileSize)
            {
                string tag = new(reader.ReadChars(4).Reverse().ToArray());
                uint size = reader.ReadUInt32();

                if (tag == "MCIN")
                {
                    mcinOffset = stream.Position;
                    break;
                }
                stream.Position += size;
            }

            if (mcinOffset < 0) return InvalidHeight;

            stream.Position = mcinOffset;
            var offsets = new uint[256];
            for (int i = 0; i < 256; i++)
            {
                offsets[i] = reader.ReadUInt32();
                stream.Position += 12;
            }

            int chunkX = (int)(localX / 33.333f);
            int chunkY = (int)(localY / 33.333f);
            if (chunkX > 15 || chunkY > 15) return InvalidHeight;

            uint offset = offsets[chunkY * 16 + chunkX];
            if (offset == 0 || offset >= fileSize) return InvalidHeight;

            stream.Position = offset;
            string cTag = new(reader.ReadBytes(4).Reverse().Select(b => (char)b).ToArray());
            if (cTag != "MCNK") return InvalidHeight;

            uint chunkSize = reader.ReadUInt32();
            stream.Position += 0x80;

            float[] heights = new float[145];
            long chunkEnd = stream.Position + chunkSize;

            while (stream.Position + HEADER_SIZE < chunkEnd)
            {
                string subTag = new(reader.ReadChars(4).Reverse().ToArray());
                uint subSize = reader.ReadUInt32();

                if (subTag == "MCVT")
                {
                    for (int i = 0; i < 145; i++)
                        heights[i] = reader.ReadSingle();
                    break;
                }
                stream.Position += subSize;
            }

            float subX = localX % 33.333f / (33.333f / 8f);
            float subY = localY % 33.333f / (33.333f / 8f);
            int gx = Math.Clamp((int)subX, 0, 7);
            int gy = Math.Clamp((int)subY, 0, 7);

            int row1 = gy * 17 + gx;
            int row2 = row1 + 1;
            int row3 = row1 + 9;
            int row4 = row3 + 1;

            float fracX = subX - gx;
            float fracY = subY - gy;

            float z1 = heights[row1];
            float z2 = heights[row2];
            float z3 = heights[row3];
            float z4 = heights[row4];

            float zTop = z1 + (z2 - z1) * fracX;
            float zBottom = z3 + (z4 - z3) * fracX;

            return zTop + (zBottom - zTop) * fracY;
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
                    int fileSize = (int)StormLib.SFileGetFileSize(file);
                    byte[] buffer = new byte[fileSize];

                    if (StormLib.SFileReadFile(file, buffer, (uint)fileSize, out uint bytesRead, IntPtr.Zero))
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
