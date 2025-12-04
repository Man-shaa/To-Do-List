using AppHost.Configurations.Settings;
using AspireConfiguration;
using CommunityToolkit.Aspire.Hosting.Dapr;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Projects;

namespace AppHost.Configurations;

public static class DistributedApplicationBuilderExtensions
{
    private const int STargetPort = 5002;

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
        
        var kafkaSettings = builder.Configuration.GetRequiredSection(KafkaSettings.SectionName).Get<KafkaSettings>()!;
        
        var kafka = builder
            .AddKafka(AspireResourcesName.Kafka, kafkaSettings.KafkaBrokerPort)
            .WithKafkaUI(configureContainer =>
                configureContainer.WithHostPort(9200), AspireResourcesName.KafkaUiContainerName);
        
        return kafka;
    }

    public static void AddTodoProject(this IDistributedApplicationBuilder builder,
        IResourceBuilder<PostgresDatabaseResource> postgres,
        IResourceBuilder<KafkaServerResource> kafka,
        string appId)
    {
        var pubsup = builder
            .AddDaprPubSub(AspireResourcesName.Dapr.KafkaPubSubName,
                new DaprComponentOptions
                {
                    LocalPath = AspireResourcesName.Dapr.KafkaPath
                })
            .WaitFor(kafka);

        builder.AddProject<Presentation>(AspireResourcesName.TodoApi)
            .WithDaprSidecar(
                new DaprSidecarOptions
                {
                    AppId = appId,
                    AppPort = STargetPort
                })
            .WithEnvironment("SwaggerEnabled", "true")
            .WithReference(postgres)
            .WaitFor(postgres)
            .WithReference(pubsup);
    }
}
