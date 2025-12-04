using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace Presentation.Configurations;

public sealed class DaprConfiguration
{
    public const string SectionName = "Dapr";

    [Required]
    [ValidateObjectMembers]
    public required PubSub PubSub { get; init; }
}

public sealed class PubSub
{
    [Required]
    public required string ComponentName { get; init; }

    [Required]
    [ValidateObjectMembers]
    public required Topics Topics { get; init; }
}

public sealed class Topics
{
    [Required]
    public required string CreateTodoTopic { get; init; }
}
