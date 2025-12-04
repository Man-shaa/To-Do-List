using Application.Todos.Commands.CreateTodo;
using Application.Todos.DTOs;
using Dapr;
using MediatR;
using Presentation.Common.Constants;
using Presentation.Configurations;

namespace Presentation.Endpoints.TodoConsumers;

public static class TodoConsumersEndpoints
{
    public static Task MapTodoConsumersEndpoints(this IEndpointRouteBuilder app,
        DaprConfiguration daprConfiguration)
    {
        app.MapPost(ApiRoutes.Consumers.Todo, HandleCreateTodo)
            .WithTopic(new TopicOptions
            {
                PubsubName = daprConfiguration.PubSub.ComponentName,
                Name = daprConfiguration.PubSub.Topics.CreateTodoTopic
            })
            .ExcludeFromDescription();

        return Task.CompletedTask;
    }

    private static async Task<IResult> HandleCreateTodo(TodoCreateDto dto, ISender sender)
    {
        var createdTodo = await sender.Send(new CreateTodoCommand(dto));
        return Results.Created(createdTodo.Url, createdTodo);
    }
}
