using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace BloogBot
{
    static public class Logger
    {
        const string StateLogFileName = "StateTransitions.log";
        const long MaxStateLogFileBytes = 10 * 1024 * 1024;

        static readonly object stateLogLock = new object();
        static readonly int processId = Process.GetCurrentProcess().Id;
        static readonly Encoding utf8NoBom = new UTF8Encoding(false);

        static BotSettings botSettings;

        static public void Initialize(BotSettings parBotSettings)
        {
            botSettings = parBotSettings;
        }

        static public void Log(object message) => Console.WriteLine(message);

        static public void LogVerbose(object message)
        {
            if (botSettings != null && botSettings.UseVerboseLogging)
                Console.WriteLine(message);
        }

        /// <summary>
        /// Appends a timestamped state transition entry to StateTransitions.log.
        /// </summary>
        static public void LogStateTransition(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            WriteStateLogLine($"{timestamp} [{processId}] {message}");
        }

        static string GetStateLogFilePath()
        {
            var location = typeof(Logger).Assembly.Location;
            var dir = !string.IsNullOrEmpty(location)
                ? Path.GetDirectoryName(location)
                : AppDomain.CurrentDomain.BaseDirectory;

            return Path.Combine(dir, StateLogFileName);
        }

        static void WriteStateLogLine(string line)
        {
            lock (stateLogLock)
            {
                try
                {
                    var logFilePath = GetStateLogFilePath();
                    var dir = Path.GetDirectoryName(logFilePath);
                    if (!string.IsNullOrEmpty(dir))
                        Directory.CreateDirectory(dir);

                    RotateStateLogIfNeeded(logFilePath);

                    using (var stream = new FileStream(logFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                    using (var writer = new StreamWriter(stream, utf8NoBom))
                    {
                        writer.WriteLine(line);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        static void RotateStateLogIfNeeded(string logFilePath)
        {
            try
            {
                var fileInfo = new FileInfo(logFilePath);
                if (!fileInfo.Exists || fileInfo.Length < MaxStateLogFileBytes)
                    return;

                var backupPath = Path.ChangeExtension(logFilePath, ".1.log");
                if (File.Exists(backupPath))
                    File.Delete(backupPath);

                File.Move(logFilePath, backupPath);
            }
            catch (IOException)
            {
                // Keep appending if another process or viewer holds a lock preventing rename.
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }
}
