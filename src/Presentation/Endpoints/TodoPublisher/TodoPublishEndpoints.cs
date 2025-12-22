using Application.Todos.DTOs;
using Dapr.Client;
using Presentation.Common.Constants;
using Presentation.Configurations;

namespace Presentation.Endpoints.TodoPublisher;

public static class TodoPublisherEndpoints
{
    public static void MapTodoPublishEndpoints(this WebApplication app, DaprConfiguration daprConfiguration)
    {
        app.MapPost($"{ApiRoutes.Todos.Create}/publish",
                async (TodoCreateDto dto, DaprClient daprClient) =>
                {
                    await daprClient.PublishEventAsync(
                        daprConfiguration.PubSub.ComponentName,
                        daprConfiguration.PubSub.Topics.CreateTodoTopic,
                        dto);

                    return Results.Accepted();
                })
            .WithName("PublishCreateTodo")
            .Produces(StatusCodes.Status202Accepted)
            .WithOpenApi();
    }
}
