using BotRunner.Clients;
using GameData.Core.Enums;
using GameData.Core.Models;
using Pathfinding;
using System.Numerics;
using WoWSharpClient.Client;
using WoWSharpClient.Models;
using WoWSharpClient.Parsers;

namespace WoWSharpClient.Movement
{
    public class MovementController
    {
        private readonly WoWClient _client;
        private readonly PathfindingClient _physics;
        private readonly WoWLocalPlayer _player;

        // Physics state
        private Vector3 _velocity = Vector3.Zero;
        private uint _fallTime = 0;

        // Network timing
        private uint _lastPacketTime;
        private MovementFlags _lastSentFlags;
        private const uint PACKET_INTERVAL_MS = 500;

        public MovementController(WoWClient client, PathfindingClient physics, WoWLocalPlayer player)
        {
            _client = client;
            _physics = physics;
            _player = player;
            _lastSentFlags = player.MovementFlags;
        }

        // ======== MAIN UPDATE - Called every frame ========
        public void Update(float deltaMs, uint gameTimeMs)
        {
            // 1. Run physics based on current player state
            var physicsResult = RunPhysics(deltaMs);
            ApplyPhysicsResult(physicsResult);

            // 2. Send network packet if needed
            if (ShouldSendPacket(gameTimeMs))
            {
                SendMovementPacket(gameTimeMs);
            }
        }

        // ======== PHYSICS ========
        private PhysicsOutput RunPhysics(float deltaMs)
        {
            // Build physics input from current player state
            var input = new PhysicsInput
            {
                DeltaTime = deltaMs,
                MapId = _player.MapId,
                MovementFlags = (uint)_player.MovementFlags,

                PosX = _player.Position.X,
                PosY = _player.Position.Y,
                PosZ = _player.Position.Z,
                Facing = _player.Facing,
                SwimPitch = _player.SwimPitch,

                VelX = _velocity.X,
                VelY = _velocity.Y,
                VelZ = _velocity.Z,

                WalkSpeed = _player.WalkSpeed,
                RunSpeed = _player.RunSpeed,
                RunBackSpeed = _player.RunBackSpeed,
                SwimSpeed = _player.SwimSpeed,
                SwimBackSpeed = _player.SwimBackSpeed,

                FallTime = _fallTime
            };

            return _physics.PhysicsStep(input);
        }

        private void ApplyPhysicsResult(PhysicsOutput output)
        {
            // Update position from physics
            _player.Position = new Position(output.NewPosX, output.NewPosY, output.NewPosZ);

            // Update velocity
            _velocity = new Vector3(output.NewVelX, output.NewVelY, output.NewVelZ);

            // Apply physics state flags (falling, swimming, etc)
            const MovementFlags PhysicsFlags =
                MovementFlags.MOVEFLAG_JUMPING |
                MovementFlags.MOVEFLAG_SWIMMING |
                MovementFlags.MOVEFLAG_FLYING |
                MovementFlags.MOVEFLAG_LEVITATING;

            // Preserve input flags, update physics flags
            var inputFlags = _player.MovementFlags & ~PhysicsFlags;
            var newPhysicsFlags = (MovementFlags)(output.MovementFlags) & PhysicsFlags;
            _player.MovementFlags = inputFlags | newPhysicsFlags;
        }

        // ======== NETWORKING ========
        private bool ShouldSendPacket(uint gameTimeMs)
        {
            // Send if movement state changed
            if (_player.MovementFlags != _lastSentFlags)
                return true;

            // Send periodic heartbeat while moving
            if (_player.MovementFlags != MovementFlags.MOVEFLAG_NONE &&
                gameTimeMs - _lastPacketTime >= PACKET_INTERVAL_MS)
                return true;

            return false;
        }

        private void SendMovementPacket(uint gameTimeMs)
        {
            var opcode = DetermineOpcode(_player.MovementFlags, _lastSentFlags);

            // Build and send packet
            var buffer = MovementPacketHandler.BuildMovementInfoBuffer(_player, gameTimeMs, _fallTime);
            _client.SendMovementOpcode(opcode, buffer);

            // Update tracking
            _lastPacketTime = gameTimeMs;
            _lastSentFlags = _player.MovementFlags;

            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {opcode} - Pos({_player.Position.X:F1}, {_player.Position.Y:F1}, {_player.Position.Z:F1})");
        }

        private Opcode DetermineOpcode(MovementFlags current, MovementFlags previous)
        {
            // Stopped moving
            if (current == MovementFlags.MOVEFLAG_NONE && previous != MovementFlags.MOVEFLAG_NONE)
                return Opcode.MSG_MOVE_STOP;

            // Started jumping
            if (current.HasFlag(MovementFlags.MOVEFLAG_JUMPING) && !previous.HasFlag(MovementFlags.MOVEFLAG_JUMPING))
                return Opcode.MSG_MOVE_JUMP;

            // Started moving forward
            if (current.HasFlag(MovementFlags.MOVEFLAG_FORWARD) && !previous.HasFlag(MovementFlags.MOVEFLAG_FORWARD))
                return Opcode.MSG_MOVE_START_FORWARD;

            // Started moving backward
            if (current.HasFlag(MovementFlags.MOVEFLAG_BACKWARD) && !previous.HasFlag(MovementFlags.MOVEFLAG_BACKWARD))
                return Opcode.MSG_MOVE_START_BACKWARD;

            // Started strafing
            if (current.HasFlag(MovementFlags.MOVEFLAG_STRAFE_LEFT) && !previous.HasFlag(MovementFlags.MOVEFLAG_STRAFE_LEFT))
                return Opcode.MSG_MOVE_START_STRAFE_LEFT;

            if (current.HasFlag(MovementFlags.MOVEFLAG_STRAFE_RIGHT) && !previous.HasFlag(MovementFlags.MOVEFLAG_STRAFE_RIGHT))
                return Opcode.MSG_MOVE_START_STRAFE_RIGHT;

            // Landed
            if (!current.HasFlag(MovementFlags.MOVEFLAG_JUMPING) && previous.HasFlag(MovementFlags.MOVEFLAG_JUMPING))
                return Opcode.MSG_MOVE_FALL_LAND;

            // Default to heartbeat
            return Opcode.MSG_MOVE_HEARTBEAT;
        }

        // ======== SPECIAL PACKETS ========
        public void SendFacingUpdate(uint gameTimeMs)
        {
            // Called by bot when it changes facing directly
            var buffer = MovementPacketHandler.BuildMovementInfoBuffer(_player, gameTimeMs, _fallTime);
            _client.SendMovementOpcode(Opcode.MSG_MOVE_SET_FACING, buffer);
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] MSG_MOVE_SET_FACING - Facing: {_player.Facing:F2}");
        }

        public void SendStopPacket(uint gameTimeMs)
        {
            // Force send a stop packet (useful after teleports or when bot stops)
            _player.MovementFlags = MovementFlags.MOVEFLAG_NONE;
            var buffer = MovementPacketHandler.BuildMovementInfoBuffer(_player, gameTimeMs, _fallTime);
            _client.SendMovementOpcode(Opcode.MSG_MOVE_STOP, buffer);
            _lastSentFlags = MovementFlags.MOVEFLAG_NONE;
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] MSG_MOVE_STOP (forced)");
        }

        // ======== STATE MANAGEMENT ========
        public void Reset()
        {
            // Reset physics state (after teleport, death, etc)
            _velocity = Vector3.Zero;
            _fallTime = 0;
            _lastSentFlags = MovementFlags.MOVEFLAG_NONE;
            _lastPacketTime = 0;
        }
    }
}