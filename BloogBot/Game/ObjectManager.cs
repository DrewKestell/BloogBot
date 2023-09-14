using BloogBot.Game.Enums;
using BloogBot.Game.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace BloogBot.Game
{
    public class ObjectManager
    {
        const int OBJECT_TYPE_OFFSET = 0x14;

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        delegate int EnumerateVisibleObjectsCallbackVanilla(int filter, ulong guid);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate int EnumerateVisibleObjectsCallbackNonVanilla(ulong guid, int filter);

        static ulong playerGuid;
        static EnumerateVisibleObjectsCallbackVanilla callbackVanilla;
        static EnumerateVisibleObjectsCallbackNonVanilla callbackNonVanilla;
        static IntPtr callbackPtr;
        static Probe probe;

        static internal IList<WoWObject> Objects = new List<WoWObject>();
        static internal IList<WoWObject> ObjectsBuffer = new List<WoWObject>();
        static internal bool KillswitchTriggered;

        static internal void Initialize(Probe parProbe)
        {
            probe = parProbe;

            if (ClientHelper.ClientVersion == ClientVersion.Vanilla)
            {
                callbackVanilla = CallbackVanilla;
                callbackPtr = Marshal.GetFunctionPointerForDelegate(callbackVanilla);
            }
            else
            {
                callbackNonVanilla = CallbackNonVanilla;
                callbackPtr = Marshal.GetFunctionPointerForDelegate(callbackNonVanilla);
            }
            
        }

        static public LocalPlayer Player { get; private set; }

        static public LocalPet Pet { get; private set; }

        static public IEnumerable<WoWObject> AllObjects => Objects;

        static public IEnumerable<WoWUnit> Units => Objects.OfType<WoWUnit>().Where(o => o.ObjectType == ObjectType.Unit).ToList();

        static public IEnumerable<WoWPlayer> Players => Objects.OfType<WoWPlayer>();

        static public IEnumerable<WoWItem> Items => Objects.OfType<WoWItem>();

        static public IEnumerable<WoWContainer> Containers => Objects.OfType<WoWContainer>();

        static public IEnumerable<WoWGameObject> GameObjects => Objects.OfType<WoWGameObject>();

        static public WoWUnit CurrentTarget => Units.FirstOrDefault(u => Player.TargetGuid == u.Guid);

        static public bool IsLoggedIn => Functions.GetPlayerGuid() > 0;

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

        static public string SubZoneText
        {
            // this is weird and throws an exception right after entering world,
            // so we catch and ignore the exception to avoid console noise
            get
            {
                try
                {
                    var ptr = MemoryManager.ReadIntPtr((IntPtr)MemoryAddresses.SubZoneTextPtr);
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
                    if (ClientHelper.ClientVersion == ClientVersion.Vanilla)
                    {
                        var objectManagerPtr = MemoryManager.ReadIntPtr((IntPtr)0x00B41414);
                        return MemoryManager.ReadUint(IntPtr.Add(objectManagerPtr, 0xCC));
                    }
                    else
                    {
                        return MemoryManager.ReadUint((IntPtr)MemoryAddresses.MapId);
                    }
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

        static public IEnumerable<WoWPlayer> GetPartyMembers()
        {
            var partyMembers = new List<WoWPlayer>();

            for (var i = 1; i < 5; i++)
            {
                var result = GetPartyMember(i);
                if (result != null)
                    partyMembers.Add(result);
            }

            return partyMembers;
        }

        // index should be 1-4
        static WoWPlayer GetPartyMember(int index)
        {
            var result = Player?.LuaCallWithResults($"{{0}} = UnitName('party{index}')");

            if (result.Length > 0)
                return Players.FirstOrDefault(p => p.Name == result[0]);

            return null;
        }

        static public IEnumerable<WoWUnit> Aggressors =>
            Units
                .Where(u => u.Health > 0)
                .Where(u => 
                    u.TargetGuid == Player?.Guid ||
                    u.TargetGuid == Pet?.Guid)
                .Where(u =>
                    u.UnitReaction == UnitReaction.Hostile ||
                    u.UnitReaction == UnitReaction.Unfriendly ||
                    u.UnitReaction == UnitReaction.Neutral)
                .Where(u => u.IsInCombat);

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

        static internal async void StartEnumeration()
        {
            while (true)
            {
                try
                {
                    EnumerateVisibleObjects();
                    await Task.Delay(500);
                }
                catch (Exception e)
                {
                    Logger.Log(e);
                }
            }
        }

        static void EnumerateVisibleObjects()
        {
            ThreadSynchronizer.RunOnMainThread(() =>
            {
                if (IsLoggedIn)
                {
                    playerGuid = Functions.GetPlayerGuid();
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

                        UpdateProbe();
                    }
                }
            });
        }

        // EnumerateVisibleObjects callback has the parameter order swapped between Vanilla and other client versions.
        static int CallbackVanilla(int filter, ulong guid)
        {
            return CallbackInternal(guid, filter);
        }

        // EnumerateVisibleObjects callback has the parameter order swapped between Vanilla and other client versions.
        static int CallbackNonVanilla(ulong guid, int filter)
        {
            return CallbackInternal(guid, filter);
        }

        static int CallbackInternal(ulong guid, int filter)
        {
            var pointer = Functions.GetObjectPtr(guid);
            var objectType = (ObjectType)MemoryManager.ReadInt(IntPtr.Add(pointer, OBJECT_TYPE_OFFSET));

            try
            {
                switch (objectType)
                {
                    case ObjectType.Container:
                        ObjectsBuffer.Add(new WoWContainer(pointer, guid, objectType));
                        break;
                    case ObjectType.Item:
                        ObjectsBuffer.Add(new WoWItem(pointer, guid, objectType));
                        break;
                    case ObjectType.Player:
                        if (guid == playerGuid)
                        {
                            var player = new LocalPlayer(pointer, guid, objectType);
                            Player = player;
                            ObjectsBuffer.Add(player);
                        }
                        else
                            ObjectsBuffer.Add(new WoWPlayer(pointer, guid, objectType));
                        break;
                    case ObjectType.GameObject:
                        ObjectsBuffer.Add(new WoWGameObject(pointer, guid, objectType));
                        break;
                    case ObjectType.Unit:
                        ObjectsBuffer.Add(new WoWUnit(pointer, guid, objectType));
                        break;
                }
            }
            catch (Exception e)
            {
                Logger.Log(e);
            }

            return 1;
        }

        static void UpdateProbe()
        {
            if (Player != null)
            {
                // hit killswitch if player is in GM Island
                if (MinimapZoneText == "GM Island" && !KillswitchTriggered)
                {
                    Logger.Log("Killswitch Engaged");
                    Player.StopAllMovement();
                    probe.Killswitch();
                    DiscordClientWrapper.KillswitchAlert(Player.Name);
                    KillswitchTriggered = true;
                }

                probe.CurrentPosition = Player.Position.ToString();
                probe.CurrentZone = MinimapZoneText;

                var target = Units.FirstOrDefault(u => u.Guid == Player.TargetGuid);
                if (target != null)
                {
                    probe.TargetName = target.Name;
                    probe.TargetClass = Player.LuaCallWithResults($"{{0}} = UnitClass(\"target\")")[0];
                    probe.TargetCreatureType = target.CreatureType.ToString();
                    probe.TargetPosition = target.Position.ToString();
                    probe.TargetRange = Player.Position.DistanceTo(target.Position).ToString();
                    probe.TargetFactionId = target.FactionId.ToString();
                    probe.TargetIsCasting = target.IsCasting.ToString();
                    probe.TargetIsChanneling = target.IsChanneling.ToString();
                }

                probe.Callback();
            }
        }
    }
}
