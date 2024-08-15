using PathfindingService.Models;

namespace BotRunner.Interfaces
{
    public interface IWoWGameObject : IWoWObject
    {
        string Name { get; }
        uint DisplayId { get;  }
        GOState GoState { get; }
        uint ArtKit { get; }
        uint AnimProgress { get; }
        uint Level { get; }
        uint FactionTemplate { get; }
        uint TypeId { get; }

        void Interact();
        Position GetPointBehindUnit(float distance);
    }

    public enum GOState
    {
        Active,
        Ready,
        ActiveAlternative
    }
}
