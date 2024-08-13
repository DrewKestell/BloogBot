using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromptHandling
{
    public interface IPromptFunction
    {
        Task CompleteAsync(CancellationToken cancellationToken);

        void ResetChat();

        void SetParameter<T>(string? name = null, T? value = default);

        T GetParameter<T>(string name);

        void TransferHistory(IPromptFunction transferTarget);
    }
}
