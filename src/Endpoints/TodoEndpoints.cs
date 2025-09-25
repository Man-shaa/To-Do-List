using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using Todo.Application.Todos.Commands.CreateTodo;
using Todo.Application.Todos.Commands.DeleteTodo;
using Todo.Application.Todos.Commands.UpdateTodo;
using Todo.Application.Todos.Queries.GetTodo;
using Todo.Infrastructure.Repositories.DTOs;

namespace Todo.Presentation.Endpoints;

public static class TodoEndpoints
{
    public static void MapTodoEndpoints(this WebApplication app)
    {
        app.MapGet("/", () => "Hello World");

        app.MapGet("/todos", (ISender sender) =>
            sender.Send(new GetAllTodoCommand()));
        
        app.MapGet("/todos/{id:int}", (int id, ISender sender) =>
            sender.Send(new GetTodoByIdCommand(id)));

        app.MapPost("/todos", async (TodoCreateDto dto, ISender sender) =>
            await sender.Send(new CreateTodoCommand(dto)));

        app.MapPatch("/todos/{id:int}", async (int id, HttpRequest request, ISender sender) =>
        {
            using var reader = new StreamReader(request.Body);
            var body = await reader.ReadToEndAsync();

            var patch = Newtonsoft.Json.JsonConvert
                .DeserializeObject<JsonPatchDocument<Domain.Entities.Todo>>(body);

            return await sender.Send(new UpdateTodoCommand(id, patch!));
        });

        app.MapDelete("/todos/{id:int}", (int id, ISender sender) =>
            sender.Send(new DeleteTodoCommand(id)));

        app.MapDelete("/todos/", (ISender sender) =>
            sender.Send(new DeleteAllTodoCommand()));
    }
}
