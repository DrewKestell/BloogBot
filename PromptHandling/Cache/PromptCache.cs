using PromptHandling.Cache.Models;
using SQLite;

namespace PromptHandling.Cache
{
    internal class PromptCache
    {
        private readonly SQLiteConnection _conn;

        public PromptCache(string sqlitePath)
        {
            _conn = new SQLiteConnection(sqlitePath);
            _conn.CreateTable<PreviousPrompts>();
        }

        public string? CheckForPreviousRun(string prompt)
        {
            var hash = prompt.GetHashCode(StringComparison.OrdinalIgnoreCase);
            var results = _conn.Query<PreviousPrompts>("select * from PreviousPrompts where PromptHash = ?", hash);
            if (results.Count <= 0)
            {
                return null;
            }

            return results.First().Response;
        }

        public void AddPrevious(string prompt, string response)
        {
            _conn.Insert(new PreviousPrompts
            {
                PromptHash = prompt.GetHashCode(StringComparison.OrdinalIgnoreCase),
                Response = response,
                TotalPrompt = prompt
            });
        }
    }
}
