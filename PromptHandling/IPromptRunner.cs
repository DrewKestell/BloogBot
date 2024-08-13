using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromptHandling
{
    public interface IPromptRunner : IDisposable
    {
        Task<string?> RunChatAsync(IEnumerable<KeyValuePair<string, string?>> chatHistory, CancellationToken cancellationToken);

        int MaxConcurrent { get; }
    }
}
