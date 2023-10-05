using RaidMemberBot.Game;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System;
using System.Threading.Tasks;

namespace RaidMemberBot.Helpers
{
#if DEBUG
    internal static class DebugAssist
    {
        private static bool Applied;

        internal static void Init()
        {
            if (Applied) return;
            Task.Run(() => ConsoleReader());
            Applied = true;
        }

        private static readonly Transport Transport;

        private static Location loc1;

        private static void ConsoleReader()
        {
            while (true)
                try
                {
                    var input = Console.ReadLine();
                    if (input != null)
                    {
                        if (input == "test")
                        {
                            Console.WriteLine(ObjectManager.Instance.Player.HasPet);
                        }
                    }
                    Task.Delay(10).Wait();
                }
                catch (Exception)
                {
                    // ignored
                }
            // ReSharper disable once FunctionNeverReturns
        }

        private static int DoSomething()
        {
            return 10;
        }
    }
#endif
}
