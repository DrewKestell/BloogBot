using System;
using System.Threading;
using System.Threading.Tasks;

namespace RaidLeaderBot.Utilities
{
    public class RetryWithTimeout
    {
        private readonly static TimeSpan DefaultTimeout = TimeSpan.FromSeconds(5);
        private readonly static TimeSpan DefaultStepTimeSpan = TimeSpan.FromSeconds(1);

        private readonly Func<bool> _function;
        private readonly TimeSpan _timeout;
        private readonly TimeSpan _stepTimeSpan;

        public RetryWithTimeout(Func<bool> function, TimeSpan? timeout = null, TimeSpan? stepTimeSpan = null)
        {
            _function = function ?? throw new ArgumentNullException(nameof(function));
            _timeout = timeout ?? DefaultTimeout;
            _stepTimeSpan = stepTimeSpan ?? DefaultStepTimeSpan;
        }

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

    public class RetryWithTimeoutAsync
    {
        private readonly static TimeSpan DefaultTimeout = TimeSpan.FromSeconds(5);
        private readonly static TimeSpan DefaultStepTimeSpan = TimeSpan.FromSeconds(1);

        private readonly Func<Task<bool>> _function;
        private readonly TimeSpan _timeout;
        private readonly TimeSpan _stepTimeSpan;

        public RetryWithTimeoutAsync(Func<Task<bool>> function, TimeSpan? timeout = null, TimeSpan? stepTimeSpan = null)
        {
            _function = function ?? throw new ArgumentNullException(nameof(function));
            _timeout = timeout ?? DefaultTimeout;
            _stepTimeSpan = stepTimeSpan ?? DefaultStepTimeSpan;
        }

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
