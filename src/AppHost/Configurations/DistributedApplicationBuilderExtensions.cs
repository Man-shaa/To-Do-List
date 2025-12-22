using AppHost.Configurations.Settings;
using AspireConfiguration;
using CommunityToolkit.Aspire.Hosting.Dapr;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Projects;

namespace AppHost.Configurations;

public static class DistributedApplicationBuilderExtensions
{
    public static IResourceBuilder<PostgresDatabaseResource> AddPostgres(this IDistributedApplicationBuilder builder)
    {
        return builder.AddPostgres(AspireResourcesName.Postgres)
            .WithPgWeb()
            .WithDataVolume()
            .AddDatabase(AspireResourcesName.TodoDatabase);
    }

    public static IResourceBuilder<KafkaServerResource> AddKafka(this IDistributedApplicationBuilder builder)
    {
        
        builder.Services.AddOptions<KafkaSettings>()
            .BindConfiguration(KafkaSettings.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        
        var kafkaSettings = builder.Configuration
            .GetRequiredSection(KafkaSettings.SectionName).Get<KafkaSettings>()!;
        
        var kafka = builder
            .AddKafka(AspireResourcesName.Kafka, kafkaSettings.KafkaBrokerPort)
            .WithKafkaUI(configureContainer =>
                    configureContainer.WithHostPort(9200),
                AspireResourcesName.KafkaUiContainerName);

        return kafka;
    }

    public static void AddTodoProject(this IDistributedApplicationBuilder builder,
        IResourceBuilder<PostgresDatabaseResource> postgres,
        IResourceBuilder<KafkaServerResource> kafka)
    {
        var pubsub = builder
            .AddDaprPubSub("toto",
                new DaprComponentOptions
                {
                    LocalPath = "./dapr/"
                });

        builder.AddProject<Presentation>(AspireResourcesName.TodoApi)
            .WithUrl("http://localhost:5000/swagger")
            .WithDaprSidecar(
                new DaprSidecarOptions
                {
                    AppId = AspireResourcesName.TodoApi,
                    AppPort = 5000
                })
            .WithReference(postgres)
            .WithReference(kafka)
            // .WithReference(pubsub)
            .WaitFor(postgres)
            .WaitFor(kafka);
    }
}
