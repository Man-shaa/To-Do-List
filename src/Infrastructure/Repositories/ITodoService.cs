using Domain.Entities;
using Infrastructure.Repositories.DTOs;

namespace Infrastructure.Repositories;

public interface ITodoService
{ 
    public Task<List<Todo>> GetAllAsync(CancellationToken ct = default);
    public Task<Todo?> GetByIdAsync(int id, CancellationToken ct = default);
    public Task<Todo> CreateAsync(TodoCreateDto dto, CancellationToken ct = default);
    public Task<bool> DeleteByIdAsync(Todo? todo, CancellationToken ct = default);
    public Task DeleteAllAsync(CancellationToken ct);

    // unit test purpose only
    public string UselessMethod(string a);
}
