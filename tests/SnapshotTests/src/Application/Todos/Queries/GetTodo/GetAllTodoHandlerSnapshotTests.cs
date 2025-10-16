using Application.Todos.Queries.GetTodo;
using Domain.Entities;
using Infrastructure.Repositories;
using Moq;
using NSubstitute;

namespace SnapshotTests.Application.Todos.Queries.GetTodo;

public class GetAllTodoHandlerSnapshotTests
{
    [Fact]
    public async Task GetAllTodoHandler_WithTwoTodos_ReturnsListOfTwoTodos()
    {
        var todoServiceMock = Substitute.For<ITodoService>();
        var getAllTodoQuery = new GetAllTodoQuery();
        var getAllTodoHandler = new GetAllTodoHandler(todoServiceMock);
        
        todoServiceMock.GetAllAsync(Arg.Any<CancellationToken>())
                       .Returns(new List<Todo>
                       {
                           new Todo(1, "Snapshot Title 1", new Uri("https://localhost:7214/todos/1"), 1),
                           new Todo(2, "Snapshot Title 2", new Uri("https://localhost:7214/todos/2"), 2)
                       });

        var sut = getAllTodoHandler.Handle(getAllTodoQuery, CancellationToken.None); 

        await Verify(sut);
    }
    
    [Fact]
    public async Task GetAllTodoHandler_WithoutTodos_ReturnsEmptyList()
    {
        var todoServiceMock = Substitute.For<ITodoService>();
        var getAllTodoQuery = new GetAllTodoQuery();
        var getAllTodoHandler = new GetAllTodoHandler(todoServiceMock);

        todoServiceMock.GetAllAsync(It.IsAny<CancellationToken>())
            .Returns(new List<Todo> {});

        var sut = getAllTodoHandler.Handle(getAllTodoQuery, CancellationToken.None); 

        await Verify(sut);
    }
}
