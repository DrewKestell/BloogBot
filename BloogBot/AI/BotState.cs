namespace BloogBot.AI
{
    public abstract class BotState
    {
        public BotState() { 
            ShouldRun = true;
        }
        public bool ShouldRun;
    }
}
