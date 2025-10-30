using Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Persistence;

public class TodoDbContext(DbContextOptions<TodoDbContext> options) : DbContext(options)
{
    public DbSet<Todo> Todos { get; set; }
}
