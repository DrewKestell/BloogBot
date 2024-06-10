using RaidMemberBot.Constants;
using RaidMemberBot.Mem;
using RaidMemberBot.Models.Dto;
using RaidMemberBot.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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

        static internal async Task Initialize(CharacterState parProbe)
        {
            _characterState = parProbe;

            CallbackDelegate = CallbackVanilla;
            callbackPtr = Marshal.GetFunctionPointerForDelegate(CallbackDelegate);

            WoWEventHandler.Instance.OnEvent += OnEvent;

            await StartEnumeration();
        }

        static public LocalPlayer Player { get; private set; }

        static public LocalPet Pet { get; private set; }

        static public IEnumerable<WoWUnit> Units => Objects.OfType<WoWUnit>().Where(o => o.ObjectType == WoWObjectTypes.OT_UNIT).ToList();

        static public IEnumerable<WoWPlayer> Players => Objects.OfType<WoWPlayer>();

        static public IEnumerable<WoWItem> Items => Objects.OfType<WoWItem>();

        static public IEnumerable<WoWContainer> Containers => Objects.OfType<WoWContainer>();

        static public IEnumerable<WoWGameObject> GameObjects => Objects.OfType<WoWGameObject>();

        static public ulong StarTargetGuid => MemoryManager.ReadUlong((IntPtr)Offsets.RaidIcon.Star, true);
        static public ulong CircleTargetGuid => MemoryManager.ReadUlong((IntPtr)Offsets.RaidIcon.Circle, true);
        static public ulong DiamondTargetGuid => MemoryManager.ReadUlong((IntPtr)Offsets.RaidIcon.Diamond, true);
        static public ulong TriangleTargetGuid => MemoryManager.ReadUlong((IntPtr)Offsets.RaidIcon.Triangle, true);
        static public ulong MoonTargetGuid => MemoryManager.ReadUlong((IntPtr)Offsets.RaidIcon.Moon, true);
        static public ulong SquareTargetGuid => MemoryManager.ReadUlong((IntPtr)Offsets.RaidIcon.Square, true);
        static public ulong CrossTargetGuid => MemoryManager.ReadUlong((IntPtr)Offsets.RaidIcon.Cross, true);
        static public ulong SkullTargetGuid => MemoryManager.ReadUlong((IntPtr)Offsets.RaidIcon.Skull, true);

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
                    var objectManagerPtr = MemoryManager.ReadIntPtr(Offsets.ObjectManager.ManagerBase);
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
                var partyMembers = new List<WoWPlayer>() { Player };

                var partyMember1 = (WoWPlayer)Objects.FirstOrDefault(p => p.Guid == Party1Guid);
                if (partyMember1 != null)
                    partyMembers.Add(partyMember1);

                var partyMember2 = (WoWPlayer)Objects.FirstOrDefault(p => p.Guid == Party2Guid);
                if (partyMember2 != null)
                    partyMembers.Add(partyMember2);

                var partyMember3 = (WoWPlayer)Objects.FirstOrDefault(p => p.Guid == Party3Guid);
                if (partyMember3 != null)
                    partyMembers.Add(partyMember3);

                var partyMember4 = (WoWPlayer)Objects.FirstOrDefault(p => p.Guid == Party4Guid);
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

        static public List<WoWUnit> CasterAggressors =>
            Aggressors
                .Where(u => u.ManaPercent > 0)
            .ToList();

        static public List<WoWUnit> MeleeAggressors =>
            Aggressors
                .Where(u => u.ManaPercent <= 0)
            .ToList();

        static public List<WoWUnit> Aggressors =>
            Hostiles
                .Where(u => u.IsInCombat || u.IsFleeing)
            //.Where(u =>
            //    u.TargetGuid == Pet?.Guid || 
            //    u.IsFleeing ||
            //    PartyMembers.Any(x => u.TargetGuid == x.Guid))
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
            var results = Functions.LuaCallWithResult($"{{0}}, {{1}}, {{2}}, {{3}}, {{4}} = GetTalentInfo({tabIndex},{talentIndex})");

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

        static internal async Task StartEnumeration()
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
            ThreadSynchronizer.RunOnMainThread(() =>
            {
                if (!IsLoggedIn) return;

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
                    _characterState.TargetGuid = Player.TargetGuid;
                    if (Player.Target != null)
                        _characterState.TargetPointer = ((Player.BoundingRadius * Player.ScaleX) + Player.CombatReach + (Player.Target.BoundingRadius * Player.Target.ScaleX) + Player.Target.CombatReach).ToString("0.00") + " " + Player.Position.DistanceTo(Player.Target.Position).ToString("0.00");
                    else
                        _characterState.TargetPointer = "";
                    _characterState.IsOnMount = Player.IsMounted;
                    _characterState.IsFalling = Player.IsFalling;
                    _characterState.IsStunned = Player.IsStunned;
                    _characterState.IsConfused = Player.IsConfused;
                    _characterState.IsPoisoned = Player.IsPoisoned;
                    _characterState.IsDiseased = Player.IsDiseased;
                    _characterState.Zone = MinimapZoneText;
                    _characterState.InParty = int.Parse(Functions.LuaCallWithResult("{0} = GetNumPartyMembers()")[0]) > 0;
                    _characterState.InRaid = int.Parse(Functions.LuaCallWithResult("{0} = GetNumRaidMembers()")[0]) > 0;
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
                    _characterState.Position = new Vector3(Player.Position.X, Player.Position.Y, Player.Position.Z);
                    _characterState.Spells = Player.PlayerSpells.Values.SelectMany(x => x).OrderBy(x => x).ToList();
                    _characterState.Skills = Player.PlayerSkills.OrderBy(x => x).ToList();

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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OBJECT MANAGER]{ex.Message} {ex.StackTrace}");
            }
        }
    }
}
