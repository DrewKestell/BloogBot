using BotRunner.Constants;

namespace BotRunner.Interfaces
{
    public interface IWoWPlayer : IWoWUnit
    {
        Race Race { get; }
        Class Class { get; }
        bool IsDrinking { get; }
        bool IsEating { get; }
    }
}
