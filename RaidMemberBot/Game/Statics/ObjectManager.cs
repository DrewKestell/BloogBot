using Newtonsoft.Json;
using RaidMemberBot.Constants;
using RaidMemberBot.Mem;
using RaidMemberBot.Models.Dto;
using RaidMemberBot.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using static RaidMemberBot.Constants.Enums;

namespace RaidMemberBot.Game.Statics
{
    public class ObjectManager
    {
        const int OBJECT_TYPE_OFFSET = 0x14;

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        delegate int EnumerateVisibleObjectsCallbackVanilla(int filter, ulong guid);

        static ulong playerGuid;
        static volatile bool _ingame1 = true;
        static volatile bool _ingame2 = true;
        static EnumerateVisibleObjectsCallbackVanilla CallbackDelegate;
        static IntPtr callbackPtr;
        static CharacterState _characterState;

        static internal IList<WoWObject> Objects = new List<WoWObject>();
        static internal IList<WoWObject> ObjectsBuffer = new List<WoWObject>();

        static internal void Initialize(CharacterState parProbe)
        {
            _characterState = parProbe;

            CallbackDelegate = CallbackVanilla;
            callbackPtr = Marshal.GetFunctionPointerForDelegate(CallbackDelegate);

            WoWEventHandler.Instance.OnEvent += OnEvent;

            StartEnumeration();
        }

        static public LocalPlayer Player { get; private set; }

        static public LocalPet Pet { get; private set; }

        static public IEnumerable<WoWObject> AllObjects => Objects;

        static public IEnumerable<WoWUnit> Units => Objects.OfType<WoWUnit>().Where(o => o.ObjectType == WoWObjectTypes.OT_UNIT).ToList();

        static public IEnumerable<WoWPlayer> Players => Objects.OfType<WoWPlayer>();

        static public IEnumerable<WoWItem> Items => Objects.OfType<WoWItem>();

        static public IEnumerable<WoWContainer> Containers => Objects.OfType<WoWContainer>();

        static public IEnumerable<WoWGameObject> GameObjects => Objects.OfType<WoWGameObject>();

        static public WoWUnit CurrentTarget => Units.FirstOrDefault(u => Player.TargetGuid == u.Guid);

        static public bool IsLoggedIn => _ingame1 && _ingame2 && MemoryManager.ReadByte((IntPtr)0xB4B424) == 1;

        static public void AntiAfk() => MemoryManager.WriteInt((IntPtr)MemoryAddresses.LastHardwareAction, Environment.TickCount);

        static public string ZoneText
        {
            // this is weird and throws an exception right after entering world,
            // so we catch and ignore the exception to avoid console noise
            get
            {
                try
                {
                    var ptr = MemoryManager.ReadIntPtr((IntPtr)MemoryAddresses.ZoneTextPtr);
                    return MemoryManager.ReadString(ptr);
                }
                catch (Exception)
                {
                    return "";
                }
            }
        }

        static public string MinimapZoneText
        {
            // this is weird and throws an exception right after entering world,
            // so we catch and ignore the exception to avoid console noise
            get
            {
                try
                {
                    var ptr = MemoryManager.ReadIntPtr((IntPtr)MemoryAddresses.MinimapZoneTextPtr);
                    return MemoryManager.ReadString(ptr);
                }
                catch (Exception)
                {
                    return "";
                }
            }
        }

        static public uint MapId
        {
            // this is weird and throws an exception right after entering world,
            // so we catch and ignore the exception to avoid console noise
            get
            {
                try
                {
                    var objectManagerPtr = MemoryManager.ReadIntPtr((IntPtr)0x00B41414);
                    return MemoryManager.ReadUint(IntPtr.Add(objectManagerPtr, 0xCC));
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }

        static public string ServerName
        {
            // this is weird and throws an exception right after entering world,
            // so we catch and ignore the exception to avoid console noise
            get
            {
                try
                {
                    // not exactly sure how this works. seems to return a string like "Endless\WoW.exe" or "Karazhan\WoW.exe"
                    var fullName = MemoryManager.ReadString((IntPtr)MemoryAddresses.ServerName);
                    return fullName.Split('\\').First();
                }
                catch (Exception)
                {
                    return "";
                }
            }
        }

        static public IEnumerable<WoWPlayer> PartyMembers
        {
            get
            {
                var partyMembers = new List<WoWPlayer>();

                var leader = Players.FirstOrDefault(p => p.Guid == PartyLeaderGuid);
                if (leader != null)
                    partyMembers.Add(leader);

                var partyMember1 = Players.FirstOrDefault(p => p.Guid == Party1Guid);
                if (partyMember1 != null)
                    partyMembers.Add(partyMember1);

                var partyMember2 = Players.FirstOrDefault(p => p.Guid == Party2Guid);
                if (partyMember2 != null)
                    partyMembers.Add(partyMember2);

                var partyMember3 = Players.FirstOrDefault(p => p.Guid == Party3Guid);
                if (partyMember3 != null)
                    partyMembers.Add(partyMember3);

                var partyMember4 = Players.FirstOrDefault(p => p.Guid == Party4Guid);
                if (partyMember4 != null)
                    partyMembers.Add(partyMember4);

                return partyMembers;
            }
        }

        static public WoWPlayer PartyLeader => Players.FirstOrDefault(p => p.Guid == PartyLeaderGuid);

        static public ulong PartyLeaderGuid => MemoryManager.ReadUlong((IntPtr)MemoryAddresses.PartyLeaderGuid);
        static public ulong Party1Guid => MemoryManager.ReadUlong((IntPtr)MemoryAddresses.Party1Guid);
        static public ulong Party2Guid => MemoryManager.ReadUlong((IntPtr)MemoryAddresses.Party2Guid);
        static public ulong Party3Guid => MemoryManager.ReadUlong((IntPtr)MemoryAddresses.Party3Guid);
        static public ulong Party4Guid => MemoryManager.ReadUlong((IntPtr)MemoryAddresses.Party4Guid);

        static public List<WoWUnit> Aggressors =>
            Hostiles
                .Where(u => u.IsInCombat)
                .Where(u =>
                    u.TargetGuid == Player?.Guid ||
                    u.TargetGuid == Pet?.Guid ||
                    PartyMembers.Any(x => x.Guid == u.TargetGuid))
            .ToList();

        static public IEnumerable<WoWUnit> Hostiles =>
            Units
                .Where(u => u.Health > 0)
                .Where(u =>
                    u.UnitReaction == UnitReaction.Hated ||
                    u.UnitReaction == UnitReaction.Hostile ||
                    u.UnitReaction == UnitReaction.Unfriendly ||
                    u.UnitReaction == UnitReaction.Neutral);

        // https://vanilla-wow.fandom.com/wiki/API_GetTalentInfo
        // tab index is 1, 2 or 3
        // talentIndex is counter left to right, top to bottom, starting at 1
        static public sbyte GetTalentRank(int tabIndex, int talentIndex)
        {
            var results = Player.LuaCallWithResults($"{{0}}, {{1}}, {{2}}, {{3}}, {{4}} = GetTalentInfo({tabIndex},{talentIndex})");

            if (results.Length == 5)
                return Convert.ToSByte(results[4]);

            return -1;
        }
        private static void OnEvent(object sender, WoWEventHandler.OnEventArgs args)
        {
            if (args.EventName == "CURSOR_UPDATE")
            {
                var online = MemoryManager.ReadByte(Offsets.Player.IsIngame) == 1;
                if (!online) _ingame1 = false;
                return;
            }
            if (args.EventName != "UNIT_MODEL_CHANGED" &&
                args.EventName != "UPDATE_SELECTED_CHARACTER" &&
                args.EventName != "DISCONNECTED_FROM_SERVER" &&
                args.EventName != "VARIABLES_LOADED") return;
            _ingame1 = true;
        }

        static internal async void StartEnumeration()
        {
            while (true)
            {
                try
                {
                    EnumerateVisibleObjects();
                    await Task.Delay(50);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[OBJECT MANAGER] {e.StackTrace}");
                }
            }
        }

        static void EnumerateVisibleObjects()
        {
            if (!IsLoggedIn) return;
            ThreadSynchronizer.RunOnMainThread(() =>
            {
                playerGuid = Functions.GetPlayerGuid();
                if (playerGuid == 0)
                {
                    Player = null;
                    return;
                }
                var playerObject = Functions.GetObjectPtr(playerGuid);
                if (playerObject == IntPtr.Zero)
                {
                    Player = null;
                    return;
                }

                ObjectsBuffer.Clear();
                Functions.EnumerateVisibleObjects(callbackPtr, 0);
                Objects = new List<WoWObject>(ObjectsBuffer);

                if (Player != null)
                {
                    var petFound = false;

                    foreach (var unit in Units)
                    {
                        if (unit.SummonedByGuid == Player?.Guid)
                        {
                            Pet = new LocalPet(unit.Pointer, unit.Guid, unit.ObjectType);
                            petFound = true;
                        }

                        if (!petFound)
                            Pet = null;
                    }

                    Player.RefreshSpells();
                    Player.RefreshSkills();
                }

                UpdateProbe();
            });
        }

        // EnumerateVisibleObjects callback has the parameter order swapped between Vanilla and other client versions.
        static int CallbackVanilla(int filter, ulong guid)
        {
            return CallbackInternal(guid, filter);
        }

        static int CallbackInternal(ulong guid, int filter)
        {
            if (guid == 0) return 0;
            var pointer = Functions.GetObjectPtr(guid);
            var objectType = (WoWObjectTypes)MemoryManager.ReadInt(IntPtr.Add(pointer, OBJECT_TYPE_OFFSET));

            try
            {
                switch (objectType)
                {
                    case WoWObjectTypes.OT_CONTAINER:
                        ObjectsBuffer.Add(new WoWContainer(pointer, guid, objectType));
                        break;
                    case WoWObjectTypes.OT_ITEM:
                        ObjectsBuffer.Add(new WoWItem(pointer, guid, objectType));
                        break;
                    case WoWObjectTypes.OT_PLAYER:
                        if (guid == playerGuid)
                        {
                            var player = new LocalPlayer(pointer, guid, objectType);
                            Player = player;
                            ObjectsBuffer.Add(player);
                        }
                        else
                            ObjectsBuffer.Add(new WoWPlayer(pointer, guid, objectType));
                        break;
                    case WoWObjectTypes.OT_GAMEOBJ:
                        ObjectsBuffer.Add(new WoWGameObject(pointer, guid, objectType));
                        break;
                    case WoWObjectTypes.OT_UNIT:
                        ObjectsBuffer.Add(new WoWUnit(pointer, guid, objectType));
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"OBJECT MANAGER: CallbackInternal => {e.StackTrace}");
            }

            return 1;
        }

        static void UpdateProbe()
        {
            try
            {
                if (IsLoggedIn)
                {
                    _characterState.Guid = playerGuid;
                    _characterState.CharacterName = Player.Name;
                    _characterState.Casting = Player.SpellcastId;
                    _characterState.ChannelingId = Player.ChannelingId;
                    _characterState.InCombat = Player.IsInCombat;
                    _characterState.IsMoving = Player.IsMoving;
                    _characterState.IsOnMount = Player.IsMounted;
                    _characterState.IsFalling = Player.IsFalling;
                    _characterState.IsStunned = Player.IsStunned;
                    _characterState.IsConfused = Player.IsConfused;
                    _characterState.IsPoisoned = Player.IsPoisoned;
                    _characterState.IsDiseased = Player.IsDiseased;
                    _characterState.Zone = MinimapZoneText;
                    _characterState.InParty = !string.IsNullOrEmpty(Functions.LuaCallWithResult($"{{0}} = UnitName('party1')")[0]);
                    _characterState.InRaid = int.Parse(Functions.LuaCallWithResult("{0} = GetNumRaidMembers()")[0]) > 0;
                    _characterState.RaidLeaderGuid = PartyLeaderGuid;
                    _characterState.MapId = (int)MapId;
                    _characterState.Class = Player.Class;
                    _characterState.Race = Enum.GetValues(typeof(Race)).Cast<Race>().Where(x => x.GetDescription() == Player.Race).First();
                    _characterState.Level = Player.Level;
                    _characterState.CurrentHealth = Player.Health;
                    _characterState.CurrentMana = Player.Mana;
                    _characterState.MaxHealth = Player.MaxHealth;
                    _characterState.MaxMana = Player.MaxMana;
                    _characterState.Rage = Player.Rage;
                    _characterState.Energy = Player.Energy;
                    _characterState.ComboPoints = Player.ComboPoints;
                    _characterState.Facing = Player.Facing;
                    _characterState.Position = new System.Numerics.Vector3(Player.Position.X, Player.Position.Y, Player.Position.Z);
                    _characterState.SpellList = Player.PlayerSpells.Values.SelectMany(x => x).OrderBy(x => x).ToList();
                    _characterState.SkillList = Player.PlayerSkills.OrderBy(x => x).ToList();

                    List<WoWUnit> units = Units.OrderBy(x => x.Position.DistanceTo(Player.Position))
                                            .ToList();
                    units.Insert(0, Player);

                    _characterState.WoWUnits = units.Where(x => !string.IsNullOrEmpty(x.Name))
                                                    .ToDictionary(x => x.Guid, x => x.Name);
                    _characterState.WoWObjects = GameObjects.OrderBy(x => x.Position.DistanceTo(Player.Position))
                                                            .Where(x => !string.IsNullOrEmpty(x.Name))
                                                            .ToDictionary(x => x.Guid, x => x.Name);


                    WoWItem headItem = Inventory.GetEquippedItem(EquipSlot.Head);
                    WoWItem neckItem = Inventory.GetEquippedItem(EquipSlot.Neck);
                    WoWItem shoulderItem = Inventory.GetEquippedItem(EquipSlot.Shoulders);
                    WoWItem backItem = Inventory.GetEquippedItem(EquipSlot.Back);
                    WoWItem chestItem = Inventory.GetEquippedItem(EquipSlot.Chest);
                    WoWItem shirtItem = Inventory.GetEquippedItem(EquipSlot.Shirt);
                    WoWItem tabardItem = Inventory.GetEquippedItem(EquipSlot.Tabard);
                    WoWItem wristItem = Inventory.GetEquippedItem(EquipSlot.Wrist);
                    WoWItem handsItem = Inventory.GetEquippedItem(EquipSlot.Hands);
                    WoWItem waistItem = Inventory.GetEquippedItem(EquipSlot.Waist);
                    WoWItem legsItem = Inventory.GetEquippedItem(EquipSlot.Legs);
                    WoWItem feetItem = Inventory.GetEquippedItem(EquipSlot.Feet);
                    WoWItem finger1Item = Inventory.GetEquippedItem(EquipSlot.Finger1);
                    WoWItem finger2Item = Inventory.GetEquippedItem(EquipSlot.Finger2);
                    WoWItem trinket1Item = Inventory.GetEquippedItem(EquipSlot.Trinket1);
                    WoWItem trinket2Item = Inventory.GetEquippedItem(EquipSlot.Trinket2);
                    WoWItem mainHandItem = Inventory.GetEquippedItem(EquipSlot.MainHand);
                    WoWItem offHandItem = Inventory.GetEquippedItem(EquipSlot.OffHand);
                    WoWItem rangedItem = Inventory.GetEquippedItem(EquipSlot.Ranged);
                    if (headItem != null)
                    {
                        _characterState.HeadItem = headItem.ItemId;
                    }
                    else
                    {
                        _characterState.HeadItem = 0;
                    }
                    if (neckItem != null)
                    {
                        _characterState.NeckItem = neckItem.ItemId;
                    }
                    else
                    {
                        _characterState.NeckItem = 0;
                    }
                    if (shoulderItem != null)
                    {
                        _characterState.ShoulderItem = shoulderItem.ItemId;
                    }
                    else
                    {
                        _characterState.ShoulderItem = 0;
                    }
                    if (backItem != null)
                    {
                        _characterState.BackItem = backItem.ItemId;
                    }
                    else
                    {
                        _characterState.BackItem = 0;
                    }
                    if (chestItem != null)
                    {
                        _characterState.ChestItem = chestItem.ItemId;
                    }
                    else
                    {
                        _characterState.ChestItem = 0;
                    }
                    if (shirtItem != null)
                    {
                        _characterState.ShirtItem = shirtItem.ItemId;
                    }
                    else
                    {
                        _characterState.ShirtItem = 0;
                    }
                    if (tabardItem != null)
                    {
                        _characterState.Tabardtem = tabardItem.ItemId;
                    }
                    else
                    {
                        _characterState.Tabardtem = 0;
                    }
                    if (wristItem != null)
                    {
                        _characterState.WristsItem = wristItem.ItemId;
                    }
                    else
                    {
                        _characterState.WristsItem = 0;
                    }
                    if (handsItem != null)
                    {
                        _characterState.HandsItem = handsItem.ItemId;
                    }
                    else
                    {
                        _characterState.HandsItem = 0;
                    }
                    if (waistItem != null)
                    {
                        _characterState.WaistItem = waistItem.ItemId;
                    }
                    else
                    {
                        _characterState.WaistItem = 0;
                    }
                    if (legsItem != null)
                    {
                        _characterState.LegsItem = legsItem.ItemId;
                    }
                    else
                    {
                        _characterState.LegsItem = 0;
                    }
                    if (feetItem != null)
                    {
                        _characterState.FeetItem = feetItem.ItemId;
                    }
                    else
                    {
                        _characterState.FeetItem = 0;
                    }
                    if (finger1Item != null)
                    {
                        _characterState.Finger1Item = finger1Item.ItemId;
                    }
                    else
                    {
                        _characterState.Finger1Item = 0;
                    }
                    if (finger2Item != null)
                    {
                        _characterState.Finger2Item = finger2Item.ItemId;
                    }
                    else
                    {
                        _characterState.Finger2Item = 0;
                    }
                    if (trinket1Item != null)
                    {
                        _characterState.Trinket1Item = trinket1Item.ItemId;
                    }
                    else
                    {
                        _characterState.Trinket1Item = 0;
                    }
                    if (trinket2Item != null)
                    {
                        _characterState.Trinket2Item = trinket2Item.ItemId;
                    }
                    else
                    {
                        _characterState.Trinket2Item = 0;
                    }
                    if (mainHandItem != null)
                    {
                        _characterState.MainHandItem = mainHandItem.ItemId;
                    }
                    else
                    {
                        _characterState.MainHandItem = 0;
                    }
                    if (offHandItem != null)
                    {
                        _characterState.OffHandItem = offHandItem.ItemId;
                    }
                    else
                    {
                        _characterState.OffHandItem = 0;
                    }
                    if (rangedItem != null)
                    {
                        _characterState.RangedItem = rangedItem.ItemId;
                    }
                    else
                    {
                        _characterState.RangedItem = 0;
                    }
                }
                else
                {
                    _characterState.Guid = 0;
                    _characterState.MapId = 0;
                    _characterState.Position = new System.Numerics.Vector3();
                    _characterState.Waypoint = new System.Numerics.Vector3();
                    _characterState.Level = 0;
                    _characterState.Class = 0;
                    _characterState.Race = 0;
                    _characterState.CharacterName = string.Empty;
                    _characterState.Zone = (LoginStates)Enum.Parse(typeof(LoginStates), MemoryManager.ReadString(Offsets.CharacterScreen.LoginState)) == LoginStates.charselect ? "Character Select Screen" : "Login Screen";
                    _characterState.HostileTargetGuid = 0;
                    _characterState.FriendlyTargetGuid = 0;
                    _characterState.CurrentHealth = 0;
                    _characterState.CurrentMana = 0;
                    _characterState.MaxHealth = 0;
                    _characterState.MaxMana = 0;
                    _characterState.Rage = 0;
                    _characterState.ComboPoints = 0;
                    _characterState.Energy = 0;
                    _characterState.Casting = 0;
                    _characterState.ChannelingId = 0;
                    _characterState.InParty = false;
                    _characterState.InCombat = false;
                    _characterState.IsMoving = false;
                    _characterState.IsOnMount = false;
                    _characterState.IsFalling = false;
                    _characterState.IsStunned = false;
                    _characterState.IsConfused = false;
                    _characterState.IsPoisoned = false;
                    _characterState.IsDiseased = false;
                    _characterState.Facing = 0;
                    _characterState.HeadItem = 0;
                    _characterState.NeckItem = 0;
                    _characterState.ShoulderItem = 0;
                    _characterState.ChestItem = 0;
                    _characterState.BackItem = 0;
                    _characterState.ShirtItem = 0;
                    _characterState.Tabardtem = 0;
                    _characterState.WristsItem = 0;
                    _characterState.HandsItem = 0;
                    _characterState.WaistItem = 0;
                    _characterState.LegsItem = 0;
                    _characterState.FeetItem = 0;
                    _characterState.Finger1Item = 0;
                    _characterState.Finger2Item = 0;
                    _characterState.Trinket1Item = 0;
                    _characterState.Trinket2Item = 0;
                    _characterState.MainHandItem = 0;
                    _characterState.OffHandItem = 0;
                    _characterState.RangedItem = 0;
                    _characterState.SpellList = new List<int>();
                    _characterState.SkillList = new List<int>();
                    _characterState.WoWObjects = new Dictionary<ulong, string>();
                    _characterState.WoWUnits = new Dictionary<ulong, string>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OBJECT MANAGER]{ex.Message} {ex.StackTrace}");
            }
        }
    }
}
