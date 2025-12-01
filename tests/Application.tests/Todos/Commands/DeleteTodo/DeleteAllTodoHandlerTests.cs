using Application.Todos;
using Application.Todos.Commands.DeleteTodo;
using Moq;

namespace Application.Tests.Todos.Commands.DeleteTodo
{
    public sealed class DeleteAllTodoHandlerTests
    {
        [Fact]
        public async Task Handle_should_call_DeleteAllAsync_once_with_token_and_no_other_calls()
        {
            var todoDbContextMock = new Mock<ITodoRepository>();
            var cts = new CancellationTokenSource();

            todoDbContextMock.Setup(s => s.DeleteAllAsync(It.IsAny<CancellationToken>()))
                           .Returns(Task.CompletedTask);

            var sut = new DeleteAllTodoHandler(todoDbContextMock.Object);

            await sut.Handle(new DeleteAllTodoCommand(), cts.Token);

            todoDbContextMock.Verify(s => s.DeleteAllAsync(It.Is<CancellationToken>(t => t == cts.Token)), Times.Once);
            todoDbContextMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_should_propagate_exception_from_service()
        {
            var todoDbContextMock = new Mock<ITodoRepository>();

            todoDbContextMock.Setup(s => s.DeleteAllAsync(It.IsAny<CancellationToken>()))
                           .ThrowsAsync(new InvalidOperationException("failure"));

            var sut = new DeleteAllTodoHandler(todoDbContextMock.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                sut.Handle(new DeleteAllTodoCommand(), CancellationToken.None));

            todoDbContextMock.Verify(s => s.DeleteAllAsync(It.IsAny<CancellationToken>()), Times.Once);
            todoDbContextMock.VerifyNoOtherCalls();
        }
    }
}
