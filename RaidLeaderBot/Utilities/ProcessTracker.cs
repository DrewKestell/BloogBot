using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static RaidLeaderBot.WinImports;

namespace RaidLeaderBot.Utilities
{
    public class ProcessTracker : IDisposable
    {
        private List<IntPtr> _processHandles = new List<IntPtr>();
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
                RetryWithTimeout closeHandlesRetry = new RetryWithTimeout(() =>
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
