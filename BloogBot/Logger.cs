using System;

namespace BloogBot
{
    static public class Logger
    {
        static BotSettings botSettings;

        static public void Initialize(BotSettings parBotSettings)
        {
            botSettings = parBotSettings;
        }

        static public void Log(object message) => Console.WriteLine(message);

        static public void LogVerbose(object message)
        {
            if (botSettings.UseVerboseLogging)
                Console.WriteLine(message);
        }
    }
}
