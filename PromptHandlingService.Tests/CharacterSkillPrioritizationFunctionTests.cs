using BotRunner.Constants;
using PromptHandling.Predefined.CharacterSkills;

namespace PromptHandlingService.Tests
{
    public class CharacterSkillPrioritizationFunctionTests
    {
        private readonly Uri _ollamaUri = new("http://localhost:11434");
        private const string ModelName = "deepseekr1:14b";

        [Fact]
        public async Task GetPrioritizedCharacterSkill_ValidCharacterDescription_ReturnsPrioritizedSkill()
        {
            // Arrange
            var ollamaPromptRunner = PromptRunnerFactory.GetOllamaPromptRunner(_ollamaUri, ModelName);

            // Create a sample CharacterDescription object for testing
            var characterDescription = new CharacterSkillPrioritizationFunction.CharacterDescription
            {
                ClassName = Class.Warrior.ToString(),
                Race = Race.Orc.ToString(),
                Level = 60,
                Skills = ["Charge", "Heroic Strike", "Mortal Strike"]
            };

            // Act
            var result = await CharacterSkillPrioritizationFunction.GetPrioritizedCharacterSkill(ollamaPromptRunner, characterDescription, CancellationToken.None);

            // Assert
            Assert.NotNull(result); // Ensure that a skill is returned
            Assert.Contains(result, characterDescription.Skills); // Check that the result is one of the character's spells
        }
    }
}