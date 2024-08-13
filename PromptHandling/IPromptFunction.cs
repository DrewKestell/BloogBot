using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromptHandling
{
    public interface IPromptFunction
    {
        Task<T> RunAsync<T>(CancellationToken cancellationToken);

        void ResetChat();

        void SetParameter<T>(T  parameter, string name);

        T GetParameter<T>(string name);

        void TransferHistory(IPromptFunction transferTarget);
    }
}
