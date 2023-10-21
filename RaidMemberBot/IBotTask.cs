namespace RaidMemberBot.AI
{
    public interface IBotTask
    {
        public TaskType TaskType { get; }
        void Update();
    }
}
