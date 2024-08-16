using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using PromptHandling;

namespace TWI.ClinicalTranslator.SharedGenerators.PromptRunners.Providers
{
    class AzureAIPromptRunner : IPromptRunner
    {
        private readonly string _apiKey;
        private readonly HttpClient client = new();

        public AzureAIPromptRunner(Uri baseAddress, string apiKey)
        {
            _apiKey = apiKey;
            client.Timeout = TimeSpan.FromMinutes(1);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            client.BaseAddress = baseAddress;
        }

        public async Task<string?> RunChatAsync(IEnumerable<KeyValuePair<string, string?>> chatHistory, CancellationToken cancellationToken)
        {
            int tryCount = 0;
            do
            {
                try
                {
                    var messages = chatHistory.Select(entry => new
                    {
                        role = entry.Key.ToLowerInvariant(),
                        content = entry.Value
                    }).ToList();

                    var requestBody = new
                    {
                        messages,
                        temperature = 0.8,
                        max_tokens = 512
                    };

                    var content = new StringContent(JsonSerializer.Serialize(requestBody));
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    HttpResponseMessage response =
                        await client.PostAsync("v1/chat/completions", content, cancellationToken);

                    if (response.IsSuccessStatusCode)
                    {
                        var result =
                            await response.Content.ReadFromJsonAsync<LlamaResponse>(
                                cancellationToken: cancellationToken);
                        return result?.choices?.FirstOrDefault()?.message?.content ?? "Empty Response";
                    }
                    else
                    {
                        Console.WriteLine($"The request failed with status code: {response.StatusCode}");
                        Console.WriteLine(response.Headers.ToString());

                        string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                        Console.WriteLine(responseContent);
                        tryCount++;
                    }
                }
                catch (Exception e)
                {
                    tryCount++;
                }
            } while (tryCount < 10);

            throw new TimeoutException("Failed to run chat");
        }

        public int MaxConcurrent => 50;

        public void Dispose()
        {
            client.Dispose();
        }
    }


    public class LlamaResponse
    {
        public string id { get; set; }
        public string _object { get; set; }
        public int created { get; set; }
        public string model { get; set; }
        public Choice[] choices { get; set; }
        public Usage usage { get; set; }
    }

    public class Usage
    {
        public int prompt_tokens { get; set; }
        public int total_tokens { get; set; }
        public int completion_tokens { get; set; }
    }

    public class Choice
    {
        public int index { get; set; }
        public Message message { get; set; }
    }

    public class Message
    {
        public string role { get; set; }
        public string? content { get; set; }
    }

}
