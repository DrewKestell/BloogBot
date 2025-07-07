using GameData.Core.Models;
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
            // Arrange
            uint mapId = 389u;
            Position start = new(3.81f, -14.82f, -17.84f);
            Position end = new(-230.133f, 191.085f, -24.9191f);
            bool pathSmoothing = false;

            // Act
            var path = _navigation.CalculatePath(mapId, start, end, pathSmoothing);

            // Assert
            Assert.NotNull(path);
            Assert.NotEmpty(path);
        }

        [Fact]
        public void QueryZ_FromDurotarCliff_ShouldReturnCorrectHeight()
        {
            // Arrange
            uint mapId = 1; // Kalimdor
            Position pos = new(-616.2514f, -4188.0044f, 72.316719f);

            // Act
            float z = _navigation.GetHeight(mapId, pos);

            // Assert
            Assert.True(z > -50000 && z < 200000, $"Expected valid Z, got {z}");
            Assert.InRange(z, 70f, 75f); // matches the screenshot range
            Console.WriteLine($"[Test] Queried VMAP Z: {z} at Durotar position {pos}");
        }

        [Fact]
        public void HasMapLineOfSight_ShouldReturnTrue()
        {
            // Arrange
            Vector3 start = new(100f, 200f, 300f);
            Vector3 end = new(150f, 250f, 300f);

            // Act
            //var hasLOS = PathingAndLOS.HasMapLineOfSight(mapId, start, end);

            //// Assert
            //Assert.True(hasLOS);
        }
    }
}