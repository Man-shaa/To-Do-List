using Microsoft.Extensions.Options;
using Todo.Infrastructure.Repositories.DTOs;
using Todo.Infrastructure.Repositories.Configurations;
namespace Todo.Infrastructure.Repositories;

public sealed class TodoService(IOptions<SettingsOptions> options)
{
    private readonly List<Domain.Entities.Todo>	_todos = [];
    private static int      	s_todoId;
    private readonly Uri	    _baseUrl = options.Value.BaseUrl;

    public IEnumerable<Domain.Entities.Todo> GetAll() =>
		_todos;

    public Domain.Entities.Todo? GetById(int id) =>
        _todos.FirstOrDefault(t => t.Id == id);

    public Domain.Entities.Todo	Create(TodoCreateDto dto)
    {
        var todo = new Domain.Entities.Todo(
            id: s_todoId,
            title: dto.Title ?? "default title",
            url: new Uri($"{_baseUrl}todos/{s_todoId}"),
            order: dto.Order ?? (s_todoId)
        );

        s_todoId++;

        _todos.Add(todo);
        return todo;
    }

    public void DeleteById(Domain.Entities.Todo todo)
    {
        _todos.Remove(todo);
    }

    public void DeleteAll()
    {
        _todos.Clear();
    }
    
    // unit test purpose only
    public string UselessMethod(string a)
    {
        string b = "Hello";

        if (a == b)
            return (a);
        return ("");
    }
}
