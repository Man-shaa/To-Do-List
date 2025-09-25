using MediatR;
using Microsoft.AspNetCore.Http;
using Todo.Infrastructure.Repositories;

namespace Todo.Application.Todos.Commands.DeleteTodo;

public record DeleteTodoCommand(int Id) : IRequest<IResult>;

public sealed class DeleteTodoHandler(TodoService todoService) : IRequestHandler<DeleteTodoCommand, IResult>
{
    public Task<IResult> Handle(DeleteTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = todoService.GetById(request.Id);

        if (todo is null)
            return Task.FromResult(Results.NoContent());

        todoService.DeleteById(todo);
        return Task.FromResult(Results.Ok());
    }
}
