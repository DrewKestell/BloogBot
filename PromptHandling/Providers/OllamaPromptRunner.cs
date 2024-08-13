using OllamaSharp;
using OllamaSharp.Models.Chat;
using PromptHandling;

namespace TWI.ClinicalTranslator.SharedGenerators.PromptRunners.Providers
{
    class OllamaPromptRunner : IPromptRunner
    {
        private OllamaApiClient _ollamaApiClient;

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
                ChatRole role;
                switch (message.Key)
                {
                    case "User":
                        role = ChatRole.User;
                        break;
                    case "Assistant":
                        role = ChatRole.Assistant;
                        break;
                    case "System":
                        role = ChatRole.System;
                        break;
                    default:
                        throw new ArgumentException($"Invalid chat role {message.Key}");
                }

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
