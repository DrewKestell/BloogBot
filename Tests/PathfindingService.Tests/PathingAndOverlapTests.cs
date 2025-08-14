using GameData.Core.Models;
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

        private const float Dt = 0.1f;
        /* ──────────────────────────────────────────────────────── */
        /*  PATH‑FINDING + LINE‑OF‑SIGHT BASICS                    */
        /* ──────────────────────────────────────────────────────── */

        [Fact]
        public void CalculatePath_ShouldReturnValidPath()
        {
            uint mapId = 1;
            Position start = new(-616.2514f, -4188.0044f, 82.316719f);
            Position end = new(1629.36f, -4373.39f, 50.2564f);

            var path = _navigation.CalculatePath(mapId, start.ToXYZ(), end.ToXYZ(), smoothPath: true);

            Assert.NotNull(path);
            Assert.NotEmpty(path);
        }

        [Fact]
        public void LineOfSight_ShouldReturnTrue_WhenNoObstruction()
        {
            uint mapId = 1;
            Position from = new(1629.0f, -4373.0f, 53.0f);
            Position to = new(1630.0f, -4372.0f, 53.0f);

            Assert.True(_navigation.LineOfSight(mapId, from.ToXYZ(), to.ToXYZ()));
        }

        [Fact]
        public void LineOfSight_ShouldReturnFalse_WhenObstructed()
        {
            uint mapId = 389;
            Position from = new(-247.728561f, -30.644503f, -58.082531f);
            Position to = new(-158.395340f, 5.857921f, -42.873611f);

            Assert.False(_navigation.LineOfSight(mapId, from.ToXYZ(), to.ToXYZ()));
        }
    }
}
