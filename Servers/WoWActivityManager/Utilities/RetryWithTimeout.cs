namespace WoWActivityManager.Utilities
{
    public class RetryWithTimeout(Func<bool> function, TimeSpan? timeout = null, TimeSpan? stepTimeSpan = null)
    {
        private readonly static TimeSpan DefaultTimeout = TimeSpan.FromSeconds(5);
        private readonly static TimeSpan DefaultStepTimeSpan = TimeSpan.FromSeconds(1);

        private readonly Func<bool> _function = function ?? throw new ArgumentNullException(nameof(function));
        private readonly TimeSpan _timeout = timeout ?? DefaultTimeout;
        private readonly TimeSpan _stepTimeSpan = stepTimeSpan ?? DefaultStepTimeSpan;

        public async Task<bool> ExecuteWithRetry(CancellationToken cancellationToken)
        {
            var startTime = DateTime.Now;
            while (DateTime.Now - startTime < _timeout)
            {
                if (_function())
                {
                    return true;
                }
                await Task.Delay(_stepTimeSpan, cancellationToken);
            }
            return false; // Timeout reached
        }
    }

    public class RetryWithTimeoutAsync(Func<Task<bool>> function, TimeSpan? timeout = null, TimeSpan? stepTimeSpan = null)
    {
        private readonly static TimeSpan DefaultTimeout = TimeSpan.FromSeconds(5);
        private readonly static TimeSpan DefaultStepTimeSpan = TimeSpan.FromSeconds(1);

        private readonly Func<Task<bool>> _function = function ?? throw new ArgumentNullException(nameof(function));
        private readonly TimeSpan _timeout = timeout ?? DefaultTimeout;
        private readonly TimeSpan _stepTimeSpan = stepTimeSpan ?? DefaultStepTimeSpan;

        public async Task<bool> ExecuteWithRetry(CancellationToken cancellationToken)
        {
            var startTime = DateTime.Now;
            while (DateTime.Now - startTime < _timeout)
            {
                if (await _function())
                {
                    return true;
                }
                await Task.Delay(_stepTimeSpan, cancellationToken);
            }
            return false; // Timeout reached
        }
    }
}
