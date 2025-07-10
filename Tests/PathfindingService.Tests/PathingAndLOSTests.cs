using GameData.Core.Models;
using PathfindingService.Repository;
using System;
using System.Numerics;
using Xunit;

namespace PathfindingService.Tests
{
    public class NavigationFixture : IDisposable
    {
        public Navigation Navigation { get; }

        public NavigationFixture()
        {
            // Initialize Navigation once for all tests in this class
            Navigation = new Navigation();
        }

        public void Dispose()
        {
            // Cleanup if necessary
        }
    }

    public class PathingAndLOSTests(NavigationFixture fixture) : IClassFixture<NavigationFixture>
    {
        private readonly Navigation _navigation = fixture.Navigation;

        [Fact]
        public void CalculatePath_ShouldReturnValidPath()
        {
            // Arrange
            uint mapId = 1;
            Position start = new(-616.2514f, -4188.0044f, 82.316719f);
            Position end = new(1629.36f, -4373.39f, 50.2564f);
            bool pathSmoothing = true;

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
            Position pos = new(-616.2514f, -4188.0044f, 82.316719f);
            Position pos1 = new(1629.36f, -4373.39f, 50.2564f);

            // Act
            float z = _navigation.GetFloorHeight(mapId, pos.X, pos.Y, pos.Z);
            float z1 = _navigation.GetFloorHeight(mapId, pos1.X, pos1.Y, pos1.Z);

            // Assert
            Assert.True(z > -50000 && z < 200000, $"Expected valid Z, got {z}");
            Assert.InRange(z, 70f, 75f); // matches expected range
            Console.WriteLine($"[Test] Queried VMAP Z: {z1} at Orgrimmar position {pos1}");
        }
    }
}
