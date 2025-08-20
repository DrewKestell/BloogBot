using BotRunner.Clients;
using GameData.Core.Constants;
using GameData.Core.Enums;
using GameData.Core.Frames;
using GameData.Core.Interfaces;
using GameData.Core.Models;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Pathfinding;
using System.Numerics;
using System.Text;
using System.Timers;
using WoWSharpClient.Client;
using WoWSharpClient.Models;
using WoWSharpClient.Movement;
using WoWSharpClient.Parsers;
using WoWSharpClient.Screens;
using WoWSharpClient.Utils;
using static GameData.Core.Enums.UpdateFields;
using Enum = System.Enum;
using Timer = System.Timers.Timer;

namespace WoWSharpClient
{
    public class WoWSharpObjectManager : IObjectManager
    {
        private static WoWSharpObjectManager _instance;

        public static WoWSharpObjectManager Instance
        {
            get
            {
                _instance ??= new WoWSharpObjectManager();

                return _instance;
            }
        }

        private ILogger<WoWSharpObjectManager> _logger;

        // Wrapper client for both auth and world transactions
        private WoWClient _woWClient;
        private PathfindingClient _pathfindingClient;

        // Movement controller - handles all movement logic
        private MovementController _movementController;

        private LoginScreen _loginScreen;
        private RealmSelectScreen _realmScreen;
        private CharacterSelectScreen _characterSelectScreen;

        private Timer _gameLoopTimer;

        private long _lastPingMs = 0;
        private ControlBits _controlBits = ControlBits.Nothing;
        private float _facing = 0;
        private uint _lastNetworkUpdate = 0;
        private const uint NETWORK_UPDATE_RATE = 100;
        public bool IsPlayerMoving => !Player.MovementFlags.Equals(MovementFlags.MOVEFLAG_NONE);

        private bool _isInControl = false;
        private bool _isBeingTeleported = true;

        private TimeSpan _lastHeartbeat = TimeSpan.Zero;
        private TimeSpan _lastPositionUpdate = TimeSpan.Zero;
        private WorldTimeTracker _worldTimeTracker;

        private long _lastSentTime = 0;
        private uint _fallTime = 0;
        private Position _lastSentPosition = new(0, 0, 0);
        private const int HeartbeatMinMs = 500;     // min gap between HB

        private Vector3 _velocity = new();
        private MovementFlags _lastMovementFlags = MovementFlags.MOVEFLAG_NONE;

        private WoWSharpObjectManager() { }

        public void Initialize(
            WoWClient wowClient,
            PathfindingClient pathfindingClient,
            ILogger<WoWSharpObjectManager> logger
        )
        {
            WoWSharpEventEmitter.Instance.Reset();
            _objects.Clear();
            _pendingUpdates.Clear();

            _logger = logger;
            _pathfindingClient = pathfindingClient;
            _woWClient = wowClient;

            WoWSharpEventEmitter.Instance.OnLoginFailure += EventEmitter_OnLoginFailure;
            WoWSharpEventEmitter.Instance.OnLoginVerifyWorld += EventEmitter_OnLoginVerifyWorld;
            WoWSharpEventEmitter.Instance.OnWorldSessionStart += EventEmitter_OnWorldSessionStart;
            WoWSharpEventEmitter.Instance.OnWorldSessionEnd += EventEmitter_OnWorldSessionEnd;
            WoWSharpEventEmitter.Instance.OnCharacterListLoaded +=
                EventEmitter_OnCharacterListLoaded;
            WoWSharpEventEmitter.Instance.OnChatMessage += EventEmitter_OnChatMessage;
            WoWSharpEventEmitter.Instance.OnForceMoveRoot += EventEmitter_OnForceMoveRoot;
            WoWSharpEventEmitter.Instance.OnForceMoveUnroot += EventEmitter_OnForceMoveUnroot;
            WoWSharpEventEmitter.Instance.OnForceRunSpeedChange +=
                EventEmitter_OnForceRunSpeedChange;
            WoWSharpEventEmitter.Instance.OnForceRunBackSpeedChange +=
                EventEmitter_OnForceRunBackSpeedChange;
            WoWSharpEventEmitter.Instance.OnForceSwimSpeedChange +=
                EventEmitter_OnForceSwimSpeedChange;
            WoWSharpEventEmitter.Instance.OnForceMoveKnockBack += EventEmitter_OnForceMoveKnockBack;
            WoWSharpEventEmitter.Instance.OnForceTimeSkipped += EventEmitter_OnForceTimeSkipped;
            WoWSharpEventEmitter.Instance.OnTeleport += EventEmitter_OnTeleport;
            WoWSharpEventEmitter.Instance.OnClientControlUpdate +=
                EventEmitter_OnClientControlUpdate;
            WoWSharpEventEmitter.Instance.OnSetTimeSpeed += EventEmitter_OnSetTimeSpeed;
            WoWSharpEventEmitter.Instance.OnSpellGo += EventEmitter_OnSpellGo;

            _loginScreen = new(_woWClient);
            _realmScreen = new(_woWClient);
            _characterSelectScreen = new(_woWClient);
        }
        private void InitializeMovementController()
        {
            // Initialize movement controller when we have a player
            if (Player != null && _woWClient != null && _pathfindingClient != null)
            {
                _movementController = new MovementController(
                    _woWClient,
                    _pathfindingClient,
                    (WoWLocalPlayer)Player
                );
            }
        }
        private void EventEmitter_OnSpellGo(object? sender, EventArgs e) { }

        private void EventEmitter_OnClientControlUpdate(object? sender, EventArgs e)
        {
            _isInControl = true;
            _isBeingTeleported = false;

            Console.WriteLine($"[OnClientControlUpdate]{Player.Position}");
        }

        private void EventEmitter_OnSetTimeSpeed(object? sender, OnSetTimeSpeedArgs e)
        {
            _woWClient.QueryTime();
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
            var now = _worldTimeTracker.NowMS;
            var delta = now - _lastPositionUpdate;

            // Advance every monster/NPC spline before physics
            Splines.Instance.Update((float)delta.TotalMilliseconds);

            // Process object updates
            ProcessUpdates();

            // Handle ping heartbeat
            HandlePingHeartbeat((long)now.TotalMilliseconds);

            // Update player movement if we're in control
            if (_isInControl && !_isBeingTeleported && Player != null && _movementController != null)
            {
                _movementController.Update(
                    (float)delta.TotalMilliseconds / 1000,
                    (uint)now.TotalMilliseconds
                );
            }

            _lastPositionUpdate = now;
        }

        private void HandlePingHeartbeat(long now)
        {
            const int interval = 30000;

            if (now - _lastPingMs < interval)
                return;

            _lastPingMs = now;
            _woWClient.SendPing();
        }

        private Opcode GetCurrentMovementOpcode(MovementFlags flags)
        {
            // Simple opcode selection based on current state
            if (flags == MovementFlags.MOVEFLAG_NONE)
                return Opcode.MSG_MOVE_STOP;
            if (flags.HasFlag(MovementFlags.MOVEFLAG_JUMPING))
                return Opcode.MSG_MOVE_JUMP;
            if (flags.HasFlag(MovementFlags.MOVEFLAG_FORWARD))
                return Opcode.MSG_MOVE_HEARTBEAT;
            if (flags.HasFlag(MovementFlags.MOVEFLAG_BACKWARD))
                return Opcode.MSG_MOVE_HEARTBEAT;
            if (flags.HasFlag(MovementFlags.MOVEFLAG_STRAFE_LEFT) || flags.HasFlag(MovementFlags.MOVEFLAG_STRAFE_RIGHT))
                return Opcode.MSG_MOVE_HEARTBEAT;

            return Opcode.MSG_MOVE_HEARTBEAT;
        }

        // ============= INPUT HANDLERS =============
        public void StartMovement(ControlBits bits)
        {
            var player = (WoWLocalPlayer)Player;
            if (player == null) return;

            // Convert control bits to movement flags and update player state
            MovementFlags flags = ConvertControlBitsToFlags(bits, player.MovementFlags, true);
            player.MovementFlags = flags;
        }

        public void StopMovement(ControlBits bits)
        {
            var player = (WoWLocalPlayer)Player;
            if (player == null) return;

            // Remove the corresponding movement flags
            MovementFlags flags = ConvertControlBitsToFlags(bits, player.MovementFlags, false);
            player.MovementFlags = flags;

            // If stopping all movement, ensure controller sends stop packet
            if (flags == MovementFlags.MOVEFLAG_NONE && _movementController != null && _isInControl)
            {
                _movementController.SendStopPacket((uint)_worldTimeTracker.NowMS.TotalMilliseconds);
            }
        }
        private MovementFlags ConvertControlBitsToFlags(ControlBits bits, MovementFlags currentFlags, bool add)
        {
            MovementFlags flags = currentFlags;

            if (bits.HasFlag(ControlBits.Front))
            {
                if (add) flags |= MovementFlags.MOVEFLAG_FORWARD;
                else flags &= ~MovementFlags.MOVEFLAG_FORWARD;
            }
            if (bits.HasFlag(ControlBits.Back))
            {
                if (add) flags |= MovementFlags.MOVEFLAG_BACKWARD;
                else flags &= ~MovementFlags.MOVEFLAG_BACKWARD;
            }
            if (bits.HasFlag(ControlBits.StrafeLeft))
            {
                if (add) flags |= MovementFlags.MOVEFLAG_STRAFE_LEFT;
                else flags &= ~MovementFlags.MOVEFLAG_STRAFE_LEFT;
            }
            if (bits.HasFlag(ControlBits.StrafeRight))
            {
                if (add) flags |= MovementFlags.MOVEFLAG_STRAFE_RIGHT;
                else flags &= ~MovementFlags.MOVEFLAG_STRAFE_RIGHT;
            }
            if (bits.HasFlag(ControlBits.Left))
            {
                if (add) flags |= MovementFlags.MOVEFLAG_TURN_LEFT;
                else flags &= ~MovementFlags.MOVEFLAG_TURN_LEFT;
            }
            if (bits.HasFlag(ControlBits.Right))
            {
                if (add) flags |= MovementFlags.MOVEFLAG_TURN_RIGHT;
                else flags &= ~MovementFlags.MOVEFLAG_TURN_RIGHT;
            }
            if (bits.HasFlag(ControlBits.Jump))
            {
                if (add) flags |= MovementFlags.MOVEFLAG_JUMPING;
                else flags &= ~MovementFlags.MOVEFLAG_JUMPING;
            }

            return flags;
        }
        public void SetFacing(float facing)
        {
            var player = (WoWLocalPlayer)Player;
            if (player == null) return;

            player.Facing = facing;

            // Send facing update immediately via movement controller
            if (_movementController != null && _isInControl && !_isBeingTeleported)
            {
                _movementController.SendFacingUpdate((uint)_worldTimeTracker.NowMS.TotalMilliseconds);
            }
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
            var buffer = MovementPacketHandler.BuildMovementInfoBuffer(
                player,
                (uint)_worldTimeTracker.NowMS.TotalMilliseconds
            );
            _woWClient.SendMovementOpcode(opcode, buffer);
        }

        private static Opcode DetermineMovementOpcode(MovementFlags current, MovementFlags previous)
        {
            // Check for new movements (transitions from not having flag to having flag)
            if (current.HasFlag(MovementFlags.MOVEFLAG_JUMPING) && !previous.HasFlag(MovementFlags.MOVEFLAG_JUMPING))
                return Opcode.MSG_MOVE_JUMP;

            // Check for stopping movement
            if (previous != MovementFlags.MOVEFLAG_NONE && current == MovementFlags.MOVEFLAG_NONE)
                return Opcode.MSG_MOVE_STOP;

            // Check for NEW forward movement
            if (current.HasFlag(MovementFlags.MOVEFLAG_FORWARD) && !previous.HasFlag(MovementFlags.MOVEFLAG_FORWARD))
                return Opcode.MSG_MOVE_START_FORWARD;

            // Check for NEW backward movement  
            if (current.HasFlag(MovementFlags.MOVEFLAG_BACKWARD) && !previous.HasFlag(MovementFlags.MOVEFLAG_BACKWARD))
                return Opcode.MSG_MOVE_START_BACKWARD;

            // Check for NEW strafe left movement
            if (current.HasFlag(MovementFlags.MOVEFLAG_STRAFE_LEFT) && !previous.HasFlag(MovementFlags.MOVEFLAG_STRAFE_LEFT))
                return Opcode.MSG_MOVE_START_STRAFE_LEFT;

            // Check for NEW strafe right movement
            if (current.HasFlag(MovementFlags.MOVEFLAG_STRAFE_RIGHT) && !previous.HasFlag(MovementFlags.MOVEFLAG_STRAFE_RIGHT))
                return Opcode.MSG_MOVE_START_STRAFE_RIGHT;

            // Check for stopping specific movements
            if (!current.HasFlag(MovementFlags.MOVEFLAG_STRAFE_LEFT) && !current.HasFlag(MovementFlags.MOVEFLAG_STRAFE_RIGHT) &&
                (previous.HasFlag(MovementFlags.MOVEFLAG_STRAFE_LEFT) || previous.HasFlag(MovementFlags.MOVEFLAG_STRAFE_RIGHT)))
                return Opcode.MSG_MOVE_STOP_STRAFE;

            if (!current.HasFlag(MovementFlags.MOVEFLAG_TURN_LEFT) && !current.HasFlag(MovementFlags.MOVEFLAG_TURN_RIGHT) &&
                (previous.HasFlag(MovementFlags.MOVEFLAG_TURN_LEFT) || previous.HasFlag(MovementFlags.MOVEFLAG_TURN_RIGHT)))
                return Opcode.MSG_MOVE_STOP_TURN;

            // Default to heartbeat if already moving
            return Opcode.MSG_MOVE_HEARTBEAT;
        }

        private WoWObject CreateObjectFromFields(
            WoWObjectType objectType,
            ulong guid,
            Dictionary<uint, object?> fields
        )
        {
            WoWObject obj = objectType switch
            {
                WoWObjectType.Item => new WoWItem(new HighGuid(guid)),
                WoWObjectType.Container => new WoWContainer(new HighGuid(guid)),
                WoWObjectType.Unit => new WoWUnit(new HighGuid(guid)),
                WoWObjectType.Player => guid == PlayerGuid.FullGuid
                    ? (WoWLocalPlayer)Player
                    : new WoWPlayer(new HighGuid(guid)),
                WoWObjectType.GameObj => new WoWGameObject(new HighGuid(guid)),
                WoWObjectType.DynamicObj => new WoWDynamicObject(new HighGuid(guid)),
                WoWObjectType.Corpse => new WoWCorpse(new HighGuid(guid)),
                _ => new WoWObject(new HighGuid(guid)),
            };
            ApplyFieldDiffs(obj, fields);
            return obj;
        }

        private void EventEmitter_OnForceTimeSkipped(
            object? sender,
            RequiresAcknowledgementArgs e
        )
        { }

        private void EventEmitter_OnForceMoveKnockBack(
            object? sender,
            RequiresAcknowledgementArgs e
        )
        {
            _woWClient.SendMSGPacked(
                Opcode.CMSG_MOVE_KNOCK_BACK_ACK,
                MovementPacketHandler.BuildMovementInfoBuffer(
                    (WoWLocalPlayer)Player,
                    (uint)_worldTimeTracker.NowMS.TotalMilliseconds
                )
            );
        }

        private void EventEmitter_OnForceSwimSpeedChange(
            object? sender,
            RequiresAcknowledgementArgs e
        )
        {
            _woWClient.SendMSGPacked(
                Opcode.CMSG_FORCE_SWIM_SPEED_CHANGE_ACK,
                MovementPacketHandler.BuildForceMoveAck(
                    (WoWLocalPlayer)Player,
                    e.Counter,
                    (uint)_worldTimeTracker.NowMS.TotalMilliseconds
                )
            );
        }

        private void EventEmitter_OnForceRunBackSpeedChange(
            object? sender,
            RequiresAcknowledgementArgs e
        )
        {
            _woWClient.SendMSGPacked(
                Opcode.CMSG_FORCE_RUN_BACK_SPEED_CHANGE_ACK,
                MovementPacketHandler.BuildForceMoveAck(
                    (WoWLocalPlayer)Player,
                    e.Counter,
                    (uint)_worldTimeTracker.NowMS.TotalMilliseconds
                )
            );
        }

        private void EventEmitter_OnForceRunSpeedChange(
            object? sender,
            RequiresAcknowledgementArgs e
        )
        {
            _woWClient.SendMSGPacked(
                Opcode.CMSG_FORCE_RUN_SPEED_CHANGE_ACK,
                MovementPacketHandler.BuildForceMoveAck(
                    (WoWLocalPlayer)Player,
                    e.Counter,
                    (uint)_worldTimeTracker.NowMS.TotalMilliseconds
                )
            );
        }

        private void EventEmitter_OnForceMoveUnroot(object? sender, RequiresAcknowledgementArgs e)
        {
            _woWClient.SendMSGPacked(
                Opcode.CMSG_FORCE_MOVE_UNROOT_ACK,
                MovementPacketHandler.BuildForceMoveAck(
                    (WoWLocalPlayer)Player,
                    e.Counter,
                    (uint)_worldTimeTracker.NowMS.TotalMilliseconds
                )
            );
        }

        private void EventEmitter_OnForceMoveRoot(object? sender, RequiresAcknowledgementArgs e)
        {
            _woWClient.SendMSGPacked(
                Opcode.CMSG_FORCE_MOVE_ROOT_ACK,
                MovementPacketHandler.BuildForceMoveAck(
                    (WoWLocalPlayer)Player,
                    e.Counter,
                    (uint)_worldTimeTracker.NowMS.TotalMilliseconds
                )
            );
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

        private void EventEmitter_OnTeleport(object? sender, RequiresAcknowledgementArgs e)
        {
            Console.WriteLine($"[ACK] TELEPORT counter={e.Counter}");

            _woWClient.SendMSGPacked(
                Opcode.MSG_MOVE_TELEPORT_ACK,
                MovementPacketHandler.BuildMoveTeleportAckPayload(
                    e.Guid,
                    e.Counter,
                    (uint)_worldTimeTracker.NowMS.TotalMilliseconds
                )
            );

            _isBeingTeleported = false;
        }

        private void EventEmitter_OnLoginVerifyWorld(object? sender, WorldInfo e)
        {
            ((WoWLocalPlayer)Player).MapId = e.MapId;

            Player.Position.X = e.PositionX;
            Player.Position.Y = e.PositionY;
            Player.Position.Z = e.PositionZ;

            _worldTimeTracker = new WorldTimeTracker();
            _lastPositionUpdate = _worldTimeTracker.NowMS;
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

        public IWoWLocalPlayer Player { get; set; } =
            new WoWLocalPlayer(new HighGuid(new byte[4], new byte[4]));

        public IWoWLocalPet Pet => throw new NotImplementedException();

        private static readonly List<WoWObject> _objects = [];
        public IEnumerable<IWoWObject> Objects => _objects;
        public ILoginScreen LoginScreen => _loginScreen;
        public IRealmSelectScreen RealmSelectScreen => _realmScreen;
        public ICharacterSelectScreen CharacterSelectScreen => _characterSelectScreen;

        public void EnterWorld(ulong characterGuid)
        {
            HasEnteredWorld = true;

            _playerGuid = new HighGuid(characterGuid);
            _woWClient.EnterWorld(characterGuid);

            InitializeMovementController();
        }

        private readonly Queue<ObjectStateUpdate> _pendingUpdates = new();

        public void QueueUpdate(ObjectStateUpdate update)
        {
            _pendingUpdates.Enqueue(update);
        }

        public void ProcessUpdates()
        {
            while (_pendingUpdates.Count > 0)
            {
                try
                {
                    var update = _pendingUpdates.Dequeue();
                    var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                    var elapsedMs = _worldTimeTracker?.NowMS.TotalMilliseconds ?? 0;

                    Console.WriteLine(
                        $"[{timestamp}][{elapsedMs:F1}ms][ProcessUpdates] Op={update.Operation} Type={update.ObjectType} Guid={update.Guid:X}"
                    );

                    switch (update.Operation)
                    {
                        case ObjectUpdateOperation.Add:
                            var newObject = CreateObjectFromFields(
                                update.ObjectType,
                                update.Guid,
                                update.UpdatedFields
                            );
                            _objects.Add(newObject);

                            if (update.MovementData != null && newObject is WoWUnit or WoWPlayer or WoWLocalPlayer)
                            {
                                ApplyMovementData((WoWUnit)newObject, update.MovementData);

                                // Log movement data for analysis
                                Console.WriteLine(
                                    $"[{timestamp}][{elapsedMs:F1}ms][Movement-Add] Guid={update.Guid:X} " +
                                    $"Pos=({update.MovementData.X:F2}, {update.MovementData.Y:F2}, {update.MovementData.Z:F2}) " +
                                    $"Flags=0x{(uint)update.MovementData.MovementFlags:X8} " +
                                    $"Time={update.MovementData.LastUpdated}"
                                );
                            }

                            if (newObject is WoWPlayer player)
                            {
                                _woWClient.SendNameQuery(update.Guid);

                                if (newObject is WoWLocalPlayer)
                                {
                                    Console.WriteLine($"[{timestamp}][{elapsedMs:F1}ms][LocalPlayer-Add] Taking control");
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
                                var oldPos = obj is WoWUnit unit ? new { unit.Position.X, unit.Position.Y, unit.Position.Z } : null;
                                var oldFlags = obj is WoWUnit u ? u.MovementFlags : MovementFlags.MOVEFLAG_NONE;

                                ApplyFieldDiffs(obj, update.UpdatedFields);

                                if (update.MovementData != null && obj is WoWUnit or WoWPlayer or WoWLocalPlayer)
                                {
                                    ApplyMovementData((WoWUnit)obj, update.MovementData);

                                    // Calculate position delta if available
                                    string deltaStr = "";
                                    if (oldPos != null)
                                    {
                                        var dx = update.MovementData.X - oldPos.X;
                                        var dy = update.MovementData.Y - oldPos.Y;
                                        var dz = update.MovementData.Z - oldPos.Z;
                                        var dist = Math.Sqrt(dx * dx + dy * dy + dz * dz);
                                        deltaStr = $"Delta={dist:F3}y ";
                                    }

                                    // Log movement updates with timing info
                                    Console.WriteLine(
                                        $"[{timestamp}][{elapsedMs:F1}ms][Movement-Update] Guid={update.Guid:X} " +
                                        $"Pos=({update.MovementData.X:F2}, {update.MovementData.Y:F2}, {update.MovementData.Z:F2}) " +
                                        $"{deltaStr}" +
                                        $"Flags=0x{(uint)update.MovementData.MovementFlags:X8} " +
                                        $"(was 0x{(uint)oldFlags:X8}) " +
                                        $"Time={update.MovementData.LastUpdated} " +
                                        (obj is WoWLocalPlayer ? "[LOCAL]" : "")
                                    );

                                    if (obj is WoWLocalPlayer)
                                    {
                                        var timeSinceLastUpdate = update.MovementData.LastUpdated - _lastSentTime;
                                        Console.WriteLine(
                                            $"[{timestamp}][{elapsedMs:F1}ms][LocalPlayer-Update] " +
                                            $"TimeSinceLastSent={timeSinceLastUpdate}ms " +
                                            $"(Server teleport check)"
                                        );
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine($"[{timestamp}][{elapsedMs:F1}ms][Warning] Update for unknown object {update.Guid:X}");
                            }
                            break;

                        case ObjectUpdateOperation.Remove:
                            var removed = _objects.RemoveAll(x => x.Guid == update.Guid);
                            Console.WriteLine(
                                $"[{timestamp}][{elapsedMs:F1}ms][Remove] Guid={update.Guid:X} " +
                                $"(removed {removed} object{(removed != 1 ? "s" : "")})"
                            );
                            break;
                    }
                }
                catch (Exception ex)
                {
                    var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                    Console.WriteLine($"[{timestamp}][ProcessUpdates-ERROR] {ex.Message}");
                    Console.WriteLine($"  Stack: {ex.StackTrace}");
                }
            }
        }

        private static void ApplyContainerFieldDiffs(
            WoWContainer container,
            uint key,
            object? value
        )
        {
            var field = (EContainerFields)key;
            switch (field)
            {
                case EContainerFields.CONTAINER_FIELD_NUM_SLOTS:
                    container.NumOfSlots = (int)value;
                    break;
                case EContainerFields.CONTAINER_ALIGN_PAD:
                    break;
                case >= EContainerFields.CONTAINER_FIELD_SLOT_1
                and <= EContainerFields.CONTAINER_FIELD_SLOT_LAST:
                    if (
                        field >= EContainerFields.CONTAINER_FIELD_SLOT_1
                        && field <= EContainerFields.CONTAINER_FIELD_SLOT_LAST
                    )
                    {
                        // Calculate slot index and whether this is the low or high GUID part
                        var slotFieldOffset =
                            (uint)field - (uint)EContainerFields.CONTAINER_FIELD_SLOT_1;
                        var slotIndex = slotFieldOffset / 2; // Each slot GUID takes 2 fields (low + high)
                        var isHighPart = (slotFieldOffset % 2) == 1;

                        if (slotIndex < container.Slots.Length)
                        {
                            if (!isHighPart)
                            {
                                // Store low part GUID value in slot
                                container.Slots[slotIndex] = (uint)value;
                            }
                            // Note: Currently not storing high part since Slots is uint[]
                            // This could be enhanced to support full 64-bit GUIDs if needed
                        }
                    }
                    break;
                case EContainerFields.CONTAINER_END:
                    break;
            }
        }

        private static void ApplyUnitFieldDiffs(WoWUnit unit, uint key, object? value)
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
                case >= EUnitFields.UNIT_VIRTUAL_ITEM_SLOT_DISPLAY
                and <= EUnitFields.UNIT_VIRTUAL_ITEM_SLOT_DISPLAY_02:
                    unit.VirtualItemSlotDisplay[
                        field - EUnitFields.UNIT_VIRTUAL_ITEM_SLOT_DISPLAY
                    ] = (uint)value;
                    break;
                case >= EUnitFields.UNIT_VIRTUAL_ITEM_INFO
                and <= EUnitFields.UNIT_VIRTUAL_ITEM_INFO_05:
                    unit.VirtualItemInfo[field - EUnitFields.UNIT_VIRTUAL_ITEM_INFO] = (uint)value;
                    break;
                case EUnitFields.UNIT_FIELD_FLAGS:
                    unit.UnitFlags = (UnitFlags)value;
                    break;
                case >= EUnitFields.UNIT_FIELD_AURA
                and <= EUnitFields.UNIT_FIELD_AURA_LAST:
                    unit.AuraFields[field - EUnitFields.UNIT_FIELD_AURA] = (uint)value;
                    break;
                case >= EUnitFields.UNIT_FIELD_AURAFLAGS
                and <= EUnitFields.UNIT_FIELD_AURAFLAGS_05:
                    unit.AuraFlags[field - EUnitFields.UNIT_FIELD_AURAFLAGS] = (uint)value;
                    break;
                case >= EUnitFields.UNIT_FIELD_AURALEVELS
                and <= EUnitFields.UNIT_FIELD_AURALEVELS_LAST:
                    unit.AuraLevels[field - EUnitFields.UNIT_FIELD_AURALEVELS] = (uint)value;
                    break;
                case >= EUnitFields.UNIT_FIELD_AURAAPPLICATIONS
                and <= EUnitFields.UNIT_FIELD_AURAAPPLICATIONS_LAST:
                    unit.AuraApplications[field - EUnitFields.UNIT_FIELD_AURAAPPLICATIONS] =
                        (uint)value;
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
                case >= EUnitFields.UNIT_FIELD_RESISTANCES
                and <= EUnitFields.UNIT_FIELD_RESISTANCES_06:
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
                case >= EUnitFields.UNIT_FIELD_POWER_COST_MODIFIER
                and <= EUnitFields.UNIT_FIELD_POWER_COST_MODIFIER_06:
                    unit.PowerCostModifers[field - EUnitFields.UNIT_FIELD_POWER_COST_MODIFIER] =
                        (uint)value;
                    break;
                case >= EUnitFields.UNIT_FIELD_POWER_COST_MULTIPLIER
                and <= EUnitFields.UNIT_FIELD_POWER_COST_MULTIPLIER_06:
                    unit.PowerCostMultipliers[
                        field - EUnitFields.UNIT_FIELD_POWER_COST_MULTIPLIER
                    ] = (uint)value;
                    break;
            }
        }

        private static void ApplyFieldDiffs(WoWObject obj, Dictionary<uint, object?> updatedFields)
        {
            foreach (var (key, value) in updatedFields)
            {
                if (value == null)
                    continue;

                bool fieldHandled = false;

                // Check object-specific fields first, in inheritance order (most specific to least specific)

                // WoWContainer (inherits from WoWItem)
                if (obj is WoWContainer container)
                {
                    if (Enum.IsDefined(typeof(EContainerFields), key))
                    {
                        ApplyContainerFieldDiffs(container, key, value);
                        fieldHandled = true;
                    }
                    else if (Enum.IsDefined(typeof(EItemFields), key))
                    {
                        ApplyItemFieldDiffs(container, key, value);
                        fieldHandled = true;
                    }
                }
                // WoWItem (but not container since container was handled above)
                else if (obj is WoWItem item)
                {
                    if (Enum.IsDefined(typeof(EItemFields), key))
                    {
                        ApplyItemFieldDiffs(item, key, value);
                        fieldHandled = true;
                    }
                }
                // WoWPlayer/WoWLocalPlayer (inherits from WoWUnit)
                else if (obj is WoWPlayer player)
                {
                    if (Enum.IsDefined(typeof(EPlayerFields), key))
                    {
                        ApplyPlayerFieldDiffs(player, key, value, _objects);
                        fieldHandled = true;
                    }
                    else if (Enum.IsDefined(typeof(EUnitFields), key))
                    {
                        ApplyUnitFieldDiffs(player, key, value);
                        fieldHandled = true;
                    }
                }
                // WoWUnit (but not player since player was handled above)
                else if (obj is WoWUnit unit)
                {
                    if (Enum.IsDefined(typeof(EUnitFields), key))
                    {
                        ApplyUnitFieldDiffs(unit, key, value);
                        fieldHandled = true;
                    }
                }
                // WoWGameObject
                else if (obj is WoWGameObject go)
                {
                    if (Enum.IsDefined(typeof(EGameObjectFields), key))
                    {
                        ApplyGameObjectFieldDiffs(go, key, value);
                        fieldHandled = true;
                    }
                }
                // WoWDynamicObject
                else if (obj is WoWDynamicObject dyn)
                {
                    if (Enum.IsDefined(typeof(EDynamicObjectFields), key))
                    {
                        ApplyDynamicObjectFieldDiffs(dyn, key, value);
                        fieldHandled = true;
                    }
                }
                // WoWCorpse
                else if (obj is WoWCorpse corpse)
                {
                    if (Enum.IsDefined(typeof(ECorpseFields), key))
                    {
                        ApplyCorpseFieldDiffs(corpse, key, value);
                        fieldHandled = true;
                    }
                }

                // Fall back to base object fields if no specific field type was handled
                if (!fieldHandled && Enum.IsDefined(typeof(EObjectFields), key))
                {
                    ApplyObjectFieldDiffs(obj, key, value);
                }
            }
        }

        private static void ApplyObjectFieldDiffs(WoWObject obj, uint key, object value)
        {
            var field = (EObjectFields)key;
            switch (field)
            {
                case EObjectFields.OBJECT_FIELD_GUID:
                    obj.HighGuid.LowGuidValue = (byte[])value; // COMMENTED OUT - should not modify object's own GUID
                    break;
                case EObjectFields.OBJECT_FIELD_GUID + 1:
                    obj.HighGuid.HighGuidValue = (byte[])value; // COMMENTED OUT - should not modify object's own GUID
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

        private static void ApplyItemFieldDiffs(WoWItem item, uint key, object value)
        {
            var field = (EItemFields)key;
            switch (field)
            {
                case EItemFields.ITEM_FIELD_OWNER:
                    item.Owner.LowGuidValue = (byte[])value;
                    break;
                case EItemFields.ITEM_FIELD_OWNER + 1:
                    item.Owner.HighGuidValue = (byte[])value;
                    break;
                case EItemFields.ITEM_FIELD_CONTAINED:
                    {
                        var bytes = (byte[])value;
                        item.Contained.LowGuidValue = bytes;
                        break;
                    }
                case EItemFields.ITEM_FIELD_CONTAINED + 1:
                    item.Contained.HighGuidValue = (byte[])value;
                    break;
                case EItemFields.ITEM_FIELD_CREATOR:
                    item.CreatedBy.LowGuidValue = (byte[])value;
                    break;
                case EItemFields.ITEM_FIELD_CREATOR + 1:
                    item.CreatedBy.HighGuidValue = (byte[])value;
                    break;
                case EItemFields.ITEM_FIELD_GIFTCREATOR:
                    item.GiftCreator.LowGuidValue = (byte[])value;
                    break;
                case EItemFields.ITEM_FIELD_GIFTCREATOR + 1:
                    item.GiftCreator.HighGuidValue = (byte[])value;
                    break;
                case EItemFields.ITEM_FIELD_STACK_COUNT:
                    item.StackCount = (uint)value;
                    break;
                case EItemFields.ITEM_FIELD_DURATION:
                    item.Duration = (uint)value;
                    break;
                case EItemFields.ITEM_FIELD_SPELL_CHARGES:
                    item.SpellCharges[0] = (uint)value;
                    break;
                case EItemFields.ITEM_FIELD_SPELL_CHARGES_01:
                    item.SpellCharges[1] = (uint)value;
                    break;
                case EItemFields.ITEM_FIELD_SPELL_CHARGES_02:
                    item.SpellCharges[2] = (uint)value;
                    break;
                case EItemFields.ITEM_FIELD_SPELL_CHARGES_03:
                    item.SpellCharges[3] = (uint)value;
                    break;
                case EItemFields.ITEM_FIELD_SPELL_CHARGES_04:
                    item.SpellCharges[4] = (uint)value;
                    break;
                case EItemFields.ITEM_FIELD_FLAGS:
                    item.ItemDynamicFlags = (ItemDynFlags)value;
                    break;
                case EItemFields.ITEM_FIELD_ENCHANTMENT:
                    item.Enchantments[0] = (uint)value;
                    break;
                case EItemFields.ITEM_FIELD_PROPERTY_SEED:
                    item.PropertySeed = (uint)value;
                    break;
                case EItemFields.ITEM_FIELD_RANDOM_PROPERTIES_ID:
                    item.PropertySeed = (uint)value;
                    break;
                case EItemFields.ITEM_FIELD_ITEM_TEXT_ID:
                    item.ItemTextId = (uint)value;
                    break;
                case EItemFields.ITEM_FIELD_DURABILITY:
                    item.Durability = (uint)value;
                    break;
                case EItemFields.ITEM_FIELD_MAXDURABILITY:
                    item.MaxDurability = (uint)value;
                    break;
                case EItemFields.ITEM_END:
                    break;
            }
        }

        private static void ApplyGameObjectFieldDiffs(WoWGameObject go, uint key, object value)
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
                case >= EGameObjectFields.GAMEOBJECT_ROTATION
                and < EGameObjectFields.GAMEOBJECT_STATE:
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

        private static void ApplyDynamicObjectFieldDiffs(
            WoWDynamicObject dyn,
            uint key,
            object value
        )
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
            }
        }

        private static void ApplyCorpseFieldDiffs(WoWCorpse corpse, uint key, object value)
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
                case >= ECorpseFields.CORPSE_FIELD_ITEM
                and < ECorpseFields.CORPSE_FIELD_BYTES_1:
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

        private static void ApplyPlayerFieldDiffs(
            WoWPlayer player,
            uint key,
            object value,
            List<WoWObject> objects
        )
        {
            var field = (EPlayerFields)key;
            Console.WriteLine(
                $"[ApplyPlayerFieldDiffs] Processing field: {field} (0x{(uint)field:X})"
            );
            switch (field)
            {
                case EPlayerFields.PLAYER_FIELD_THIS_WEEK_CONTRIBUTION:
                    player.ThisWeekContribution = (uint)value;
                    break;
                case EPlayerFields.PLAYER_DUEL_ARBITER:
                    player.DuelArbiter.LowGuidValue = (byte[])value;
                    break;
                case EPlayerFields.PLAYER_DUEL_ARBITER + 1:
                    player.DuelArbiter.HighGuidValue = (byte[])value;
                    break;
                case EPlayerFields.PLAYER_FLAGS:
                    player.PlayerFlags = (PlayerFlags)value;
                    break;
                case EPlayerFields.PLAYER_GUILDID:
                    player.GuildId = (uint)value;
                    break;
                case EPlayerFields.PLAYER_GUILDRANK:
                    player.GuildRank = (uint)value;
                    break;
                case EPlayerFields.PLAYER_BYTES:
                    player.PlayerBytes = (byte[])value;
                    break;
                case EPlayerFields.PLAYER_BYTES_2:
                    player.PlayerBytes2 = (byte[])value;
                    break;
                case EPlayerFields.PLAYER_BYTES_3:
                    player.PlayerBytes3 = (byte[])value;
                    break;
                case EPlayerFields.PLAYER_DUEL_TEAM:
                    player.GuildTimestamp = (uint)value;
                    break;
                case EPlayerFields.PLAYER_GUILD_TIMESTAMP:
                    player.GuildTimestamp = (uint)value;
                    break;
                case >= EPlayerFields.PLAYER_QUEST_LOG_1_1
                and <= EPlayerFields.PLAYER_QUEST_LOG_25_2:
                    {
                        uint questField = (field - EPlayerFields.PLAYER_QUEST_LOG_1_1) % 3;
                        int questIndex = (int)((field - EPlayerFields.PLAYER_QUEST_LOG_1_1) / 3);

                        if (questIndex >= 0 && questIndex < player.QuestLog.Length)
                        {
                            switch (questField)
                            {
                                case 0:
                                    player.QuestLog[questIndex].QuestId = (uint)value;
                                    break;
                                case 1:
                                    player.QuestLog[questIndex].QuestCounters = (byte[])value;
                                    break;
                                case 2:
                                    player.QuestLog[questIndex].QuestState = (uint)value;
                                    break;
                            }
                        }
                        else
                        {
                            Console.WriteLine(
                                $"[ApplyPlayerFieldDiffs] QuestLog index out of bounds: {questIndex}, array length: {player.QuestLog.Length}"
                            );
                        }
                    }
                    break;
                case >= EPlayerFields.PLAYER_VISIBLE_ITEM_1_CREATOR
                and <= EPlayerFields.PLAYER_VISIBLE_ITEM_19_PAD:
                    {
                        uint visibleItemField =
                            (field - EPlayerFields.PLAYER_VISIBLE_ITEM_1_CREATOR) % 12;
                        int itemIndex = (int)(
                            (field - EPlayerFields.PLAYER_VISIBLE_ITEM_1_CREATOR) / 12
                        );
                        var visibleItem = player.VisibleItems[itemIndex];
                        switch (visibleItemField)
                        {
                            case 0:
                                visibleItem.CreatedBy.LowGuidValue = (byte[])value;
                                break;
                            case 1:
                                visibleItem.CreatedBy.HighGuidValue = (byte[])value;
                                break;
                            case 2:
                                ((WoWItem)visibleItem).ItemId = (uint)value;
                                break;
                            case 3:
                                visibleItem.Owner.LowGuidValue = (byte[])value;
                                break;
                            case 4:
                                visibleItem.Owner.HighGuidValue = (byte[])value;
                                break;
                            case 5:
                                visibleItem.Contained.LowGuidValue = (byte[])value;
                                break;
                            case 6:
                                visibleItem.Contained.HighGuidValue = (byte[])value;
                                break;
                            case 7:
                                visibleItem.GiftCreator.LowGuidValue = (byte[])value;
                                break;
                            case 8:
                                visibleItem.GiftCreator.HighGuidValue = (byte[])value;
                                break;
                            case 9:
                                ((WoWItem)visibleItem).StackCount = (uint)value;
                                break;
                            case 10:
                                ((WoWItem)visibleItem).Durability = (uint)value;
                                break;
                            case 11:
                                ((WoWItem)visibleItem).PropertySeed = (uint)value;
                                break;
                        }
                    }
                    break;

                case >= EPlayerFields.PLAYER_FIELD_INV_SLOT_HEAD
                and < EPlayerFields.PLAYER_FIELD_PACK_SLOT_1:
                    {
                        var inventoryIndex = field - EPlayerFields.PLAYER_FIELD_INV_SLOT_HEAD;
                        if (inventoryIndex >= 0 && inventoryIndex < player.Inventory.Length)
                        {
                            player.Inventory[inventoryIndex] = (uint)value;

                            // If this is a 2-byte field pair representing a GUID, populate VisibleItems
                            var itemGuid = (ulong)(uint)value;
                            if (itemGuid != 0 && inventoryIndex < player.VisibleItems.Length)
                            {
                                var actualItem =
                                    objects.FirstOrDefault(o => o.Guid == itemGuid) as WoWItem;
                                if (actualItem != null)
                                {
                                    player.VisibleItems[inventoryIndex] = actualItem;
                                }
                                else
                                {
                                    Console.WriteLine(
                                        $"[ApplyPlayerFieldDiffs] No item found for GUID {itemGuid:X} at inventory index {inventoryIndex}"
                                    );
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine(
                                $"[ApplyPlayerFieldDiffs] inventoryIndex {inventoryIndex} out of bounds for Inventory array (length: {player.Inventory.Length}), field: {field}"
                            );
                        }
                    }
                    break;
                case >= EPlayerFields.PLAYER_FIELD_PACK_SLOT_1
                and <= EPlayerFields.PLAYER_FIELD_PACK_SLOT_LAST:
                    {
                        var packIndex = field - EPlayerFields.PLAYER_FIELD_PACK_SLOT_1;
                        if (packIndex >= 0 && packIndex < player.PackSlots.Length)
                        {
                            player.PackSlots[packIndex] = (uint)value;
                        }
                        else
                        {
                            Console.WriteLine(
                                $"[ApplyPlayerFieldDiffs] packIndex {packIndex} out of bounds for PackSlots array (length: {player.PackSlots.Length}), field: {field}"
                            );
                        }
                    }
                    break;
                case >= EPlayerFields.PLAYER_FIELD_BANK_SLOT_1
                and <= EPlayerFields.PLAYER_FIELD_BANK_SLOT_LAST:
                    {
                        var bankIndex = field - EPlayerFields.PLAYER_FIELD_BANK_SLOT_1;
                        if (bankIndex >= 0 && bankIndex < player.BankSlots.Length)
                        {
                            player.BankSlots[bankIndex] = (uint)value;
                        }
                        else
                        {
                            Console.WriteLine(
                                $"[ApplyPlayerFieldDiffs] bankIndex {bankIndex} out of bounds for BankSlots array (length: {player.BankSlots.Length}), field: {field}"
                            );
                        }
                    }
                    break;
                case >= EPlayerFields.PLAYER_FIELD_BANKBAG_SLOT_1
                and <= EPlayerFields.PLAYER_FIELD_BANKBAG_SLOT_LAST:
                    {
                        var bankBagIndex = field - EPlayerFields.PLAYER_FIELD_BANKBAG_SLOT_1;
                        if (bankBagIndex >= 0 && bankBagIndex < player.BankBagSlots.Length)
                        {
                            player.BankBagSlots[bankBagIndex] = (uint)value;
                        }
                        else
                        {
                            Console.WriteLine(
                                $"[ApplyPlayerFieldDiffs] bankBagIndex {bankBagIndex} out of bounds for BankBagSlots array (length: {player.BankBagSlots.Length}), field: {field}"
                            );
                        }
                    }
                    break;
                case >= EPlayerFields.PLAYER_FIELD_VENDORBUYBACK_SLOT_1
                and <= EPlayerFields.PLAYER_FIELD_VENDORBUYBACK_SLOT_LAST:
                    {
                        var vendorIndex = field - EPlayerFields.PLAYER_FIELD_VENDORBUYBACK_SLOT_1;
                        if (vendorIndex >= 0 && vendorIndex < player.VendorBuybackSlots.Length)
                        {
                            player.VendorBuybackSlots[vendorIndex] = (uint)value;
                        }
                        else
                        {
                            Console.WriteLine(
                                $"[ApplyPlayerFieldDiffs] vendorIndex {vendorIndex} out of bounds for VendorBuybackSlots array (length: {player.VendorBuybackSlots.Length}), field: {field}"
                            );
                        }
                    }
                    break;
                case >= EPlayerFields.PLAYER_FIELD_KEYRING_SLOT_1
                and <= EPlayerFields.PLAYER_FIELD_KEYRING_SLOT_LAST:
                    {
                        var keyringIndex = field - EPlayerFields.PLAYER_FIELD_KEYRING_SLOT_1;
                        if (keyringIndex >= 0 && keyringIndex < player.KeyringSlots.Length)
                        {
                            player.KeyringSlots[keyringIndex] = (uint)value;
                        }
                        else
                        {
                            Console.WriteLine(
                                $"[ApplyPlayerFieldDiffs] keyringIndex {keyringIndex} out of bounds for KeyringSlots array (length: {player.KeyringSlots.Length}), field: {field}"
                            );
                        }
                    }
                    break;
                case EPlayerFields.PLAYER_FARSIGHT:
                    player.Farsight = (uint)value;
                    break;
                case EPlayerFields.PLAYER_FIELD_COMBO_TARGET:
                    player.ComboTarget.LowGuidValue = (byte[])value;
                    break;
                case EPlayerFields.PLAYER_FIELD_COMBO_TARGET + 1:
                    player.ComboTarget.HighGuidValue = (byte[])value;
                    break;
                case EPlayerFields.PLAYER_XP:
                    player.XP = (uint)value;
                    break;
                case EPlayerFields.PLAYER_NEXT_LEVEL_XP:
                    player.NextLevelXP = (uint)value;
                    break;
                case >= EPlayerFields.PLAYER_SKILL_INFO_1_1
                and <= EPlayerFields.PLAYER_SKILL_INFO_1_1 + 383:
                    {
                        uint skillField = (field - EPlayerFields.PLAYER_SKILL_INFO_1_1) % 4;
                        int skillIndex = (int)((field - EPlayerFields.PLAYER_SKILL_INFO_1_1) / 4);
                        switch (skillField)
                        {
                            case 0:
                                player.SkillInfo[skillIndex].SkillInt1 = (uint)value;
                                break;
                            case 1:
                                player.SkillInfo[skillIndex].SkillInt2 = (uint)value;
                                break;
                            case 2:
                                player.SkillInfo[skillIndex].SkillInt3 = (uint)value;
                                break;
                            case 3:
                                player.SkillInfo[skillIndex].SkillInt4 = (uint)value;
                                break;
                        }
                    }
                    break;
                case EPlayerFields.PLAYER_CHARACTER_POINTS1:
                    player.CharacterPoints1 = (uint)value;
                    break;
                case EPlayerFields.PLAYER_CHARACTER_POINTS2:
                    player.CharacterPoints2 = (uint)value;
                    break;
                case EPlayerFields.PLAYER_TRACK_CREATURES:
                    player.TrackCreatures = (uint)value;
                    break;
                case EPlayerFields.PLAYER_TRACK_RESOURCES:
                    player.TrackResources = (uint)value;
                    break;
                case EPlayerFields.PLAYER_BLOCK_PERCENTAGE:
                    player.BlockPercentage = (uint)value;
                    break;
                case EPlayerFields.PLAYER_DODGE_PERCENTAGE:
                    player.DodgePercentage = (uint)value;
                    break;
                case EPlayerFields.PLAYER_PARRY_PERCENTAGE:
                    player.ParryPercentage = (uint)value;
                    break;
                case EPlayerFields.PLAYER_CRIT_PERCENTAGE:
                    player.CritPercentage = (uint)value;
                    break;
                case EPlayerFields.PLAYER_RANGED_CRIT_PERCENTAGE:
                    player.RangedCritPercentage = (uint)value;
                    break;
                case >= EPlayerFields.PLAYER_EXPLORED_ZONES_1
                and < EPlayerFields.PLAYER_REST_STATE_EXPERIENCE:
                    player.ExploredZones[field - EPlayerFields.PLAYER_EXPLORED_ZONES_1] =
                        (uint)value;
                    break;
                case EPlayerFields.PLAYER_REST_STATE_EXPERIENCE:
                    player.RestStateExperience = (uint)value;
                    break;
                case EPlayerFields.PLAYER_FIELD_COINAGE:
                    player.Coinage = (uint)value;
                    break;
                case >= EPlayerFields.PLAYER_FIELD_POSSTAT0
                and <= EPlayerFields.PLAYER_FIELD_POSSTAT4:
                    player.StatBonusesPos[field - EPlayerFields.PLAYER_FIELD_POSSTAT0] =
                        (uint)value;
                    break;
                case >= EPlayerFields.PLAYER_FIELD_NEGSTAT0
                and <= EPlayerFields.PLAYER_FIELD_NEGSTAT4:
                    player.StatBonusesNeg[field - EPlayerFields.PLAYER_FIELD_NEGSTAT0] =
                        (uint)value;
                    break;
                case >= EPlayerFields.PLAYER_FIELD_RESISTANCEBUFFMODSPOSITIVE
                and <= EPlayerFields.PLAYER_FIELD_RESISTANCEBUFFMODSNEGATIVE + 6:
                    if (field <= EPlayerFields.PLAYER_FIELD_RESISTANCEBUFFMODSNEGATIVE)
                        player.ResistBonusesPos[
                            field - EPlayerFields.PLAYER_FIELD_RESISTANCEBUFFMODSPOSITIVE
                        ] = (uint)value;
                    else
                        player.ResistBonusesNeg[
                            field - EPlayerFields.PLAYER_FIELD_RESISTANCEBUFFMODSNEGATIVE
                        ] = (uint)value;
                    break;
                case >= EPlayerFields.PLAYER_FIELD_MOD_DAMAGE_DONE_POS
                and <= EPlayerFields.PLAYER_FIELD_MOD_DAMAGE_DONE_POS + 6:
                    player.ModDamageDonePos[
                        field - EPlayerFields.PLAYER_FIELD_MOD_DAMAGE_DONE_POS
                    ] = (uint)value;
                    break;
                case >= EPlayerFields.PLAYER_FIELD_MOD_DAMAGE_DONE_NEG
                and <= EPlayerFields.PLAYER_FIELD_MOD_DAMAGE_DONE_NEG + 6:
                    player.ModDamageDoneNeg[
                        field - EPlayerFields.PLAYER_FIELD_MOD_DAMAGE_DONE_NEG
                    ] = (uint)value;
                    break;
                case >= EPlayerFields.PLAYER_FIELD_MOD_DAMAGE_DONE_PCT
                and <= EPlayerFields.PLAYER_FIELD_MOD_DAMAGE_DONE_PCT + 6:
                    player.ModDamageDonePct[
                        field - EPlayerFields.PLAYER_FIELD_MOD_DAMAGE_DONE_PCT
                    ] = (float)value;
                    break;
                case EPlayerFields.PLAYER_AMMO_ID:
                    player.AmmoId = (uint)value;
                    break;
                case EPlayerFields.PLAYER_SELF_RES_SPELL:
                    player.SelfResSpell = (uint)value;
                    break;
                case EPlayerFields.PLAYER_FIELD_PVP_MEDALS:
                    player.PvpMedals = (uint)value;
                    break;
                case >= EPlayerFields.PLAYER_FIELD_BUYBACK_PRICE_1
                and <= EPlayerFields.PLAYER_FIELD_BUYBACK_PRICE_LAST:
                    player.BuybackPrices[field - EPlayerFields.PLAYER_FIELD_BUYBACK_PRICE_1] =
                        (uint)value;
                    break;
                case >= EPlayerFields.PLAYER_FIELD_BUYBACK_TIMESTAMP_1
                and <= EPlayerFields.PLAYER_FIELD_BUYBACK_TIMESTAMP_LAST:
                    player.BuybackTimestamps[
                        field - EPlayerFields.PLAYER_FIELD_BUYBACK_TIMESTAMP_1
                    ] = (uint)value;
                    break;
                case EPlayerFields.PLAYER_FIELD_YESTERDAY_KILLS:
                    player.YesterdayKills = (uint)value;
                    break;
                case EPlayerFields.PLAYER_FIELD_LAST_WEEK_KILLS:
                    player.LastWeekKills = (uint)value;
                    break;
                case EPlayerFields.PLAYER_FIELD_LIFETIME_HONORABLE_KILLS:
                    player.LifetimeHonorableKills = (uint)value;
                    break;
                case EPlayerFields.PLAYER_FIELD_WATCHED_FACTION_INDEX:
                    player.WatchedFactionIndex = (uint)value;
                    break;
                case >= EPlayerFields.PLAYER_FIELD_COMBAT_RATING_1
                and <= EPlayerFields.PLAYER_FIELD_COMBAT_RATING_1 + 20:
                    player.CombatRating[field - EPlayerFields.PLAYER_FIELD_COMBAT_RATING_1] =
                        (uint)value;
                    break;
                case EPlayerFields.PLAYER_CHOSEN_TITLE:
                    // Note: ChosenTitle property not implemented in WoWPlayer yet
                    break;
                case EPlayerFields.PLAYER__FIELD_KNOWN_TITLES:
                    // Note: KnownTitles property not implemented in WoWPlayer yet
                    break;
                case EPlayerFields.PLAYER__FIELD_KNOWN_TITLES + 1:
                    // Note: KnownTitles property not implemented in WoWPlayer yet
                    break;
                case EPlayerFields.PLAYER_FIELD_MOD_HEALING_DONE_POS:
                    // Note: ModHealingDonePos property not implemented in WoWPlayer yet
                    break;
                case EPlayerFields.PLAYER_FIELD_MOD_TARGET_RESISTANCE:
                    // Note: ModTargetResistance property not implemented in WoWPlayer yet
                    break;
                case EPlayerFields.PLAYER_FIELD_BYTES:
                    // Note: FieldBytes property not implemented in WoWPlayer yet
                    break;
                case EPlayerFields.PLAYER_OFFHAND_CRIT_PERCENTAGE:
                    // Note: OffhandCritPercentage property not implemented in WoWPlayer yet
                    break;
                case >= EPlayerFields.PLAYER_SPELL_CRIT_PERCENTAGE1
                and <= EPlayerFields.PLAYER_SPELL_CRIT_PERCENTAGE1 + 6:
                    // Note: SpellCritPercentage array not implemented in WoWPlayer yet
                    break;
                case >= EPlayerFields.PLAYER_FIELD_ARENA_TEAM_INFO_1_1
                and <= EPlayerFields.PLAYER_FIELD_ARENA_TEAM_INFO_1_1 + 17:
                    // Note: ArenaTeamInfo array not implemented in WoWPlayer yet
                    break;
                case EPlayerFields.PLAYER_FIELD_HONOR_CURRENCY:
                    // Note: HonorCurrency property not implemented in WoWPlayer yet
                    break;
                case EPlayerFields.PLAYER_FIELD_ARENA_CURRENCY:
                    // Note: ArenaCurrency property not implemented in WoWPlayer yet
                    break;
                case EPlayerFields.PLAYER_FIELD_MOD_MANA_REGEN:
                    // Note: ModManaRegen property not implemented in WoWPlayer yet
                    break;
                case EPlayerFields.PLAYER_FIELD_MOD_MANA_REGEN_INTERRUPT:
                    // Note: ModManaRegenInterrupt property not implemented in WoWPlayer yet
                    break;
                case EPlayerFields.PLAYER_FIELD_MAX_LEVEL:
                    // Note: MaxLevel property not implemented in WoWPlayer yet
                    break;
                case >= EPlayerFields.PLAYER_FIELD_DAILY_QUESTS_1
                and <= EPlayerFields.PLAYER_FIELD_DAILY_QUESTS_1 + 9:
                    // Note: DailyQuests array not implemented in WoWPlayer yet
                    break;
                case EPlayerFields.PLAYER_FIELD_PADDING:
                    // Padding field, usually ignored
                    break;
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
                unit.SplineFinalPoint =
                    data.MovementBlockUpdate.SplineFinalPoint ?? unit.SplineFinalPoint;
                unit.SplineTargetGuid = data.MovementBlockUpdate.SplineTargetGuid ?? 0;
                unit.SplineFinalOrientation = data.MovementBlockUpdate.SplineFinalOrientation ?? 0f;
                unit.SplineTimePassed = data.MovementBlockUpdate.SplineTimePassed ?? 0;
                unit.SplineDuration = data.MovementBlockUpdate.SplineDuration ?? 0;
                unit.SplineId = data.MovementBlockUpdate.SplineId ?? 0;
                unit.SplineNodes = data.MovementBlockUpdate.SplineNodes ?? [];
                unit.SplineFinalDestination =
                    data.MovementBlockUpdate.SplineFinalDestination ?? unit.SplineFinalDestination;
                unit.SplineType = data.MovementBlockUpdate.SplineType;
                unit.SplineTargetGuid = data.MovementBlockUpdate.FacingTargetGuid;
                unit.FacingAngle = data.MovementBlockUpdate.FacingAngle;
                unit.FacingSpot = data.MovementBlockUpdate.FacingSpot;
                unit.SplineTimestamp = data.MovementBlockUpdate.SplineTimestamp;
                unit.SplinePoints = data.MovementBlockUpdate.SplinePoints;
            }
        }

        #region NotImplemented

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

        public void SplitStack(
            int bag,
            int slot,
            int quantity,
            int destinationBag,
            int destinationSlot
        )
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

        public void AntiAfk() { }

        public IWoWUnit GetTarget(IWoWUnit woWUnit)
        {
            return null;
        }

        public sbyte GetTalentRank(uint tabIndex, uint talentIndex)
        {
            return 0;
        }

        public void PickupInventoryItem(uint inventorySlot) { }

        public void DeleteCursorItem() { }

        public void EquipCursorItem() { }

        public void ConfirmItemEquip() { }

        public void SendChatMessage(string chatMessage) { }

        public void SetRaidTarget(IWoWUnit target, TargetMarker v) { }

        public void JoinBattleGroundQueue() { }

        public void ResetInstances() { }

        public void PickupMacro(uint v) { }

        public void PlaceAction(uint v) { }

        public void InviteToGroup(ulong guid) { }

        public void KickPlayer(ulong guid) { }

        public void AcceptGroupInvite() { }

        public void DeclineGroupInvite() { }

        public void LeaveGroup() { }

        public void DisbandGroup() { }

        public void ConvertToRaid() { }

        public bool HasPendingGroupInvite()
        {
            return false;
        }

        public bool HasLootRollWindow(int itemId)
        {
            return false;
        }

        public void LootPass(int itemId) { }

        public void LootRollGreed(int itemId) { }

        public void LootRollNeed(int itemId) { }

        public void AssignLoot(int itemId, ulong playerGuid) { }

        public void SetGroupLoot(GroupLootSetting setting) { }

        public void PromoteLootManager(ulong playerGuid) { }

        public void PromoteAssistant(ulong playerGuid) { }

        public void PromoteLeader(ulong playerGuid) { }

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

        public enum ObjectUpdateOperation
        {
            Add,
            Update,
            Remove,
        }

        public record ObjectStateUpdate(
            ulong Guid,
            ObjectUpdateOperation Operation,
            WoWObjectType ObjectType,
            MovementInfoUpdate? MovementData,
            Dictionary<uint, object?> UpdatedFields
        );
    }
}
