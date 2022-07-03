using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using static System.Environment;

namespace Pororoca.TestServer.Configurations.Filters;

public class LogActionFilter : IAsyncActionFilter
{
    private readonly ILogger<LogActionFilter> logger;

    public LogActionFilter(ILogger<LogActionFilter> logger) => this.logger = logger;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        LogRequest(context);

        var response = await next();

        stopwatch.Stop();
        LogResponse(context.HttpContext, response, stopwatch.Elapsed.TotalMilliseconds);
    }

    public void LogRequest(ActionExecutingContext context)
    {
        if (this.logger.IsEnabled(LogLevel.Debug))
            this.logger.LogInformation("{method} {endpoint}" + NewLine + "{@args}", context.HttpContext.Request.Method, GetPathWithQueryParams(context.HttpContext.Request), context.ActionArguments);
        else
            this.logger.LogInformation("{method} {endpoint}", context.HttpContext.Request.Method, GetPathWithQueryParams(context.HttpContext.Request));

    }

    public void LogResponse(HttpContext context, ActionExecutedContext actionExecutedContext, double durationInMs)
    {
        int? statusCode = null;
        if (actionExecutedContext.Result is IStatusCodeActionResult statusCodeResult)
        {
            statusCode = statusCodeResult.StatusCode;
        }
        statusCode ??= context.Response.StatusCode;

        if (this.logger.IsEnabled(LogLevel.Debug))
            this.logger.LogInformation("{statusCode} {method} {endpoint} ({durationInMs}ms)" + NewLine + "{@result}", statusCode, context.Request.Method, GetPathWithQueryParams(context.Request), durationInMs, actionExecutedContext.Result);
        else
            this.logger.LogInformation("{statusCode} {method} {endpoint} ({durationInMs}ms)", statusCode, context.Request.Method, GetPathWithQueryParams(context.Request), durationInMs);
    }

    private static string GetPathWithQueryParams(HttpRequest req) =>
        $"{req.Path}{req.QueryString}";
}