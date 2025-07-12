using GameData.Core.Enums;
using GameData.Core.Models;
using PathfindingService.Repository;
using static PathfindingService.Repository.RaceDimensions;

namespace PathfindingService.Tests
{
    public class NavigationFixture : IDisposable
    {
        public Navigation Navigation { get; }

        public NavigationFixture()
        {
            Navigation = new Navigation(); // loaded only once
        }

        public void Dispose()
        {
            // No unmanaged state needs disposal for now
        }
    }

    public class PathingAndOverlapTests(NavigationFixture fixture) : IClassFixture<NavigationFixture>
    {
        private readonly Navigation _navigation = fixture.Navigation;

        [Fact]
        public void CalculatePath_ShouldReturnValidPath()
        {
            uint mapId = 1;
            Position start = new(-616.2514f, -4188.0044f, 82.316719f);
            Position end = new(1629.36f, -4373.39f, 50.2564f);

            var path = _navigation.CalculatePath(mapId, start, end, straightPath: true);

            Assert.NotNull(path);
            Assert.NotEmpty(path);
        }

        [Fact]
        public void GetFloorHeight_ShouldReturnExpectedZ()
        {
            uint mapId = 1;
            Position pos = new(-616.25f, -4188.00f, 82.31f);
            float z = _navigation.GetFloorHeight(mapId, pos.X, pos.Y, pos.Z);

            Assert.True(z > -10000 && z < 10000, $"Z out of range: {z}");
        }

        [Fact]
        public void LineOfSight_ShouldReturnTrue_WhenNoObstruction()
        {
            uint mapId = 1;
            Position from = new(1629.0f, -4373.0f, 53.0f);
            Position to = new(1630.0f, -4372.0f, 53.0f);

            bool los = _navigation.IsLineOfSight(mapId, from, to);

            Assert.True(los, "Expected LOS to be clear.");
        }

        [Fact]
        public void LineOfSight_ShouldReturnFalse_WhenObstructed()
        {
            uint mapId = 389;
            Position from = new(-247.728561f, -30.644503f, -58.082531f);
            Position to = new(-158.395340f, 5.857921f, -42.873611f); // much lower

            bool los = _navigation.IsLineOfSight(mapId, from, to);

            Assert.False(los, "Expected LOS to be blocked by terrain.");
        }

        [Theory]
        [InlineData(0, 537.798401f, 279.534973f, 31.208981f, Race.Human, 2, 1, 0f)]  // Standing on shore
        [InlineData(0, 538.374878f, 279.601929f, 31.150923f, Race.Human, 2, 1, 32.934f)]  // Standing in shallow water
        [InlineData(0, 582.693848f, 342.985321f, 31.149933f, Race.Human, 0, 0, 32.934f)]  // Lordamere Lake swimming in deep area
        [InlineData(0, 623.683838f, 349.455780f, 31.245306f, Race.Orc, 1, 1, 32.934f)]  // Fenris Isle standing
        [InlineData(0, 623.246948f, 349.184143f, 31.149933f, Race.Orc, 1, 1, 32.934f)]  // Fenris Isle swimming
        [InlineData(389, -247.728561f, -30.644503f, -58.082531f, Race.Orc, 2, 3, 0.0f)] // Ragefire ledge (ceiling + floor)
        [InlineData(389, -158.395340f, 5.857921f, -42.873611f, Race.Orc, 0, 1, 0.0f)] // Ragefire ground tile
        public void CapsuleOverlap_ReturnsExpectedPolygons(
        uint mapId,
        float x,
        float y,
        float z,
        Race race,
        int expectedUpperPolys,
        int expectedLowerPolys,
        float expectedWaterZ)
        {
            // Arrange
            var pos = new Position(x, y, z);
            var (radius, height) = GetCapsuleForRace((Game.Race)race);

            // Act
            var polys = _navigation.GetCapsuleOverlaps(mapId, pos, radius, height);

            // Basic sanity
            Assert.NotNull(polys);

            // Partition polys into walkable (floor) vs non‑walkable (potential ceiling/walls)
            var walkablePolys = polys.Where(p => IsGroundAndWalkable(p.Flags)).ToArray();
            var nonWalkablePolys = polys.Where(p => !IsGroundAndWalkable(p.Flags)).ToArray();

            Assert.Equal(expectedLowerPolys, walkablePolys.Length); // lower/floor polys
            Assert.Equal(expectedUpperPolys, nonWalkablePolys.Length); // upper/ceiling polys

            // Assert that any expected water surface is correctly reported via ADT liquid data
            (float, float) adtZs = _navigation.GetADTHeight(mapId, x, y);

            if (expectedWaterZ > 0.0f)
            {
                Assert.InRange(adtZs.Item2, expectedWaterZ - 0.05f, expectedWaterZ + 0.05f);
            }
        }

        // ───────────────────────────────────────────────────────────
        //  Positive samples
        // ───────────────────────────────────────────────────────────
        [Theory]
        [InlineData(0, 623.246948f, 349.184143f, 31.3834095f, 32.93401f)]
        [InlineData(0, -6240.32f, 331.033f, 382.619171f, 0f)]      // Ironforge
        [InlineData(0, -8949.95f, -132.49f, 83.229485f, 0f)]       // Stormwind
        [InlineData(1, -582.580383f, -4236.643973f, 38.04463f, 0f)]// Valley of Trials
        [InlineData(1, -600.4f, -4244.6f, 38.955986f, 0f)]
        [InlineData(1, -576.927856f, -4242.207031f, 37.980587f, 0f)]
        [InlineData(1, 10311.3f, 831.463f, 1326.41077f, 0f)]
        [InlineData(1, -2917.58f, -257.98f, 53.36235f, 0f)]
        public void GetGroundZ_ReturnsExpected(int map, float x, float y, float expected, float expectedLiq)
        {
            (float, float) adtZs = _navigation.GetADTHeight((uint)map, x, y);

            Assert.Equal(expected, adtZs.Item1, precision: 5);
            Assert.Equal(expectedLiq, adtZs.Item2, precision: 5);
        }

        // ───────────────────────────────────────────────────────────
        //  Negative / out‑of‑bounds samples
        // ───────────────────────────────────────────────────────────
        [Theory]
        [InlineData(999, 0f, 0f)]             // invalid map
        [InlineData(0, 999999f, 999999f)]     // far outside any tile
        public void GetGroundZ_ReturnsFalse_WhenOutOfBounds(int map, float x, float y)
        {
            (float, float) adtZs = _navigation.GetADTHeight((uint)map, x, y);

            Assert.Equal(float.NaN, adtZs.Item1);
            Assert.Equal(float.NegativeInfinity, adtZs.Item2);
        }
        private static bool IsGroundAndWalkable(uint flags)
        {
            return (flags & 0x1) != 0 && (flags & 0x10) == 0;
        }
    }
}
