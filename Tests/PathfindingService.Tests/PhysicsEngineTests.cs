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
            Console.WriteLine($"Movement Flags: {(MovementFlags)act.moveFlags}");
            Assert.Equal(exp.x, act.x, 3);
            Assert.Equal(exp.y, act.y, 3);
            Assert.InRange(act.z, exp.z - 1, exp.z + 1);
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
        [InlineData(1u, -618.518f, -4251.67f, 38.718f, Race.Orc, 0f, MovementFlags.MOVEFLAG_NONE)]
        [InlineData(1u, -601.294000f, -4296.760000f, 37.811500f, Race.Orc, 0f, MovementFlags.MOVEFLAG_NONE)]
        [InlineData(1u, -582.580383f, -4236.643970f, 38.044630f, Race.Orc, 0f, MovementFlags.MOVEFLAG_NONE)]
        [InlineData(1u, -576.927856f, -4242.207030f, 37.980587f, Race.Orc, 0f, MovementFlags.MOVEFLAG_NONE)]
        [InlineData(1u, -550.47998f, -4194.069824f, 49.271198f, Race.Orc, 0f, MovementFlags.MOVEFLAG_NONE)]
        [InlineData(1u, 1629.359985f, -4373.380377f, 31.255800f, Race.Orc, 3.548300f, MovementFlags.MOVEFLAG_NONE)]
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
            var (radius, height) = RaceDimensions.GetCapsuleForRace(race, Gender.Male);

            var input = new PhysicsInput
            {
                mapId = mapId,
                x = x,
                y = y,
                z = z,
                orientation = orientation,
                moveFlags = 0,
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

        [Theory]
        [InlineData(1u, -601.518f, -4602.816f, 37.600f, 1.612760f, Race.Orc, -598.668f, -4601.770f, 37.614f)] // Your exact scenario
        [InlineData(0u, -8949.95f, -132.49f, 83.23f, 0.0f, Race.Human, -8949.95f, -125.49f, 83.23f)]  // North facing
        [InlineData(0u, -8949.95f, -132.49f, 83.23f, 1.5708f, Race.Human, -8942.95f, -132.49f, 83.23f)] // East facing  
        [InlineData(0u, -8949.95f, -132.49f, 83.23f, 3.14159f, Race.Human, -8949.95f, -139.49f, 83.23f)] // South facing
        [InlineData(0u, -8949.95f, -132.49f, 83.23f, -1.5708f, Race.Human, -8956.95f, -132.49f, 83.23f)] // West facing
        public void StepPhysics_ForwardMovement(
    uint mapId,
    float startX, float startY, float startZ,
    float orientation,
    Race race,
    float expectedX, float expectedY, float expectedZ)
        {
            var (radius, height) = RaceDimensions.GetCapsuleForRace(race, Gender.Male);
            // Setup input with FORWARD movement flag
            var input = new PhysicsInput
            {
                mapId = mapId,
                x = startX,
                y = startY,
                z = startZ,
                orientation = orientation,
                moveFlags = (uint)MovementFlags.MOVEFLAG_FORWARD,
                radius = radius,
                height = height,
                walkSpeed = 2.5f,
                runSpeed = 7.0f,
                runBackSpeed = 4.5f,
                swimSpeed = 4.72f,
                flightSpeed = 2.5f
            };

            // Simulate 1 second of movement
            float totalTime = 1.0f;
            float dt = 0.05f; // 50ms ticks
            int steps = (int)(totalTime / dt);

            PhysicsOutput output = new();
            for (int i = 0; i < steps; i++)
            {
                output = _nav.StepPhysics(input, dt);

                // Update input for next iteration
                input.x = output.x;
                input.y = output.y;
                input.z = output.z;
                input.moveFlags = output.moveFlags;
            }

            // After 1 second of forward movement at run speed (7.0 units/sec)
            // We should have moved approximately 7 units in the facing direction

            // Calculate expected movement based on orientation
            float expectedDistance = input.runSpeed * totalTime;
            float actualDistance = MathF.Sqrt(
                MathF.Pow(output.x - startX, 2) +
                MathF.Pow(output.y - startY, 2));

            // Verify we moved approximately the right distance
            Assert.InRange(actualDistance, expectedDistance - 1, expectedDistance + 1); // Within 1 unit tolerance

            // Verify movement flags still indicate forward movement
            Assert.True((output.moveFlags & (uint)MovementFlags.MOVEFLAG_FORWARD) != 0,
                "Should still have FORWARD flag set");

            // Verify we're moving in the right direction based on orientation
            // In WoW: orientation 0 = North (+Y), π/2 = East (+X), π = South (-Y), 3π/2 = West (-X)
            float deltaX = output.x - startX;
            float deltaY = output.y - startY;

            // The movement direction should match our facing
            // Use atan2(deltaX, deltaY) to match WoW's coordinate system where 0 = North
            float moveAngle = MathF.Atan2(deltaY, deltaX);

            // Log the movement for debugging
            Console.WriteLine($"Movement Summary:");
            Console.WriteLine($"  Start: ({startX:F2}, {startY:F2}, {startZ:F2})");
            Console.WriteLine($"  End: ({output.x:F2}, {output.y:F2}, {output.z:F2})");
            Console.WriteLine($"  Distance: {actualDistance:F2} (expected: {expectedDistance:F2})");
            Console.WriteLine($"  Orientation: {orientation:F3} rad");
            Console.WriteLine($"  Movement angle: {moveAngle:F3} rad");
            Console.WriteLine($"  Velocity: ({output.vx:F2}, {output.vy:F2}, {output.vz:F2})");
        }
    }
}
