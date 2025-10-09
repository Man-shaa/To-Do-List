using Domain.Entities;
using Infrastructure.Repositories;
using Infrastructure.Repositories.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Todos.Commands.CreateTodo;

public record CreateTodoCommand(TodoCreateDto Todo) : IRequest<IResult>;

public sealed class CreateTodoHandler(ITodoService todoService) : IRequestHandler<CreateTodoCommand, IResult>
{
    public async Task<IResult> Handle(CreateTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = await todoService.CreateAsync(request.Todo, cancellationToken);
        return Results.Ok(todo);
    }
}
