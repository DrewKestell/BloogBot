namespace BloogBot.AI
{
    public abstract class BotTask
    {
        public BotTask() { 
            ShouldRun = true;
        }
        public bool ShouldRun;
    }
}
