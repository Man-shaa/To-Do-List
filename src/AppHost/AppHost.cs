using AspireConfiguration;
using CommunityToolkit.Aspire.Hosting.Dapr;
using Projects;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres(AspireResourcesName.Postgres)
    .WithPgWeb()
    .WithDataVolume()
    .AddDatabase(AspireResourcesName.TodoDatabase);

builder.AddProject<Presentation>(AspireResourcesName.TodoApi)
    .WithDaprSidecar(new DaprSidecarOptions()
    {
        AppId = "presentation"
    })
    .WithEnvironment("SwaggerEnabled", "true")
    .WithReference(postgres)
    .WaitFor(postgres);

await builder.Build().RunAsync();
