using Microsoft.Extensions.Logging;

namespace StateManagerUI
{
    public class BasicLogger(Action<string> logMessage, string name) : ILogger
    {
        private readonly Action<string> _logMessage = logMessage;
        private readonly string _name = name;

        public IDisposable BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            string message = formatter(state, exception);
            if (!string.IsNullOrEmpty(message))
            {
                _logMessage?.Invoke($"{DateTime.Now} [{logLevel}] {_name}: {message}");
            }

            if (exception != null)
            {
                _logMessage?.Invoke($"{DateTime.Now} [{logLevel}] {_name}: {exception}");
            }
        }
    }
}