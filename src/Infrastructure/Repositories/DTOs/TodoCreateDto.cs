namespace Infrastructure.Repositories.DTOs;

public sealed record TodoCreateDto
{
    public TodoCreateDto()
    {
    }

    public TodoCreateDto(string? title, int? order)
    {
        Title = title;
        Order = order;
    }

    public TodoCreateDto(string? title) : this()
    {
        Title = title;
    }

    public string?	Title { get; init; }
    public int?		Order { get; init; }
}
