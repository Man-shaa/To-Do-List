using Application.Todos.Commands.DeleteTodo;
using Domain.Entities;
using Infrastructure.Repositories;
using Moq;

namespace UnitTests.Application.Todos.Commands.DeleteTodo
{
    public sealed class DeleteTodoByIdHandlerTests
    {
        private static Todo MakeTodo(int id = 1, string title = "Title", string url = "https://localhost/todos/1", int order = 1)
            => new Todo(id: id, title: title, url: new Uri(url), order: order);

        [Fact]
        public async Task Handle_should_get_by_id_then_delete_and_return_true()
        {
            const int id = 42;
            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            var todo = MakeTodo(id);

            var svc = new Mock<ITodoService>(MockBehavior.Strict);
            svc.Setup(s => s.GetByIdAsync(id, token)).ReturnsAsync(todo);
            svc.Setup(s => s.DeleteByIdAsync(todo, token)).ReturnsAsync(true);

            var handler = new DeleteTodoByIdHandler(svc.Object);

            var result = await handler.Handle(new DeleteTodoCommand(id), token);

            Assert.True(result);
            svc.Verify(s => s.GetByIdAsync(id, It.Is<CancellationToken>(t => t == token)), Times.Once);
            svc.Verify(s => s.DeleteByIdAsync(It.Is<Todo>(x => ReferenceEquals(x, todo)), It.Is<CancellationToken>(t => t == token)), Times.Once);
            svc.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_should_get_by_id_inexistant_todo_then_delete_with_null_and_return_false_when_not_found()
        {
            const int id = 7;
            var token = CancellationToken.None;

            var svc = new Mock<ITodoService>(MockBehavior.Strict);
            svc.Setup(s => s.GetByIdAsync(id, token)).ReturnsAsync((Todo?)null);
            svc.Setup(s => s.DeleteByIdAsync(null, token)).ReturnsAsync(false);

            var handler = new DeleteTodoByIdHandler(svc.Object);

            var result = await handler.Handle(new DeleteTodoCommand(id), token);

            Assert.False(result);
            svc.Verify(s => s.GetByIdAsync(id, It.Is<CancellationToken>(t => t == token)), Times.Once);
            svc.Verify(s => s.DeleteByIdAsync(It.Is<Todo?>(x => x == null), It.Is<CancellationToken>(t => t == token)), Times.Once);
            svc.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_should_propagate_exception_from_GetByIdAsync_and_not_call_delete()
        {
            const int id = 5;
            var token = CancellationToken.None;

            var svc = new Mock<ITodoService>(MockBehavior.Strict);
            svc.Setup(s => s.GetByIdAsync(id, token)).ThrowsAsync(new InvalidOperationException("boom"));

            var handler = new DeleteTodoByIdHandler(svc.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                handler.Handle(new DeleteTodoCommand(id), token));

            svc.Verify(s => s.GetByIdAsync(id, It.Is<CancellationToken>(t => t == token)), Times.Once);
            svc.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_should_propagate_exception_from_DeleteByIdAsync()
        {
            const int id = 9;
            var token = new CancellationTokenSource().Token;
            var todo = MakeTodo(id);

            var svc = new Mock<ITodoService>(MockBehavior.Strict);
            svc.Setup(s => s.GetByIdAsync(id, token)).ReturnsAsync(todo);
            svc.Setup(s => s.DeleteByIdAsync(todo, token)).ThrowsAsync(new InvalidOperationException("delete failed"));

            var handler = new DeleteTodoByIdHandler(svc.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                handler.Handle(new DeleteTodoCommand(id), token));

            svc.Verify(s => s.GetByIdAsync(id, It.Is<CancellationToken>(t => t == token)), Times.Once);
            svc.Verify(s => s.DeleteByIdAsync(It.Is<Todo>(x => ReferenceEquals(x, todo)), It.Is<CancellationToken>(t => t == token)), Times.Once);
            svc.VerifyNoOtherCalls();
        }
    }
}
