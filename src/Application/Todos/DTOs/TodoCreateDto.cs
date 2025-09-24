namespace ToDo.Application.Todos.DTOs;

public sealed record TodoCreateDto
{
    public string?	Title { get; init; }
    public int?		Order { get; init; }
}
