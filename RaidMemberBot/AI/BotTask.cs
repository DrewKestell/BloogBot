namespace RaidMemberBot.AI
{
    public abstract class BotTask
    {
        public BotTask() { 
            ShouldRun = true;
        }
        public bool ShouldRun;
    }
}
