namespace Domain.Entities;
public sealed class Todo(int id, string title, Uri url, int order)
{
    public int		Id { get; init; } = id;
    public string	Title { get; set; } = title;
    public Uri	    Url { get; init; } = url;
    public bool		Completed { get; set; } = false;
    public int		Order { get; set; } = order;
}
