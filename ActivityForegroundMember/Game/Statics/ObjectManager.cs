using ActivityForegroundMember.Constants;
using ActivityForegroundMember.Mem;
using ActivityForegroundMember.Objects;
using BotRunner.Base;
using BotRunner.Constants;
using BotRunner.Interfaces;
using BotRunner.Models;
using System.Runtime.InteropServices;

namespace ActivityForegroundMember.Game.Statics
{
    public class ObjectManager : IObjectManager
    {
        private const int OBJECT_TYPE_OFFSET = 0x14;

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate int EnumerateVisibleObjectsCallbackVanilla(int filter, ulong guid);

        public HighGuid PlayerGuid { get; internal set; } = new HighGuid(new byte[4], new byte[4]);
        private volatile bool _ingame1 = true;
        private readonly bool _ingame2 = true;
        public LoginStates LoginState => (LoginStates)Enum.Parse(typeof(LoginStates), MemoryManager.ReadString(Offsets.CharacterScreen.LoginState));
        private EnumerateVisibleObjectsCallbackVanilla CallbackDelegate;
        private nint callbackPtr;
        private ActivityMemberState _characterState;

        public IList<WoWObject> Objects = [];
        internal IList<WoWObject> ObjectsBuffer = [];

        public ObjectManager(IWoWEventHandler eventHandler, ActivityMemberState parProbe)
        {
            _characterState = parProbe;

            CallbackDelegate = CallbackVanilla;
            callbackPtr = Marshal.GetFunctionPointerForDelegate(CallbackDelegate);

            eventHandler.OnEvent += OnEvent;

            Task.Factory.StartNew(async () => await StartEnumeration());
        }

        public ILocalPlayer Player { get; private set; }

        public ILocalPet Pet { get; private set; }
        public IEnumerable<IWoWGameObject> GameObjects { get; }
        public IEnumerable<IWoWUnit> Units { get; }
        public IEnumerable<IWoWPlayer> Players { get; }
        public IEnumerable<IWoWItem> Items { get; }
        public IEnumerable<IWoWContainer> Containers { get; }
        public ulong StarTargetGuid => MemoryManager.ReadUlong((nint)Offsets.RaidIcon.Star, true);
        public ulong CircleTargetGuid => MemoryManager.ReadUlong((nint)Offsets.RaidIcon.Circle, true);
        public ulong DiamondTargetGuid => MemoryManager.ReadUlong((nint)Offsets.RaidIcon.Diamond, true);
        public ulong TriangleTargetGuid => MemoryManager.ReadUlong((nint)Offsets.RaidIcon.Triangle, true);
        public ulong MoonTargetGuid => MemoryManager.ReadUlong((nint)Offsets.RaidIcon.Moon, true);
        public ulong SquareTargetGuid => MemoryManager.ReadUlong((nint)Offsets.RaidIcon.Square, true);
        public ulong CrossTargetGuid => MemoryManager.ReadUlong((nint)Offsets.RaidIcon.Cross, true);
        public ulong SkullTargetGuid => MemoryManager.ReadUlong((nint)Offsets.RaidIcon.Skull, true);

        public bool IsLoggedIn => _ingame1 && _ingame2 && MemoryManager.ReadByte(0xB4B424) == 1;

        public void AntiAfk() => MemoryManager.WriteInt(MemoryAddresses.LastHardwareAction, Environment.TickCount);

        public string ZoneText
        {
            // this is weird and throws an exception right after entering world,
            // so we catch and ignore the exception to avoid console noise
            get
            {
                try
                {
                    var ptr = MemoryManager.ReadIntPtr(MemoryAddresses.ZoneTextPtr);
                    return MemoryManager.ReadString(ptr);
                }
                catch (Exception)
                {
                    return "";
                }
            }
        }

        public string MinimapZoneText
        {
            // this is weird and throws an exception right after entering world,
            // so we catch and ignore the exception to avoid console noise
            get
            {
                try
                {
                    var ptr = MemoryManager.ReadIntPtr(MemoryAddresses.MinimapZoneTextPtr);
                    return MemoryManager.ReadString(ptr);
                }
                catch (Exception)
                {
                    return "";
                }
            }
        }

        public uint MapId
        {
            // this is weird and throws an exception right after entering world,
            // so we catch and ignore the exception to avoid console noise
            get
            {
                try
                {
                    var objectManagerPtr = MemoryManager.ReadIntPtr(Offsets.ObjectManager.ManagerBase);
                    return MemoryManager.ReadUint(nint.Add(objectManagerPtr, 0xCC));
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }

        public string ServerName
        {
            // this is weird and throws an exception right after entering world,
            // so we catch and ignore the exception to avoid console noise
            get
            {
                try
                {
                    // not exactly sure how this works. seems to return a string like "Endless\WoW.exe" or "Karazhan\WoW.exe"
                    var fullName = MemoryManager.ReadString(MemoryAddresses.ServerName);
                    return fullName.Split('\\').First();
                }
                catch (Exception)
                {
                    return "";
                }
            }
        }

        public IEnumerable<IWoWPlayer> PartyMembers
        {
            get
            {
                var partyMembers = new List<IWoWPlayer>() { Player };

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

        public IWoWPlayer PartyLeader => Players.FirstOrDefault(p => p.Guid == PartyLeaderGuid);

        public ulong PartyLeaderGuid => MemoryManager.ReadUlong(MemoryAddresses.PartyLeaderGuid);
        public ulong Party1Guid => MemoryManager.ReadUlong(MemoryAddresses.Party1Guid);
        public ulong Party2Guid => MemoryManager.ReadUlong(MemoryAddresses.Party2Guid);
        public ulong Party3Guid => MemoryManager.ReadUlong(MemoryAddresses.Party3Guid);
        public ulong Party4Guid => MemoryManager.ReadUlong(MemoryAddresses.Party4Guid);

        public IEnumerable<IWoWUnit> CasterAggressors =>
            Aggressors
                .Where(u => u.ManaPercent > 0);

        public IEnumerable<IWoWUnit> MeleeAggressors =>
            Aggressors
                .Where(u => u.ManaPercent <= 0);

        public IEnumerable<IWoWUnit> Aggressors =>
            Hostiles
                .Where(u => u.IsInCombat || u.IsFleeing);
        //.Where(u =>
        //    u.TargetGuid == Pet?.Guid || 
        //    u.IsFleeing ||
        //    PartyMembers.Any(x => u.TargetGuid == x.Guid));            

        public IEnumerable<IWoWUnit> Hostiles =>
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
        public sbyte GetTalentRank(int tabIndex, int talentIndex)
        {
            var results = Functions.LuaCallWithResult($"{{0}}, {{1}}, {{2}}, {{3}}, {{4}} = GetTalentInfo({tabIndex},{talentIndex})");

            if (results.Length == 5)
                return Convert.ToSByte(results[4]);

            return -1;
        }

        public void PickupInventoryItem(int inventorySlot)
        {
            Functions.LuaCall($"PickupInventoryItem({inventorySlot})");
        }

        public void DeleteCursorItem()
        {
            Functions.LuaCall("DeleteCursorItem()");
        }

        public void SendChatMessage(string chatMessage)
        {
            Functions.LuaCall($"SendChatMessage(\"{chatMessage}\")");
        }

        public void SetRaidTarget(IWoWUnit target, TargetMarker targetMarker)
        {
            Player.SetTarget(target.Guid);
            Functions.LuaCall($"SetRaidTarget('target', {targetMarker})");
        }

        public void AcceptGroupInvite()
        {
            ThreadSynchronizer.RunOnMainThread(() =>
            {
                Functions.LuaCall($"StaticPopup1Button1:Click()");
                Functions.LuaCall($"AcceptGroup()");
            });
        }

        public void EquipCursorItem()
        {
            Functions.LuaCall("AutoEquipCursorItem()");
        }

        public void ConfirmItemEquip()
        {
            Functions.LuaCall($"AutoEquipCursorItem()");
            Functions.LuaCall($"StaticPopup1Button1:Click()");
        }
        public void EnterWorld()
        {
            const string str = "if CharSelectEnterWorldButton ~= nil then CharSelectEnterWorldButton:Click()  end";
            Functions.LuaCall(str);
        }
        public void DefaultServerLogin(string accountName, string password)
        {
            if (LoginState != LoginStates.login) return;
            Functions.LuaCall($"DefaultServerLogin('{accountName}', '{password}');");
        }

        public string GlueDialogText => Functions.LuaCallWithResult("{0} = GlueDialogText:GetText()")[0];

        public int MaxCharacterCount => MemoryManager.ReadInt(0x00B42140);
        public void ResetLogin()
        {
            Functions.LuaCall("arg1 = 'ESCAPE' GlueDialog_OnKeyDown()");
            Functions.LuaCall("if RealmListCancelButton ~= nil then if RealmListCancelButton:IsVisible() then RealmListCancelButton:Click(); end end ");
        }

        public void JoinBattleGroundQueue()
        {
            string enabled = Functions.LuaCallWithResult("{0} = BattlefieldFrameGroupJoinButton:IsEnabled()")[0];

            if (enabled == "1")
                Functions.LuaCall("BattlefieldFrameGroupJoinButton:Click()");
            else
                Functions.LuaCall("BattlefieldFrameJoinButton:Click()");
        }
        public int GetItemCount(string parItemName)
        {
            var totalCount = 0;
            for (var i = 0; i < 5; i++)
            {
                int slots;
                if (i == 0)
                {
                    slots = 16;
                }
                else
                {
                    var iAdjusted = i - 1;
                    var bag = GetExtraBag(iAdjusted);
                    if (bag == null) continue;
                    slots = bag.Slots;
                }

                for (var k = 0; k <= slots; k++)
                {
                    var item = GetItem(i, k);
                    if (item?.Info.Name == parItemName) totalCount += (int)item.StackCount;
                }
            }
            return totalCount;
        }

        public int GetItemCount(int itemId)
        {
            var totalCount = 0;
            for (var i = 0; i < 5; i++)
            {
                int slots;
                if (i == 0)
                {
                    slots = 16;
                }
                else
                {
                    var iAdjusted = i - 1;
                    var bag = GetExtraBag(iAdjusted);
                    if (bag == null) continue;
                    slots = bag.Slots;
                }

                for (var k = 0; k <= slots; k++)
                {
                    var item = GetItem(i, k);
                    if (item?.ItemId == itemId) totalCount += (int)item.StackCount;
                }
            }
            return totalCount;
        }

        public IList<IWoWItem> GetAllItems()
        {
            var items = new List<IWoWItem>();
            for (int bag = 0; bag < 5; bag++)
            {
                var container = GetExtraBag(bag - 1);
                if (bag != 0 && container == null)
                {
                    continue;
                }

                for (int slot = 0; slot < (bag == 0 ? 16 : container.Slots); slot++)
                {
                    var item = GetItem(bag, slot);
                    if (item == null)
                    {
                        continue;
                    }

                    items.Add(item);
                }
            }

            return items;
        }

        public int CountFreeSlots(bool parCountSpecialSlots)
        {
            var freeSlots = 0;
            for (var i = 0; i < 16; i++)
            {
                var tmpSlotGuid = Player.GetBackpackItemGuid(i);
                if (tmpSlotGuid == 0) freeSlots++;
            }
            var bagGuids = new List<ulong>();
            for (var i = 0; i < 4; i++)
                bagGuids.Add(MemoryManager.ReadUlong(nint.Add(MemoryAddresses.LocalPlayerFirstExtraBag, i * 8)));

            var tmpItems = Containers
                .Where(i => i.Slots != 0 && bagGuids.Contains(i.Guid)).ToList();

            foreach (var bag in tmpItems)
            {
                if ((bag.Info.Name.Contains("Quiver") || bag.Info.Name.Contains("Ammo") || bag.Info.Name.Contains("Shot") ||
                     bag.Info.Name.Contains("Herb") || bag.Info.Name.Contains("Soul")) && !parCountSpecialSlots) continue;

                for (var i = 1; i < bag.Slots; i++)
                {
                    var tmpSlotGuid = bag.GetItemGuid(i);
                    if (tmpSlotGuid == 0) freeSlots++;
                }
            }
            return freeSlots;
        }

        public int EmptyBagSlots
        {
            get
            {
                var bagGuids = new List<ulong>();
                for (var i = 0; i < 4; i++)
                    bagGuids.Add(MemoryManager.ReadUlong(nint.Add(MemoryAddresses.LocalPlayerFirstExtraBag, i * 8)));

                return bagGuids.Count(b => b == 0);
            }
        }

        IList<IWoWObject> IObjectManager.Objects => throw new NotImplementedException();

        public uint GetBagId(ulong itemGuid)
        {
            var totalCount = 0;
            for (var i = 0; i < 5; i++)
            {
                int slots;
                if (i == 0)
                {
                    slots = 16;
                }
                else
                {
                    var iAdjusted = i - 1;
                    var bag = GetExtraBag(iAdjusted);
                    if (bag == null) continue;
                    slots = bag.Slots;
                }

                for (var k = 0; k < slots; k++)
                {
                    var item = GetItem(i, k);
                    if (item?.Guid == itemGuid) return (uint)i;
                }
            }
            return (uint)totalCount;
        }

        public uint GetSlotId(ulong itemGuid)
        {
            var totalCount = 0;
            for (var i = 0; i < 5; i++)
            {
                int slots;
                if (i == 0)
                {
                    slots = 16;
                }
                else
                {
                    var iAdjusted = i - 1;
                    var bag = GetExtraBag(iAdjusted);
                    if (bag == null) continue;
                    slots = bag.Slots;
                }

                for (var k = 0; k < slots; k++)
                {
                    var item = GetItem(i, k);
                    if (item?.Guid == itemGuid) return (uint)k + 1;
                }
            }
            return (uint)totalCount;
        }

        public IWoWItem GetEquippedItem(EquipSlot slot)
        {
            var guid = Player.GetEquippedItemGuid(slot);
            if (guid == 0) return null;
            return Items.FirstOrDefault(i => i.Guid == guid);
        }
        public IEnumerable<IWoWItem> GetEquippedItems()
        {
            IWoWItem headItem = GetEquippedItem(EquipSlot.Head);
            IWoWItem neckItem = GetEquippedItem(EquipSlot.Neck);
            IWoWItem shoulderItem = GetEquippedItem(EquipSlot.Shoulders);
            IWoWItem backItem = GetEquippedItem(EquipSlot.Back);
            IWoWItem chestItem = GetEquippedItem(EquipSlot.Chest);
            IWoWItem shirtItem = GetEquippedItem(EquipSlot.Shirt);
            IWoWItem tabardItem = GetEquippedItem(EquipSlot.Tabard);
            IWoWItem wristItem = GetEquippedItem(EquipSlot.Wrist);
            IWoWItem handsItem = GetEquippedItem(EquipSlot.Hands);
            IWoWItem waistItem = GetEquippedItem(EquipSlot.Waist);
            IWoWItem legsItem = GetEquippedItem(EquipSlot.Legs);
            IWoWItem feetItem = GetEquippedItem(EquipSlot.Feet);
            IWoWItem finger1Item = GetEquippedItem(EquipSlot.Finger1);
            IWoWItem finger2Item = GetEquippedItem(EquipSlot.Finger2);
            IWoWItem trinket1Item = GetEquippedItem(EquipSlot.Trinket1);
            IWoWItem trinket2Item = GetEquippedItem(EquipSlot.Trinket2);
            IWoWItem mainHandItem = GetEquippedItem(EquipSlot.MainHand);
            IWoWItem offHandItem = GetEquippedItem(EquipSlot.OffHand);
            IWoWItem rangedItem = GetEquippedItem(EquipSlot.Ranged);

            List<IWoWItem> list =
            [
                .. headItem != null ? new List<IWoWItem> { headItem } : [],
                .. neckItem != null ? new List<IWoWItem> { neckItem } : [],
                .. shoulderItem != null ? new List<IWoWItem> { shoulderItem } : [],
                .. backItem != null ? new List<IWoWItem> { backItem } : [],
                .. chestItem != null ? new List<IWoWItem> { chestItem } : [],
                .. shirtItem != null ? new List<IWoWItem> { shirtItem } : [],
                .. tabardItem != null ? new List<IWoWItem> { tabardItem } : [],
                .. wristItem != null ? new List<IWoWItem> { wristItem } : [],
                .. handsItem != null ? new List<IWoWItem> { handsItem } : [],
                .. waistItem != null ? new List<IWoWItem> { waistItem } : [],
                .. legsItem != null ? new List<IWoWItem> { legsItem } : [],
                .. feetItem != null ? new List<IWoWItem> { feetItem } : [],
                .. finger1Item != null ? new List<IWoWItem> { finger1Item } : [],
                .. finger2Item != null ? new List<IWoWItem> { finger2Item } : [],
                .. trinket1Item != null ? new List<IWoWItem> { trinket1Item } : [],
                .. trinket2Item != null ? new List<IWoWItem> { trinket2Item } : [],
                .. mainHandItem != null ? new List<IWoWItem> { mainHandItem } : [],
                .. offHandItem != null ? new List<IWoWItem> { offHandItem } : [],
                .. rangedItem != null ? new List<IWoWItem> { rangedItem } : [],
            ];
            return list;
        }

        private IWoWContainer GetExtraBag(int parSlot)
        {
            if (parSlot > 3 || parSlot < 0) return null;
            var bagGuid = MemoryManager.ReadUlong(nint.Add(MemoryAddresses.LocalPlayerFirstExtraBag, parSlot * 8));
            return bagGuid == 0 ? null : Containers.FirstOrDefault(i => i.Guid == bagGuid);
        }

        public IWoWItem GetItem(int parBag, int parSlot)
        {
            parBag += 1;
            switch (parBag)
            {
                case 1:
                    ulong itemGuid = 0;
                    if (parSlot < 16 && parSlot >= 0)
                        itemGuid = Player.GetBackpackItemGuid(parSlot);
                    return itemGuid == 0 ? null : Items.FirstOrDefault(i => i.Guid == itemGuid);

                case 2:
                case 3:
                case 4:
                case 5:
                    var tmpBag = GetExtraBag(parBag - 2);
                    if (tmpBag == null) return null;
                    var tmpItemGuid = tmpBag.GetItemGuid(parSlot);
                    if (tmpItemGuid == 0) return null;
                    return Items.FirstOrDefault(i => i.Guid == tmpItemGuid);

                default:
                    return null;
            }
        }

        private void OnEvent(object sender, OnEventArgs args)
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

        internal async Task StartEnumeration()
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
                    Console.WriteLine($"[OBJECT MANAGER] {e}");
                }
            }
        }

        private void EnumerateVisibleObjects()
        {
            ThreadSynchronizer.RunOnMainThread(() =>
            {
                if (!IsLoggedIn) return;
                ulong playerGuid = Functions.GetPlayerGuid();
                byte[] playerGuidParts = BitConverter.GetBytes(playerGuid);
                PlayerGuid = new HighGuid(playerGuidParts[0..4], playerGuidParts[4..8]);

                if (PlayerGuid.FullGuid == 0)
                {
                    Player = null;
                    return;
                }
                var playerObject = Functions.GetObjectPtr(PlayerGuid.FullGuid);
                if (playerObject == nint.Zero)
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
                            Pet = new LocalPet(((WoWObject)unit).Pointer, unit.HighGuid, unit.ObjectType);
                            petFound = true;
                        }
                    }

                    if (!petFound)
                        Pet = null;

                    Player.RefreshSpells();
                    Player.RefreshSkills();
                }

                UpdateProbe();
            });
        }

        // EnumerateVisibleObjects callback has the parameter order swapped between Vanilla and other client versions.
        private int CallbackVanilla(int filter, ulong guid)
        {
            return CallbackInternal(guid, filter);
        }

        private int CallbackInternal(ulong guid, int filter)
        {
            if (guid == 0) return 0;
            var pointer = Functions.GetObjectPtr(guid);
            var objectType = (WoWObjectType)MemoryManager.ReadInt(nint.Add(pointer, OBJECT_TYPE_OFFSET));
            byte[] guidParts = BitConverter.GetBytes(guid);
            HighGuid highGuid = new(guidParts[0..3], guidParts[4..8]);
            try
            {
                switch (objectType)
                {
                    case WoWObjectType.Container:
                        ObjectsBuffer.Add(new WoWContainer(pointer, highGuid, objectType));
                        break;
                    case WoWObjectType.Item:
                        ObjectsBuffer.Add(new WoWItem(pointer, highGuid, objectType));
                        break;
                    case WoWObjectType.Player:
                        if (guid == PlayerGuid.FullGuid)
                        {
                            var player = new LocalPlayer(pointer, highGuid, objectType);
                            Player = player;
                            ObjectsBuffer.Add(player);
                        }
                        else
                            ObjectsBuffer.Add(new WoWPlayer(pointer, highGuid, objectType));
                        break;
                    case WoWObjectType.GameObj:
                        ObjectsBuffer.Add(new WoWGameObject(pointer, highGuid, objectType));
                        break;
                    case WoWObjectType.Unit:
                        ObjectsBuffer.Add(new WoWUnit(pointer, highGuid, objectType));
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"OBJECT MANAGER: CallbackInternal => {e.StackTrace}");
            }

            return 1;
        }

        private void UpdateProbe()
        {
            try
            {
                if (IsLoggedIn)
                {
                    //_characterState.Guid = playerGuid;
                    //_characterState.CharacterName = Player.Name;
                    //_characterState.Zone = MinimapZoneText;
                    //_characterState.InParty = int.Parse(Functions.LuaCallWithResult("{0} = GetNumPartyMembers()")[0]) > 0;
                    //_characterState.InRaid = int.Parse(Functions.LuaCallWithResult("{0} = GetNumRaidMembers()")[0]) > 0;
                    //_characterState.MapId = (int)MapId;
                    //_characterState.Race = Enum.GetValues(typeof(Race)).Cast<Race>().Where(x => x.GetDescription() == Player.Race).First();
                    //_characterState.Facing = Player.Facing;
                    //_characterState.Position = new Vector3(Player.Position.X, Player.Position.Y, Player.Position.Z);

                    IWoWItem headItem = GetEquippedItem(EquipSlot.Head);
                    IWoWItem neckItem = GetEquippedItem(EquipSlot.Neck);
                    IWoWItem shoulderItem = GetEquippedItem(EquipSlot.Shoulders);
                    IWoWItem backItem = GetEquippedItem(EquipSlot.Back);
                    IWoWItem chestItem = GetEquippedItem(EquipSlot.Chest);
                    IWoWItem shirtItem = GetEquippedItem(EquipSlot.Shirt);
                    IWoWItem tabardItem = GetEquippedItem(EquipSlot.Tabard);
                    IWoWItem wristItem = GetEquippedItem(EquipSlot.Wrist);
                    IWoWItem handsItem = GetEquippedItem(EquipSlot.Hands);
                    IWoWItem waistItem = GetEquippedItem(EquipSlot.Waist);
                    IWoWItem legsItem = GetEquippedItem(EquipSlot.Legs);
                    IWoWItem feetItem = GetEquippedItem(EquipSlot.Feet);
                    IWoWItem finger1Item = GetEquippedItem(EquipSlot.Finger1);
                    IWoWItem finger2Item = GetEquippedItem(EquipSlot.Finger2);
                    IWoWItem trinket1Item = GetEquippedItem(EquipSlot.Trinket1);
                    IWoWItem trinket2Item = GetEquippedItem(EquipSlot.Trinket2);
                    IWoWItem mainHandItem = GetEquippedItem(EquipSlot.MainHand);
                    IWoWItem offHandItem = GetEquippedItem(EquipSlot.OffHand);
                    IWoWItem rangedItem = GetEquippedItem(EquipSlot.Ranged);

                    //if (headItem != null)
                    //{
                    //    _characterState.HeadItem = headItem.ItemId;
                    //}
                    //else
                    //{
                    //    _characterState.HeadItem = 0;
                    //}
                    //if (neckItem != null)
                    //{
                    //    _characterState.NeckItem = neckItem.ItemId;
                    //}
                    //else
                    //{
                    //    _characterState.NeckItem = 0;
                    //}
                    //if (shoulderItem != null)
                    //{
                    //    _characterState.ShoulderItem = shoulderItem.ItemId;
                    //}
                    //else
                    //{
                    //    _characterState.ShoulderItem = 0;
                    //}
                    //if (backItem != null)
                    //{
                    //    _characterState.BackItem = backItem.ItemId;
                    //}
                    //else
                    //{
                    //    _characterState.BackItem = 0;
                    //}
                    //if (chestItem != null)
                    //{
                    //    _characterState.ChestItem = chestItem.ItemId;
                    //}
                    //else
                    //{
                    //    _characterState.ChestItem = 0;
                    //}
                    //if (shirtItem != null)
                    //{
                    //    _characterState.ShirtItem = shirtItem.ItemId;
                    //}
                    //else
                    //{
                    //    _characterState.ShirtItem = 0;
                    //}
                    //if (tabardItem != null)
                    //{
                    //    _characterState.TabardItem = tabardItem.ItemId;
                    //}
                    //else
                    //{
                    //    _characterState.TabardItem = 0;
                    //}
                    //if (wristItem != null)
                    //{
                    //    _characterState.WristsItem = wristItem.ItemId;
                    //}
                    //else
                    //{
                    //    _characterState.WristsItem = 0;
                    //}
                    //if (handsItem != null)
                    //{
                    //    _characterState.HandsItem = handsItem.ItemId;
                    //}
                    //else
                    //{
                    //    _characterState.HandsItem = 0;
                    //}
                    //if (waistItem != null)
                    //{
                    //    _characterState.WaistItem = waistItem.ItemId;
                    //}
                    //else
                    //{
                    //    _characterState.WaistItem = 0;
                    //}
                    //if (legsItem != null)
                    //{
                    //    _characterState.LegsItem = legsItem.ItemId;
                    //}
                    //else
                    //{
                    //    _characterState.LegsItem = 0;
                    //}
                    //if (feetItem != null)
                    //{
                    //    _characterState.FeetItem = feetItem.ItemId;
                    //}
                    //else
                    //{
                    //    _characterState.FeetItem = 0;
                    //}
                    //if (finger1Item != null)
                    //{
                    //    _characterState.Finger1Item = finger1Item.ItemId;
                    //}
                    //else
                    //{
                    //    _characterState.Finger1Item = 0;
                    //}
                    //if (finger2Item != null)
                    //{
                    //    _characterState.Finger2Item = finger2Item.ItemId;
                    //}
                    //else
                    //{
                    //    _characterState.Finger2Item = 0;
                    //}
                    //if (trinket1Item != null)
                    //{
                    //    _characterState.Trinket1Item = trinket1Item.ItemId;
                    //}
                    //else
                    //{
                    //    _characterState.Trinket1Item = 0;
                    //}
                    //if (trinket2Item != null)
                    //{
                    //    _characterState.Trinket2Item = trinket2Item.ItemId;
                    //}
                    //else
                    //{
                    //    _characterState.Trinket2Item = 0;
                    //}
                    //if (mainHandItem != null)
                    //{
                    //    _characterState.MainHandItem = mainHandItem.ItemId;
                    //}
                    //else
                    //{
                    //    _characterState.MainHandItem = 0;
                    //}
                    //if (offHandItem != null)
                    //{
                    //    _characterState.OffHandItem = offHandItem.ItemId;
                    //}
                    //else
                    //{
                    //    _characterState.OffHandItem = 0;
                    //}
                    //if (rangedItem != null)
                    //{
                    //    _characterState.RangedItem = rangedItem.ItemId;
                    //}
                    //else
                    //{
                    //    _characterState.RangedItem = 0;
                    //}
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OBJECT MANAGER]{ex.Message} {ex.StackTrace}");
            }
        }

        public void LeaveGroup()
        {
            Functions.LuaCall("LeaveParty()");
        }

        public void ResetInstances()
        {
            Functions.LuaCall("ResetInstances()");
        }

        public void PickupMacro(uint v)
        {
            Functions.LuaCall($"PickupMacro({v})");
        }

        public void PlaceAction(uint v)
        {
            Functions.LuaCall($"PlaceAction({v})");
        }

        public void ConvertToRaid()
        {
            Functions.LuaCall("ConvertToRaid()");
        }

        public void InviteToGroup(string characterName)
        {
            Functions.LuaCall($"InviteByName('{characterName}')");
        }

        public void Initialize(ActivityMemberState parProbe)
        {
            throw new NotImplementedException();
        }

        public sbyte GetTalentRank(uint tabIndex, uint talentIndex)
        {
            throw new NotImplementedException();
        }

        public void PickupInventoryItem(uint inventorySlot)
        {
            throw new NotImplementedException();
        }

        public uint GetItemCount(uint itemId)
        {
            throw new NotImplementedException();
        }

        public void UseContainerItem(int v1, int v2)
        {
            throw new NotImplementedException();
        }

        public void PickupContainerItem(uint v1, uint v2)
        {
            throw new NotImplementedException();
        }

        public IWoWUnit GetTarget(IWoWUnit woWUnit)
        {
            throw new NotImplementedException();
        }
    }
}
