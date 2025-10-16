using Domain.Entities;
using Infrastructure.Repositories.Configurations;
using Infrastructure.Repositories.DTOs;
using Microsoft.Extensions.Options;

namespace Infrastructure.Repositories;

public sealed class TodoService(IOptions<SettingsOptions> options) : ITodoService
{
    private readonly List<Todo>	_todos = [];
    private static int      	s_todoId = 1;
    private readonly Uri	    _baseUrl = options.Value.BaseUrl;

    public async Task<List<Todo>> GetAllAsync(CancellationToken ct)
    {
        return await Task.FromResult(_todos);
    }

    public async Task<Todo?> GetByIdAsync(int id, CancellationToken ct)
    {
        var todo = _todos.FirstOrDefault(t => t.Id == id);
        return await Task.FromResult(todo);
    }

    public async Task<Todo> CreateAsync(TodoCreateDto dto, CancellationToken ct)
    {
        var todo = new Todo(
            id: s_todoId,
            title: dto.Title ?? "default title",
            url: new Uri($"{_baseUrl}todos/{s_todoId}"),
            order: dto.Order ?? (s_todoId)
        );

        s_todoId++;

        _todos.Add(todo);
        return await Task.FromResult(todo);
    }
    
    public async Task<bool> DeleteByIdAsync(Todo? todo, CancellationToken ct)
    {
        if (todo is not null)
        {
            _todos.Remove(todo);
            await Task.CompletedTask;
            return true;
        }
        return false;
    }

    public async Task DeleteAllAsync(CancellationToken ct)
    {
        _todos.Clear();
        await Task.CompletedTask;
    }
}
