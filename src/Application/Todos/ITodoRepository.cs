using Application.Todos.DTOs;
using Domain.Entities;

namespace Application.Todos;

public interface ITodoRepository
{ 
    public Task<List<Todo>> GetAllAsync(CancellationToken ct = default);
    public Task<Todo?> GetByIdAsync(int id, CancellationToken ct = default);
    public Task<Todo> CreateAsync(TodoCreateDto dto, CancellationToken ct = default);
    public Task<bool> DeleteByIdAsync(Todo? todo, CancellationToken ct = default);
    public Task<Todo> UpdateAsync(Todo todo, CancellationToken ct = default);
    public Task DeleteAllAsync(CancellationToken ct);
}
