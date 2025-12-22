using Application.Todos;
using Application.Todos.DTOs;
using Domain.Entities;
using Infrastructure.Persistence;
using Infrastructure.Repositories.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Options;

namespace Infrastructure.Repositories;

public sealed class TodoRepository(IOptions<SettingsOptions> options, TodoDbContext dbContext) : ITodoRepository
{
    private readonly Uri? _baseUrl = options.Value.BaseUrl;

    public async Task<List<Todo>> GetAllAsync(CancellationToken ct)
    {
        return await dbContext.Todos.AsNoTracking().ToListAsync(ct);
    }

    public async Task<Todo?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await dbContext.Todos.FindAsync([id], ct);
    }

    public async Task<Todo> CreateAsync(TodoCreateDto dto, CancellationToken ct)
    {
        Todo todo = new(
            0,
            dto.Title ?? "default title",
            new Uri($"{_baseUrl}todos/0"),
            dto.Order ?? 2
        );


        dbContext.Todos.Add(todo);
        await dbContext.SaveChangesAsync(ct);

        todo.Url = new Uri($"{_baseUrl}todos/{todo.Id}");
        todo.Order = dto.Order ?? todo.Id;

        dbContext.Todos.Update(todo);
        await dbContext.SaveChangesAsync(ct);

        return await Task.FromResult(todo);
    }

    public async Task<Todo> UpdateAsync(Todo todo, CancellationToken ct = default)
    {
        EntityEntry<Todo> updatedTodo = dbContext.Todos.Update(todo);
        await dbContext.SaveChangesAsync(ct);
        return Task.FromResult(updatedTodo.Entity).Result;
    }

    public async Task<bool> DeleteByIdAsync(Todo? todo, CancellationToken ct)
    {
        if (todo is null) { return false; }

        dbContext.Todos.Remove(todo);
        int response = await dbContext.SaveChangesAsync(ct);
        return response != 0;
    }

    public async Task DeleteAllAsync(CancellationToken ct)
    {
        dbContext.Todos.RemoveRange(dbContext.Todos);
        await dbContext.SaveChangesAsync(ct);
        await Task.CompletedTask; // utile d'attendre Task.CompletedTask ici ?
    }
}
