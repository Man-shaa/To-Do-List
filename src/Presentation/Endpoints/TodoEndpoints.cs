using Application.Todos.Commands.CreateTodo;
using Application.Todos.Commands.DeleteTodo;
using Application.Todos.Commands.UpdateTodo;
using Application.Todos.Queries.GetTodo;
using Domain.Entities;
using Infrastructure.Repositories.DTOs;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;

namespace Presentation.Endpoints;

public static class TodoEndpoints
{
    public static void MapTodoEndpoints(this WebApplication app)
    {
        app.MapGet("/", () => "Hello World");

        app.MapGet("/todos", GetAllTodoAsync)
            .Produces<Todo>(StatusCodes.Status200OK);

        app.MapGet("/todos/{id:int}", GetTodoByIdAsync)
            .Produces<Todo>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        app.MapPost("/todos", CreateTodoAsync);

        app.MapPatch("/todos/{id:int}", UpdateTodoByIdAsync);

        app.MapDelete("/todos/", DeleteAllTodoAsync);

        app.MapDelete("/todos/{id:int}", DeleteTodoByIdAsync);
    }

    private static async Task<List<Todo>> GetAllTodoAsync(ISender sender) =>
        await sender.Send(new GetAllTodoQuery());

    private static async Task<IResult> GetTodoByIdAsync(int id, ISender sender)
    {
        var todo = await sender.Send(new GetTodoByIdCommand(id));

        if (todo is null)
            return Results.NotFound();
        return Results.Ok(todo);
    }

    private static async Task<IResult> CreateTodoAsync(TodoCreateDto dto, ISender sender) =>
        await sender.Send(new CreateTodoCommand(dto));

    private static async Task<IResult> UpdateTodoByIdAsync(int id, HttpRequest request, ISender sender)
    {
        using var reader = new StreamReader(request.Body);
        var body = await reader.ReadToEndAsync();

        var patch = Newtonsoft.Json.JsonConvert.DeserializeObject<JsonPatchDocument<Todo>>(body);

        var response = await sender.Send(new UpdateTodoCommand(id, patch!));

        if (response.Todo is null)
            return Results.NotFound();
        if (response.Errors is not null && response.Errors.Count > 0)
            return Results.BadRequest(response.Errors);
        return Results.Ok(response.Todo);
    }

    private static Task DeleteAllTodoAsync(ISender sender) =>
        sender.Send(new DeleteAllTodoCommand());

    private static Task DeleteTodoByIdAsync(int id, ISender sender) =>
        sender.Send(new DeleteTodoCommand(id));
}
