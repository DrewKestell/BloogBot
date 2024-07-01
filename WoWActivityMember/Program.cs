using WoWActivityMember.Tasks;

namespace WoWActivityMember
{
    public class Program
    {
        private static BotRunner _botRunner;
        public static void Main(string[] args)
        {
            _botRunner = new();
        }
    }
}
