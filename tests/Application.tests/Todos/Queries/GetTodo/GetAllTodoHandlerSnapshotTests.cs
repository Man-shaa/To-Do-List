using Application.Todos.Queries.GetTodo;
using Domain.Entities;
using Infrastructure.Repositories;
using Moq;
using NSubstitute;

namespace ApplicationTests.Todos.Queries.GetTodo;

public sealed class GetAllTodoQueryTests
{
    [Fact]
    public async Task GetAllTodoHandler_WithTwoTodos_ReturnsListOfTwoTodos()
    {
        var todoDbContext = Substitute.For<ITodoRepository>();
        var getAllTodoQuery = new GetAllTodoQuery();
        var getAllTodoHandler = new GetAllTodoHandler(todoDbContext);
        
        todoDbContext.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns([
                new Todo(1, "Snapshot Title 1", new Uri("https://localhost:7214/todos/1"), 1),
                new Todo(2, "Snapshot Title 2", new Uri("https://localhost:7214/todos/2"), 2)
            ]);

        var sut = getAllTodoHandler.Handle(getAllTodoQuery, CancellationToken.None); 

        await Verify(sut);
    }
    
    [Fact]
    public async Task GetAllTodoHandler_WithoutTodos_ReturnsEmptyList()
    {
        var todoDbContext = Substitute.For<ITodoRepository>();
        var getAllTodoQuery = new GetAllTodoQuery();
        var getAllTodoHandler = new GetAllTodoHandler(todoDbContext);

        todoDbContext.GetAllAsync(It.IsAny<CancellationToken>())
            .Returns([]);

        var sut = getAllTodoHandler.Handle(getAllTodoQuery, CancellationToken.None); 

        await Verify(sut);
    }

    [Fact]
    public async Task Handle_should_return_all_todos_from_service()
    {
        var expectedTodoList = new List<Todo>
        {
            new(id: 1, title: "a", url: new Uri("http://localhost/todos/1"), order: 1),
            new(id: 2, title: "b", url: new Uri("http://localhost/todos/2"), order: 2)
        };

        var todoDbContext = new Mock<ITodoRepository>();
        todoDbContext.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTodoList);

        var handler = new GetAllTodoHandler(todoDbContext.Object);
        var returnedTodoList = await handler.Handle(new GetAllTodoQuery(), CancellationToken.None);

        Assert.Same(expectedTodoList, returnedTodoList);
        todoDbContext.Verify(s => s.GetAllAsync(CancellationToken.None), Times.Once);
        todoDbContext.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_ReturnsEmptyList_WhenServiceHasNoTodos()
    {

        var ct = CancellationToken.None;
        var expected = new List<Todo>();

        var todoDbContext = new Mock<ITodoRepository>();
        todoDbContext.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var handler = new GetAllTodoHandler(todoDbContext.Object);


        var result = await handler.Handle(new GetAllTodoQuery(), ct);


        Assert.Empty(result);
        todoDbContext.Verify(s => s.GetAllAsync(ct), Times.Once);
        todoDbContext.VerifyNoOtherCalls();
    }
}

