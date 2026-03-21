using Microsoft.Extensions.Logging;

namespace http_server.helpers;

public class Logger : ILogger
{
    private LogLevel _logLevel = LogLevel.Information;

    public Logger()
    {
    }
    
    private Logger(LogLevel logLevel)
    {
        _logLevel = logLevel;
    }
    
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception exception,
        Func<TState, Exception, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        string message = formatter != null
            ? formatter(state, exception)
            : state?.ToString() ?? string.Empty;

        Console.WriteLine($"{DateTime.UtcNow:O} [{logLevel}] {message}");

        if (exception != null)
        {
            Console.WriteLine(exception);
        }
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel != LogLevel.None && logLevel >= _logLevel;
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        throw new NotImplementedException();
    }
}