using GameData.Core.Constants;
using GameData.Core.Enums;
using PathfindingService.Repository; // Navigation wrapper
using static PathfindingService.Repository.Navigation;
using Xunit;
using MovementFlags = GameData.Core.Enums.MovementFlags;

namespace PathfindingService.Tests
{
    /// <summary>
    /// End-to-end tests for Navigation + PhysicsEngine.
    /// Uses InlineData for each idle-tick sample, preserving the NavigationFixture.
    /// </summary>
    public class PhysicsEngineTests : IClassFixture<NavigationFixture>
    {
        private readonly Navigation _nav;
        private const float Dt = 0.60f; // one tick = 100 ms

        public PhysicsEngineTests(NavigationFixture fixture)
        {
            _nav = fixture.Navigation;
        }

        // Helper to compare PhysicsOutput with tolerance
        private static void AssertEqual(PhysicsOutput exp, PhysicsOutput act)
        {
            Assert.Equal(exp.x, act.x, 3);
            Assert.Equal(exp.y, act.y, 3);
            Assert.Equal(exp.z, act.z, 3);
            Assert.Equal(exp.vx, act.vx, 3);
            Assert.Equal(exp.vy, act.vy, 3);
            Assert.Equal(exp.vz, act.vz, 3);
            Assert.Equal(exp.moveFlags, act.moveFlags);
        }

        [Theory]
        // mapId,      x,           y,           z,           race,         adtGroundZ,     adtLiquidZ
        [InlineData(0u, -8949.950000f, -132.490000f, 83.229485f, Race.Human, 0f, MovementFlags.MOVEFLAG_NONE)]
        [InlineData(0u, -6240.320000f, 331.033000f, 382.619171f, Race.Human, 0f, MovementFlags.MOVEFLAG_NONE)]
        [InlineData(0u, 524.311279f, 312.037323f, 31.260843f, Race.Orc, 0.002989f, MovementFlags.MOVEFLAG_NONE)]
        [InlineData(0u, 537.798401f, 279.534973f, 31.208981f, Race.Orc, 0f, MovementFlags.MOVEFLAG_NONE)]
        [InlineData(0u, 538.0f, 279.0f, 31.237110f, Race.Orc, 0f, MovementFlags.MOVEFLAG_NONE)]
        [InlineData(0u, 582.693848f, 342.985321f, 31.149933f, Race.Orc, 0f, MovementFlags.MOVEFLAG_SWIMMING)]
        [InlineData(0u, 623.246948f, 349.184143f, 31.149933f, Race.Orc, 0f, MovementFlags.MOVEFLAG_NONE)]
        [InlineData(0u, 623.683838f, 349.455780f, 31.245306f, Race.Orc, 0f, MovementFlags.MOVEFLAG_NONE)]
        [InlineData(1u, -2917.580000f, -257.980000f, 53.362350f, Race.Orc, 0f, MovementFlags.MOVEFLAG_NONE)]
        [InlineData(1u, -601.294000f, -4296.760000f, 37.811500f, Race.Orc, 0f, MovementFlags.MOVEFLAG_NONE)]
        [InlineData(1u, -582.580383f, -4236.643970f, 38.044630f, Race.Orc, 0f, MovementFlags.MOVEFLAG_NONE)]
        [InlineData(1u, -576.927856f, -4242.207030f, 37.980587f, Race.Orc, 0f, MovementFlags.MOVEFLAG_NONE)]
        [InlineData(1u, -618.518f, -4251.67f, 38.718f, Race.Orc, 0f, MovementFlags.MOVEFLAG_NONE)]
        [InlineData(1u, 10334.000000f, 833.902000f, 1326.110000f, Race.Orc, 0f, MovementFlags.MOVEFLAG_NONE)]
        [InlineData(389u, -247.728561f, -30.644503f, -58.082531f, Race.Orc, 0f, MovementFlags.MOVEFLAG_NONE)]
        [InlineData(389u, -158.395340f, 5.857921f, -42.873611f, Race.Orc, 0f, MovementFlags.MOVEFLAG_NONE)]
        public void StepPhysics_IdleExpectations(
            uint mapId,
            float x, float y, float z,
            Race race,
            float orientation,
            MovementFlags expMovementFlags)
        {
            // derive capsule dims and expected swimming flag
            var (radius, height) = RaceDimensions.GetCapsuleForRace(race);

            var input = new PhysicsInput
            {
                mapId = mapId,
                x = x,
                y = y,
                z = z,
                orientation = orientation,
                moveFlags = 0,
                vx = 0f,
                vy = 0f,
                vz = 0f,
                radius = radius,
                height = height,
                //gravity = 19.29f,
                walkSpeed = 2.5f,
                runSpeed = 7f,
                //runBackSpeed = 4.5f,
                swimSpeed = 6.45f,
                //swimBackSpeed = 3.14f,
            };

            var expected = new PhysicsOutput
            {
                x = x,
                y = y,
                z = z,
                vx = 0f,
                vy = 0f,
                vz = 0f,
                moveFlags = (uint)expMovementFlags
            };

            var actual = _nav.StepPhysics(input, Dt);
            AssertEqual(expected, actual);
        }
    }
}
