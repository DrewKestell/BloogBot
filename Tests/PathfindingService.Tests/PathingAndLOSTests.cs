using GameData.Core.Models;
using Pathfinding;
using PathfindingService.Repository;
using System.Numerics;

namespace PathfindingService.Tests
{
    public class PathingAndLOSTests
    {
        private readonly Navigation _navigation = new();

        [Fact]
        public void CalculatePath_ShouldReturnValidPath()
        {
            uint mapId = 389u;
            Position start = new(3.81f, -14.82f, -17.84f);
            Position end = new(-230.133f, 191.085f, -24.9191f);
            bool pathSmoothing = false;

            var path = _navigation.CalculatePath(mapId, start, end, pathSmoothing);

            Assert.NotNull(path);
            Assert.NotEmpty(path);
        }

        [Fact]
        public void HasMapLineOfSight_ShouldReturnTrue()
        {
            Vector3 start = new(100f, 200f, 300f);
            Vector3 end = new(150f, 250f, 300f);

            // TODO: Implement and validate LOS logic.
            // var hasLOS = PathingAndLOS.HasMapLineOfSight(mapId, start, end);
            // Assert.True(hasLOS);
        }
    }
}
