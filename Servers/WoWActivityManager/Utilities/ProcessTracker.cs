namespace WoWActivityManager.Utilities
{
    public class ProcessTracker : IDisposable
    {
        private readonly List<IntPtr> _processHandles = [];
        private bool _disposed;

        public void AddProcess(IntPtr processHandle)
        {
            _processHandles.Add(processHandle);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            foreach (var handle in _processHandles)
            {
                RetryWithTimeout closeHandlesRetry = new(() =>
                {
                    try
                    {
                        CloseProcess(handle);
                        return true;
                    }
                    catch (Exception)
                    {
                        Console.WriteLine($"Failed to close handle {handle}");
                    }
                    return false;
                });
                closeHandlesRetry.ExecuteWithRetry(CancellationToken.None).ConfigureAwait(false);
            }
        }
    }
}
