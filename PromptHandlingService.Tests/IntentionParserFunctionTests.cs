using Communication;
using PromptHandlingService.Predefined.IntentParser;
using PromptHandlingService.Providers;

namespace PromptHandlingService.Tests
{
    public class IntentionParserFunctionTests
    {
        private readonly Uri _ollamaUri = new("http://localhost:11434");
        private const string ModelName = "deepseek-r1";

        public async Task ParsePromptIntent_GMCommand_ReturnsCorrectHandOffString()
        {
            // Arrange
            var ollamaPromptRunner = PromptRunnerFactory.GetOllamaPromptRunner(_ollamaUri, ModelName);
            var testRequest = new IntentionParserFunction.UserRequest
            {
                Request = "I want to run Molten Core.",
                ActivitySnapshot = new ActivitySnapshot()
                {
                    Player = new Game.WoWPlayer()
                    {
                        Unit = new Game.WoWUnit()
                        {
                            GameObject = new Game.WoWGameObject()
                            {
                                Base = new Game.WoWObject()
                                {
                                    Guid = 150,
                                    MapId = 0,
                                    Position = new Game.Position()
                                    {
                                        X = 0,
                                        Y = 0,
                                        Z = 0
                                    }
                                }
                            }
                        }
                    }
                }
            };

            // Act
            var result = await IntentionParserFunction.ParsePromptIntent(ollamaPromptRunner, testRequest, CancellationToken.None);

            // Assert
            Assert.Equal("Send to GMCommandRunner: I want to run Molten Core", result);
        }

        public async Task ParsePromptIntent_MechanicsExplanation_ReturnsCorrectHandOffString()
        {
            // Arrange
            var ollamaPromptRunner = new OllamaPromptRunner(_ollamaUri, ModelName);
            var testRequest = new IntentionParserFunction.UserRequest
            {
                Request = "Explain how threat works in World of Warcraft"
            };

            // Act
            var result = await IntentionParserFunction.ParsePromptIntent(ollamaPromptRunner, testRequest, CancellationToken.None);

            // Assert
            Assert.Equal("Send to MechanicsExplainerRunner: Explain how threat works in World of Warcraft", result);
        }

        public async Task ParsePromptIntent_DataQuery_ReturnsCorrectHandOffString()
        {
            // Arrange
            var ollamaPromptRunner = new OllamaPromptRunner(_ollamaUri, ModelName);
            var testRequest = new IntentionParserFunction.UserRequest
            {
                Request = "Can you teleport me to Orgrimmar?"
            };

            // Act
            var result = await IntentionParserFunction.ParsePromptIntent(ollamaPromptRunner, testRequest, CancellationToken.None);

            // Assert
            Assert.Equal("Send to DataQueryRunner: Fetch stats for item 12345", result);
        }

        public async Task ParsePromptIntent_Miscellaneous_ReturnsCorrectHandOffString()
        {
            // Arrange
            var ollamaPromptRunner = new OllamaPromptRunner(_ollamaUri, ModelName);
            var testRequest = new IntentionParserFunction.UserRequest
            {
                Request = "How do I organize a guild event?"
            };

            // Act
            var result = await IntentionParserFunction.ParsePromptIntent(ollamaPromptRunner, testRequest, CancellationToken.None);

            // Assert
            Assert.Equal("Send to MiscellaneousRequestRunner: How do I organize a guild event?", result);
        }
    }
}