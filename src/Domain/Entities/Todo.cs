namespace ToDo.Domain.Entities;
public sealed class Todo(int id, string title, string url, int order)
{
    public int		Id { get; init; } = id;
    public string	Title { get; set; } = title;
    public string	Url { get; init; } = url;
    public bool		Completed { get; set; } = false;
    public int		Order { get; set; } = order;
}