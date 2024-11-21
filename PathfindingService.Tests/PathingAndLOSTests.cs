using PathfindingService.Models;
using PathfindingService.Repository;
using System.Numerics;

namespace PathfindingService.Tests
{
    public class PathingAndLOSTests
    {
        [Fact]
        public void CalculatePath_ShouldReturnValidPath()
        {
            // Arrange
            uint mapId = 389u;
            Position start = new(3.81f, -14.82f, -17.84f);
            Position end = new(-230.133f, 191.085f, -24.9191f);
            bool pathSmoothing = false;

            // Act
            var path = Navigation.CalculatePath(mapId, start, end, pathSmoothing);

            // Assert
            Assert.NotNull(path);
            Assert.NotEmpty(path);
        }

        [Fact]
        public void HasMapLineOfSight_ShouldReturnTrue()
        {
            // Arrange
            uint mapId = 1u;
            Vector3 start = new(100f, 200f, 300f);
            Vector3 end = new(150f, 250f, 300f);

            // Act
            //var hasLOS = PathingAndLOS.HasMapLineOfSight(mapId, start, end);

            //// Assert
            //Assert.True(hasLOS);
        }
    }
}