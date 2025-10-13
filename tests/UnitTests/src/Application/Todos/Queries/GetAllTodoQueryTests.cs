using Application.Todos.Queries.GetTodo;
using Domain.Entities;
using Infrastructure.Repositories;
using Moq;

namespace UnitTests.Application.Todos.Queries;

public class GetAllTodoQueryTests
{
    [Fact]
    public async Task Handle_should_return_all_todos_from_service()
    {
        var expectedTodoList = new List<Todo>
        {
            new(id: 1, title: "a", url: new Uri("http://localhost/todos/1"), order: 1),
            new(id: 2, title: "b", url: new Uri("http://localhost/todos/2"), order: 2),
        };

        var todoServiceMock = new Mock<ITodoService>();
        todoServiceMock.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTodoList);

        var handler = new GetAllTodoHandler(todoServiceMock.Object);
        var returnedTodoList = await handler.Handle(new GetAllTodoQuery(), CancellationToken.None);

        Assert.Same(expectedTodoList, returnedTodoList);
        todoServiceMock.Verify(s => s.GetAllAsync(CancellationToken.None), Times.Once);
        todoServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_ReturnsEmptyList_WhenServiceHasNoTodos()
    {
        // Arrange
        var ct = CancellationToken.None;
        var expected = new List<Todo>();

        var todoServiceMock = new Mock<ITodoService>();
        todoServiceMock.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var handler = new GetAllTodoHandler(todoServiceMock.Object);

        // Act
        var result = await handler.Handle(new GetAllTodoQuery(), ct);

        // Assert
        Assert.Empty(result);
        todoServiceMock.Verify(s => s.GetAllAsync(ct), Times.Once);
        todoServiceMock.VerifyNoOtherCalls();
    }
}
