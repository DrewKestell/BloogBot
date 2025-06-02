namespace PromptHandlingService
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
