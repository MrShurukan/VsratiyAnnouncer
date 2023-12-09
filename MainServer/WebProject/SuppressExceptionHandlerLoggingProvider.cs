using Microsoft.Extensions.Logging.Abstractions;

namespace WebProject;

public class SuppressExceptionHandlerLoggingProvider(ILoggerFactory loggerFactory) 
    : ILoggerProvider
{
    private readonly ILogger _consoleLogger = loggerFactory.CreateLogger("ConsoleLogger");
    public ILogger CreateLogger(string categoryName)
    {
        if (categoryName == "Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddleware")
        {
            return NullLogger.Instance;
        }
        
        return _consoleLogger;
    }

    public void Dispose() {}
}
