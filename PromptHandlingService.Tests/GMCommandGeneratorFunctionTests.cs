using PromptHandlingService.Predefined.GMCommands;

namespace PromptHandlingService.Tests
{
    public class GMCommandGeneratorFunctionTests(OllamaPromptRunnerFixture fixture) : IClassFixture<OllamaPromptRunnerFixture>
    {
        private readonly IPromptRunner _ollamaPromptRunner = fixture.OllamaPromptRunner;

        public async Task GetGMCommands_LevelUpCommand_ReturnsCorrectGMCommand()
        {
            // Create a sample CharacterDescription object for testing
            var gmRequest = new GMCommandConstructionFunction.GMCommandContext
            {
                Command = "Set my level to 60."
            };

            // Act
            var result = await GMCommandConstructionFunction.GetGMCommand(_ollamaPromptRunner, gmRequest, CancellationToken.None);

            // Assert
            Assert.Equal(".character level [$playername] 60", result);
        }

        public async Task GetGMCommands_SetCharacterMoney_ReturnsMoneyCommand()
        {
            // Arrange
            var gmRequest = new GMCommandConstructionFunction.GMCommandContext
            {
                Command = "Give me 1 million gold."
            };

            // Act
            var result = await GMCommandConstructionFunction.GetGMCommand(_ollamaPromptRunner, gmRequest, CancellationToken.None);

            // Assert
            Assert.Equal(".modify money 1000000", result);
        }

        public async Task GetGMCommands_InvalidCommand_ReturnsErrorMessage()
        {
            // Arrange
            var gmRequest = new GMCommandConstructionFunction.GMCommandContext
            {
                Command = "Turn me into a dragon!"
            };

            // Act
            var result = await GMCommandConstructionFunction.GetGMCommand(_ollamaPromptRunner, gmRequest, CancellationToken.None);

            // Assert
            Assert.Equal("Error: Invalid GM Command", result);
        }

        public async Task GetGMCommands_LearnAllSpells_ReturnsLearnAllMyClassCommand()
        {
            // Arrange
            var gmRequest = new GMCommandConstructionFunction.GMCommandContext
            {
                Command = "Teach me all my class spells."
            };

            // Act
            var result = await GMCommandConstructionFunction.GetGMCommand(_ollamaPromptRunner, gmRequest, CancellationToken.None);

            // Assert
            Assert.Equal(".learn all_myclass", result);
        }

        public async Task GetGMCommands_ResetTalents_ReturnsResetTalentsCommand()
        {
            // Arrange
            var gmRequest = new GMCommandConstructionFunction.GMCommandContext
            {
                Command = "Reset my talents."
            };

            // Act
            var result = await GMCommandConstructionFunction.GetGMCommand(_ollamaPromptRunner, gmRequest, CancellationToken.None);

            // Assert
            Assert.Equal(".reset talents [Playername]", result);
        }

        public async Task GetGMCommands_ModifyFactionReputation_ReturnsModifyFactionCommand()
        {
            // Arrange
            var gmRequest = new GMCommandConstructionFunction.GMCommandContext
            {
                Command = "Make me exalted with the Darnassus faction."
            };

            // Act
            var result = await GMCommandConstructionFunction.GetGMCommand(_ollamaPromptRunner, gmRequest, CancellationToken.None);

            // Assert
            Assert.Equal(".modify rep 530 exalted", result);
        }

        public async Task GetGMCommands_GoToCoordinates_ReturnsGoCommand()
        {
            // Arrange
            var gmRequest = new GMCommandConstructionFunction.GMCommandContext
            {
                Command = "Teleport me to coordinates 1000, 2000, 3000 on map 1."
            };

            // Act
            var result = await GMCommandConstructionFunction.GetGMCommand(_ollamaPromptRunner, gmRequest, CancellationToken.None);

            // Assert
            Assert.Equal(".go xyz 1000 2000 1", result);
        }
    }

    public class OllamaPromptRunnerFixture : IDisposable
    {
        public IPromptRunner OllamaPromptRunner { get; private set; }
        private readonly Uri _ollamaUri = new("http://localhost:11434");
        private const string ModelName = "llama3";

        public OllamaPromptRunnerFixture()
        {
            OllamaPromptRunner = PromptRunnerFactory.GetOllamaPromptRunner(_ollamaUri, ModelName);
        }

        // Implement IDisposable to clean up any resources if needed
        public void Dispose()
        {
            // Cleanup code if necessary
        }
    }
}