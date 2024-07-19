namespace WoWSlimClient.Tasks
{
    public interface IBotTask
    {
        public TaskType TaskType { get; }
        void Update();
    }
}
