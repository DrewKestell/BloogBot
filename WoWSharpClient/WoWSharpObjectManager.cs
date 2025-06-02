using BotRunner.Clients;
using GameData.Core.Enums;
using GameData.Core.Frames;
using GameData.Core.Interfaces;
using GameData.Core.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using System.Timers;
using WoWSharpClient.Client;
using WoWSharpClient.Models;
using WoWSharpClient.Parsers;
using WoWSharpClient.Screens;
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
        private readonly object _objectsLock = new();
        private ControlBits _controlBits = ControlBits.Nothing;

        public bool IsPlayerMoving => _teleportCooldown <= 0 && (_controlBits != ControlBits.Nothing || Player.JumpVerticalSpeed > 0 || Player.JumpHorizontalSpeed > 0);

        public WoWSharpObjectManager(string ipAddress, PathfindingClient pathfindingClient, ILogger<WoWSharpObjectManager> logger)
        {
            _logger = logger;

            _eventEmitter.OnLoginFailure += EventEmitter_OnLoginFailure;
            _eventEmitter.OnLoginVerifyWorld += EventEmitter_OnLoginVerifyWorld;
            _eventEmitter.OnWorldSessionStart += EventEmitter_OnWorldSessionStart;
            _eventEmitter.OnWorldSessionEnd += EventEmitter_OnWorldSessionEnd;
            _eventEmitter.OnCharacterListLoaded += EventEmitter_OnCharacterListLoaded;
            _eventEmitter.OnChatMessage += EventEmitter_OnChatMessage;
            _eventEmitter.OnGameObjectCreated += EventEmitter_OnGameObjectCreated;
            _eventEmitter.OnCharacterJumpStart += EventEmitter_OnCharacterJumpStart;
            _eventEmitter.OnCharacterFallLand += EventEmitter_OnCharacterFallLand;
            _eventEmitter.OnCharacterSetFacing += EventEmitter_OnCharacterSetFacing;
            _eventEmitter.OnForceMoveRoot += EventEmitter_OnForceMoveRoot;
            _eventEmitter.OnForceMoveUnroot += EventEmitter_OnForceMoveUnroot;
            _eventEmitter.OnForceRunSpeedChange += EventEmitter_OnForceRunSpeedChange;
            _eventEmitter.OnForceRunBackSpeedChange += EventEmitter_OnForceRunBackSpeedChange;
            _eventEmitter.OnForceSwimSpeedChange += EventEmitter_OnForceSwimSpeedChange;
            _eventEmitter.OnForceMoveKnockBack += EventEmitter_OnForceMoveKnockBack;
            _eventEmitter.OnForceTimeSkipped += EventEmitter_OnForceTimeSkipped;
            _eventEmitter.OnTeleport += EventEmitter_OnTeleport;

            _woWClient = new(ipAddress, this);

            _loginScreen = new(_woWClient);
            _realmScreen = new(_woWClient);
            _characterSelectScreen = new(_woWClient);

            _pathfindingClient = pathfindingClient;
        }

        public void StartGameLoop()
        {
            _gameLoopTimer = new Timer(50);
            _gameLoopTimer.Elapsed += OnGameLoopTick;
            _gameLoopTimer.AutoReset = true;
            _gameLoopTimer.Start();
        }

        private TimeSpan _lastHeartbeat = TimeSpan.Zero;
        private TimeSpan _lastPositionUpdate = TimeSpan.Zero;

        private void OnGameLoopTick(object? sender, ElapsedEventArgs e)
        {
            if (Player == null)
                return;

            var now = _movementTimer.Elapsed;
            var delta = now - _lastPositionUpdate;

            // Update player position every tick
            UpdatePlayerPosition((float)delta.TotalSeconds);
            _lastPositionUpdate = now;

            // Send movement heartbeat if needed
            HandleMovementHeartbeat(now);

            // Send ping heartbeat
            HandlePingHeartbeat((long)now.TotalMilliseconds);
        }

        private void HandleMovementHeartbeat(TimeSpan now)
        {
            var elapsed = now - _lastHeartbeat;

            if (elapsed < (IsPlayerMoving ? TimeSpan.FromMilliseconds(150) : TimeSpan.FromMilliseconds(500)))
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

        private const float Gravity = -19.29f;         // Units/sec² (based on empirical WoW fall data)
        private const float JumpVelocity = 7.8f;       // Matches WoW's classic jump height
        private const float GroundTolerance = 0.05f;    // Margin for "on ground"
        private const float MaxFallSpeed = -60.0f;     // Terminal velocity
        private Position _lastKnownPosition;
        private uint _lastMapId;
        private float _teleportCooldown = 0f;
        private bool _groundZLocked;

        private void UpdatePlayerPosition(float deltaTime)
        {
            var player = (WoWLocalPlayer)Player;

            // 1) Detect teleportation or first-time init
            if (_lastKnownPosition == null || CheckForTeleport(player, deltaTime))
            {
                _lastMapId = MapId;
                _lastKnownPosition = player.Position;
                _groundZLocked = false;
                return;
            }

            // 2) Ignore update if player is rooted
            if (player.MovementFlags.HasFlag(MovementFlags.MOVEFLAG_ROOT))
                return;

            // 3) Compute XY movement
            Vector2 moveDelta = ComputeMovementDelta(player, deltaTime);
            float nextX = player.Position.X + moveDelta.X;
            float nextY = player.Position.Y + moveDelta.Y;

            // 4) Sample VMAP ground height at destination
            var probePosition = new Position(nextX, nextY, player.Position.Z);
            float groundZ = _pathfindingClient.GetGroundZ(MapId, probePosition);

            // 5) Acceptable ground hit (VMAP or ADT fallback)
            bool validGroundZ = groundZ > -500.0f && groundZ < 10000.0f;

            //if (validGroundZ && Math.Abs(player.Position.Z - groundZ) < 10.0f)
            //{
            //    // Grounded: update Z
            //    player.Position = new Position(nextX, nextY, groundZ);
            //    _groundZLocked = true;
            //    ClearFallingFlags(player);
            //}
            //else
            //{
            //    // No reliable hit, retain airborne Z
            //    player.Position = new Position(nextX, nextY, player.Position.Z);
            //    _groundZLocked = false;
            //    SetFallingFlags(player);
            //}
            // 6) Finalize step
            _lastKnownPosition = player.Position;
            player.Position = new Position(nextX, nextY, player.Position.Z);
        }

        private bool CheckForTeleport(WoWLocalPlayer player, float deltaTime)
        {
            // Cooldown window to let the server settle you in
            if (_teleportCooldown > 0f)
            {
                _teleportCooldown -= deltaTime;
                return true;
            }

            return false;
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

            return dir * player.RunSpeed * deltaTime;
        }

        private static void ClearFallingFlags(WoWLocalPlayer player)
        {
            player.MovementFlags &= ~MovementFlags.MOVEFLAG_FALLING;
            player.MovementFlags &= ~MovementFlags.MOVEFLAG_FALLINGFAR;
        }

        private static void SetFallingFlags(WoWLocalPlayer player)
        {
            player.MovementFlags |= MovementFlags.MOVEFLAG_FALLING;
            player.MovementFlags &= ~MovementFlags.MOVEFLAG_FALLINGFAR;
        }
        private void EventEmitter_OnForceTimeSkipped(object? sender, RequiresAcknowledgementArgs e)
        {

        }

        private void EventEmitter_OnTeleport(object? sender, RequiresAcknowledgementArgs e)
        {
            _woWClient.ResetMovementCounter();
            _woWClient.SendMSGPacked(Opcode.MSG_MOVE_TELEPORT_ACK, MovementPacketHandler.BuildMoveTeleportAckPayload(e.Guid, _woWClient.MovementCounter, (uint)_movementTimer.ElapsedMilliseconds));
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

            _woWClient.SendMoveWorldPortAcknowledge();
            _woWClient.SendSetActiveMover(PlayerGuid.FullGuid);

            StartGameLoop();

            _movementTimer.Restart();
        }

        private void EventEmitter_OnCharacterSetFacing(object? sender, CharacterActionArgs e)
        {
            WoWUnit unit = (WoWUnit)Objects.First(x => x.Guid == e.Guid);
        }

        private void EventEmitter_OnCharacterFallLand(object? sender, CharacterActionArgs e)
        {
            WoWUnit unit = (WoWUnit)Objects.First(x => x.Guid == e.Guid);
        }

        private void EventEmitter_OnCharacterJumpStart(object? sender, CharacterActionArgs e)
        {
            WoWUnit unit = (WoWUnit)Objects.First(x => x.Guid == e.Guid);
        }

        private void EventEmitter_OnCharacterListLoaded(object? sender, EventArgs e)
        {
            _characterSelectScreen.HasReceivedCharacterList = true;
        }

        private void EventEmitter_OnWorldSessionStart(object? sender, EventArgs e)
        {
            _characterSelectScreen.RefreshCharacterListFromServer();
        }

        private void EventEmitter_OnGameObjectCreated(object? sender, GameObjectCreatedArgs e)
        {
            _woWClient.SendNameQuery(e.Guid);
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


        private ImmutableList<WoWObject> _objects = [];
        public IEnumerable<IWoWObject> Objects
        {
            get
            {
                lock (_updateLock)
                {
                    return _objects;
                }
            }
        }

        private readonly Queue<ObjectUpdate> _pendingUpdates = new();
        private readonly object _updateLock = new();

        public void QueueUpdate(ObjectUpdate update)
        {
            lock (_updateLock)
            {
                _pendingUpdates.Enqueue(update);
            }
        }

        public void ProcessUpdates()
        {
            lock (_updateLock)
            {
                while (_pendingUpdates.Count > 0)
                {
                    var update = _pendingUpdates.Dequeue();
                    switch (update.Type)
                    {
                        case ObjectUpdateOperation.Add:
                            _objects = _objects.Add(update.Object);
                            break;

                        case ObjectUpdateOperation.Remove:
                            _objects = _objects.Remove(update.Object);
                            break;
                    }
                }
            }
        }
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
            if (_teleportCooldown > 0)
                return;

            ((WoWLocalPlayer)Player).Facing = facing;
            _woWClient.SendMovementOpcode(Opcode.MSG_MOVE_SET_FACING, MovementPacketHandler.BuildMovementInfoBuffer((WoWLocalPlayer)Player, (uint)_movementTimer.ElapsedMilliseconds));
        }

        public void StartMovement(ControlBits bits)
        {
            if (_teleportCooldown > 0)
                return;

            _controlBits = bits;

            var player = (WoWLocalPlayer)Player;
            Opcode opcode = Opcode.MSG_MOVE_STOP;

            switch (bits)
            {
                case ControlBits.Nothing:
                case ControlBits.MovingFrontOrBack:
                case ControlBits.CtmWalk:
                case ControlBits.Strafing:
                case ControlBits.Turning:
                    break;

                case ControlBits.Front:
                    opcode = Opcode.MSG_MOVE_START_FORWARD;
                    player.MovementFlags |= MovementFlags.MOVEFLAG_FORWARD;
                    player.MovementFlags &= ~MovementFlags.MOVEFLAG_BACKWARD;
                    break;

                case ControlBits.Back:
                    opcode = Opcode.MSG_MOVE_START_BACKWARD;
                    player.MovementFlags |= MovementFlags.MOVEFLAG_BACKWARD;
                    player.MovementFlags &= ~MovementFlags.MOVEFLAG_FORWARD;
                    break;

                case ControlBits.StrafeLeft:
                    opcode = Opcode.MSG_MOVE_START_STRAFE_LEFT;
                    player.MovementFlags |= MovementFlags.MOVEFLAG_STRAFE_LEFT;
                    player.MovementFlags &= ~MovementFlags.MOVEFLAG_STRAFE_RIGHT;
                    break;

                case ControlBits.StrafeRight:
                    opcode = Opcode.MSG_MOVE_START_STRAFE_RIGHT;
                    player.MovementFlags |= MovementFlags.MOVEFLAG_STRAFE_RIGHT;
                    player.MovementFlags &= ~MovementFlags.MOVEFLAG_STRAFE_LEFT;
                    break;

                case ControlBits.Left:
                    opcode = Opcode.MSG_MOVE_START_TURN_LEFT;
                    player.MovementFlags |= MovementFlags.MOVEFLAG_TURN_LEFT;
                    player.MovementFlags &= ~MovementFlags.MOVEFLAG_TURN_RIGHT;
                    break;

                case ControlBits.Right:
                    opcode = Opcode.MSG_MOVE_START_TURN_RIGHT;
                    player.MovementFlags |= MovementFlags.MOVEFLAG_TURN_RIGHT;
                    player.MovementFlags &= ~MovementFlags.MOVEFLAG_TURN_LEFT;
                    break;

                case ControlBits.Jump:
                    opcode = Opcode.MSG_MOVE_JUMP;
                    player.MovementFlags |= MovementFlags.MOVEFLAG_FALLING;
                    break;
            }

            if (player.TransportGuid != 0)
                player.MovementFlags |= MovementFlags.MOVEFLAG_ONTRANSPORT;

            if (player.MovementFlags.HasFlag(MovementFlags.MOVEFLAG_WALK_MODE))
                player.MovementFlags |= MovementFlags.MOVEFLAG_WALK_MODE;
            else
                player.MovementFlags &= ~MovementFlags.MOVEFLAG_WALK_MODE;

            // Optional cleanup: strip conflicting flags (spline/root/flying)
            if (player.MovementFlags.HasFlag(MovementFlags.MOVEFLAG_SPLINE_ENABLED))
            {
                player.MovementFlags &= ~MovementFlags.MOVEFLAG_ROOT;
                player.MovementFlags &= ~MovementFlags.MOVEFLAG_FORWARD;
                player.MovementFlags &= ~MovementFlags.MOVEFLAG_BACKWARD;
            }

            _woWClient.SendMovementOpcode(opcode, MovementPacketHandler.BuildMovementInfoBuffer((WoWLocalPlayer)Player, (uint)_movementTimer.ElapsedMilliseconds));
        }

        public void StopMovement(ControlBits bits)
        {
            var player = (WoWLocalPlayer)Player;

            // Handle directional movement
            if (bits.HasFlag(ControlBits.Front) || bits.HasFlag(ControlBits.Back))
            {
                player.MovementFlags &= ~MovementFlags.MOVEFLAG_FORWARD;
                player.MovementFlags &= ~MovementFlags.MOVEFLAG_BACKWARD;

                var buffer = MovementPacketHandler.BuildMovementInfoBuffer((WoWLocalPlayer)Player, (uint)_movementTimer.ElapsedMilliseconds);
                _woWClient.SendMovementOpcode(Opcode.MSG_MOVE_STOP, buffer);
            }

            // Handle strafing
            if (bits.HasFlag(ControlBits.StrafeLeft) || bits.HasFlag(ControlBits.StrafeRight))
            {
                player.MovementFlags &= ~MovementFlags.MOVEFLAG_STRAFE_LEFT;
                player.MovementFlags &= ~MovementFlags.MOVEFLAG_STRAFE_RIGHT;

                var buffer = MovementPacketHandler.BuildMovementInfoBuffer((WoWLocalPlayer)Player, (uint)_movementTimer.ElapsedMilliseconds);
                _woWClient.SendMovementOpcode(Opcode.MSG_MOVE_STOP_STRAFE, buffer);
            }

            // Handle turning
            if (bits.HasFlag(ControlBits.Left) || bits.HasFlag(ControlBits.Right))
            {
                player.MovementFlags &= ~MovementFlags.MOVEFLAG_TURN_LEFT;
                player.MovementFlags &= ~MovementFlags.MOVEFLAG_TURN_RIGHT;

                var buffer = MovementPacketHandler.BuildMovementInfoBuffer((WoWLocalPlayer)Player, (uint)_movementTimer.ElapsedMilliseconds);
                _woWClient.SendMovementOpcode(Opcode.MSG_MOVE_STOP_TURN, buffer);
            }

            // Handle jumping (falling clears after jump anyway)
            if (bits.HasFlag(ControlBits.Jump))
            {
                player.MovementFlags &= ~MovementFlags.MOVEFLAG_FALLING;

                var buffer = MovementPacketHandler.BuildMovementInfoBuffer((WoWLocalPlayer)Player, (uint)_movementTimer.ElapsedMilliseconds);
                _woWClient.SendMovementOpcode(Opcode.MSG_MOVE_STOP, buffer);
            }
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
                    sb.Append($"[{e.SenderGuid}]");
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
                        _teleportCooldown = 5.0f;
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

    public class ObjectUpdate
    {
        public ObjectUpdateOperation Type { get; init; }
        public WoWObject Object { get; init; } = null!;
    }
}
