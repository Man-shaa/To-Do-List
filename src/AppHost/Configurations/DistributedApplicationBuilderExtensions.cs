using System.Globalization;
using AppHost.Configurations.Settings;
using AspireConfiguration;
using CommunityToolkit.Aspire.Hosting.Dapr;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Projects;

namespace AppHost.Configurations;

public static class DistributedApplicationBuilderExtensions
{
    private static int s_targetPort = 5002;

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
                configureContainer.WithHostPort(9200), "kafka-ui");
        
        return kafka;
    }

    public static void AddTodoProject(this IDistributedApplicationBuilder builder,
        IResourceBuilder<PostgresDatabaseResource> postgres)
    {
        builder.AddProject<Presentation>(AspireResourcesName.TodoApi)
            .WithDaprSidecar(new DaprSidecarOptions()
            {
                AppId = "presentation"
            })
            .WithEnvironment("SwaggerEnabled", "true")
            .WithReference(postgres)
            .WaitFor(postgres);
    }
    
    public static void AddKafkaConsumerResources(this IDistributedApplicationBuilder builder,
        IResourceBuilder<PostgresDatabaseResource> postgres,
        IResourceBuilder<KafkaServerResource> kafka)
    {
        // builder.AddConsumerResource(
        //     postgres,
        //     kafka,
        //     "./dapr/enrichedecommercegalerie",
        //     "kafka-pubsub-enrichedecommercegalerie",
        //     AspireResourcesName.EnrichedECommerceGalerieConsumer,
        //     AspireResourcesName.EnrichedECommerceGalerieConsumer,
        //     new List<(string, string)> { ("FeatureManagement:EnrichedECommerceGalerieConsumer", "true") });
    }
    
    private static void AddConsumerResource(
        this IDistributedApplicationBuilder distributedApplicationBuilder,
        IResourceBuilder<PostgresDatabaseResource> postgres,
        IResourceBuilder<KafkaServerResource> kafka,
        string componentFilePath,
        string pubsubComponentName,
        string resourceName,
        string appId,
        IEnumerable<(string name, string value)> environmentVariables)
    {
        var pubsub = distributedApplicationBuilder
            .AddDaprPubSub(pubsubComponentName,
                new DaprComponentOptions
                {
                    LocalPath = componentFilePath
                })
            .WaitFor(kafka);

        var catalogResource = distributedApplicationBuilder
            .AddProject<Presentation>(resourceName)
            .WithDaprSidecar(
                new DaprSidecarOptions
                {
                    AppId = appId,
                    AppPort = s_targetPort
                })
            .WithEndpoint(targetPort: s_targetPort)
            .WithEnvironment("ASPNETCORE_URLS",
                $"http://localhost:{s_targetPort.ToString(CultureInfo.InvariantCulture)}")
            .WithUrl($"http://localhost:{s_targetPort.ToString(CultureInfo.InvariantCulture)}")
            .WaitFor(postgres)
            .WaitFor(kafka)
            .WithReference(postgres)
            .WithReference(pubsub)
            .WithExplicitStart();

        foreach ((string name, string value) in environmentVariables) { catalogResource.WithEnvironment(name, value); }

        s_targetPort++;
    }

    public static void AddECommerceEnrichers(
        this IDistributedApplicationBuilder distributedApplicationBuilder,
        IResourceBuilder<KafkaServerResource> kafka)
    {
        // var relPublicationCompositionEnricher = distributedApplicationBuilder
        //     .AddECommerceRelPublicationCompositionEnricher()
        //     .WithPublishImage()
        //     .WaitFor(kafka)
        //     .WithReference(kafka)
        //     .WithExplicitStart();
    }
}
