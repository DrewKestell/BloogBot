using GameData.Core.Constants;
using GameData.Core.Enums;
using GameData.Core.Models;
using Pathfinding;
using PathfindingService.Repository;

namespace PathfindingService.Tests
{
    /// <summary>
    /// End‑to‑end tests for the consolidated Navigation API.
    ///   • CalculatePath
    ///   • IsLineOfSight
    ///   • GetTerrainProbe (capsule sweep + height + liquid)
    /// </summary>
    public class NavigationFixture : IDisposable
    {
        public Navigation Navigation { get; }

        public NavigationFixture() => Navigation = new Navigation();

        public void Dispose() { /* Navigation lives for the AppDomain – nothing to do. */ }
    }

    public class PathingAndTerrainTests(NavigationFixture fixture) : IClassFixture<NavigationFixture>
    {
        private readonly Navigation _navigation = fixture.Navigation;

        /* ──────────────────────────────────────────────────────── */
        /*  PATH‑FINDING + LINE‑OF‑SIGHT BASICS                    */
        /* ──────────────────────────────────────────────────────── */

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
        public void LineOfSight_ShouldReturnTrue_WhenNoObstruction()
        {
            uint mapId = 1;
            Position from = new(1629.0f, -4373.0f, 53.0f);
            Position to = new(1630.0f, -4372.0f, 53.0f);

            Assert.True(_navigation.IsLineOfSight(mapId, from, to));
        }

        [Fact]
        public void LineOfSight_ShouldReturnFalse_WhenObstructed()
        {
            uint mapId = 389;
            Position from = new(-247.728561f, -30.644503f, -58.082531f);
            Position to = new(-158.395340f, 5.857921f, -42.873611f);

            Assert.False(_navigation.IsLineOfSight(mapId, from, to));
        }

        /* ──────────────────────────────────────────────────────── */
        /*  CONSOLIDATED TERRAIN‑PROBE TEST                        */
        /* ──────────────────────────────────────────────────────── */

        [Theory]
        //  map   x              y               z              race        upper lower groundZ         liquidZ
        // ────────────────────────────────────────────────────────────────────────────────────────────────────────────
        [InlineData(0, 537.798401f, 279.534973f, 31.208981f, Race.Human, 2, 1, 27.96183, 32.93401)] // Shore
        [InlineData(0, 538.374878f, 279.601929f, 31.150923f, Race.Human, 2, 1, 28.45064, 32.934f)] // Shallow water
        [InlineData(0, 582.693848f, 342.985321f, 31.149933f, Race.Human, 0, 0, 22.83199, 32.934f)] // Swim (deep)
        [InlineData(0, 623.683838f, 349.455780f, 31.245306f, Race.Orc, 1, 1, 31.59709, 32.934f)] // Fenris shore
        [InlineData(0, 623.246948f, 349.184143f, 31.149933f, Race.Orc, 1, 1, 31.3834095f, 32.93401f)] // Fenris swim + ground
        [InlineData(389, -247.728561f, -30.644503f, -58.082531f, Race.Orc, 2, 3, float.NaN, float.NegativeInfinity)] // RFC ledge (ceiling & floor)
        [InlineData(389, -158.395340f, 5.857921f, -42.873611f, Race.Orc, 0, 1, float.NaN, float.NegativeInfinity)] // RFC ground tile
        [InlineData(0, -6240.32f, 331.033f, 382.619171f, Race.Human, 0, 2, 382.619171f, 0f)]
        [InlineData(0, -8949.95f, -132.49f, 83.229485f, Race.Human, 0, 1, 83.229485f, 0f)]
        [InlineData(1, -582.580383f, -4236.643973f, 38.04463f, Race.Orc, 0, 3, 38.04463f, 0f)]
        [InlineData(1, -600.4f, -4244.6f, 38.955986f, Race.Orc, 0, 2, 38.955986f, 0f)]
        [InlineData(1, -576.927856f, -4242.207031f, 37.980587f, Race.Orc, 0, 4, 37.980587f, 0f)]
        [InlineData(1, 10311.3f, 831.463f, 1326.41077f, Race.Orc, 0, 4, 1326.41077f, 0f)]
        [InlineData(1, -2917.58f, -257.98f, 53.36235f, Race.Orc, 0, 2, 53.36235f, 0f)]
        // out‑of‑bounds → NaN / -∞
        [InlineData(999, 0f, 0f, 0f, Race.Human, 0, 0, float.NaN, float.NegativeInfinity)]
        [InlineData(0, 999999f, 999999f, 0f, Race.Human, 0, 0, float.NaN, float.NegativeInfinity)]
        public void TerrainProbe_AllExpectationsMet(uint mapId, float x, float y, float z,
                                                    Race race, int expUpper, int expLower,
                                                    float expGroundZ, float expLiquidZ)
        {
            var posProto = new Game.Position { X = x, Y = y, Z = z };
            var (radius, height) = RaceDimensions.GetCapsuleForRace(race);

            var probe = _navigation.GetTerrainProbe(mapId, posProto, radius, height);
            Assert.NotNull(probe);

            // ─── Polygon expectations ───────────────────────────
            if (expUpper >= 0 && expLower >= 0)
            {
                var walkable = probe.Overlaps.Count(IsGroundAndWalkable);
                var nonWalkable = probe.Overlaps.Count - walkable;

                Assert.Equal(expLower, walkable);   // floor polys
                Assert.Equal(expUpper, nonWalkable); // ceiling/other polys
            }

            // ─── Ground‑Z expectations ─────────────────────────
            if (!float.IsNaN(expGroundZ))
                Assert.Equal(expGroundZ, probe.GroundZ, precision: 5);
            else if (float.IsNaN(expGroundZ))
                Assert.True(float.IsNaN(probe.GroundZ));

            // ─── Liquid‑Z expectations ─────────────────────────
            if (float.IsNegativeInfinity(expLiquidZ))
                Assert.Equal(float.NegativeInfinity, probe.LiquidZ);
            else if (expLiquidZ > 0f)
                Assert.InRange(probe.LiquidZ, expLiquidZ - 0.05f, expLiquidZ + 0.05f);
            else
                Assert.Equal(0f, probe.LiquidZ, precision: 5);
        }

        /* ──────────────────────────────────────────────────────── */
        /*  Helpers                                               */
        /* ──────────────────────────────────────────────────────── */
        private static bool IsGroundAndWalkable(NavPolyHit poly)
        {
            return poly.Area == NavTerrain.NavGround &&
                   poly.Flags.HasFlag(NavPolyFlag.PolyFlagWalk) &&
                   !poly.Flags.HasFlag(NavPolyFlag.PolyFlagDisabled);
        }
    }
}
