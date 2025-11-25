namespace Domain.Entities;
public sealed class Todo(int id, string title, Uri url, int order)
{
    public int		Id { get; init; } = id;
    public string	Title { get; set; } = title;
    public Uri	    Url { get; set; } = url;
    public bool		IsCompleted { get; set; }
    public int		Order { get; set; } = order;
}
