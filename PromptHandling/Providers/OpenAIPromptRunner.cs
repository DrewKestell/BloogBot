using Azure.AI.OpenAI;
using PromptHandling;

namespace TWI.ClinicalTranslator.SharedGenerators.PromptRunners.Providers
{
    class OpenAIPromptRunner(OpenAIClient client, string deploymentName) : IPromptRunner
    {
        public async Task<string?> RunChatAsync(IEnumerable<KeyValuePair<string, string?>> chatHistory, CancellationToken cancellationToken)
        {
            ChatCompletionsOptions chatCompletionOptions = new()
            {
                DeploymentName = deploymentName,
                MaxTokens = 1000
            };
            foreach (var (role, message) in chatHistory)
            {
                if (role == "User")
                {
                    chatCompletionOptions.Messages.Add(new ChatRequestUserMessage(message));
                }
                else
                {
                    chatCompletionOptions.Messages.Add(new ChatRequestAssistantMessage(message));
                }
            }
            return (await client.GetChatCompletionsAsync(chatCompletionOptions, cancellationToken)).Value.Choices[0].Message
                .Content;
        }

        public int MaxConcurrent => 50;

        public void Dispose()
        {
            // Dispose of nothing because there is nothing to dispose of
        }
    }
}
