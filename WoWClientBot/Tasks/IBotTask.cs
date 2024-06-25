namespace WoWClientBot.AI
{
    public interface IBotTask
    {
        public TaskType TaskType { get; }
        void Update();
    }
}
