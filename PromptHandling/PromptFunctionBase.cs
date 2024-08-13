using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromptHandling
{
    public abstract class PromptFunctionBase(IPromptRunner promptRunner) : IPromptFunction
    {
        private const string SystemUser = "System";

        private readonly IDictionary<string,object> _parameters = new Dictionary<string, object>();
        private readonly ICollection<KeyValuePair<string, string>> _chatHistory = new List<KeyValuePair<string, string>>();
        protected internal readonly IPromptRunner PromptRunner = promptRunner;

        public virtual void ResetChat()
        {
            _chatHistory.Clear();
            _chatHistory.Add(new KeyValuePair<string, string>(SystemUser, SystemPrompt));
        }

        public void SetParameter<T>(T parameter, string name)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            _parameters[name] = parameter;
        }

        public T GetParameter<T>(string name)
        {
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
        public abstract Task<T> RunAsync<T>(CancellationToken cancellationToken);
    }
}
