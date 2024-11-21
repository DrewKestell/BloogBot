using SQLite;

namespace PromptHandlingService.Cache.Models
{
    internal class PreviousPrompts
    {
        [PrimaryKey, AutoIncrement]
        public int PreviousPromptId { get; set; }
        [Indexed]
        public int PromptHash { get; set; }
        public string TotalPrompt { get; set; }
        public string Response { get; set; }
        public DateTime SaveDate { get; set; } = DateTime.Now;
    }
}
