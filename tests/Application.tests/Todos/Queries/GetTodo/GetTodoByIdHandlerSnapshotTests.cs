using Application.Todos.Queries.GetTodo;
using Domain.Entities;
using Infrastructure.Repositories;
using Moq;
using NSubstitute;

namespace ApplicationTests.Todos.Queries.GetTodo;

public sealed class GetTodoByIdQueryTests
{
    [Fact]
    public async Task GetTodoByIdHandler_WithExistingTodo_ReturnsTodo()
    {
        var todoServiceMock = Substitute.For<ITodoService>();
        var getTodoByIdQuery = new GetTodoByIdQuery(1);

        todoServiceMock.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(new Todo(
                id: 1,
                title: "Snapshot Title 1",
                url: new Uri("https://localhost:7214/todos/1"),
                order: 1));

        var getTodoByIdHandler = new GetTodoByIdHandler(todoServiceMock);
        var sut = getTodoByIdHandler.Handle(getTodoByIdQuery, CancellationToken.None); 

        await Verify(sut);
    }

    [Fact]
    public async Task GetTodoByIdHandler_TodoNotFound_ReturnsNull()
    {
        var todoServiceMock = Substitute.For<ITodoService>();
        var getTodoByIdQuery = new GetTodoByIdQuery(1);
        var getTodoByIdHandler = new GetTodoByIdHandler(todoServiceMock);

        todoServiceMock.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns((Todo?)null);

        var sut = getTodoByIdHandler.Handle(getTodoByIdQuery, CancellationToken.None);

        await Verify(sut);
    }

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
