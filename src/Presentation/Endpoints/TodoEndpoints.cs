using Application.Todos.Commands.CreateTodo;
using Application.Todos.Commands.DeleteTodo;
using Application.Todos.Commands.UpdateTodo;
using Application.Todos.Queries.GetTodo;
using Domain.Entities;
using Infrastructure.Repositories.DTOs;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json;

namespace Presentation.Endpoints;

public static class TodoEndpoints
{
    public static void MapTodoEndpoints(this WebApplication app)
    {
        app.MapGet("/", () => "Hello World");

        app.MapGet("/todos", GetAllTodoAsync);

        app.MapGet("/todos/{id:int}", GetTodoByIdAsync)
            .Produces(StatusCodes.Status404NotFound);

        app.MapPost("/todos", CreateTodoAsync)
            .Produces<Todo>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        app.MapPatch("/todos/{id:int}", UpdateTodoByIdAsync)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

        app.MapDelete("/todos/", DeleteAllTodoAsync)
            .Produces(StatusCodes.Status200OK);

        app.MapDelete("/todos/{id:int}", DeleteTodoByIdAsync)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<List<Todo>> GetAllTodoAsync(ISender sender) =>
        await sender.Send(new GetAllTodoQuery());

    private static async Task<IResult> GetTodoByIdAsync(int id, ISender sender)
    {
        var todo = await sender.Send(new GetTodoByIdQuery(id));

        if (todo is null)
            return Results.NotFound();
        return Results.Ok(todo);
    }

    private static async Task<IResult> CreateTodoAsync(TodoCreateDto dto, ISender sender)
    {
        var todo = await sender.Send(new CreateTodoCommand(dto));

        return Results.Ok(todo);
    }

    private static async Task<IResult> UpdateTodoByIdAsync(int id, HttpRequest request, ISender sender)
    {
        using var reader = new StreamReader(request.Body);
        var body = await reader.ReadToEndAsync();
        var patch = JsonConvert.DeserializeObject<JsonPatchDocument<Todo>>(body);
        var todo = await sender.Send(new GetTodoByIdQuery(id));

        if (todo is null)
            return Results.NotFound($"Todo `{id}` not found");

        var updatedTodo = await sender.Send(new UpdateTodoCommand(todo, patch!));

        return Results.Ok(updatedTodo);
    }

    private static async Task DeleteAllTodoAsync(ISender sender) =>
        await sender.Send(new DeleteAllTodoCommand());

    private static async Task<IResult> DeleteTodoByIdAsync(int id, ISender sender)
    {
        var hasBeenDeleted = await sender.Send(new DeleteTodoCommand(id));

        if (hasBeenDeleted)
            return Results.Empty;
        return Results.NotFound();
    }
}
