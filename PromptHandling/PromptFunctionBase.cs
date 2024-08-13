using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PromptHandling
{
    public abstract class PromptFunctionBase(IPromptRunner promptRunner) : IPromptFunction
    {
        private const string SystemUser = "System";

        private readonly IDictionary<string,object> _parameters = new Dictionary<string, object>();
        private readonly List<KeyValuePair<string, string?>> _chatHistory = new List<KeyValuePair<string, string?>>();
        // ReSharper disable once MemberCanBePrivate.Global
        protected internal IPromptRunner PromptRunner = promptRunner;

        public void SetParameter<T>([CallerMemberName] string? name = null, T? value = default)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            _parameters[name] = value ?? throw new ArgumentNullException(nameof(value));
        }

        public T GetParameter<T>([CallerMemberName] string? name = null)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (!_parameters.ContainsKey(name))
            {
                throw new KeyNotFoundException($"key {name} not found in parameters list");
            }

            return (T)_parameters[name];
        }

        public void TransferHistory(IPromptFunction transferTarget)
        {
            if (transferTarget is not PromptFunctionBase)
            {
                throw new NotImplementedException("Can only transfer from and to PromptFunctionBase");
            }

            var promptFunctionBase = transferTarget as PromptFunctionBase;
            Debug.Assert(promptFunctionBase != null, nameof(promptFunctionBase) + " != null");
            promptFunctionBase._chatHistory.Clear();
            foreach (var chat in _chatHistory)
            {
                if (chat.Key == SystemUser)
                {
                    continue;
                }

                promptFunctionBase._chatHistory.Add(chat);
            }
        }

        protected abstract string SystemPrompt { get; }
        public abstract Task CompleteAsync(CancellationToken cancellationToken);

        public async Task SaveChat(string directoryPath, string filePath = "chat.txt", CancellationToken cancellationToken = default)
        {
            if (String.IsNullOrWhiteSpace(filePath) || String.IsNullOrWhiteSpace(directoryPath))
            {
                throw new ArgumentException($"{nameof(filePath)} and {nameof(directoryPath)} cannot be null or empty");
            }

            if (!filePath.EndsWith(".txt"))
            {
                throw new ArgumentException($"{nameof(filePath)} must end with .txt");
            }

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            var prettyChatHistory = PrintChatHistory();
            await File.WriteAllTextAsync(Path.Combine(directoryPath, filePath), prettyChatHistory, cancellationToken);
            var jsonChatHistory = JsonSerializer.Serialize(_chatHistory);
            await File.WriteAllTextAsync(Path.Combine(directoryPath, filePath.Replace(".txt", ".json")), jsonChatHistory, cancellationToken);
        }

        public async Task LoadChat(string directoryPath, string filePath = "chat.txt", CancellationToken cancellationToken = default)
        {
            if (String.IsNullOrWhiteSpace(filePath) || String.IsNullOrWhiteSpace(directoryPath))
            {
                throw new ArgumentException($"{nameof(filePath)} and {nameof(directoryPath)} cannot be null or empty");
            }

            if (!filePath.EndsWith(".txt"))
            {
                throw new ArgumentException($"{nameof(filePath)} must end with .txt");
            }

            var jsonChatHistory = await File.ReadAllTextAsync(Path.Combine(directoryPath, filePath.Replace(".txt", ".json")), cancellationToken);
            var chatHistory = JsonSerializer.Deserialize<List<KeyValuePair<string, string>>>(jsonChatHistory);
            _chatHistory.Clear();
            _chatHistory.AddRange(chatHistory ?? throw new InvalidOperationException($"{nameof(chatHistory)} is null"));
        }

        protected virtual string PrintChatHistory()
        {
            var sb = new StringBuilder();
            foreach (var chat in _chatHistory)
            {
                sb.AppendLine($"{chat.Key}: {chat.Value}");
            }
            return sb.ToString();
        }

        public void AddChatMessage(string sender, string? message)
        {
            _chatHistory.Add(new KeyValuePair<string, string?>(sender, message));
        }

        public void ResetChat()
        {
            _chatHistory.Clear();
            _chatHistory.Add(new KeyValuePair<string, string?>("System", SystemPrompt));
            InitializeChat();
        }

        protected abstract void InitializeChat();

        protected bool IsChatActive => _chatHistory.Any(x => x.Key == "User" || x.Key == "Assistant");

        protected void RemoveLastChatEntry(int entriesToRemove = 1)
        {
            int totalRemoved = 0;
            while (_chatHistory.Count > 1)
            {
                _chatHistory.RemoveAt(_chatHistory.Count - 1);
                totalRemoved++;
                if (totalRemoved >= entriesToRemove)
                {
                    break;
                }
            }
        }

        public virtual void TransferChatHistory(PromptFunctionBase function)
        {
            function._chatHistory.Clear();
            function._chatHistory.AddRange(_chatHistory);
            function._chatHistory.RemoveAll(x => x.Key == "System");
            function._chatHistory.Insert(0, new KeyValuePair<string, string?>("System", function.SystemPrompt));
            function.InitializeChat();
        }

        public virtual void TransferPromptRunner(PromptFunctionBase function)
        {
            function.PromptRunner = PromptRunner;
        }

        private string? ReplaceInPrompt(string? prompt)
        {
            foreach (var parameterDescription in _parameters.Keys)
            {
                if (prompt.Contains($"{{{parameterDescription}}}"))
                {
                    prompt = prompt.Replace($"{{{parameterDescription}}}", GetParameter<string>(parameterDescription));
                }
            }
            return prompt;
        }

        protected async Task<string?> RunChatAsync(string? prompt, CancellationToken cancellationToken)
        {
            var fixedPrompt = ReplaceInPrompt(prompt);
            var index = _chatHistory.FindIndex(x => x.Key == "User" && x.Value == fixedPrompt);
            if (index > 0)
            {
                return _chatHistory[index + 1].Value;
            }
            AddChatMessage("User", fixedPrompt);
            var result = await PromptRunner.RunChatAsync(_chatHistory, cancellationToken);
            if (String.IsNullOrWhiteSpace(result))
            {
                throw new InvalidDataException("The response from the request is null or whitespace");
            }
            AddChatMessage("Assistant", result);
            return result;
        }
    }
}
