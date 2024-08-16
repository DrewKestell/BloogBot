namespace PromptHandling
{
    public interface IPromptRunner : IDisposable
    {
        Task<string?> RunChatAsync(IEnumerable<KeyValuePair<string, string?>> chatHistory, CancellationToken cancellationToken);

        int MaxConcurrent { get; }
    }
}
