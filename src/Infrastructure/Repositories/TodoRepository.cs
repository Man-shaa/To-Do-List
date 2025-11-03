using Domain.Entities;
using Infrastructure.Persistence;
using Infrastructure.Repositories.Configurations;
using Infrastructure.Repositories.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Infrastructure.Repositories;

public sealed class TodoRepository(IOptions<SettingsOptions> options, TodoDbContext dbContext) : ITodoRepository
{
    private static int      	s_todoId = 1;
    private readonly Uri	    _baseUrl = options.Value.BaseUrl;

    public async Task<List<Todo>> GetAllAsync(CancellationToken ct)
    {
        return await dbContext.Todos.AsNoTracking().ToListAsync(cancellationToken: ct);
    }

    public async Task<Todo?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await dbContext.Todos.FindAsync([id], ct);
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

        dbContext.Todos.Add(todo);
        await dbContext.SaveChangesAsync(ct);
        return await Task.FromResult(todo);
    }
    
    public async Task<Todo> UpdateAsync(Todo todo)
    {
        var updatedTodo = dbContext.Todos.Update(todo);
        await dbContext.SaveChangesAsync();
        return updatedTodo.Entity;
    }
    
    public async Task<bool> DeleteByIdAsync(Todo? todo, CancellationToken ct)
    {
        if (todo is null)
            return false;
                
        dbContext.Todos.Remove(todo);
        var response = await dbContext.SaveChangesAsync(ct);
        return response != 0;
    }

    public async Task DeleteAllAsync(CancellationToken ct)
    {
        dbContext.Todos.RemoveRange(dbContext.Todos);
        await dbContext.SaveChangesAsync(ct);
        await Task.CompletedTask; // utile d'attendre Task.CompletedTask ici ?
    }
}
