using Azure.AI.OpenAI;
using TWI.ClinicalTranslator.SharedGenerators.PromptRunners.Providers;

namespace PromptHandling
{
    public static class PromptRunnerFactory
    {
        public static IPromptRunner GetAzureOpenAiPromptRunner(Uri baseUri, string apiKey) =>
            new AzureAIPromptRunner(baseUri, apiKey);

        public static IPromptRunner GetFakePropPromptRunner() => new FakePromptRunner();

        public static IPromptRunner GetOllamaPromptRunner(Uri baseUri, string model) => new OllamaPromptRunner(baseUri, model);

        public static IPromptRunner GetOpenAiPromptRunner(OpenAIClient client, string deploymentName) =>
            new OpenAIPromptRunner(client, deploymentName);
    }
}
