using Microsoft.Extensions.Options;
using ToDo.Presentation.Configurations;
using ToDo.Domain.Entities;
using ToDo.Application.DTOs;

namespace ToDo.Infrastructure.Repositories;

public sealed class TodoService
{
    private readonly List<Todo>	_todos = [];
    private static int	_todoId;
    private readonly string	_baseUrl;

    public TodoService(IOptions<SettingsOptions> options)
    {
        _baseUrl = options.Value.BaseUrl;
    }

    public IEnumerable<Todo> GetAll() =>
		_todos;

    public Todo? GetById(int id) =>
        _todos.FirstOrDefault(t => t.Id == id);

    public Todo	Create(TodoCreateDto dto)
    {
        var todo = new Todo(
            id: _todoId,
            title: dto.Title ?? "default title",
            url: $"{_baseUrl}/todos/{_todoId}",
            order: dto.Order ?? (_todoId)
        );

        _todoId++;

        _todos.Add(todo);
        return todo;
    }

    public void DeleteById(Todo todo)
    {
        _todos.Remove(todo);
    }

    public void DeleteAll()
    {
        _todos.Clear();
    }
}
