namespace Presentation.ExceptionHandlers;

public static class ExceptionHandlingExtensions
{
    public static IApplicationBuilder UseApplicationExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
