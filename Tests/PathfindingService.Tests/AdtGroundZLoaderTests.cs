using System;
using Xunit;
using PathfindingService.Repository;

namespace PathfindingService.Tests
{
    public class AdtGroundZLoaderTests
    {
        private readonly AdtGroundZLoader _loader;

        public AdtGroundZLoaderTests()
        {
            _loader = new AdtGroundZLoader();
        }

        [Theory]
        [InlineData(0, -8949.95f, -132.49f, -9999f)] // Stormwind
        [InlineData(0, -6240.32f, 331.03f, -9999f)]  // Goldshire
        [InlineData(1, -600.4f, -4244.6f, 38.9f)]  // Durotar
        [InlineData(1, -576.927856f, -4242.207031f, 38.203f)]  // Durotar
        public void GetGroundZ_ReturnsReasonableHeight(int mapId, float x, float y, float expectedZ)
        {
            float fallbackZ = -99999.0f;
            float z = _loader.GetGroundZ(mapId, x, y, fallbackZ);

            Console.WriteLine($"Sample Z at ({x}, {y}) on map {mapId} => {z}");

            Assert.True(z > -5000 && z < 5000, $"Z value out of bounds: {z}");
        }

        [Theory]
        [InlineData(999, 0f, 0f)] // Invalid map ID
        [InlineData(0, 999999f, 999999f)] // Invalid coordinates
        public void GetGroundZ_ReturnsFallback_WhenOutOfBounds(int mapId, float x, float y)
        {
            float fallbackZ = -12345.0f;
            float z = _loader.GetGroundZ(mapId, x, y, fallbackZ);
            Assert.Equal(fallbackZ, z);
        }
    }
}
