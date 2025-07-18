using BotRunner.Clients;
using GameData.Core.Enums;
using GameData.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PathfindingService.Repository;
using WoWSharpClient.Client;
using WoWSharpClient.Models;
using WoWSharpClient.Movement;

namespace WoWSharpClient.Tests
{
    public class IntegratedWoWFixture : IDisposable
    {
        public Navigation Navigation { get; }
        public Mock<WoWClient> WoWClient { get; }
        public PathfindingClient PathfindingClient { get; }
        private readonly ILogger<WoWSharpObjectManager> _logger;

        public IntegratedWoWFixture()
        {
            Navigation = new Navigation();
            WoWClient = new Mock<WoWClient>();
            PathfindingClient = new InMemoryPathfindingClient(Navigation);
            _logger = NullLoggerFactory.Instance.CreateLogger<WoWSharpObjectManager>();

            WoWSharpObjectManager.Instance.Initialize(WoWClient.Object, new Mock<PathfindingClient>().Object, _logger);
        }

        public void Dispose()
        {
            WoWSharpObjectManager.Instance.Initialize(WoWClient.Object, new Mock<PathfindingClient>().Object, _logger);
        }
    }
    public class PhysicsZTests(IntegratedWoWFixture _) : IClassFixture<IntegratedWoWFixture>
    {
        private readonly PhysicsManager _physics = new(_.PathfindingClient);

        [Theory]
        [InlineData(0, 1400.61f, -1493.87f, 54.7844f, "RuinsOfAndorhal")]        // id 1
        [InlineData(0, 1728.65f, -1602.85f, 63.429f, "WesternPlaguelands")]     // id 2
        [InlineData(0, 659.762f, -959.316f, 164.404f, "Strahnbrad")]            // id 3
        [InlineData(0, 1869.13f, -3213.89f, 124.624f, "TheMarrisStead")]        // id 4
        [InlineData(0, -1256.99f, -1189.47f, 38.9804f, "DunGarok")]             // id 5
        [InlineData(0, -483.455f, -1426.23f, 89.1916f, "DurnholdeKeep")]        // id 6
        [InlineData(0, -344.1467f, -923.366f, 54.5576f, "TarrenMill")]          // id 7
        [InlineData(0, -436.657f, -581.254f, 53.5944f, "HillsbradFoothills")]   // id 8
        [InlineData(0, -853.221f, -533.529f, 9.98556f, "Southshore")]            // id 9
        public void Should_Get_Terrain_Height_From_Physics(int map, float x, float y, float expectedZ, string label)
        {
            var position = new Position(x, y, expectedZ); // way below expected terrain
            var fakePlayer = new WoWLocalPlayer(new HighGuid(new byte[4], new byte[4]))
            {
                Race = Race.Orc,
                Position = position
            };

            var result = _physics.ApplyPhysics(fakePlayer, dt: 0.05f, now: 0, map: (uint)map);

            Assert.True(result.Grounded, $"Expected to be grounded at {label}");
            Assert.True(result.GroundZ > -100 && result.GroundZ < 10000, $"Invalid Z at {label}: {result.GroundZ}");
            Assert.InRange(result.GroundZ, expectedZ - 0.5f, expectedZ + 0.5f);
            Assert.Equal(result.GroundZ, result.DesiredZ, precision: 3);
        }
    }
}