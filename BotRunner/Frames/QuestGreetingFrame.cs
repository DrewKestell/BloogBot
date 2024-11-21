namespace BotRunner.Frames
{
    public interface IQuestGreetingFrame
    {
        bool IsOpen { get; }
        void Close();
        void AcceptQuest(int parId);
        void CompleteQuest(int parId);
        List<QuestOption> Quests { get; }
    }
}
