using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using ToDo.Application.Todos.DTOs;
using ToDo.Application.Todos.Commands.CreateTodo;
using ToDo.Application.Todos.Commands.UpdateTodo;
using ToDo.Application.Todos.Commands.DeleteTodo;
using ToDo.Application.Todos.Queries.GetTodo;
using ToDo.Domain.Entities;

namespace ToDo.Presentation.Endpoints;

public static class TodoEndpoints
{
    public static void MapTodoEndpoints(this WebApplication app)
    {
        app.MapGet("/", () => "Hello World");

        app.MapGet("/todos", (ISender sender) =>
            sender.Send(new GetAllTodoCommand()));

        app.MapGet("/todos/{id}", (int id, ISender sender) =>
            sender.Send(new GetTodoByIdCommand(id)));

        app.MapPost("/todos", async (TodoCreateDto dto, ISender sender) =>
            await sender.Send(new CreateTodoCommand(dto)));

        app.MapPatch("/todos/{id}", async (int id, HttpRequest request, ISender sender) =>
        {
            using var reader = new StreamReader(request.Body);
            var body = await reader.ReadToEndAsync();

            var patch = Newtonsoft.Json.JsonConvert
                .DeserializeObject<JsonPatchDocument<Todo>>(body);

            return await sender.Send(new UpdateTodoCommand(id, patch!));
        });

        app.MapDelete("/todos/{id}", (int id, ISender sender) =>
            sender.Send(new DeleteTodoCommand(id)));

        app.MapDelete("/todos/", (ISender sender) =>
            sender.Send(new DeleteAllTodoCommand()));
    }
}
