using Application.Todos.Queries.GetTodo;
using Domain.Entities;
using Infrastructure.Repositories;
using Moq;

namespace UnitTests.Application.Todos.Queries;

public sealed class GetTodoByIdQueryTests
{
    [Fact]
    public async Task Handle_should_return_todo_by_id_from_service()
    {
        var ct = CancellationToken.None;
        const int id = 42;
        var expected = new Todo(id, "todo 42", new Uri("http://localhost/todos/42"), 42);

        var todoServiceMock = new Mock<ITodoService>();
        todoServiceMock
            .Setup(s => s.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var handler = new GetTodoByIdHandler(todoServiceMock.Object);

        var result = await handler.Handle(new GetTodoByIdQuery(id), ct);

        Assert.Same(expected, result);
        todoServiceMock.Verify(s => s.GetByIdAsync(id, ct), Times.Once);
        todoServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_ReturnsNull_WhenTodoNotFound()
    {
        var ct = CancellationToken.None;
        const int id = 7;

        var todoServiceMock = new Mock<ITodoService>();
        todoServiceMock
            .Setup(s => s.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Todo?)null);

        var handler = new GetTodoByIdHandler(todoServiceMock.Object);

        var result = await handler.Handle(new GetTodoByIdQuery(id), ct);

        Assert.Null(result);
        todoServiceMock.Verify(s => s.GetByIdAsync(id, ct), Times.Once);
        todoServiceMock.VerifyNoOtherCalls();
    }
}
