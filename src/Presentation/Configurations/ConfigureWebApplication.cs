using Presentation.Endpoints;
using Presentation.ExceptionHandlers;
using ServiceDefaults;

namespace Presentation.Configurations;

public static class ConfigureWebApplication
{
    public static async Task AddWebApplicationConfiguration(this WebApplication application)
    {
        application.UseRouting();
        application.UseExceptionHandler();
        application.UseApplicationExceptionHandling();
        application.MapTodoEndpoints();
        application.MapDefaultEndpoints();
        application.MapOpenApi();
        application.AddSwagger();
    }

    private static void AddSwagger(this WebApplication application)
    {
        application.UseSwagger();
        application.UseSwaggerUI(options =>
        {
            var descriptions = application.DescribeApiVersions();

            foreach (var groupName in descriptions.Select(x => x.GroupName))
                options.SwaggerEndpoint($"/swagger/{groupName}/swagger.json",
                    groupName.ToUpperInvariant());
        });
        application.MapSwagger();
    }
}
