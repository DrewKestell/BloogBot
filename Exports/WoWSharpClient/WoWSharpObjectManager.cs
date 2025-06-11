using BotRunner.Clients;
using GameData.Core.Enums;
using GameData.Core.Frames;
using GameData.Core.Interfaces;
using GameData.Core.Models;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using System.Timers;
using WoWSharpClient.Client;
using WoWSharpClient.Models;
using WoWSharpClient.Parsers;
using WoWSharpClient.Screens;
using static Database.DatabaseResponse.Types;
using static GameData.Core.Enums.UpdateFields;
using Timer = System.Timers.Timer;

namespace WoWSharpClient
{
    public class WoWSharpObjectManager : IObjectManager
    {
        private readonly ILogger<WoWSharpObjectManager> _logger;

        // Wrapper client for both auth and world transactions
        private readonly WoWClient _woWClient;
        private readonly PathfindingClient _pathfindingClient;

        private readonly WoWSharpEventEmitter _eventEmitter = new();

        public WoWSharpEventEmitter EventEmitter => _eventEmitter;

        private readonly LoginScreen _loginScreen;
        private readonly RealmSelectScreen _realmScreen;
        private readonly CharacterSelectScreen _characterSelectScreen;

        private Timer _gameLoopTimer;
        private readonly Stopwatch _movementTimer = Stopwatch.StartNew();

        private long _lastPingMs = 0;
        private ControlBits _controlBits = ControlBits.Nothing;

        public bool IsPlayerMoving => !_lastMovementFlags.Equals(MovementFlags.MOVEFLAG_NONE);

        // Movement Constants (based on 1.12 client)
        private float _fallSpeed = 0.0f;              // Gravity velocity (m/s)
        private long _lastFallStartTime = 0;
        private float _verticalVelocity = 0.0f;
        private long _lastSentTime = 0;
        private Position _lastSentPosition = new(0, 0, 0);

        private const float Gravity = 19.29f;         // Units/sec²
        private const float JumpVelocity = 7.8f;       // Initial Z velocity on jump
        private const float GroundTolerance = 0.05f;   // Distance from ground to count as grounded
        private const float MaxFallSpeed = 60.0f;     // Terminal velocity
        private bool _isInControl = false;
        private bool _isBeingTeleported = true;
        private bool _hasSentAcknowledgement = false;
        private bool _groundZLocked;
        private MovementFlags _lastMovementFlags = MovementFlags.MOVEFLAG_NONE;

        private TimeSpan _lastHeartbeat = TimeSpan.Zero;
        private TimeSpan _lastPositionUpdate = TimeSpan.Zero;

        public WoWSharpObjectManager(string ipAddress, PathfindingClient pathfindingClient, ILogger<WoWSharpObjectManager> logger)
        {
            _logger = logger;

            _eventEmitter.OnLoginFailure += EventEmitter_OnLoginFailure;
            _eventEmitter.OnLoginVerifyWorld += EventEmitter_OnLoginVerifyWorld;
            _eventEmitter.OnWorldSessionStart += EventEmitter_OnWorldSessionStart;
            _eventEmitter.OnWorldSessionEnd += EventEmitter_OnWorldSessionEnd;
            _eventEmitter.OnCharacterListLoaded += EventEmitter_OnCharacterListLoaded;
            _eventEmitter.OnChatMessage += EventEmitter_OnChatMessage;
            _eventEmitter.OnForceMoveRoot += EventEmitter_OnForceMoveRoot;
            _eventEmitter.OnForceMoveUnroot += EventEmitter_OnForceMoveUnroot;
            _eventEmitter.OnForceRunSpeedChange += EventEmitter_OnForceRunSpeedChange;
            _eventEmitter.OnForceRunBackSpeedChange += EventEmitter_OnForceRunBackSpeedChange;
            _eventEmitter.OnForceSwimSpeedChange += EventEmitter_OnForceSwimSpeedChange;
            _eventEmitter.OnForceMoveKnockBack += EventEmitter_OnForceMoveKnockBack;
            _eventEmitter.OnForceTimeSkipped += EventEmitter_OnForceTimeSkipped;
            _eventEmitter.OnTeleport += EventEmitter_OnTeleport;
            _eventEmitter.OnClientControlUpdate += EventEmitter_OnClientControlUpdate;

            _woWClient = new(ipAddress, this);

            _loginScreen = new(_woWClient);
            _realmScreen = new(_woWClient);
            _characterSelectScreen = new(_woWClient);

            _pathfindingClient = pathfindingClient;
        }

        private void EventEmitter_OnClientControlUpdate(object? sender, EventArgs e)
        {         
            _isInControl = true;
            _isBeingTeleported = false;

            Console.WriteLine($"[OnClientControlUpdate]{Player.Position}");
        }

        public void StartGameLoop()
        {
            _gameLoopTimer = new Timer(50);
            _gameLoopTimer.Elapsed += OnGameLoopTick;
            _gameLoopTimer.AutoReset = true;
            _gameLoopTimer.Start();
        }

        private void OnGameLoopTick(object? sender, ElapsedEventArgs e)
        {
            var now = _movementTimer.Elapsed;
            var delta = now - _lastPositionUpdate;

            ProcessUpdates();

            // Send ping heartbeat
            HandlePingHeartbeat((long)now.TotalMilliseconds);

            if (_isInControl && !_isBeingTeleported)
            {
                if (Player == null)
                    return;

                UpdatePlayerPosition((float)delta.TotalMilliseconds);

                HandleMovementHeartbeat(now);
            }
        }

        private void HandleMovementHeartbeat(TimeSpan now)
        {
            var elapsed = now - _lastHeartbeat;

            if (elapsed < (IsPlayerMoving ? TimeSpan.FromMilliseconds(150) : TimeSpan.FromMilliseconds(1000)))
                return;

            var player = (WoWLocalPlayer)Player;

            var buffer = MovementPacketHandler.BuildMovementInfoBuffer(player, (uint)_movementTimer.ElapsedMilliseconds);
            _woWClient.SendMovementOpcode(Opcode.MSG_MOVE_HEARTBEAT, buffer);

            _lastHeartbeat = now;
        }

        private void HandlePingHeartbeat(long now)
        {
            const int interval = 30000;

            if (now - _lastPingMs < interval)
                return;

            _lastPingMs = now;

            _woWClient.SendPing();
        }
        // This file contains the updated logic for WoW 1.12.1-like player movement handling
        // including swimming, falling, walking, and terrain Z physics.

        // Assumes that MovementFlags enum includes:
        // MOVEFLAG_SWIMMING = 0x00200000,
        // MOVEFLAG_WALK_MODE = 0x01000000,
        private void UpdatePlayerPosition(float deltaTime)
        {
            var player = (WoWLocalPlayer)Player;
            if (player.MovementFlags.HasFlag(MovementFlags.MOVEFLAG_ROOT))
                return;

            Vector2 moveDelta = ComputeMovementDelta(player, deltaTime);
            float nextX = player.Position.X + moveDelta.X;
            float nextY = player.Position.Y + moveDelta.Y;
            float currentZ = player.Position.Z;

            var probePosition = new Position(nextX, nextY, currentZ);
            var zQuery = _pathfindingClient.GetZQuery((uint)MapId, probePosition);

            float floorZ = zQuery.FloorZ;
            float locationZ = zQuery.LocationZ;
            float terrainZ = zQuery.TerrainZ;
            float raycastZ = zQuery.RaycastZ;
            float adtZ = zQuery.AdtZ;
            float waterZ = zQuery.WaterLevel;
            bool isInWater = zQuery.IsInWater || currentZ < waterZ;

            // Update swimming state
            if (isInWater)
            {
                if (!player.MovementFlags.HasFlag(MovementFlags.MOVEFLAG_SWIMMING))
                {
                    player.MovementFlags |= MovementFlags.MOVEFLAG_SWIMMING;
                    SendMovementFlagChange(player, Opcode.MSG_MOVE_START_SWIM);
                }
            }
            else if (player.MovementFlags.HasFlag(MovementFlags.MOVEFLAG_SWIMMING))
            {
                player.MovementFlags &= ~MovementFlags.MOVEFLAG_SWIMMING;
                SendMovementFlagChange(player, Opcode.MSG_MOVE_STOP_SWIM);
            }

            bool grounded = false;
            float desiredZ = currentZ;
            string zReason = "";

            if (!float.IsNaN(locationZ) && Math.Abs(currentZ - locationZ) < 2.0f)
            {
                desiredZ = locationZ;
                grounded = true;
                zReason = "LocationZ";
            }
            else if (!float.IsNaN(floorZ) && Math.Abs(currentZ - floorZ) < 2.0f)
            {
                desiredZ = floorZ;
                grounded = true;
                zReason = "FloorZ";
            }
            else if (!float.IsNaN(terrainZ) && Math.Abs(currentZ - terrainZ) < 2.5f)
            {
                desiredZ = terrainZ;
                grounded = true;
                zReason = "TerrainZ";
            }
            else if (!float.IsNaN(raycastZ) && raycastZ < currentZ && Math.Abs(currentZ - raycastZ) < 15.0f)
            {
                desiredZ = raycastZ;
                grounded = true;
                zReason = "RaycastZ";
            }

            if (grounded)
            {
                desiredZ += 0.05f; // server-style ground nudge
                _verticalVelocity = 0.0f;
                _groundZLocked = true;

                if (_lastMovementFlags.HasFlag(MovementFlags.MOVEFLAG_FALLING))
                {
                    player.MovementFlags &= ~MovementFlags.MOVEFLAG_FALLING;
                    EmitMovementPacketIfNeeded(player);
                }
            }
            else
            {
                _groundZLocked = false;
                SetFallingFlags(player);

                if (!_lastMovementFlags.HasFlag(MovementFlags.MOVEFLAG_FALLING))
                    _lastFallStartTime = _movementTimer.ElapsedMilliseconds;

                _verticalVelocity -= Gravity * (deltaTime / 1000.0f);
                _verticalVelocity = Math.Max(_verticalVelocity, -MaxFallSpeed);

                desiredZ = currentZ + _verticalVelocity * (deltaTime / 1000.0f);
                zReason = "Falling";
            }

            if (_groundZLocked)
            {
                _verticalVelocity = 0.0f;
                if (!float.IsNaN(floorZ))
                    desiredZ = MathF.Max(desiredZ, floorZ);

                if (_lastMovementFlags.HasFlag(MovementFlags.MOVEFLAG_FALLING))
                {
                    player.MovementFlags &= ~MovementFlags.MOVEFLAG_FALLING;
                    EmitMovementPacketIfNeeded(player);
                }
            }

            if (float.IsNaN(desiredZ) && !float.IsNaN(adtZ))
            {
                desiredZ = adtZ;
                zReason += " [ADT Fallback]";
            }

            player.Position = new Position(nextX, nextY, desiredZ);
            _lastPositionUpdate = _movementTimer.Elapsed;

            if (player.MovementFlags != _lastMovementFlags)
            {
                bool justJumped = player.MovementFlags.HasFlag(MovementFlags.MOVEFLAG_FALLING) &&
                                  !_lastMovementFlags.HasFlag(MovementFlags.MOVEFLAG_FALLING) &&
                                  _groundZLocked;

                if (justJumped)
                {
                    _verticalVelocity = JumpVelocity;
                    _groundZLocked = false;
                    _lastFallStartTime = _movementTimer.ElapsedMilliseconds;
                    Console.WriteLine("[Movement] Jump initiated");
                }

                _lastMovementFlags = player.MovementFlags;
            }

            if (moveDelta.LengthSquared() > 0.0001f)
                Console.WriteLine($"[Movement] {nextX:0.00}, {nextY:0.00}, {desiredZ:0.00} ({zReason}), GZLocked={_groundZLocked}, MoveFlags={player.MovementFlags}");

            EmitMovementPacketIfNeeded(player);
        }

        public void ToggleWalkMode()
        {
            var player = (WoWLocalPlayer)Player;
            bool isWalking = player.MovementFlags.HasFlag(MovementFlags.MOVEFLAG_WALK_MODE);

            if (isWalking)
            {
                player.MovementFlags &= ~MovementFlags.MOVEFLAG_WALK_MODE;
                SendMovementFlagChange(player, Opcode.MSG_MOVE_SET_RUN_MODE);
            }
            else
            {
                player.MovementFlags |= MovementFlags.MOVEFLAG_WALK_MODE;
                SendMovementFlagChange(player, Opcode.MSG_MOVE_SET_WALK_MODE);
            }

            Console.WriteLine($"[Movement] WalkMode: {(isWalking ? "Run" : "Walk")}");
        }

        private void SendMovementFlagChange(WoWLocalPlayer player, Opcode opcode)
        {
            var buffer = MovementPacketHandler.BuildMovementInfoBuffer(player, (uint)_movementTimer.ElapsedMilliseconds);
            _woWClient.SendMovementOpcode(opcode, buffer);
        }


        private void EmitMovementPacketIfNeeded(WoWLocalPlayer player)
        {
            var now = _movementTimer.ElapsedMilliseconds;

            bool positionChanged = !player.Position.Equals(_lastSentPosition);
            bool flagsChanged = player.MovementFlags != _lastMovementFlags;
            bool timeElapsed = now - _lastSentTime >= 100; // at least every 100ms

            if (!positionChanged && !flagsChanged && !timeElapsed)
                return;

            Opcode opcode = DetermineMovementOpcode(player.MovementFlags, _lastMovementFlags);
            var buffer = MovementPacketHandler.BuildMovementInfoBuffer(player, (uint)now);

            _woWClient.SendMovementOpcode(opcode, buffer);

            _lastSentTime = now;
            _lastSentPosition = player.Position;
            _lastMovementFlags = player.MovementFlags;
        }
        private Opcode DetermineMovementOpcode(MovementFlags current, MovementFlags previous)
        {
            if (current.HasFlag(MovementFlags.MOVEFLAG_FALLING) && !previous.HasFlag(MovementFlags.MOVEFLAG_FALLING))
                return Opcode.MSG_MOVE_JUMP;

            if (current.HasFlag(MovementFlags.MOVEFLAG_NONE) && !previous.HasFlag(MovementFlags.MOVEFLAG_NONE))
                return Opcode.MSG_MOVE_STOP;

            if (current.HasFlag(MovementFlags.MOVEFLAG_FORWARD))
                return Opcode.MSG_MOVE_START_FORWARD;
            if (current.HasFlag(MovementFlags.MOVEFLAG_BACKWARD))
                return Opcode.MSG_MOVE_START_BACKWARD;
            if (current.HasFlag(MovementFlags.MOVEFLAG_STRAFE_LEFT))
                return Opcode.MSG_MOVE_START_STRAFE_LEFT;
            if (current.HasFlag(MovementFlags.MOVEFLAG_STRAFE_RIGHT))
                return Opcode.MSG_MOVE_START_STRAFE_RIGHT;

            return Opcode.MSG_MOVE_HEARTBEAT;
        }

        /// <summary>
        /// Calculates a simple terrain "gradient" between two points, giving slope and a scaling factor.
        /// Extend this with a real surface normal if you want accurate ground collision.
        /// </summary>
        private bool TryComputeTerrainGradient(Position from, Position to, out Vector3 slope, out float scalar)
        {
            slope = Vector3.Zero;
            scalar = 1.0f;

            float tickDelta = 0.0333f; // ~30Hz simulation frame
            float terrainTickSpan = 1.0f; // Could be distance normalization factor

            float dx = to.X - from.X;
            float dy = to.Y - from.Y;
            float dz = to.Z - from.Z;

            if (Math.Abs(dx) < 0.001f && Math.Abs(dy) < 0.001f)
                return false; // No slope to calculate

            slope = new Vector3(dx / terrainTickSpan, dz / tickDelta, dy / terrainTickSpan);
            scalar = 1.0f / tickDelta;

            return true;
        }
        private static Vector2 ComputeMovementDelta(WoWLocalPlayer player, float deltaTime)
        {
            Vector2 dir = Vector2.Zero;
            float cosF = MathF.Cos(player.Facing);
            float sinF = MathF.Sin(player.Facing);

            if (player.MovementFlags.HasFlag(MovementFlags.MOVEFLAG_FORWARD))
                dir += new Vector2(cosF, sinF);
            if (player.MovementFlags.HasFlag(MovementFlags.MOVEFLAG_BACKWARD))
                dir -= new Vector2(cosF, sinF);
            if (player.MovementFlags.HasFlag(MovementFlags.MOVEFLAG_STRAFE_LEFT))
                dir += new Vector2(-sinF, cosF);
            if (player.MovementFlags.HasFlag(MovementFlags.MOVEFLAG_STRAFE_RIGHT))
                dir += new Vector2(sinF, -cosF);

            if (dir.LengthSquared() > 0)
                dir = Vector2.Normalize(dir);

            return dir * player.RunSpeed * (deltaTime / 1000.0f);
        }

        /// <summary>
        /// Placeholder: clears the player's falling state
        /// </summary>
        private void ClearFallingFlags(WoWLocalPlayer player)
        {
            player.MovementFlags &= ~MovementFlags.MOVEFLAG_FALLING;
        }

        /// <summary>
        /// Placeholder: sets the player's falling state
        /// </summary>
        private void SetFallingFlags(WoWLocalPlayer player)
        {
            player.MovementFlags |= MovementFlags.MOVEFLAG_FALLING;
        }

        public void StartMovement(ControlBits bits)
        {
            if (_isBeingTeleported || !_isInControl)
                return;

            _controlBits = bits;
            var player = (WoWLocalPlayer)Player;
            MovementFlags newFlags = player.MovementFlags;
            Opcode opcode = Opcode.MSG_MOVE_STOP;

            switch (bits)
            {
                case ControlBits.Front:
                    opcode = Opcode.MSG_MOVE_START_FORWARD;
                    newFlags |= MovementFlags.MOVEFLAG_FORWARD;
                    newFlags &= ~MovementFlags.MOVEFLAG_BACKWARD;
                    break;

                case ControlBits.Back:
                    opcode = Opcode.MSG_MOVE_START_BACKWARD;
                    newFlags |= MovementFlags.MOVEFLAG_BACKWARD;
                    newFlags &= ~MovementFlags.MOVEFLAG_FORWARD;
                    break;

                case ControlBits.StrafeLeft:
                    opcode = Opcode.MSG_MOVE_START_STRAFE_LEFT;
                    newFlags |= MovementFlags.MOVEFLAG_STRAFE_LEFT;
                    newFlags &= ~MovementFlags.MOVEFLAG_STRAFE_RIGHT;
                    break;

                case ControlBits.StrafeRight:
                    opcode = Opcode.MSG_MOVE_START_STRAFE_RIGHT;
                    newFlags |= MovementFlags.MOVEFLAG_STRAFE_RIGHT;
                    newFlags &= ~MovementFlags.MOVEFLAG_STRAFE_LEFT;
                    break;

                case ControlBits.Left:
                    opcode = Opcode.MSG_MOVE_START_TURN_LEFT;
                    newFlags |= MovementFlags.MOVEFLAG_TURN_LEFT;
                    newFlags &= ~MovementFlags.MOVEFLAG_TURN_RIGHT;
                    break;

                case ControlBits.Right:
                    opcode = Opcode.MSG_MOVE_START_TURN_RIGHT;
                    newFlags |= MovementFlags.MOVEFLAG_TURN_RIGHT;
                    newFlags &= ~MovementFlags.MOVEFLAG_TURN_LEFT;
                    break;

                case ControlBits.Jump:
                    opcode = Opcode.MSG_MOVE_JUMP;
                    newFlags |= MovementFlags.MOVEFLAG_FALLING;
                    break;
            }

            if (newFlags == _lastMovementFlags)
                return;

            _lastMovementFlags = newFlags;
            player.MovementFlags = newFlags;

            var buffer = MovementPacketHandler.BuildMovementInfoBuffer(player, (uint)_movementTimer.ElapsedMilliseconds);
            Console.WriteLine($"[Movement] Start: Opcode={opcode}, Flags={newFlags}");
            _woWClient.SendMovementOpcode(opcode, buffer);
        }

        public void StopMovement(ControlBits bits)
        {
            var player = (WoWLocalPlayer)Player;

            if (bits.HasFlag(ControlBits.Front) || bits.HasFlag(ControlBits.Back))
                player.MovementFlags &= ~(MovementFlags.MOVEFLAG_FORWARD | MovementFlags.MOVEFLAG_BACKWARD);

            if (bits.HasFlag(ControlBits.StrafeLeft) || bits.HasFlag(ControlBits.StrafeRight))
                player.MovementFlags &= ~(MovementFlags.MOVEFLAG_STRAFE_LEFT | MovementFlags.MOVEFLAG_STRAFE_RIGHT);

            if (bits.HasFlag(ControlBits.Left) || bits.HasFlag(ControlBits.Right))
                player.MovementFlags &= ~(MovementFlags.MOVEFLAG_TURN_LEFT | MovementFlags.MOVEFLAG_TURN_RIGHT);

            if (bits.HasFlag(ControlBits.Jump))
                player.MovementFlags &= ~MovementFlags.MOVEFLAG_FALLING;

            if (player.MovementFlags == _lastMovementFlags)
                return;

            _lastMovementFlags = player.MovementFlags;

            Opcode opcode = Opcode.MSG_MOVE_STOP;
            if (bits.HasFlag(ControlBits.StrafeLeft) || bits.HasFlag(ControlBits.StrafeRight))
                opcode = Opcode.MSG_MOVE_STOP_STRAFE;
            else if (bits.HasFlag(ControlBits.Left) || bits.HasFlag(ControlBits.Right))
                opcode = Opcode.MSG_MOVE_STOP_TURN;

            var buffer = MovementPacketHandler.BuildMovementInfoBuffer(player, (uint)_movementTimer.ElapsedMilliseconds);
            Console.WriteLine($"[Movement] Stop: Opcode={opcode}, Flags={player.MovementFlags}");
            _woWClient.SendMovementOpcode(opcode, buffer);
        }

        private void EventEmitter_OnTeleport(object? sender, RequiresAcknowledgementArgs e)
        {
            _woWClient.ResetMovementCounter();
            _woWClient.SendMSGPacked(Opcode.MSG_MOVE_TELEPORT_ACK, MovementPacketHandler.BuildMoveTeleportAckPayload(e.Guid, _woWClient.MovementCounter, (uint)_movementTimer.ElapsedMilliseconds));

            _isBeingTeleported = false;
        }

        private void EventEmitter_OnForceTimeSkipped(object? sender, RequiresAcknowledgementArgs e)
        {

        }

        private void EventEmitter_OnForceMoveKnockBack(object? sender, RequiresAcknowledgementArgs e)
        {
            _woWClient.SendMSGPacked(Opcode.CMSG_MOVE_KNOCK_BACK_ACK, MovementPacketHandler.BuildMovementInfoBuffer((WoWLocalPlayer)Player, (uint)_movementTimer.ElapsedMilliseconds));
        }

        private void EventEmitter_OnForceSwimSpeedChange(object? sender, RequiresAcknowledgementArgs e)
        {
            _woWClient.SendMSGPacked(Opcode.CMSG_FORCE_SWIM_SPEED_CHANGE_ACK, MovementPacketHandler.BuildForceMoveAck((WoWLocalPlayer)Player, _woWClient.MovementCounter, (uint)_movementTimer.ElapsedMilliseconds));
        }

        private void EventEmitter_OnForceRunBackSpeedChange(object? sender, RequiresAcknowledgementArgs e)
        {
            _woWClient.SendMSGPacked(Opcode.CMSG_FORCE_RUN_BACK_SPEED_CHANGE_ACK, MovementPacketHandler.BuildForceMoveAck((WoWLocalPlayer)Player, _woWClient.MovementCounter, (uint)_movementTimer.ElapsedMilliseconds));
        }

        private void EventEmitter_OnForceRunSpeedChange(object? sender, RequiresAcknowledgementArgs e)
        {
            _woWClient.SendMSGPacked(Opcode.CMSG_FORCE_RUN_SPEED_CHANGE_ACK, MovementPacketHandler.BuildForceMoveAck((WoWLocalPlayer)Player, _woWClient.MovementCounter, (uint)_movementTimer.ElapsedMilliseconds));
        }

        private void EventEmitter_OnForceMoveUnroot(object? sender, RequiresAcknowledgementArgs e)
        {
            _woWClient.SendMSGPacked(Opcode.CMSG_FORCE_MOVE_UNROOT_ACK, MovementPacketHandler.BuildForceMoveAck((WoWLocalPlayer)Player, _woWClient.MovementCounter, (uint)_movementTimer.ElapsedMilliseconds));
        }

        private void EventEmitter_OnForceMoveRoot(object? sender, RequiresAcknowledgementArgs e)
        {
            _woWClient.SendMSGPacked(Opcode.CMSG_FORCE_MOVE_ROOT_ACK, MovementPacketHandler.BuildForceMoveAck((WoWLocalPlayer)Player, _woWClient.MovementCounter, (uint)_movementTimer.ElapsedMilliseconds));
        }

        private void EventEmitter_OnLoginVerifyWorld(object? sender, WorldInfo e)
        {
            MapId = e.MapId;

            Player.Position.X = e.PositionX;
            Player.Position.Y = e.PositionY;
            Player.Position.Z = e.PositionZ;

            _movementTimer.Restart();

            StartGameLoop();

            _woWClient.SendMoveWorldPortAcknowledge();
        }

        private void EventEmitter_OnCharacterListLoaded(object? sender, EventArgs e)
        {
            _characterSelectScreen.HasReceivedCharacterList = true;
        }

        private void EventEmitter_OnWorldSessionStart(object? sender, EventArgs e)
        {
            _characterSelectScreen.RefreshCharacterListFromServer();
        }

        private void EventEmitter_OnLoginFailure(object? sender, EventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            _woWClient.Dispose();
        }
        private void EventEmitter_OnWorldSessionEnd(object? sender, EventArgs e)
        {
            HasEnteredWorld = false;
        }
        public bool HasEnteredWorld { get; internal set; }
        public List<Spell> Spells { get; internal set; } = [];
        public List<Cooldown> Cooldowns { get; internal set; } = [];

        private HighGuid _playerGuid = new(new byte[4], new byte[4]);
        public HighGuid PlayerGuid
        {
            get => _playerGuid;
            set
            {
                _playerGuid = value;
                Player = new WoWLocalPlayer(_playerGuid);
            }
        }

        public IWoWLocalPlayer Player { get; set; } = new WoWLocalPlayer(new HighGuid(new byte[4], new byte[4]));

        public IWoWLocalPet Pet => throw new NotImplementedException();


        private readonly List<WoWObject> _objects = [];
        public IEnumerable<IWoWObject> Objects => _objects;
        public ILoginScreen LoginScreen => _loginScreen;
        public IRealmSelectScreen RealmSelectScreen => _realmScreen;
        public ICharacterSelectScreen CharacterSelectScreen => _characterSelectScreen;
        public void EnterWorld(ulong characterGuid)
        {
            _playerGuid = new HighGuid(characterGuid);
            _woWClient.EnterWorld(characterGuid);

            HasEnteredWorld = true;
        }

        public void SetFacing(float facing)
        {
            if (_isInControl && _isBeingTeleported)
                return;

            ((WoWLocalPlayer)Player).Facing = facing;
            _woWClient.SendMovementOpcode(Opcode.MSG_MOVE_SET_FACING, MovementPacketHandler.BuildMovementInfoBuffer((WoWLocalPlayer)Player, (uint)_movementTimer.ElapsedMilliseconds));
        }

        private void EventEmitter_OnChatMessage(object? sender, ChatMessageArgs e)
        {
            Console.ResetColor();
            StringBuilder sb = new();
            switch (e.MsgType)
            {
                case ChatMsg.CHAT_MSG_SAY:
                case ChatMsg.CHAT_MSG_MONSTER_SAY:
                    sb.Append($"[{e.SenderGuid}]");

                    Console.ForegroundColor = ConsoleColor.White;
                    break;

                case ChatMsg.CHAT_MSG_YELL:
                case ChatMsg.CHAT_MSG_MONSTER_YELL:
                    sb.Append($"[{e.SenderGuid}]");
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;

                case ChatMsg.CHAT_MSG_WHISPER:
                case ChatMsg.CHAT_MSG_MONSTER_WHISPER:
                    string name;

                    if (_objects.Any(x => x.Guid == e.SenderGuid))
                    {
                        WoWUnit senderUnit = (WoWUnit)_objects.First(x => x.Guid == e.SenderGuid);
                        name = senderUnit.Name;
                    }
                    else
                    {
                        name = string.Empty;
                    }
                    sb.Append($"[{name}]");
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    break;

                case ChatMsg.CHAT_MSG_WHISPER_INFORM:
                    sb.Append($"To[{e.SenderGuid}]");
                    break;
                case ChatMsg.CHAT_MSG_EMOTE:
                case ChatMsg.CHAT_MSG_TEXT_EMOTE:
                case ChatMsg.CHAT_MSG_MONSTER_EMOTE:
                case ChatMsg.CHAT_MSG_RAID_BOSS_EMOTE:
                    sb.Append($"[{e.SenderGuid}]");
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;

                case ChatMsg.CHAT_MSG_SYSTEM:
                    if (e.Text.StartsWith("You are being teleported"))
                    {
                        StopMovement(_controlBits);

                        _isBeingTeleported = true;
                    }

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    sb.Append($"[System]");
                    break;

                case ChatMsg.CHAT_MSG_PARTY:
                case ChatMsg.CHAT_MSG_RAID:
                case ChatMsg.CHAT_MSG_GUILD:
                case ChatMsg.CHAT_MSG_OFFICER:
                    sb.Append($"[{e.SenderGuid}]");
                    Console.ForegroundColor = ConsoleColor.Blue;
                    break;

                case ChatMsg.CHAT_MSG_CHANNEL:
                case ChatMsg.CHAT_MSG_CHANNEL_NOTICE:
                    sb.Append($"[Channel]");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;

                case ChatMsg.CHAT_MSG_RAID_WARNING:
                    sb.Append($"[Raid Warning]");
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    break;

                case ChatMsg.CHAT_MSG_LOOT:
                    sb.Append($"[Loot]");
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                default:
                    sb.Append($"[{e.SenderGuid}][{e.MsgType}]");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
            }
            sb.Append(e.Text);

            Console.WriteLine(sb.ToString());

            Console.ForegroundColor = ConsoleColor.White;
        }
        public uint MapId { get; private set; }

        public string ZoneText { get; private set; }

        public string MinimapZoneText { get; private set; }

        public string ServerName { get; private set; }

        public IGossipFrame GossipFrame { get; private set; }

        public ILootFrame LootFrame { get; private set; }

        public IMerchantFrame MerchantFrame { get; private set; }

        public ICraftFrame CraftFrame { get; private set; }

        public IQuestFrame QuestFrame { get; private set; }

        public IQuestGreetingFrame QuestGreetingFrame { get; private set; }

        public ITaxiFrame TaxiFrame { get; private set; }

        public ITradeFrame TradeFrame { get; private set; }

        public ITrainerFrame TrainerFrame { get; private set; }

        public ITalentFrame TalentFrame { get; private set; }

        private readonly Queue<ObjectStateUpdate> _pendingUpdates = new();

        public void QueueUpdate(ObjectStateUpdate update) => _pendingUpdates.Enqueue(update);

        public void ProcessUpdates()
        {
            while (_pendingUpdates.Count > 0)
            {
                try
                {
                    var update = _pendingUpdates.Dequeue();

                    switch (update.Operation)
                    {
                        case ObjectUpdateOperation.Add:
                            var newObject = CreateObjectFromFields(update.ObjectType, update.Guid, update.UpdatedFields);
                            _objects.Add(newObject);

                            if (update.MovementData != null && newObject is WoWUnit or WoWPlayer or WoWLocalPlayer)
                                ApplyMovementData((WoWUnit)newObject, update.MovementData);

                            if (newObject is WoWPlayer)
                            {
                                _woWClient.SendNameQuery(update.Guid);

                                if (newObject is WoWLocalPlayer)
                                {
                                    _woWClient.SendSetActiveMover(PlayerGuid.FullGuid);

                                    _isInControl = true;
                                    _isBeingTeleported = false;
                                }
                            }
                            break;

                        case ObjectUpdateOperation.Update:
                            var index = _objects.FindIndex(o => o.Guid == update.Guid);
                            if (index != -1)
                            {
                                var obj = _objects[index];
                                ApplyFieldDiffs(obj, update.UpdatedFields);

                                if (update.MovementData != null && obj is WoWUnit or WoWPlayer or WoWLocalPlayer)
                                    ApplyMovementData((WoWUnit)obj, update.MovementData);

                                if (obj is WoWLocalPlayer)
                                    Console.WriteLine("Potential teleport from server.");
                            }
                            break;

                        case ObjectUpdateOperation.Remove:
                            _objects.RemoveAll(x => x.Guid == update.Guid);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ProcessUpdates] {ex}");
                }
            }
        }

        private static void ApplyFieldDiffs(WoWObject obj, Dictionary<uint, object?> updatedFields)
        {
            foreach (var (key, value) in updatedFields)
            {
                if (value == null) continue;

                if (Enum.IsDefined(typeof(EObjectFields), key))
                {
                    var field = (EObjectFields)key;
                    switch (field)
                    {
                        case EObjectFields.OBJECT_FIELD_GUID:
                            obj.HighGuid.LowGuidValue = (byte[])value;
                            break;
                        case EObjectFields.OBJECT_FIELD_GUID + 1:
                            obj.HighGuid.HighGuidValue = (byte[])value;
                            break;
                        case EObjectFields.OBJECT_FIELD_TYPE:

                            break;
                        case EObjectFields.OBJECT_FIELD_ENTRY:
                            obj.Entry = (uint)value;
                            break;
                        case EObjectFields.OBJECT_FIELD_SCALE_X:
                            obj.ScaleX = (float)value;
                            break;
                    }
                }
                else if (Enum.IsDefined(typeof(EGameObjectFields), key))
                {
                    if (obj is WoWGameObject go)
                    {
                        var field = (EGameObjectFields)key;
                        switch (field)
                        {
                            case EGameObjectFields.OBJECT_FIELD_CREATED_BY:
                                go.CreatedBy.LowGuidValue = (byte[])value;
                                break;
                            case EGameObjectFields.OBJECT_FIELD_CREATED_BY + 1:
                                go.CreatedBy.HighGuidValue = (byte[])value;
                                break;
                            case EGameObjectFields.GAMEOBJECT_DISPLAYID:
                                go.DisplayId = (uint)value;
                                break;
                            case EGameObjectFields.GAMEOBJECT_FLAGS:
                                go.Flags = (uint)value;
                                break;
                            case >= EGameObjectFields.GAMEOBJECT_ROTATION and < EGameObjectFields.GAMEOBJECT_STATE:
                                go.Rotation[key - (uint)EGameObjectFields.GAMEOBJECT_ROTATION] = (float)value;
                                break;
                            case EGameObjectFields.GAMEOBJECT_STATE:
                                go.GoState = (GOState)value;
                                break;
                            case EGameObjectFields.GAMEOBJECT_POS_X:
                                go.Position.X = (float)value;
                                break;
                            case EGameObjectFields.GAMEOBJECT_POS_Y:
                                go.Position.Y = (float)value;
                                break;
                            case EGameObjectFields.GAMEOBJECT_POS_Z:
                                go.Position.Z = (float)value;
                                break;
                            case EGameObjectFields.GAMEOBJECT_FACING:
                                go.Facing = (float)value;
                                break;
                            case EGameObjectFields.GAMEOBJECT_DYN_FLAGS:
                                go.DynamicFlags = (DynamicFlags)value;
                                break;
                            case EGameObjectFields.GAMEOBJECT_FACTION:
                                go.FactionTemplate = (uint)value;
                                break;
                            case EGameObjectFields.GAMEOBJECT_TYPE_ID:
                                go.TypeId = (uint)value;
                                break;
                            case EGameObjectFields.GAMEOBJECT_LEVEL:
                                go.Level = (uint)value;
                                break;
                            case EGameObjectFields.GAMEOBJECT_ARTKIT:
                                go.ArtKit = (uint)value;
                                break;
                            case EGameObjectFields.GAMEOBJECT_ANIMPROGRESS:
                                go.AnimProgress = (uint)value;
                                break;
                        }
                    }
                }
                else if (Enum.IsDefined(typeof(EDynamicObjectFields), key))
                {
                    if (obj is WoWDynamicObject dyn)
                    {
                        var field = (EDynamicObjectFields)key;
                        switch (field)
                        {
                            case EDynamicObjectFields.DYNAMICOBJECT_CASTER:
                                dyn.Caster.LowGuidValue = (byte[])value;
                                break;
                            case EDynamicObjectFields.DYNAMICOBJECT_CASTER + 1:
                                dyn.Caster.HighGuidValue = (byte[])value;
                                break;
                            case EDynamicObjectFields.DYNAMICOBJECT_BYTES:
                                dyn.Bytes = (byte[])value;
                                break;
                            case EDynamicObjectFields.DYNAMICOBJECT_SPELLID:
                                dyn.SpellId = (uint)value;
                                break;
                            case EDynamicObjectFields.DYNAMICOBJECT_RADIUS:
                                dyn.Radius = (float)value;
                                break;
                            case EDynamicObjectFields.DYNAMICOBJECT_POS_X:
                                dyn.Position.X = (float)value;
                                break;
                            case EDynamicObjectFields.DYNAMICOBJECT_POS_Y:
                                dyn.Position.Y = (float)value;
                                break;
                            case EDynamicObjectFields.DYNAMICOBJECT_POS_Z:
                                dyn.Position.Z = (float)value;
                                break;
                            case EDynamicObjectFields.DYNAMICOBJECT_FACING:
                                dyn.Facing = (float)value;
                                break;
                                // DYNAMICOBJECT_PAD is skipped
                        }
                    }
                }
                else if (Enum.IsDefined(typeof(ECorpseFields), key))
                {
                    if (obj is WoWCorpse corpse)
                    {
                        var field = (ECorpseFields)key;
                        switch (field)
                        {
                            case ECorpseFields.CORPSE_FIELD_OWNER:
                                corpse.OwnerGuid.LowGuidValue = (byte[])value;
                                break;
                            case ECorpseFields.CORPSE_FIELD_OWNER + 1:
                                corpse.OwnerGuid.HighGuidValue = (byte[])value;
                                break;
                            case ECorpseFields.CORPSE_FIELD_FACING:
                                corpse.Facing = (float)value;
                                break;
                            case ECorpseFields.CORPSE_FIELD_POS_X:
                                corpse.Position.X = (float)value;
                                break;
                            case ECorpseFields.CORPSE_FIELD_POS_Y:
                                corpse.Position.Y = (float)value;
                                break;
                            case ECorpseFields.CORPSE_FIELD_POS_Z:
                                corpse.Position.Z = (float)value;
                                break;
                            case ECorpseFields.CORPSE_FIELD_DISPLAY_ID:
                                corpse.DisplayId = (uint)value;
                                break;
                            case >= ECorpseFields.CORPSE_FIELD_ITEM and < ECorpseFields.CORPSE_FIELD_BYTES_1:
                                corpse.Items[key - (uint)ECorpseFields.CORPSE_FIELD_ITEM] = (uint)value;
                                break;
                            case ECorpseFields.CORPSE_FIELD_BYTES_1:
                                corpse.Bytes1 = (byte[])value;
                                break;
                            case ECorpseFields.CORPSE_FIELD_BYTES_2:
                                corpse.Bytes2 = (byte[])value;
                                break;
                            case ECorpseFields.CORPSE_FIELD_GUILD:
                                corpse.Guild = (uint)value;
                                break;
                            case ECorpseFields.CORPSE_FIELD_FLAGS:
                                corpse.CorpseFlags = (CorpseFlags)value;
                                break;
                            case ECorpseFields.CORPSE_FIELD_DYNAMIC_FLAGS:
                                corpse.DynamicFlags = (DynamicFlags)value;
                                break;
                        }
                    }
                }

                else if (Enum.IsDefined(typeof(EUnitFields), key))
                {
                    if (obj is WoWUnit unit)
                    {
                        var field = (EUnitFields)key;
                        switch (field)
                        {
                            case EUnitFields.UNIT_FIELD_CHARM:
                                unit.Charm.LowGuidValue = (byte[])value;
                                break;
                            case EUnitFields.UNIT_FIELD_CHARM + 1:
                                unit.Charm.HighGuidValue = (byte[])value;
                                break;
                            case EUnitFields.UNIT_FIELD_SUMMON:
                                unit.Summon.LowGuidValue = (byte[])value;
                                break;
                            case EUnitFields.UNIT_FIELD_SUMMON + 1:
                                unit.Summon.HighGuidValue = (byte[])value;
                                break;
                            case EUnitFields.UNIT_FIELD_CHARMEDBY:
                                unit.CharmedBy.LowGuidValue = (byte[])value;
                                break;
                            case EUnitFields.UNIT_FIELD_CHARMEDBY + 1:
                                unit.CharmedBy.HighGuidValue = (byte[])value;
                                break;
                            case EUnitFields.UNIT_FIELD_SUMMONEDBY:
                                unit.SummonedBy.LowGuidValue = (byte[])value;
                                break;
                            case EUnitFields.UNIT_FIELD_SUMMONEDBY + 1:
                                unit.SummonedBy.HighGuidValue = (byte[])value;
                                break;
                            case EUnitFields.UNIT_FIELD_CREATEDBY:
                                unit.CreatedBy.LowGuidValue = (byte[])value;
                                break;
                            case EUnitFields.UNIT_FIELD_CREATEDBY + 1:
                                unit.CreatedBy.HighGuidValue = (byte[])value;
                                break;
                            case EUnitFields.UNIT_FIELD_TARGET:
                                unit.TargetHighGuid.LowGuidValue = (byte[])value;
                                break;
                            case EUnitFields.UNIT_FIELD_TARGET + 1:
                                unit.TargetHighGuid.HighGuidValue = (byte[])value;
                                break;
                            case EUnitFields.UNIT_FIELD_PERSUADED:
                                unit.Persuaded.LowGuidValue = (byte[])value;
                                break;
                            case EUnitFields.UNIT_FIELD_PERSUADED + 1:
                                unit.Persuaded.HighGuidValue = (byte[])value;
                                break;
                            case EUnitFields.UNIT_FIELD_CHANNEL_OBJECT:
                                unit.ChannelObject.LowGuidValue = (byte[])value;
                                break;
                            case EUnitFields.UNIT_FIELD_CHANNEL_OBJECT + 1:
                                unit.ChannelObject.HighGuidValue = (byte[])value;
                                break;
                            case EUnitFields.UNIT_FIELD_HEALTH:
                                unit.Health = (uint)value;
                                break;
                            case EUnitFields.UNIT_FIELD_POWER1:
                                unit.Powers[Powers.MANA] = (uint)value;
                                break;
                            case EUnitFields.UNIT_FIELD_POWER2:
                                unit.Powers[Powers.RAGE] = (uint)value;
                                break;
                            case EUnitFields.UNIT_FIELD_POWER3:
                                unit.Powers[Powers.FOCUS] = (uint)value;
                                break;
                            case EUnitFields.UNIT_FIELD_POWER4:
                                unit.Powers[Powers.ENERGY] = (uint)value;
                                break;
                            case EUnitFields.UNIT_FIELD_POWER5:
                                unit.Powers[Powers.HAPPINESS] = (uint)value;
                                break;
                            case EUnitFields.UNIT_FIELD_MAXHEALTH:
                                unit.MaxHealth = (uint)value;
                                break;
                            case EUnitFields.UNIT_FIELD_MAXPOWER1:
                                unit.MaxPowers[Powers.MANA] = (uint)value;
                                break;
                            case EUnitFields.UNIT_FIELD_MAXPOWER2:
                                unit.MaxPowers[Powers.RAGE] = (uint)value;
                                break;
                            case EUnitFields.UNIT_FIELD_MAXPOWER3:
                                unit.MaxPowers[Powers.FOCUS] = (uint)value;
                                break;
                            case EUnitFields.UNIT_FIELD_MAXPOWER4:
                                unit.MaxPowers[Powers.ENERGY] = (uint)value;
                                break;
                            case EUnitFields.UNIT_FIELD_MAXPOWER5:
                                unit.MaxPowers[Powers.HAPPINESS] = (uint)value;
                                break;
                            case EUnitFields.UNIT_FIELD_LEVEL:
                                unit.Level = (uint)value;
                                break;
                            case EUnitFields.UNIT_FIELD_FACTIONTEMPLATE:
                                unit.FactionTemplate = (uint)value;
                                break;
                            case EUnitFields.UNIT_FIELD_BYTES_0:
                                byte[] value1 = (byte[])value;

                                unit.Bytes0[0] = value1[0];
                                unit.Bytes0[1] = value1[1];
                                unit.Bytes0[2] = value1[2];
                                unit.Bytes0[3] = value1[3];
                                break;
                            case >= EUnitFields.UNIT_VIRTUAL_ITEM_SLOT_DISPLAY and <= EUnitFields.UNIT_VIRTUAL_ITEM_SLOT_DISPLAY_02:
                                unit.VirtualItemSlotDisplay[field - EUnitFields.UNIT_VIRTUAL_ITEM_SLOT_DISPLAY] = (uint)value;
                                break;
                            case >= EUnitFields.UNIT_VIRTUAL_ITEM_INFO and <= EUnitFields.UNIT_VIRTUAL_ITEM_INFO_05:
                                unit.VirtualItemInfo[field - EUnitFields.UNIT_VIRTUAL_ITEM_INFO] = (uint)value;
                                break;
                            case EUnitFields.UNIT_FIELD_FLAGS:
                                unit.UnitFlags = (UnitFlags)value;
                                break;
                            case >= EUnitFields.UNIT_FIELD_AURA and <= EUnitFields.UNIT_FIELD_AURA_LAST:
                                unit.AuraFields[field - EUnitFields.UNIT_FIELD_AURA] = (uint)value;
                                break;
                            case >= EUnitFields.UNIT_FIELD_AURAFLAGS and <= EUnitFields.UNIT_FIELD_AURAFLAGS_05:
                                unit.AuraFlags[field - EUnitFields.UNIT_FIELD_AURAFLAGS] = (uint)value;
                                break;
                            case >= EUnitFields.UNIT_FIELD_AURALEVELS and <= EUnitFields.UNIT_FIELD_AURALEVELS_LAST:
                                unit.AuraLevels[field - EUnitFields.UNIT_FIELD_AURALEVELS] = (uint)value;
                                break;
                            case >= EUnitFields.UNIT_FIELD_AURAAPPLICATIONS and <= EUnitFields.UNIT_FIELD_AURAAPPLICATIONS_LAST:
                                unit.AuraApplications[field - EUnitFields.UNIT_FIELD_AURAAPPLICATIONS] = (uint)value;
                                break;
                            case EUnitFields.UNIT_FIELD_AURASTATE:
                                unit.AuraState = (uint)value;
                                break;
                            case EUnitFields.UNIT_FIELD_BASEATTACKTIME:
                                unit.BaseAttackTime = (float)value;
                                break;
                            case EUnitFields.UNIT_FIELD_OFFHANDATTACKTIME:
                                unit.OffhandAttackTime = (float)value;
                                break;
                            case EUnitFields.UNIT_FIELD_RANGEDATTACKTIME:
                                unit.OffhandAttackTime1 = (float)value;
                                break;
                            case EUnitFields.UNIT_FIELD_BOUNDINGRADIUS:
                                unit.BoundingRadius = (float)value;
                                break;
                            case EUnitFields.UNIT_FIELD_COMBATREACH:
                                unit.CombatReach = (float)value;
                                break;
                            case EUnitFields.UNIT_FIELD_DISPLAYID:
                                unit.DisplayId = (uint)value;
                                break;
                            case EUnitFields.UNIT_FIELD_NATIVEDISPLAYID:
                                unit.NativeDisplayId = (uint)value;
                                break;
                            case EUnitFields.UNIT_FIELD_MINDAMAGE:
                                unit.MinDamage = (uint)value;
                                break;
                            case EUnitFields.UNIT_FIELD_MAXDAMAGE:
                                unit.MaxDamage = (uint)value;
                                break;
                            case EUnitFields.UNIT_FIELD_MINOFFHANDDAMAGE:
                                unit.MinOffhandDamage = (uint)value;
                                break;
                            case EUnitFields.UNIT_FIELD_MAXOFFHANDDAMAGE:
                                unit.MaxOffhandDamage = (uint)value;
                                break;
                            case EUnitFields.UNIT_FIELD_BYTES_1:
                                byte[] value2 = (byte[])value;

                                unit.Bytes1[0] = value2[0];
                                unit.Bytes1[1] = value2[1];
                                unit.Bytes1[2] = value2[2];
                                unit.Bytes1[3] = value2[3];
                                break;
                            case EUnitFields.UNIT_FIELD_PETNUMBER:
                                unit.PetNumber = (uint)value;
                                break;
                            case EUnitFields.UNIT_FIELD_PET_NAME_TIMESTAMP:
                                unit.PetNameTimestamp = (uint)value;
                                break;
                            case EUnitFields.UNIT_FIELD_PETEXPERIENCE:
                                unit.PetExperience = (uint)value;
                                break;
                            case EUnitFields.UNIT_FIELD_PETNEXTLEVELEXP:
                                unit.PetNextLevelExperience = (uint)value;
                                break;
                            case EUnitFields.UNIT_DYNAMIC_FLAGS:
                                unit.DynamicFlags = (DynamicFlags)value;
                                break;
                            case EUnitFields.UNIT_CHANNEL_SPELL:
                                unit.ChannelingId = (uint)value;
                                break;
                            case EUnitFields.UNIT_MOD_CAST_SPEED:
                                unit.ModCastSpeed = (float)value;
                                break;
                            case EUnitFields.UNIT_CREATED_BY_SPELL:
                                unit.CreatedBySpell = (uint)value;
                                break;
                            case EUnitFields.UNIT_NPC_FLAGS:
                                unit.NpcFlags = (NPCFlags)value;
                                break;
                            case EUnitFields.UNIT_NPC_EMOTESTATE:
                                unit.NpcEmoteState = (uint)value;
                                break;
                            case EUnitFields.UNIT_TRAINING_POINTS:
                                unit.TrainingPoints = (uint)value;
                                break;
                            case EUnitFields.UNIT_FIELD_STAT0:
                                unit.Strength = (uint)value;
                                break;
                            case EUnitFields.UNIT_FIELD_STAT1:
                                unit.Agility = (uint)value;
                                break;
                            case EUnitFields.UNIT_FIELD_STAT2:
                                unit.Stamina = (uint)value;
                                break;
                            case EUnitFields.UNIT_FIELD_STAT3:
                                unit.Intellect = (uint)value;
                                break;
                            case EUnitFields.UNIT_FIELD_STAT4:
                                unit.Spirit = (uint)value;
                                break;
                            case >= EUnitFields.UNIT_FIELD_RESISTANCES and <= EUnitFields.UNIT_FIELD_RESISTANCES_06:
                                unit.Resistances[field - EUnitFields.UNIT_FIELD_RESISTANCES] = (uint)value;
                                break;
                            case EUnitFields.UNIT_FIELD_BASE_MANA:
                                unit.BaseMana = (uint)value;
                                break;
                            case EUnitFields.UNIT_FIELD_BASE_HEALTH:
                                unit.BaseHealth = (uint)value;
                                break;
                            case EUnitFields.UNIT_FIELD_BYTES_2:
                                byte[] value3 = (byte[])value;

                                unit.Bytes2[0] = value3[0];
                                unit.Bytes2[1] = value3[1];
                                unit.Bytes2[2] = value3[2];
                                unit.Bytes2[3] = value3[3];
                                break;
                            case EUnitFields.UNIT_FIELD_ATTACK_POWER:
                                unit.AttackPower = (uint)value;
                                break;
                            case EUnitFields.UNIT_FIELD_ATTACK_POWER_MODS:
                                unit.AttackPowerMods = (uint)value;
                                break;
                            case EUnitFields.UNIT_FIELD_ATTACK_POWER_MULTIPLIER:
                                unit.AttackPowerMultipler = (uint)value;
                                break;
                            case EUnitFields.UNIT_FIELD_RANGED_ATTACK_POWER:
                                unit.RangedAttackPower = (uint)value;
                                break;
                            case EUnitFields.UNIT_FIELD_RANGED_ATTACK_POWER_MODS:
                                unit.RangedAttackPowerMods = (uint)value;
                                break;
                            case EUnitFields.UNIT_FIELD_RANGED_ATTACK_POWER_MULTIPLIER:
                                unit.RangedAttackPowerMultipler = (uint)value;
                                break;
                            case EUnitFields.UNIT_FIELD_MINRANGEDDAMAGE:
                                unit.MinRangedDamage = (uint)value;
                                break;
                            case EUnitFields.UNIT_FIELD_MAXRANGEDDAMAGE:
                                unit.MaxRangedDamage = (uint)value;
                                break;
                            case >= EUnitFields.UNIT_FIELD_POWER_COST_MODIFIER and <= EUnitFields.UNIT_FIELD_POWER_COST_MODIFIER_06:
                                unit.PowerCostModifers[field - EUnitFields.UNIT_FIELD_POWER_COST_MODIFIER] = (uint)value;
                                break;
                            case >= EUnitFields.UNIT_FIELD_POWER_COST_MULTIPLIER and <= EUnitFields.UNIT_FIELD_POWER_COST_MULTIPLIER_06:
                                unit.PowerCostMultipliers[field - EUnitFields.UNIT_FIELD_POWER_COST_MULTIPLIER] = (uint)value;
                                break;

                            case EUnitFields.PLAYER_FLAGS:
                                ((WoWPlayer)unit).PlayerFlags = (PlayerFlags)value;
                                break;
                            case EUnitFields.PLAYER_GUILDID:
                                ((WoWPlayer)unit).GuildId = (uint)value;
                                break;
                            case EUnitFields.PLAYER_GUILDRANK:
                                ((WoWPlayer)unit).GuildRank = (uint)value;
                                break;
                            case EUnitFields.PLAYER_BYTES:
                                ((WoWPlayer)unit).PlayerBytes = (byte[])value;
                                break;
                            case EUnitFields.PLAYER_BYTES_2:
                                ((WoWPlayer)unit).PlayerBytes2 = (byte[])value;
                                break;
                            case EUnitFields.PLAYER_BYTES_3:
                                ((WoWPlayer)unit).PlayerBytes3 = (byte[])value;
                                break;
                            case EUnitFields.PLAYER_DUEL_TEAM:
                                ((WoWPlayer)unit).GuildTimestamp = (uint)value;
                                break;
                            case >= EUnitFields.PLAYER_QUEST_LOG_1_1 and <= EUnitFields.PLAYER_QUEST_LOG_LAST_3:
                                {
                                    uint questField = (field - EUnitFields.PLAYER_QUEST_LOG_1_1) % 3;
                                    int questIndex = (int)((field - EUnitFields.PLAYER_QUEST_LOG_1_1) / 3);
                                    switch (questField)
                                    {
                                        case 0:
                                            ((WoWPlayer)unit).QuestLog[questIndex].QuestId = (uint)value;
                                            break;
                                        case 1:
                                            ((WoWPlayer)unit).QuestLog[questIndex].QuestCounters = (byte[])value;
                                            break;
                                        case 2:
                                            ((WoWPlayer)unit).QuestLog[questIndex].QuestState = (uint)value;
                                            break;
                                    }
                                }
                                break;
                            case >= EUnitFields.PLAYER_VISIBLE_ITEM_1_CREATOR and <= EUnitFields.PLAYER_VISIBLE_ITEM_LAST_PAD:
                                {
                                    uint visibleItemField = (field - EUnitFields.PLAYER_VISIBLE_ITEM_1_CREATOR) % 12;
                                    int itemIndex = (int)((field - EUnitFields.PLAYER_VISIBLE_ITEM_1_CREATOR) / 12);
                                    var item = ((WoWPlayer)unit).VisibleItems[itemIndex];
                                    switch (visibleItemField)
                                    {
                                        case 0:
                                            item.CreatedBy.LowGuidValue = (byte[])value;
                                            break;
                                        case 1:
                                            item.CreatedBy.HighGuidValue = (byte[])value;
                                            break;
                                        case 2:
                                            ((WoWItem)item).ItemId = (uint)value;
                                            break;
                                        case 3:
                                            item.Owner.LowGuidValue = (byte[])value;
                                            break;
                                        case 4:
                                            item.Owner.HighGuidValue = (byte[])value;
                                            break;
                                        case 5:
                                            item.Contained.LowGuidValue = (byte[])value;
                                            break;
                                        case 6:
                                            item.Contained.HighGuidValue = (byte[])value;
                                            break;
                                        case 7:
                                            item.GiftCreator.LowGuidValue = (byte[])value;
                                            break;
                                        case 8:
                                            item.GiftCreator.HighGuidValue = (byte[])value;
                                            break;
                                        case 9:
                                            ((WoWItem)item).StackCount = (uint)value;
                                            break;
                                        case 10:
                                            ((WoWItem)item).Durability = (uint)value;
                                            break;
                                        case 11:
                                            ((WoWItem)item).PropertySeed = (uint)value;
                                            break;
                                    }
                                }
                                break;
                            case >= EUnitFields.PLAYER_FIELD_INV_SLOT_HEAD and < EUnitFields.PLAYER_FIELD_PACK_SLOT_1:
                                ((WoWPlayer)unit).Inventory[field - EUnitFields.PLAYER_FIELD_INV_SLOT_HEAD] = (uint)value;
                                break;
                            case >= EUnitFields.PLAYER_FIELD_PACK_SLOT_1 and <= EUnitFields.PLAYER_FIELD_PACK_SLOT_LAST:
                                ((WoWPlayer)unit).PackSlots[field - EUnitFields.PLAYER_FIELD_PACK_SLOT_1] = (uint)value;
                                break;
                            case >= EUnitFields.PLAYER_FIELD_BANK_SLOT_1 and <= EUnitFields.PLAYER_FIELD_BANK_SLOT_LAST:
                                ((WoWPlayer)unit).BankSlots[field - EUnitFields.PLAYER_FIELD_BANK_SLOT_1] = (uint)value;
                                break;
                            case >= EUnitFields.PLAYER_FIELD_BANKBAG_SLOT_1 and <= EUnitFields.PLAYER_FIELD_BANKBAG_SLOT_LAST:
                                ((WoWPlayer)unit).BankBagSlots[field - EUnitFields.PLAYER_FIELD_BANKBAG_SLOT_1] = (uint)value;
                                break;
                            case >= EUnitFields.PLAYER_FIELD_VENDORBUYBACK_SLOT_1 and <= EUnitFields.PLAYER_FIELD_VENDORBUYBACK_SLOT_LAST:
                                ((WoWPlayer)unit).VendorBuybackSlots[field - EUnitFields.PLAYER_FIELD_VENDORBUYBACK_SLOT_1] = (uint)value;
                                break;
                            case >= EUnitFields.PLAYER_FIELD_KEYRING_SLOT_1 and <= EUnitFields.PLAYER_FIELD_KEYRING_SLOT_LAST:
                                ((WoWPlayer)unit).KeyringSlots[field - EUnitFields.PLAYER_FIELD_KEYRING_SLOT_1] = (uint)value;
                                break;
                            case EUnitFields.PLAYER_FARSIGHT:
                                ((WoWPlayer)unit).Farsight = (uint)value;
                                break;
                            case EUnitFields.PLAYER_FIELD_COMBO_TARGET:
                                ((WoWPlayer)unit).ComboTarget.LowGuidValue = (byte[])value;
                                break;
                            case EUnitFields.PLAYER_FIELD_COMBO_TARGET + 1:
                                ((WoWPlayer)unit).ComboTarget.HighGuidValue = (byte[])value;
                                break;
                            case EUnitFields.PLAYER_XP:
                                ((WoWPlayer)unit).XP = (uint)value;
                                break;
                            case EUnitFields.PLAYER_NEXT_LEVEL_XP:
                                ((WoWPlayer)unit).NextLevelXP = (uint)value;
                                break;
                            case >= EUnitFields.PLAYER_SKILL_INFO_1_1 and <= EUnitFields.PLAYER_SKILL_INFO_1_1 + 383:
                                {
                                    uint skillField = (field - EUnitFields.PLAYER_SKILL_INFO_1_1) % 4;
                                    int skillIndex = (int)((field - EUnitFields.PLAYER_SKILL_INFO_1_1) / 4);
                                    switch (skillField)
                                    {
                                        case 0:
                                            ((WoWPlayer)unit).SkillInfo[skillIndex].SkillInt1 = (uint)value;
                                            break;
                                        case 1:
                                            ((WoWPlayer)unit).SkillInfo[skillIndex].SkillInt2 = (uint)value;
                                            break;
                                        case 2:
                                            ((WoWPlayer)unit).SkillInfo[skillIndex].SkillInt3 = (uint)value;
                                            break;
                                        case 3:
                                            ((WoWPlayer)unit).SkillInfo[skillIndex].SkillInt4 = (uint)value;
                                            break;
                                    }
                                }
                                break;
                            case EUnitFields.PLAYER_CHARACTER_POINTS1:
                                ((WoWPlayer)unit).CharacterPoints1 = (uint)value;
                                break;
                            case EUnitFields.PLAYER_CHARACTER_POINTS2:
                                ((WoWPlayer)unit).CharacterPoints2 = (uint)value;
                                break;
                            case EUnitFields.PLAYER_TRACK_CREATURES:
                                ((WoWPlayer)unit).TrackCreatures = (uint)value;
                                break;
                            case EUnitFields.PLAYER_TRACK_RESOURCES:
                                ((WoWPlayer)unit).TrackResources = (uint)value;
                                break;
                            case EUnitFields.PLAYER_BLOCK_PERCENTAGE:
                                ((WoWPlayer)unit).BlockPercentage = (uint)value;
                                break;
                            case EUnitFields.PLAYER_DODGE_PERCENTAGE:
                                ((WoWPlayer)unit).DodgePercentage = (uint)value;
                                break;
                            case EUnitFields.PLAYER_PARRY_PERCENTAGE:
                                ((WoWPlayer)unit).ParryPercentage = (uint)value;
                                break;
                            case EUnitFields.PLAYER_CRIT_PERCENTAGE:
                                ((WoWPlayer)unit).CritPercentage = (uint)value;
                                break;
                            case EUnitFields.PLAYER_RANGED_CRIT_PERCENTAGE:
                                ((WoWPlayer)unit).RangedCritPercentage = (uint)value;
                                break;
                            case >= EUnitFields.PLAYER_EXPLORED_ZONES_1 and < EUnitFields.PLAYER_REST_STATE_EXPERIENCE:
                                ((WoWPlayer)unit).ExploredZones[field - EUnitFields.PLAYER_EXPLORED_ZONES_1] = (uint)value;
                                break;
                            case EUnitFields.PLAYER_REST_STATE_EXPERIENCE:
                                ((WoWPlayer)unit).RestStateExperience = (uint)value;
                                break;
                            case EUnitFields.PLAYER_FIELD_COINAGE:
                                ((WoWPlayer)unit).Coinage = (uint)value;
                                break;
                            case >= EUnitFields.PLAYER_FIELD_POSSTAT0 and <= EUnitFields.PLAYER_FIELD_POSSTAT4:
                                ((WoWPlayer)unit).StatBonusesPos[field - EUnitFields.PLAYER_FIELD_POSSTAT0] = (uint)value;
                                break;
                            case >= EUnitFields.PLAYER_FIELD_NEGSTAT0 and <= EUnitFields.PLAYER_FIELD_NEGSTAT4:
                                ((WoWPlayer)unit).StatBonusesNeg[field - EUnitFields.PLAYER_FIELD_NEGSTAT0] = (uint)value;
                                break;
                            case >= EUnitFields.PLAYER_FIELD_RESISTANCEBUFFMODSPOSITIVE and <= EUnitFields.PLAYER_FIELD_RESISTANCEBUFFMODSNEGATIVE + 6:
                                if (field <= EUnitFields.PLAYER_FIELD_RESISTANCEBUFFMODSNEGATIVE)
                                    ((WoWPlayer)unit).ResistBonusesPos[field - EUnitFields.PLAYER_FIELD_RESISTANCEBUFFMODSPOSITIVE] = (uint)value;
                                else
                                    ((WoWPlayer)unit).ResistBonusesNeg[field - EUnitFields.PLAYER_FIELD_RESISTANCEBUFFMODSNEGATIVE] = (uint)value;
                                break;
                            case >= EUnitFields.PLAYER_FIELD_MOD_DAMAGE_DONE_POS and <= EUnitFields.PLAYER_FIELD_MOD_DAMAGE_DONE_POS + 6:
                                ((WoWPlayer)unit).ModDamageDonePos[field - EUnitFields.PLAYER_FIELD_MOD_DAMAGE_DONE_POS] = (uint)value;
                                break;
                            case >= EUnitFields.PLAYER_FIELD_MOD_DAMAGE_DONE_NEG and <= EUnitFields.PLAYER_FIELD_MOD_DAMAGE_DONE_NEG + 6:
                                ((WoWPlayer)unit).ModDamageDoneNeg[field - EUnitFields.PLAYER_FIELD_MOD_DAMAGE_DONE_NEG] = (uint)value;
                                break;
                            case >= EUnitFields.PLAYER_FIELD_MOD_DAMAGE_DONE_PCT and <= EUnitFields.PLAYER_FIELD_MOD_DAMAGE_DONE_PCT + 6:
                                ((WoWPlayer)unit).ModDamageDonePct[field - EUnitFields.PLAYER_FIELD_MOD_DAMAGE_DONE_PCT] = (float)value;
                                break;
                            case EUnitFields.PLAYER_AMMO_ID:
                                ((WoWPlayer)unit).AmmoId = (uint)value;
                                break;
                            case EUnitFields.PLAYER_SELF_RES_SPELL:
                                ((WoWPlayer)unit).SelfResSpell = (uint)value;
                                break;
                            case EUnitFields.PLAYER_FIELD_PVP_MEDALS:
                                ((WoWPlayer)unit).PvpMedals = (uint)value;
                                break;
                            case >= EUnitFields.PLAYER_FIELD_BUYBACK_PRICE_1 and <= EUnitFields.PLAYER_FIELD_BUYBACK_PRICE_LAST:
                                ((WoWPlayer)unit).BuybackPrices[field - EUnitFields.PLAYER_FIELD_BUYBACK_PRICE_1] = (uint)value;
                                break;
                            case >= EUnitFields.PLAYER_FIELD_BUYBACK_TIMESTAMP_1 and <= EUnitFields.PLAYER_FIELD_BUYBACK_TIMESTAMP_LAST:
                                ((WoWPlayer)unit).BuybackTimestamps[field - EUnitFields.PLAYER_FIELD_BUYBACK_TIMESTAMP_1] = (uint)value;
                                break;
                            case EUnitFields.PLAYER_FIELD_SESSION_KILLS:
                                ((WoWPlayer)unit).SessionKills = (uint)value;
                                break;
                            case EUnitFields.PLAYER_FIELD_YESTERDAY_KILLS:
                                ((WoWPlayer)unit).YesterdayKills = (uint)value;
                                break;
                            case EUnitFields.PLAYER_FIELD_LAST_WEEK_KILLS:
                                ((WoWPlayer)unit).LastWeekKills = (uint)value;
                                break;
                            case EUnitFields.PLAYER_FIELD_THIS_WEEK_KILLS:
                                ((WoWPlayer)unit).ThisWeekKills = (uint)value;
                                break;
                            case EUnitFields.PLAYER_FIELD_THIS_WEEK_CONTRIBUTION:
                                ((WoWPlayer)unit).ThisWeekContribution = (uint)value;
                                break;
                            case EUnitFields.PLAYER_FIELD_LIFETIME_HONORABLE_KILLS:
                                ((WoWPlayer)unit).LifetimeHonorableKills = (uint)value;
                                break;
                            case EUnitFields.PLAYER_FIELD_LIFETIME_DISHONORABLE_KILLS:
                                ((WoWPlayer)unit).LifetimeDishonorableKills = (uint)value;
                                break;
                            case EUnitFields.PLAYER_FIELD_BYTES2:
                                ((WoWPlayer)unit).FieldBytes2 = (byte[])value;
                                break;
                            case EUnitFields.PLAYER_FIELD_WATCHED_FACTION_INDEX:
                                ((WoWPlayer)unit).WatchedFactionIndex = (uint)value;
                                break;
                            case >= EUnitFields.PLAYER_FIELD_COMBAT_RATING_1 and <= EUnitFields.PLAYER_FIELD_COMBAT_RATING_1 + 20:
                                ((WoWPlayer)unit).CombatRating[field - EUnitFields.PLAYER_FIELD_COMBAT_RATING_1] = (uint)value;
                                break;
                        }
                    }
                }


            }
        }

        private static void ApplyMovementData(WoWUnit unit, MovementInfoUpdate data)
        {
            unit.MovementFlags = data.MovementFlags;
            unit.LastUpdated = data.LastUpdated;
            unit.Position.X = data.X;
            unit.Position.Y = data.Y;
            unit.Position.Z = data.Z;
            unit.Facing = data.Facing;
            unit.TransportGuid = data.TransportGuid ?? 0;
            unit.TransportOffset = data.TransportOffset ?? unit.TransportOffset;
            unit.TransportOrientation = data.TransportOrientation ?? 0f;
            unit.TransportLastUpdated = data.TransportLastUpdated ?? 0;
            unit.SwimPitch = data.SwimPitch ?? 0f;
            unit.FallTime = data.FallTime;
            unit.JumpVerticalSpeed = data.JumpVerticalSpeed ?? 0f;
            unit.JumpSinAngle = data.JumpSinAngle ?? 0f;
            unit.JumpCosAngle = data.JumpCosAngle ?? 0f;
            unit.JumpHorizontalSpeed = data.JumpHorizontalSpeed ?? 0f;
            unit.SplineElevation = data.SplineElevation ?? 0f;

            if (data.MovementBlockUpdate != null)
            {
                unit.WalkSpeed = data.MovementBlockUpdate.WalkSpeed;
                unit.RunSpeed = data.MovementBlockUpdate.RunSpeed;
                unit.RunBackSpeed = data.MovementBlockUpdate.RunBackSpeed;
                unit.SwimSpeed = data.MovementBlockUpdate.SwimSpeed;
                unit.SwimBackSpeed = data.MovementBlockUpdate.SwimBackSpeed;
                unit.TurnRate = data.MovementBlockUpdate.TurnRate;
                unit.SplineFlags = data.MovementBlockUpdate.SplineFlags ?? SplineFlags.None;
                unit.SplineFinalPoint = data.MovementBlockUpdate.SplineFinalPoint ?? unit.SplineFinalPoint;
                unit.SplineTargetGuid = data.MovementBlockUpdate.SplineTargetGuid ?? 0;
                unit.SplineFinalOrientation = data.MovementBlockUpdate.SplineFinalOrientation ?? 0f;
                unit.SplineTimePassed = data.MovementBlockUpdate.SplineTimePassed ?? 0;
                unit.SplineDuration = data.MovementBlockUpdate.SplineDuration ?? 0;
                unit.SplineId = data.MovementBlockUpdate.SplineId ?? 0;
                unit.SplineNodes = data.MovementBlockUpdate.SplineNodes ?? [];
                unit.SplineFinalDestination = data.MovementBlockUpdate.SplineFinalDestination ?? unit.SplineFinalDestination;
                unit.SplineType = data.MovementBlockUpdate.SplineType;
                unit.SplineTargetGuid = data.MovementBlockUpdate.FacingTargetGuid;
                unit.FacingAngle = data.MovementBlockUpdate.FacingAngle;
                unit.FacingSpot = data.MovementBlockUpdate.FacingSpot;
                unit.SplineTimestamp = data.MovementBlockUpdate.SplineTimestamp;
                unit.SplinePoints = data.MovementBlockUpdate.SplinePoints;
            }
        }

        private WoWObject CreateObjectFromFields(WoWObjectType objectType, ulong guid, Dictionary<uint, object?> fields)
        {
            WoWObject obj = objectType switch
            {
                WoWObjectType.Item => new WoWItem(new HighGuid(guid)),
                WoWObjectType.Container => new WoWContainer(new HighGuid(guid)),
                WoWObjectType.Unit => new WoWUnit(new HighGuid(guid)),
                WoWObjectType.Player => guid == PlayerGuid.FullGuid ? (WoWLocalPlayer)Player : new WoWPlayer(new HighGuid(guid)),
                WoWObjectType.GameObj => new WoWGameObject(new HighGuid(guid)),
                WoWObjectType.DynamicObj => new WoWDynamicObject(new HighGuid(guid)),
                WoWObjectType.Corpse => new WoWCorpse(new HighGuid(guid)),
                _ => new WoWObject(new HighGuid(guid)),
            };
            ApplyFieldDiffs(obj, fields);
            return obj;
        }
        #region NotImplemented


        public bool IsSpellReady(string spellName)
        {
            throw new NotImplementedException();
        }

        public void StopCasting()
        {
            throw new NotImplementedException();
        }

        public void CastSpell(string spellName, int rank = -1, bool castOnSelf = false)
        {
            throw new NotImplementedException();
        }

        public void CastSpell(int spellId, int rank = -1, bool castOnSelf = false)
        {
            throw new NotImplementedException();
        }

        public bool CanCastSpell(int spellId, ulong targetGuid)
        {
            throw new NotImplementedException();
        }

        public void UseItem(int bagId, int slotId, ulong targetGuid = 0)
        {
            throw new NotImplementedException();
        }

        public ulong GetBackpackItemGuid(int parSlot)
        {
            throw new NotImplementedException();
        }

        public ulong GetEquippedItemGuid(EquipSlot slot)
        {
            throw new NotImplementedException();
        }

        public IWoWItem GetEquippedItem(EquipSlot ranged)
        {
            throw new NotImplementedException();
        }

        public IWoWItem GetContainedItem(int bagSlot, int slotId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IWoWItem> GetEquippedItems()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IWoWItem> GetContainedItems()
        {
            throw new NotImplementedException();
        }

        public uint GetBagGuid(EquipSlot equipSlot)
        {
            throw new NotImplementedException();
        }

        public void PickupContainedItem(int bagSlot, int slotId, int quantity)
        {
            throw new NotImplementedException();
        }

        public void PlaceItemInContainer(int bagSlot, int slotId)
        {
            throw new NotImplementedException();
        }

        public void DestroyItemInContainer(int bagSlot, int slotId, int quantity = -1)
        {
            throw new NotImplementedException();
        }

        public void Logout()
        {
            throw new NotImplementedException();
        }

        public void SplitStack(int bag, int slot, int quantity, int destinationBag, int destinationSlot)
        {
            throw new NotImplementedException();
        }

        public void EquipItem(int bagSlot, int slotId, EquipSlot? equipSlot = null)
        {
            throw new NotImplementedException();
        }

        public void UnequipItem(EquipSlot slot)
        {
            throw new NotImplementedException();
        }

        public void AcceptResurrect()
        {
            throw new NotImplementedException();
        }

        public IWoWPlayer PartyLeader => throw new NotImplementedException();

        public ulong PartyLeaderGuid => throw new NotImplementedException();

        public ulong Party1Guid => throw new NotImplementedException();

        public ulong Party2Guid => throw new NotImplementedException();

        public ulong Party3Guid => throw new NotImplementedException();

        public ulong Party4Guid => throw new NotImplementedException();

        public ulong StarTargetGuid => throw new NotImplementedException();

        public ulong CircleTargetGuid => throw new NotImplementedException();

        public ulong DiamondTargetGuid => throw new NotImplementedException();

        public ulong TriangleTargetGuid => throw new NotImplementedException();

        public ulong MoonTargetGuid => throw new NotImplementedException();

        public ulong SquareTargetGuid => throw new NotImplementedException();

        public ulong CrossTargetGuid => throw new NotImplementedException();

        public ulong SkullTargetGuid => throw new NotImplementedException();

        public string GlueDialogText => throw new NotImplementedException();

        public LoginStates LoginState => throw new NotImplementedException();

        public void AntiAfk()
        {

        }

        public IWoWUnit GetTarget(IWoWUnit woWUnit)
        {
            return null;
        }

        public sbyte GetTalentRank(uint tabIndex, uint talentIndex)
        {
            return 0;
        }

        public void PickupInventoryItem(uint inventorySlot)
        {

        }

        public void DeleteCursorItem()
        {

        }

        public void EquipCursorItem()
        {

        }

        public void ConfirmItemEquip()
        {

        }

        public void SendChatMessage(string chatMessage)
        {

        }

        public void SetRaidTarget(IWoWUnit target, TargetMarker v)
        {

        }

        public void JoinBattleGroundQueue()
        {

        }

        public void ResetInstances()
        {

        }

        public void PickupMacro(uint v)
        {

        }

        public void PlaceAction(uint v)
        {

        }

        public void InviteToGroup(ulong guid)
        {

        }

        public void KickPlayer(ulong guid)
        {

        }

        public void AcceptGroupInvite()
        {

        }

        public void DeclineGroupInvite()
        {

        }

        public void LeaveGroup()
        {

        }

        public void DisbandGroup()
        {

        }

        public void ConvertToRaid()
        {

        }

        public bool HasPendingGroupInvite()
        {
            return false;
        }

        public bool HasLootRollWindow(int itemId)
        {
            return false;
        }

        public void LootPass(int itemId)
        {

        }

        public void LootRollGreed(int itemId)
        {

        }

        public void LootRollNeed(int itemId)
        {

        }

        public void AssignLoot(int itemId, ulong playerGuid)
        {

        }

        public void SetGroupLoot(GroupLootSetting setting)
        {

        }

        public void PromoteLootManager(ulong playerGuid)
        {

        }

        public void PromoteAssistant(ulong playerGuid)
        {

        }

        public void PromoteLeader(ulong playerGuid)
        {

        }

        public void DoEmote(Emote emote)
        {
            throw new NotImplementedException();
        }

        public void DoEmote(TextEmote emote)
        {
            throw new NotImplementedException();
        }

        public uint GetManaCost(string healingTouch)
        {
            throw new NotImplementedException();
        }

        public void MoveToward(Position position, float facing)
        {
            throw new NotImplementedException();
        }

        public void RefreshSkills()
        {
            throw new NotImplementedException();
        }

        public void RefreshSpells()
        {
            throw new NotImplementedException();
        }

        public void RetrieveCorpse()
        {
            throw new NotImplementedException();
        }

        public void SetTarget(ulong guid)
        {
            throw new NotImplementedException();
        }

        public void StopAttack()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
    public enum ObjectUpdateOperation
    {
        Add,
        Update,
        Remove
    }
    public record ObjectStateUpdate(
        ulong Guid,
        ObjectUpdateOperation Operation,
        WoWObjectType ObjectType,
        MovementInfoUpdate? MovementData,
        Dictionary<uint, object?> UpdatedFields
    );
}
