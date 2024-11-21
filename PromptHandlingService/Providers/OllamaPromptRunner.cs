using OllamaSharp;
using OllamaSharp.Models.Chat;
using OllamaSharp.Models.Exceptions;
using System.Text;

namespace PromptHandlingService.Providers
{
    public class OllamaPromptRunner(Uri uri, string model) : IPromptRunner, IDisposable
    {
        private readonly OllamaApiClient _ollamaClient = new(uri);
        private readonly string _model = model;

        public async Task<string?> RunChatAsync(IEnumerable<KeyValuePair<string, string?>> chatHistory, CancellationToken cancellationToken)
        {
            var chatMessages = chatHistory.Select(message => new OllamaSharp.Models.Chat.Message
            {
                Role = message.Key switch
                {
                    "User" => ChatRole.User,
                    "Assistant" => ChatRole.Assistant,
                    "System" => ChatRole.System,
                    _ => throw new ArgumentException($"Invalid chat role {message.Key}")
                },
                Content = message.Value
            }).ToList();

            var chatRequest = new ChatRequest
            {
                Model = _model,
                Messages = chatMessages,
                Stream = true // Enable streaming
            };

            try
            {
                StringBuilder stringBuilder = new();
                await foreach (var chatResponseStream in _ollamaClient.Chat(chatRequest, cancellationToken))
                {
                    if (chatResponseStream != null && chatResponseStream.Message != null)
                    {
                        stringBuilder.Append(chatResponseStream.Message.Content);
                    }
                }

                return stringBuilder.ToString();
            }
            catch (OllamaException ex)
            {
                // Log or handle the API-specific error here
                return $"Error: {ex.Message}";
            }
            catch (Exception ex)
            {
                // General exception handling
                return $"Unexpected error: {ex.Message}";
            }
        }


        public int MaxConcurrent => 1;

        public void Dispose()
        {

        }
    }
}
