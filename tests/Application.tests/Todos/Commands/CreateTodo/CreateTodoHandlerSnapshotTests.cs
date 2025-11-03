using Application.Todos.Commands.CreateTodo;
using Domain.Entities;
using Infrastructure.Repositories;
using Infrastructure.Repositories.DTOs;
using NSubstitute;
using Moq;

namespace ApplicationTests.Todos.Commands.CreateTodo;


public sealed class CreateTodoHandlerTests
{
    [Fact]
    public async Task CreateTodoHandler_WithExplicitIdAndOrder_ReturnsTodo()
    {
        var todoDbContext = Substitute.For<ITodoRepository>();
        var todoCreateDto = new TodoCreateDto
        {
            Title = "Snapshot Title",
            Order = 1
        };
        
        todoDbContext.CreateAsync(todoCreateDto, Arg.Any<CancellationToken>())
            .Returns(new Todo(
                1,
                todoCreateDto.Title,
                new Uri("https://localhost:7214/todos/1"),
                todoCreateDto.Order ?? 1));

        var createTodoCommand = new CreateTodoCommand(todoCreateDto);
        var createHandler = new CreateTodoHandler(todoDbContext);
        
        var sut = await createHandler.Handle(createTodoCommand, CancellationToken.None);
        
        await Verify(sut);
    }
    
    [Fact]
    public async Task Handle_should_return_created_result_with_created_todo()
    {
        var dto = new TodoCreateDto { Title = "Test Title", Order = 3 };
        var createdTodo = new Todo(12, dto.Title, new Uri("https://localhost:7214/todos/12"), dto.Order ?? 3);
        var todoDbContext = new Mock<ITodoRepository>();

        todoDbContext.Setup(s => s.CreateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdTodo);

        var createHandler = new CreateTodoHandler(todoDbContext.Object);

        var result = await createHandler.Handle(new CreateTodoCommand(dto), CancellationToken.None);

        var createdResult = Assert.IsType<Todo>(result);
        Assert.Same(createdTodo, createdResult);

        todoDbContext.Verify(s => s.CreateAsync(dto, It.IsAny<CancellationToken>()), Times.Once);
        todoDbContext.VerifyNoOtherCalls();
    }
}
