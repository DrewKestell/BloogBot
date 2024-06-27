namespace WoWActivityMember.Tasks
{
    public interface IBotTask
    {
        public TaskType TaskType { get; }
        void Update();
    }
}
