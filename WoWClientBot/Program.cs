using WoWClientBot.AI;

namespace WoWClientBot
{
    public class Program
    {
        static BotRunner _botRunner;
        public static void Main(string[] args)
        {
            Console.WriteLine(args.Length);

            _botRunner = new(args[0], args[1], 
                args[2], int.Parse(args[3]), 
                args[4], int.Parse(args[5]));
        }
    }
}
