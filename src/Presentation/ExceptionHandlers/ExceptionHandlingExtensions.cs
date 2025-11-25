namespace Presentation.ExceptionHandlers;

public static class ExceptionHandlingExtensions
{
    public static void UseApplicationExceptionHandling(this IApplicationBuilder app)
    {
        app.UseMiddleware<GlobalExceptionHandler>();
    }
}
