#region using
using Diginsight.Diagnostics;
using Microsoft.AspNetCore.Http;

//using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
#endregion

namespace Diginsight.Components;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        this.logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        using var activity = Observability.ActivitySource.StartMethodActivity(logger);

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var action = context.Request.Method;
            var uri = context.Request.Path;
            var message = $"Unhandled exception '{ex.GetType().Name}' occurred processing request {action} {uri}.";
            logger.LogError(ex, message);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync(message);
        }
    }
}
