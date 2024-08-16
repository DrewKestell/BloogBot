using OllamaSharp;
using OllamaSharp.Models.Chat;
using PromptHandling;

namespace TWI.ClinicalTranslator.SharedGenerators.PromptRunners.Providers
{
    class OllamaPromptRunner : IPromptRunner
    {
        private readonly OllamaApiClient _ollamaApiClient;

        public OllamaPromptRunner(Uri uri, string model)
        {
            _ollamaApiClient = new OllamaApiClient(uri)
            {
                SelectedModel = model
            };
        }

        public async Task<string?> RunChatAsync(IEnumerable<KeyValuePair<string, string?>> chatHistory, CancellationToken cancellationToken)
        {
            string? response = "";
            var chat = _ollamaApiClient.Chat(stream => { response = stream.Message.Content; });
            foreach (var message in chatHistory)
            {
                var role = message.Key switch
                {
                    "User" => ChatRole.User,
                    "Assistant" => ChatRole.Assistant,
                    "System" => ChatRole.System,
                    _ => throw new ArgumentException($"Invalid chat role {message.Key}"),
                };
                var messages = await chat.SendAs(role, message.Value, cancellationToken);
                response = messages.Last().Content;
            }
            return response;
        }

        public int MaxConcurrent => 1;

        public void Dispose()
        {
            //Dispose of nothing as there is nothing to dispose
        }
    }
}
