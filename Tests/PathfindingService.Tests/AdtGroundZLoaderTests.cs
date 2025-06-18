using System.Reflection;

namespace PathfindingService.Tests
{
    public class AdtGroundZLoaderTests
    {
        public AdtGroundZLoaderTests()
        {
            var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            var mpqPath = Path.Combine(baseDir, "Data", "terrain.MPQ");
            AdtGroundZLoader.SetMPQPaths([mpqPath]);
        }

        // ───────────────────────────────────────────────────────────
        //  Positive samples
        // ───────────────────────────────────────────────────────────
        [Theory]
        [InlineData(0, -6240.32f, 331.033f, 382.619171f)]   // Ironforge
        [InlineData(0, -8949.95f, -132.49f, 83.229485f)]    // Stormwind
        [InlineData(1, -582.580383f, -4236.643973f, 38.04463f)]     // Valley of Trials
        [InlineData(1, -600.4f, -4244.6f, 38.955986f)]
        [InlineData(1, -576.927856f, -4242.207031f, 37.980587f)]
        [InlineData(1, 10311.3, 831.463, 1326.41077)]
        [InlineData(1, -2917.58, -257.98, 53.36235)]
        [InlineData(0, 1676.35, 1677.45, 135.20277)]
        public void GetGroundZ_ReturnsExpected(int map, float x, float y, float expected)
        {
            Assert.True(AdtGroundZLoader.TryGetZ(map, x, y, out var z));
            Assert.Equal(expected, z, precision: 5);   // float tolerance
        }

        // ───────────────────────────────────────────────────────────
        //  Negative / out-of-bounds samples
        // ───────────────────────────────────────────────────────────
        [Theory]
        [InlineData(999, 0f, 0f)]        // invalid map
        [InlineData(0, 999999f, 999999f)]   // far outside any tile
        public void GetGroundZ_ReturnsFalse_WhenOutOfBounds(int map, float x, float y)
        {
            Assert.False(AdtGroundZLoader.TryGetZ(map, x, y, out float z));
        }
    }
}
