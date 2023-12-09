using System.Net.Mime;
using DiscordBotProject;
using Microsoft.AspNetCore.Diagnostics;
using Overlord.StatusExceptions;

namespace WebProject;

public class ExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var consoleMessage = $"[{httpContext.Request.Path}]: ($STATUS_ID$) {exception.Message}";
        if (exception is StatusException statusException)
        {
            consoleMessage = consoleMessage.Replace("$STATUS_ID$", statusException.StatusNumber.ToString());
            if (statusException.StatusNumber is >= 400 and < 500)
            {
                ConsoleWriter.WriteWarningLn(consoleMessage);
            }
            else
            {
                ConsoleWriter.WriteDangerLn(consoleMessage);
            }

            httpContext.Response.ContentType = MediaTypeNames.Text.Plain;
            httpContext.Response.StatusCode = statusException.StatusNumber;
            await httpContext.Response.WriteAsync(statusException.Message, cancellationToken);
        }
        else
        {
            consoleMessage = consoleMessage.Replace("$STATUS_ID$", "500");
            ConsoleWriter.WriteDangerLn(consoleMessage);
            if (exception.InnerException != null)
                ConsoleWriter.WriteDangerLn($"Прикладываю внутреннюю ошибку:\n{exception.InnerException}");

            httpContext.Response.ContentType = MediaTypeNames.Text.Plain;
            httpContext.Response.StatusCode = 500;
            await httpContext.Response.WriteAsync(
                $"Необработанная ошибка! Скажите Илье, что он наложал!", cancellationToken);
        }
        
        await httpContext.Response.BodyWriter.FlushAsync(cancellationToken);
        return true;
    }
}