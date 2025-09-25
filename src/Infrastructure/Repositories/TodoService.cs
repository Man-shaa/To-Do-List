using Microsoft.Extensions.Options;
using ToDo.Presentation.Configurations;
using ToDo.Domain.Entities;
using ToDo.Application.Todos.DTOs;

namespace ToDo.Infrastructure.Repositories;

public sealed class TodoService(IOptions<SettingsOptions> options)
{
    private readonly List<Todo>	_todos = [];
    private static int      	s_todoId;
    private readonly Uri	    _baseUrl = options.Value.BaseUrl;

    public IEnumerable<Todo> GetAll() =>
		_todos;

    public Todo? GetById(int id) =>
        _todos.FirstOrDefault(t => t.Id == id);

    public Todo	Create(TodoCreateDto dto)
    {
        var todo = new Todo(
            id: s_todoId,
            title: dto.Title ?? "default title",
            url: new Uri($"{_baseUrl}todos/{s_todoId}"),
            order: dto.Order ?? (s_todoId)
        );

        s_todoId++;

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
