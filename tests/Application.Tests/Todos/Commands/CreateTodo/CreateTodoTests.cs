using System.Threading.Tasks;
using Application.Todos.Commands.CreateTodo;

namespace Application.Tests.Todos.Commands.CreateTodo;

public class CreateTodoHandlerShould
{
    [Test]
    public Task CreateTodo()
    {
        var command = CreateTodoCommand();
        return Task.CompletedTask;
    }
}
