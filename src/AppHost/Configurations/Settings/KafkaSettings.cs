using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;

namespace AppHost.Configurations.Settings;

public sealed class KafkaSettings
{
    public const string SectionName = "KAFKA";

    [Required]
    [ConfigurationKeyName("KAFKA_BROKER_PORT")]
    public required int KafkaBrokerPort { get; init; }
}
