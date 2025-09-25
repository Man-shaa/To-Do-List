using MediatR;
using Microsoft.AspNetCore.Http;
using Todo.Infrastructure.Repositories;

namespace Todo.Application.Todos.Commands.DeleteTodo;

public record DeleteAllTodoCommand() : IRequest<IResult>;

public sealed class DeleteAllTodoHandler(TodoService todoService) : IRequestHandler<DeleteAllTodoCommand, IResult>
{
    public Task<IResult> Handle(DeleteAllTodoCommand request, CancellationToken cancellationToken)
    {
        todoService.DeleteAll();

        return Task.FromResult(Results.NoContent());
    }
}
