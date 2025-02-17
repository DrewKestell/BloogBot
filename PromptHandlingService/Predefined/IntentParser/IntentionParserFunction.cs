using Communication;
using Newtonsoft.Json;

namespace PromptHandlingService.Predefined.IntentParser
{
    public class IntentionParserFunction(IPromptRunner promptRunner) : PromptFunctionBase(promptRunner)
    {
        public class UserRequest
        {
            public string Request { get; set; } = string.Empty;
            public ActivitySnapshot ActivitySnapshot { get; set; } = new ActivitySnapshot();

            public override string ToString()
            {
                return $"User Request: {Request} ";
            }
        }

        public static async Task<string> ParsePromptIntent(IPromptRunner promptRunner, UserRequest userRequest,
            CancellationToken cancellationToken)
        {
            var overallPromptIntentionParserFunction = new IntentionParserFunction(promptRunner)
            {
                UserRequestDescriptor = userRequest
            };
            await overallPromptIntentionParserFunction.CompleteAsync(cancellationToken);
            return overallPromptIntentionParserFunction.ParsedIntent;
        }

        public UserRequest UserRequestDescriptor
        {
            get => GetParameter<UserRequest>();
            set
            {
                SetParameter(value: value);
                ResetChat();
            }
        }

        private string? _parsedIntent;

        // ReSharper disable once MemberCanBePrivate.Global
        public string ParsedIntent => _parsedIntent ?? throw new NullReferenceException("The parsed intent has not been set. Call 'CompleteAsync' to set the intent");

        // Detailed system prompt with hand-off instructions
        protected override string SystemPrompt => "";

        public override async Task CompleteAsync(CancellationToken cancellationToken)
        {
            _parsedIntent = await RunChatAsync("Think like a World of Warcraft GM. Your purpose is to interpret the request given to you by players so that you can fulfill their needs.You have access to execute all of the GM commands necessary to help players. You can also access all of the tables within the games database if that actions would help. Given the following user request, what are the actions that should be taken? \"" + UserRequestDescriptor.Request + "\"", cancellationToken);
        }

        protected override void InitializeChat()
        {
        }
    }
}
