using Application.Todos.Commands.CreateTodo;
using Application.Todos.Commands.DeleteTodo;
using Application.Todos.Commands.UpdateTodo;
using Application.Todos.DTOs;
using Application.Todos.Queries.GetTodo;
using Asp.Versioning;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json;
using Presentation.Common.Constants;
using ServiceDefaults;

namespace Presentation.Endpoints;

public static class TodoEndpoints
{
    public static void MapTodoEndpoints(this WebApplication app)
    {
        var versionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();
        
        var group = app.MapGroup(ApiRoutes.Root).WithApiVersionSet(versionSet);
        
        app.MapGet("/", () => "Hello World");

        app.MapDefaultEndpoints();
        
        group.MapGet(ApiRoutes.Todos.GetAll, GetAllTodoV1)
            .Produces(StatusCodes.Status200OK)
            .WithName(nameof(GetAllTodoV1))
            .MapToApiVersion(1)
            .WithOpenApi();

        group.MapGet(ApiRoutes.Todos.GetById, GetTodoByIdV1)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithName(nameof(GetTodoByIdV1))
            .MapToApiVersion(1)
            .WithOpenApi();

        group.MapPost(ApiRoutes.Todos.Create, CreateTodoV1)
            .Produces<Todo>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName(nameof(CreateTodoV1))
            .MapToApiVersion(1)
            .WithOpenApi();

        group.MapPatch(ApiRoutes.Todos.UpdateById, UpdateTodoByIdV1)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName(nameof(UpdateTodoByIdV1))
            .MapToApiVersion(1)
            .WithOpenApi();

        group.MapDelete(ApiRoutes.Todos.DeleteAll, DeleteAllTodoV1)
            .Produces(StatusCodes.Status200OK)
            .WithName(nameof(DeleteAllTodoV1))
            .MapToApiVersion(1)
            .WithOpenApi();

        group.MapDelete(ApiRoutes.Todos.DeleteById, DeleteTodoByIdV1)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithName(nameof(DeleteTodoByIdV1))
            .MapToApiVersion(1)
            .WithOpenApi();
    }

    private static async Task<List<Todo>> GetAllTodoV1(ISender sender) =>
        await sender.Send(new GetAllTodoQuery());

    private static async Task<IResult> GetTodoByIdV1(int todoId, ISender sender)
    {
        var todo = await sender.Send(new GetTodoByIdQuery(todoId));

        if (todo is null)
            return Results.NotFound();
        return Results.Ok(todo);
    }

    private static async Task<IResult> CreateTodoV1(TodoCreateDto dto, ISender sender)
    {
        var todo = await sender.Send(new CreateTodoCommand(dto));

        return Results.Created(todo.Url, todo);
    }

    private static async Task<IResult> UpdateTodoByIdV1(int todoId, HttpRequest request, ISender sender)
    {
        using var reader = new StreamReader(request.Body);
        var body = await reader.ReadToEndAsync();
        var patch = JsonConvert.DeserializeObject<JsonPatchDocument<Todo>>(body);
        var todo = await sender.Send(new GetTodoByIdQuery(todoId));

        if (todo is null)
            return Results.NotFound($"Todo `{todoId}` not found");

        var updatedTodo = await sender.Send(new UpdateTodoCommand(todo, patch!));

        return Results.Ok(updatedTodo);
    }

    private static async Task DeleteAllTodoV1(ISender sender) =>
        await sender.Send(new DeleteAllTodoCommand());

    private static async Task<IResult> DeleteTodoByIdV1(int todoId, ISender sender)
    {
        var hasBeenDeleted = await sender.Send(new DeleteTodoCommand(todoId));

        if (hasBeenDeleted)
            return Results.Empty;
        return Results.NotFound();
    }
}
