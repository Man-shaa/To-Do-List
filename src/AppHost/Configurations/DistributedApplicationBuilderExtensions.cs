using AspireConfiguration;
using Projects;

namespace AppHost.Configurations;

public static class DistributedApplicationBuilderExtensions
{
    private static IResourceBuilder<PostgresDatabaseResource> AddPostgres(this IDistributedApplicationBuilder builder)
    {
        return builder.AddPostgres(AspireResourcesName.Postgres)
            .WithPgWeb()
            .WithDataVolume()
            .AddDatabase(AspireResourcesName.TodoDatabase);
    }

    public static void AddPresentationProject(this IDistributedApplicationBuilder builder)
    {
        var postgres = builder.AddPostgres();

        builder.AddProject<Presentation>(AspireResourcesName.Presentation)
            .WithEnvironment("SwaggerEnabled", "true")
            .WithReference(postgres)
            .WaitFor(postgres);
    }
}
