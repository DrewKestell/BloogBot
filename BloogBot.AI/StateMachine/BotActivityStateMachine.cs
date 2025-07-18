using BloogBot.AI.States;
using GameData.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Stateless;

namespace BloogBot.AI.StateMachine;

public sealed class BotActivityStateMachine
{
    private readonly StateMachine<BotActivity, Trigger> _sm;
    public BotActivity Current => _sm.State;
    public IObjectManager ObjectManager { get; private set; }
    private ILogger Logger;

    public BotActivityStateMachine(
        ILoggerFactory loggerFactory,
        IObjectManager objectManager,
        BotActivity initial = BotActivity.Resting
    )
    {
        Logger = loggerFactory.CreateLogger<BotActivityStateMachine>();
        ObjectManager = objectManager;
        _sm = new(initial);

        ConfigureGlobalTransitions();
        ConfigureResting();
        ConfigureQuesting();
        ConfigureGrinding();
        ConfigureProfessions();
        ConfigureTalenting();
        ConfigureEquipping();
        ConfigureTrading();
        ConfigureGuilding();
        ConfigureChatting();
        ConfigureHelping();
        ConfigureMailing();
        ConfigurePartying();
        ConfigureRolePlaying();
        ConfigureCombat();
        ConfigureBattlegrounding();
        ConfigureDungeoning();
        ConfigureRaiding();
        ConfigureWorldPvPing();
        ConfigureCamping();
        ConfigureAuction();
        ConfigureBanking();
        ConfigureVending();
        ConfigureExploring();
        ConfigureTraveling();
        ConfigureEscaping();
        ConfigureEventing();
        DecideNextActiveState();
    }

    void ConfigureGlobalTransitions()
    {
        foreach (BotActivity activity in Enum.GetValues(typeof(BotActivity)))
        {
            _sm.Configure(activity)
                .PermitDynamic(Trigger.PartyInvite, DecideNextActiveState)
                .PermitDynamic(Trigger.GuildInvite, DecideNextActiveState)
                .PermitDynamic(Trigger.LowHealth, DecideNextActiveState)
                .PermitDynamic(Trigger.TalentPointsAvailable, DecideNextActiveState)
                .PermitDynamic(Trigger.TradeRequested, DecideNextActiveState)
                .PermitDynamic(Trigger.ChatMessageReceived, DecideNextActiveState)
                .PermitDynamic(Trigger.HelpRequested, DecideNextActiveState)
                .PermitDynamic(Trigger.MailReceived, DecideNextActiveState)
                .PermitDynamic(Trigger.RolePlayEngaged, DecideNextActiveState)
                .PermitDynamic(Trigger.CombatStarted, DecideNextActiveState)
                .PermitDynamic(Trigger.BattlegroundStarted, DecideNextActiveState)
                .PermitDynamic(Trigger.DungeonStarted, DecideNextActiveState)
                .PermitDynamic(Trigger.RaidStarted, DecideNextActiveState)
                .PermitDynamic(Trigger.PvPEngaged, DecideNextActiveState)
                .PermitDynamic(Trigger.CampingStarted, DecideNextActiveState)
                .PermitDynamic(Trigger.AuctionStarted, DecideNextActiveState)
                .PermitDynamic(Trigger.BankingNeeded, DecideNextActiveState)
                .PermitDynamic(Trigger.VendorNeeded, DecideNextActiveState)
                .PermitDynamic(Trigger.ExplorationStarted, DecideNextActiveState)
                .PermitDynamic(Trigger.TravelRequired, DecideNextActiveState)
                .PermitDynamic(Trigger.EscapeRequired, DecideNextActiveState)
                .PermitDynamic(Trigger.EventStarted, DecideNextActiveState);
        }
    }

    void ConfigureResting() =>
        _sm.Configure(BotActivity.Resting)
            .PermitDynamic(Trigger.HealthRestored, DecideNextActiveState);

    void ConfigureQuesting() =>
        _sm.Configure(BotActivity.Questing)
            .OnEntry(ctx => Logger.LogInformation("Starting quest"))
            .PermitDynamic(Trigger.QuestComplete, DecideNextActiveState)
            .PermitDynamic(Trigger.QuestFailed, DecideNextActiveState);

    void ConfigureGrinding() =>
        _sm.Configure(BotActivity.Grinding)
            .OnEntry(ctx => Logger.LogInformation("Starting grind"))
            .PermitDynamic(Trigger.ProfessionLevelUp, DecideNextActiveState);

    void ConfigureProfessions() =>
        _sm.Configure(BotActivity.Professions)
            .OnEntry(ctx => Logger.LogInformation("Starting professions"))
            .PermitDynamic(Trigger.ProfessionLevelUp, DecideNextActiveState);

    void ConfigureTalenting() =>
        _sm.Configure(BotActivity.Talenting)
            .OnEntry(ctx => Logger.LogInformation("Starting talent management"))
            .PermitDynamic(Trigger.TalentPointsAttributed, DecideNextActiveState);

    void ConfigureEquipping() =>
        _sm.Configure(BotActivity.Equipping)
            .OnEntry(ctx => Logger.LogInformation("Starting equipment management"))
            .PermitDynamic(Trigger.EquipmentChanged, DecideNextActiveState);

    void ConfigureTrading() =>
        _sm.Configure(BotActivity.Trading)
            .OnEntry(ctx => Logger.LogInformation("Starting trading"))
            .PermitDynamic(Trigger.TradeComplete, DecideNextActiveState);

    void ConfigureGuilding() =>
        _sm.Configure(BotActivity.Guilding)
            .OnEntry(ctx => Logger.LogInformation("Starting guild activities"))
            .PermitDynamic(Trigger.GuildingEnded, DecideNextActiveState);

    void ConfigureChatting() =>
        _sm.Configure(BotActivity.Chatting)
            .OnEntry(ctx => Logger.LogInformation("Starting chat interaction"))
            .PermitDynamic(Trigger.ChattingEnded, DecideNextActiveState);

    void ConfigureHelping() =>
        _sm.Configure(BotActivity.Helping)
            .OnEntry(ctx => Logger.LogInformation("Starting helping activity"))
            .PermitDynamic(Trigger.HelpingEnded, DecideNextActiveState);

    void ConfigureMailing() =>
        _sm.Configure(BotActivity.Mailing)
            .OnEntry(ctx => Logger.LogInformation("Starting mail management"))
            .PermitDynamic(Trigger.MailingEnded, DecideNextActiveState);

    void ConfigurePartying() =>
        _sm.Configure(BotActivity.Partying)
            .OnEntry(ctx => Logger.LogInformation("Starting party activities"))
            .PermitDynamic(Trigger.PartyEnded, DecideNextActiveState);

    void ConfigureRolePlaying() =>
        _sm.Configure(BotActivity.RolePlaying)
            .OnEntry(ctx => Logger.LogInformation("Starting roleplay"))
            .PermitDynamic(Trigger.RolePlayEnded, DecideNextActiveState);

    void ConfigureCombat() =>
        _sm.Configure(BotActivity.Combat)
            .OnEntry(ctx => Logger.LogInformation("Starting combat"))
            .PermitDynamic(Trigger.CombatEnded, DecideNextActiveState);

    void ConfigureBattlegrounding() =>
        _sm.Configure(BotActivity.Battlegrounding)
            .OnEntry(ctx => Logger.LogInformation("Starting battleground"))
            .PermitDynamic(Trigger.BattlegroundEnded, DecideNextActiveState);

    void ConfigureDungeoning() =>
        _sm.Configure(BotActivity.Dungeoning)
            .OnEntry(ctx => Logger.LogInformation("Starting dungeon"))
            .PermitDynamic(Trigger.DungeonEnded, DecideNextActiveState);

    void ConfigureRaiding() =>
        _sm.Configure(BotActivity.Raiding)
            .OnEntry(ctx => Logger.LogInformation("Starting raid"))
            .PermitDynamic(Trigger.RaidEnded, DecideNextActiveState);

    void ConfigureWorldPvPing() =>
        _sm.Configure(BotActivity.WorldPvPing)
            .OnEntry(ctx => Logger.LogInformation("Starting world PvP"))
            .PermitDynamic(Trigger.PvPEnded, DecideNextActiveState);

    void ConfigureCamping() =>
        _sm.Configure(BotActivity.Camping)
            .OnEntry(ctx => Logger.LogInformation("Starting camping"))
            .PermitDynamic(Trigger.CampingEnded, DecideNextActiveState);

    void ConfigureAuction() =>
        _sm.Configure(BotActivity.Auction)
            .OnEntry(ctx => Logger.LogInformation("Starting auction house"))
            .PermitDynamic(Trigger.AuctionEnded, DecideNextActiveState);

    void ConfigureBanking() =>
        _sm.Configure(BotActivity.Banking)
            .OnEntry(ctx => Logger.LogInformation("Starting banking"))
            .PermitDynamic(Trigger.BankingEnded, DecideNextActiveState);

    void ConfigureVending() =>
        _sm.Configure(BotActivity.Vending)
            .OnEntry(ctx => Logger.LogInformation("Starting vending"))
            .PermitDynamic(Trigger.VendingEnded, DecideNextActiveState);

    void ConfigureExploring() =>
        _sm.Configure(BotActivity.Exploring)
            .OnEntry(ctx => Logger.LogInformation("Starting exploration"))
            .PermitDynamic(Trigger.ExploringEnded, DecideNextActiveState);

    void ConfigureTraveling() =>
        _sm.Configure(BotActivity.Traveling)
            .OnEntry(ctx => Logger.LogInformation("Starting travel"))
            .PermitDynamic(Trigger.TravelEnded, DecideNextActiveState);

    void ConfigureEscaping() =>
        _sm.Configure(BotActivity.Escaping)
            .OnEntry(ctx => Logger.LogInformation("Starting escape"))
            .PermitDynamic(Trigger.EscapeSucceeded, DecideNextActiveState)
            .PermitDynamic(Trigger.EscapeFailed, DecideNextActiveState);

    void ConfigureEventing() =>
        _sm.Configure(BotActivity.Eventing)
            .OnEntry(ctx => Logger.LogInformation("Starting event"))
            .PermitDynamic(Trigger.EventEnded, DecideNextActiveState);

    BotActivity DecideNextActiveState()
    {
        // Logic to decide the next active state based on current conditions
        // This could involve checking health, quests, inventory, etc.
        // For now, we will just log and return Resting as a placeholder.
        Logger.LogInformation($"Deciding next activity based on current state:{Current}");

        return BotActivity.Resting; // Temporary default fallback
    }
}
