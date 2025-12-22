using System.Text.Json.Serialization;

namespace Application.Todos.DTOs;

public sealed record TodoCreateDto
{
    [JsonPropertyName("title")]
    public string?	Title { get; init; }
    
    [JsonPropertyName("order")]
    public int?		Order { get; init; }
}
