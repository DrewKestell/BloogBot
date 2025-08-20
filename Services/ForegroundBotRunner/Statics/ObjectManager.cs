using Communication;
using ForegroundBotRunner.Mem;
using ForegroundBotRunner.Objects;
using GameData.Core.Enums;
using GameData.Core.Frames;
using GameData.Core.Interfaces;
using GameData.Core.Models;
using System.Runtime.InteropServices;

namespace ForegroundBotRunner.Statics
{
    public class ObjectManager : IObjectManager
    {
        // LUA SCRIPTS
        private const string WandLuaScript = "if IsCurrentAction(72) == nil then CastSpellByName('Shoot') end";
        private const string TurnOffWandLuaScript = "if IsCurrentAction(72) ~= nil then CastSpellByName('Shoot') end";
        private const string AutoAttackLuaScript = "if IsCurrentAction(72) == nil then CastSpellByName('Attack') end";
        private const string TurnOffAutoAttackLuaScript = "if IsCurrentAction(72) ~= nil then CastSpellByName('Attack') end";
        private const int OBJECT_TYPE_OFFSET = 0x14;

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate int EnumerateVisibleObjectsCallbackVanilla(int filter, ulong guid);

        public HighGuid PlayerGuid { get; internal set; } = new HighGuid(new byte[4], new byte[4]);
        private volatile bool _ingame1 = true;
        private readonly bool _ingame2 = true;
        public LoginStates LoginState => (LoginStates)Enum.Parse(typeof(LoginStates), MemoryManager.ReadString(Offsets.CharacterScreen.LoginState));
        private readonly EnumerateVisibleObjectsCallbackVanilla CallbackDelegate;
        private readonly nint callbackPtr;
        private readonly ActivitySnapshot _characterState;
        public IEnumerable<IWoWObject> Objects
        {
            get
            {
                lock (_objectsLock)
                {
                    return [.. ObjectsBuffer.Cast<IWoWObject>()]; // safe snapshot
                }
            }
        }
        internal IList<WoWObject> ObjectsBuffer = [];

        private readonly object _objectsLock = new();
        public ObjectManager(IWoWEventHandler eventHandler, ActivitySnapshot parProbe)
        {
            _characterState = parProbe;

            CallbackDelegate = CallbackVanilla;
            callbackPtr = Marshal.GetFunctionPointerForDelegate(CallbackDelegate);

            eventHandler.OnEvent += OnEvent;

            Task.Factory.StartNew(async () => await StartEnumeration());
        }

        public IWoWLocalPlayer Player { get; private set; }

        public IWoWLocalPet Pet { get; private set; }
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
        public bool HasEnteredWorld { get; set; }

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
        public static sbyte GetTalentRank(int tabIndex, int talentIndex)
        {
            var results = Functions.LuaCallWithResult($"{{0}}, {{1}}, {{2}}, {{3}}, {{4}} = GetTalentInfo({tabIndex},{talentIndex})");

            if (results.Length == 5)
                return Convert.ToSByte(results[4]);

            return -1;
        }

        public static void PickupInventoryItem(int inventorySlot)
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
            SetTarget(target.Guid);
            Functions.LuaCall($"SetRaidTarget('target', {targetMarker})");
        }

        public void SetTarget(ulong guid)
        {
            Functions.SetTarget(guid);
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
        public void EnterWorld(ulong characterGuid)
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

        public static int MaxCharacterCount => MemoryManager.ReadInt(0x00B42140);
        public static void ResetLogin()
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
                    slots = bag.NumOfSlots;
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
                    slots = bag.NumOfSlots;
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

                for (int slot = 0; slot < (bag == 0 ? 16 : container.NumOfSlots); slot++)
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
                var tmpSlotGuid = GetBackpackItemGuid(i);
                if (tmpSlotGuid == 0) freeSlots++;
            }
            var bagGuids = new List<ulong>();
            for (var i = 0; i < 4; i++)
                bagGuids.Add(MemoryManager.ReadUlong(nint.Add(MemoryAddresses.LocalPlayerFirstExtraBag, i * 8)));

            var tmpItems = Containers
                .Where(i => i.NumOfSlots != 0 && bagGuids.Contains(i.Guid)).ToList();

            foreach (var bag in tmpItems)
            {
                if ((bag.Info.Name.Contains("Quiver") || bag.Info.Name.Contains("Ammo") || bag.Info.Name.Contains("Shot") ||
                     bag.Info.Name.Contains("Herb") || bag.Info.Name.Contains("Soul")) && !parCountSpecialSlots) continue;

                for (var i = 1; i < bag.NumOfSlots; i++)
                {
                    var tmpSlotGuid = bag.GetItemGuid(i);
                    if (tmpSlotGuid == 0) freeSlots++;
                }
            }
            return freeSlots;
        }

        public static int EmptyBagSlots
        {
            get
            {
                var bagGuids = new List<ulong>();
                for (var i = 0; i < 4; i++)
                    bagGuids.Add(MemoryManager.ReadUlong(nint.Add(MemoryAddresses.LocalPlayerFirstExtraBag, i * 8)));

                return bagGuids.Count(b => b == 0);
            }
        }

        ILoginScreen IObjectManager.LoginScreen => throw new NotImplementedException();

        IRealmSelectScreen IObjectManager.RealmSelectScreen => throw new NotImplementedException();

        ICharacterSelectScreen IObjectManager.CharacterSelectScreen => throw new NotImplementedException();

        IGossipFrame IObjectManager.GossipFrame => throw new NotImplementedException();

        ILootFrame IObjectManager.LootFrame => throw new NotImplementedException();

        IMerchantFrame IObjectManager.MerchantFrame => throw new NotImplementedException();

        ICraftFrame IObjectManager.CraftFrame => throw new NotImplementedException();

        IQuestFrame IObjectManager.QuestFrame => throw new NotImplementedException();

        IQuestGreetingFrame IObjectManager.QuestGreetingFrame => throw new NotImplementedException();

        ITaxiFrame IObjectManager.TaxiFrame => throw new NotImplementedException();

        ITradeFrame IObjectManager.TradeFrame => throw new NotImplementedException();

        ITrainerFrame IObjectManager.TrainerFrame => throw new NotImplementedException();

        ITalentFrame IObjectManager.TalentFrame => throw new NotImplementedException();

        public List<CharacterSelect> CharacterSelects => throw new NotImplementedException();

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
                    slots = bag.NumOfSlots;
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
                    slots = bag.NumOfSlots;
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
            var guid = GetEquippedItemGuid(slot);
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
                        itemGuid = GetBackpackItemGuid(parSlot);
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

                    RefreshSpells();
                    RefreshSkills();
                }

                UpdateProbe();
            });
        }

        public void RefreshSpells()
        {
            ((LocalPlayer)Player).PlayerSpells.Clear();
            for (var i = 0; i < 1024; i++)
            {
                var currentSpellId = MemoryManager.ReadInt(MemoryAddresses.LocalPlayerSpellsBase + 4 * i);
                if (currentSpellId == 0) break;

                string name;
                var spellsBasePtr = MemoryManager.ReadIntPtr(0x00C0D788);
                var spellPtr = MemoryManager.ReadIntPtr(spellsBasePtr + currentSpellId * 4);

                var spellNamePtr = MemoryManager.ReadIntPtr(spellPtr + 0x1E0);
                name = MemoryManager.ReadString(spellNamePtr);

                if (((LocalPlayer)Player).PlayerSpells.TryGetValue(name, out int[]? value))
                    ((LocalPlayer)Player).PlayerSpells[name] =
                    [.. value, currentSpellId];
                else
                    ((LocalPlayer)Player).PlayerSpells.Add(name, [currentSpellId]);
            }
        }

        public void RefreshSkills()
        {
            ((LocalPlayer)Player).PlayerSkills.Clear();
            var skillPtr1 = MemoryManager.ReadIntPtr(nint.Add(((LocalPlayer)Player).Pointer, 8));
            var skillPtr2 = nint.Add(skillPtr1, 0xB38);

            var maxSkills = MemoryManager.ReadInt(0x00B700B4);
            for (var i = 0; i < maxSkills + 12; i++)
            {
                var curPointer = nint.Add(skillPtr2, i * 12);

                var id = (Skills)MemoryManager.ReadShort(curPointer);
                if (!Enum.IsDefined(typeof(Skills), id))
                {
                    continue;
                }

                ((LocalPlayer)Player).PlayerSkills.Add((short)id);
            }
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

        public static void InviteToGroup(string characterName)
        {
            Functions.LuaCall($"InviteByName('{characterName}')");
        }
        public ulong GetBackpackItemGuid(int slot) => MemoryManager.ReadUlong(((LocalPlayer)Player).GetDescriptorPtr() + (MemoryAddresses.LocalPlayer_BackpackFirstItemOffset + slot * 8));

        public ulong GetEquippedItemGuid(EquipSlot slot) => MemoryManager.ReadUlong(nint.Add(((LocalPlayer)Player).Pointer, MemoryAddresses.LocalPlayer_EquipmentFirstItemOffset + ((int)slot - 1) * 0x8));


        public void StartMeleeAttack()
        {
            if (!Player.IsCasting && (Player.Class == Class.Warlock || Player.Class == Class.Mage || Player.Class == Class.Priest))
            {
                Functions.LuaCall(WandLuaScript);
            }
            else if (Player.Class != Class.Hunter)
            {
                Functions.LuaCall(AutoAttackLuaScript);
            }
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

        public void StartRangedAttack()
        {
            throw new NotImplementedException();
        }

        public void StopAttack()
        {
            throw new NotImplementedException();
        }

        public bool IsSpellReady(string spellName)
        {
            throw new NotImplementedException();
        }

        public void CastSpell(string spellName, int rank = -1, bool castOnSelf = false)
        {
            throw new NotImplementedException();
        }

        public void CastSpell(uint spellId, int rank = -1, bool castOnSelf = false)
        {
            throw new NotImplementedException();
        }

        public void StartWandAttack()
        {
            throw new NotImplementedException();
        }

        public void MoveToward(Position position, float facing)
        {
            throw new NotImplementedException();
        }

        public void StopCasting()
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

        public IWoWItem GetContainedItem(int bagSlot, int slotId)
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

        public void Initialize(ActivitySnapshot parProbe)
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

        public void InviteToGroup(ulong guid)
        {
            throw new NotImplementedException();
        }

        public void KickPlayer(ulong guid)
        {
            throw new NotImplementedException();
        }

        public void DeclineGroupInvite()
        {
            throw new NotImplementedException();
        }

        public void DisbandGroup()
        {
            throw new NotImplementedException();
        }

        public bool HasPendingGroupInvite()
        {
            throw new NotImplementedException();
        }

        public bool HasLootRollWindow(int itemId)
        {
            throw new NotImplementedException();
        }

        public void LootPass(int itemId)
        {
            throw new NotImplementedException();
        }

        public void LootRollGreed(int itemId)
        {
            throw new NotImplementedException();
        }

        public void LootRollNeed(int itemId)
        {
            throw new NotImplementedException();
        }

        public void AssignLoot(int itemId, ulong playerGuid)
        {
            throw new NotImplementedException();
        }

        public void SetGroupLoot(GroupLootSetting setting)
        {
            throw new NotImplementedException();
        }

        public void PromoteLootManager(ulong playerGuid)
        {
            throw new NotImplementedException();
        }

        public void PromoteAssistant(ulong playerGuid)
        {
            throw new NotImplementedException();
        }

        public void PromoteLeader(ulong playerGuid)
        {
            throw new NotImplementedException();
        }
        public void SetFacing(float facing)
        {
            Functions.SetFacing(nint.Add(((LocalPlayer)Player).Pointer, MemoryAddresses.LocalPlayer_SetFacingOffset), facing);
            Functions.SendMovementUpdate(((LocalPlayer)Player).Pointer, (int)Opcode.MSG_MOVE_SET_FACING);
        }
        // the client will NOT send a packet to the server if a key is already pressed, so you're safe to spam this
        public void StartMovement(ControlBits bits)
        {
            if (bits == ControlBits.Nothing)
                return;

            Functions.SetControlBit((int)bits, 1, Environment.TickCount);
        }

        public void StopAllMovement()
        {
            if (Player.MovementFlags != MovementFlags.MOVEFLAG_NONE)
            {
                var bits = ControlBits.Front | ControlBits.Back | ControlBits.Left | ControlBits.Right | ControlBits.StrafeLeft | ControlBits.StrafeRight;

                StopMovement(bits);
            }
        }

        public void StopMovement(ControlBits bits)
        {
            if (bits == ControlBits.Nothing)
                return;

            Functions.SetControlBit((int)bits, 0, Environment.TickCount);
        }

        public void Jump()
        {
            StopMovement(ControlBits.Jump);
            StartMovement(ControlBits.Jump);
        }

        public static void Stand() => Functions.LuaCall("DoEmote(\"STAND\")");

        public void ReleaseCorpse() => Functions.ReleaseCorpse(((LocalPlayer)Player).Pointer);

        public void RetrieveCorpse() => Functions.RetrieveCorpse();

    }
}
