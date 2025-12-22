using Microsoft.Extensions.Options;
using Presentation.Endpoints.TodoConsumers;
using Presentation.Endpoints.TodoPublisher;
using Presentation.Endpoints.V1;
using Presentation.ExceptionHandlers;
using ServiceDefaults;

namespace Presentation.Configurations;

public static class ConfigureWebApplication
{
    public static void AddWebApplicationConfiguration(this WebApplication application)
    {
        var daprConfiguration = application.Services
            .GetRequiredService<IOptions<DaprConfiguration>>().Value;

        application.UseRouting();
        application.UseExceptionHandler();
        application.UseApplicationExceptionHandling();
        application.MapTodoEndpoints();
        application.MapDefaultEndpoints();
        application.MapTodoConsumersEndpoints(daprConfiguration);
        application.MapTodoPublishEndpoints(daprConfiguration);
        application.MapOpenApi();
        application.AddSwagger();
        application.MapSubscribeHandler();
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
