using System.Text;

namespace PromptHandling.Predefined.CharacterSkills
{
    public class CharacterSkillPrioritizationFunction(IPromptRunner promptRunner) : PromptFunctionBase(promptRunner)
    {
        public static async Task<string> GetPrioritizedCharacterSkill(IPromptRunner promptRunner,CharacterDescription description,
            CancellationToken cancellationToken)
        {
            var characterSkillPrioritizationFunction = new CharacterSkillPrioritizationFunction(promptRunner)
                {
                    CharacterDescriptor = description
                };
            await characterSkillPrioritizationFunction.CompleteAsync(cancellationToken);
            return characterSkillPrioritizationFunction.PrioritizedSkill;
        }

        public class CharacterDescription
        {
            public List<string> Skills { get; set; } = [];

            public string ClassName { get; set; } = string.Empty;

            public string Race { get; set; } = string.Empty;

            public int Level { get; set; }

            public override string ToString()
            {
                StringBuilder sb = new();
                sb.AppendLine($"Class - {ClassName}");
                sb.AppendLine($"Race - {Race}");
                sb.AppendLine($"Level - {Level}");

                if (Skills != null && Skills.Count > 0)
                {
                    sb.AppendLine("Skills:");
                    foreach (var skill in Skills)
                    {
                        sb.AppendLine($"- {skill}");
                    }
                }
                else
                {
                    sb.AppendLine("Skills: None");
                }

                return sb.ToString();
            }
        }

        public CharacterDescription CharacterDescriptor
        {
            get => GetParameter<CharacterDescription>();
            set
            {
                SetParameter(value: value);
                ResetChat();
            }
        }

        private string? _prioritizedSkill;

        // ReSharper disable once MemberCanBePrivate.Global
        public string PrioritizedSkill => _prioritizedSkill ?? throw new NullReferenceException("The Prioritized Skill has not been set. Call 'CompleteAsync' to set the skill");


        protected override string SystemPrompt => "You are a World of Warcraft character skill prioritizer. " +
                                                  "You are able to determine which skills are important to a character in world of warcraft. " +
                                                  "You are able to identify which skill is most valuable to the character.";


        public override async Task CompleteAsync(CancellationToken cancellationToken)
        {
            _prioritizedSkill = await RunChatAsync("Given the following character, what skill should it focus on next?\r\n" +
                                                    $"{{{nameof(CharacterDescriptor)}}}\r\n\r\n" +
                                                    $"Output only the name of the skill to prioritize. No yappin.", cancellationToken);
        }

        protected override void InitializeChat()
        {
        }
    }
}
