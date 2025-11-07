using Microsoft.Extensions.Configuration;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);
IResourceBuilder<PostgresDatabaseResource> postgres;

if (builder.Configuration.GetValue("Testing", false))
{
    postgres = builder.AddPostgres("postgres")
        .WithHostPort(15432)
        .WithPgWeb()
        .AddDatabase("todo-db");
}
else
{
    postgres = builder.AddPostgres("postgres")
        .WithHostPort(15432)
        .WithPgWeb()
        .WithDataVolume()
        .WithLifetime(ContainerLifetime.Persistent)
        .AddDatabase("todo-db");
}

builder.AddProject<Presentation>("Presentation")
    .WithEnvironment("SwaggerEnabled", "true")
    .WithReference(postgres)
    .WaitFor(postgres);

await builder.Build().RunAsync();
