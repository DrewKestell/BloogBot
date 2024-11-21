namespace PromptHandlingService.Providers
{
    class FakePromptRunner : IPromptRunner
    {
        public Task<string?> RunChatAsync(IEnumerable<KeyValuePair<string, string?>> chatHistory, CancellationToken cancellationToken)
        {
            return Task.FromResult("{ nothin }");
        }

        public int MaxConcurrent => 100;

        public void Dispose()
        {
            //Dispose of nothing as there is nothing to dispose
        }
    }
}
