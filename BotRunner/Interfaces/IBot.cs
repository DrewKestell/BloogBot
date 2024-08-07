using Communication;

namespace BotRunner.Interfaces
{
    public interface IBot
    {
        string Name { get; }

        string FileName { get; }

        IClassContainer GetClassContainer(ActivityMemberState probe);

        IBotTask CreateRestTask(IBotContext botContext);

        IBotTask CreateBuffTask(IBotContext botContext);

        IBotTask CreateMoveToTargetTask(IBotContext botContext);

        IBotTask CreatePvERotationTask(IBotContext botContext);

        IBotTask CreatePvPRotationTask(IBotContext botContext);
    }
}
