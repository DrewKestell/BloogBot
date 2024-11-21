using BotRunner.Constants;
using BotRunner.Frames;
using BotRunner.Interfaces;
using BotRunner.Models;
using Communication;

namespace BotRunner.Base
{
    public abstract class BaseObjectManager(IWoWEventHandler woWEventHandler, ActivitySnapshot activitySnapshot) 
    {
        private readonly IWoWEventHandler _woWEventHandler = woWEventHandler;
        private readonly ActivitySnapshot _activitySnapshot = activitySnapshot;

        #region Player Information
        public HighGuid PlayerGuid { get; } = new HighGuid(new byte[4], new byte[4]);

        public uint MapId { get; }
        public string ZoneText { get; } = string.Empty;
        public string MinimapZoneText { get; } = string.Empty;
        public string ServerName { get; } = string.Empty;
        public ILocalPlayer Player => (ILocalPlayer)Players.First(x => x.Guid == PlayerGuid.FullGuid);
        public ILocalPet Pet { get; } = null;
        public IEnumerable<IWoWContainer> Containers => Objects.OfType<IWoWContainer>();
        public IEnumerable<IWoWItem> Items => Objects.OfType<IWoWItem>();
        #endregion

        #region Game Object Management
        public IList<IWoWObject> Objects { get; } = [];
        public IEnumerable<IWoWGameObject> GameObjects => Objects.OfType<IWoWGameObject>();
        public IEnumerable<IWoWUnit> Units => Objects.OfType<IWoWUnit>();
        public IEnumerable<IWoWPlayer> Players => Objects.OfType<IWoWPlayer>();
        public IEnumerable<IWoWUnit> CasterAggressors => Units.OfType<IWoWUnit>();
        public IEnumerable<IWoWUnit> MeleeAggressors => Units.OfType<IWoWUnit>();
        public IEnumerable<IWoWUnit> Aggressors => Units.OfType<IWoWUnit>();
        public IEnumerable<IWoWUnit> Hostiles => Units.OfType<IWoWUnit>();
        #endregion

        #region Party & Raid
        public IEnumerable<IWoWPlayer> PartyMembers => Players.Where(x =>
                                                                   x.Guid == Party1Guid
                                                                || x.Guid == Party2Guid
                                                                || x.Guid == Party3Guid
                                                                || x.Guid == Party4Guid);
        public IWoWPlayer PartyLeader => PartyMembers.First(x => x.Guid == PartyLeaderGuid);
        public ulong PartyLeaderGuid { get; }
        public ulong Party1Guid { get; }
        public ulong Party2Guid { get; }
        public ulong Party3Guid { get; }
        public ulong Party4Guid { get; }

        public ulong StarTargetGuid { get; }
        public ulong CircleTargetGuid { get; }
        public ulong DiamondTargetGuid { get; }
        public ulong TriangleTargetGuid { get; }
        public ulong MoonTargetGuid { get; }
        public ulong SquareTargetGuid { get; }
        public ulong CrossTargetGuid { get; }
        public ulong SkullTargetGuid { get; }
        #endregion

        #region Login & Character Selection
        public string GlueDialogText { get; } = string.Empty;
        public LoginStates LoginState { get; }
        public ILoginScreen LoginScreen { get; }
        public IRealmSelectScreen RealmSelectScreen { get; }
        public ICharacterSelectScreen CharacterSelectScreen { get; }
        #endregion

        #region Frames
        public IGossipFrame GossipFrame { get; }
        public ILootFrame LootFrame { get; }
        public IMerchantFrame MerchantFrame { get; }
        public IQuestFrame QuestFrame { get; }
        public IQuestGreetingFrame QuestGreetingFrame { get; }
        public ITaxiFrame TaxiFrame { get; }
        public ITradeFrame TradeFrame { get; }
        public ITrainerFrame TrainerFrame { get; }
        public ICraftFrame CraftFrame { get; }
        public ITalentFrame TalentFrame { get; }
        #endregion
    }
}
