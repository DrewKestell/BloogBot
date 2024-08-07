using ActivityForegroundMember.Game.Statics;

namespace ActivityForegroundMember
{
    public class Program
    {
        private static BotRunner.BotRunner _botRunner;
        public static void Main(string[] args)
        {
            _botRunner = new(new ObjectManager(), WoWEventHandler.Instance);
        }
    }
}
