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

        ConfigureResting();
        ConfigureQuesting();
        // … repeat for each activity
    }

    void ConfigureResting() =>
        _sm.Configure(BotActivity.Resting)
            .PermitDynamic(Trigger.HealthRestored, DecideNextActiveState);

    void ConfigureQuesting() =>
        _sm.Configure(BotActivity.Questing)
            .OnEntry(ctx => Logger.LogInformation("Starting quest"))
            .Permit(Trigger.LowHealth, BotActivity.Resting);

    BotActivity DecideNextActiveState() =>
        ObjectManager.Player.InBattleground ? BotActivity.Battlegrounding
        : ObjectManager.Player.HasQuestTargets ? BotActivity.Questing
        : BotActivity.Grinding;
}

internal enum Trigger
{
    HealthRestored,
    LowHealth,
}
