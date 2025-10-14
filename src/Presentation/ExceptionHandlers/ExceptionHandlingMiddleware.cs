using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.ExceptionHandlers;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, IEnumerable<IExceptionHandler> handlers)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            if (context.Response.HasStarted)
                throw;

            var ct = context.RequestAborted;
            foreach (var handler in handlers)
            {
                if (await handler.TryHandleAsync(context, ex, ct))
                    return;
            }

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";
            var pd = new ProblemDetails
            {
                Title = "An unexpected error occurred.",
                Status = StatusCodes.Status500InternalServerError
            };
            await context.Response.WriteAsJsonAsync(pd, ct);
        }
    }
}
