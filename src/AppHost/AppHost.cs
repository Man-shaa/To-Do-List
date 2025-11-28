using Projects;

var builder = DistributedApplication.CreateBuilder(args);
IResourceBuilder<PostgresDatabaseResource> postgres;

postgres = builder.AddPostgres("postgres")
    .WithPgWeb()
    .WithDataVolume()
    .AddDatabase("todo-db");

builder.AddProject<Presentation>("Presentation")
    .WithEnvironment("SwaggerEnabled", "true")
    .WithReference(postgres)
    .WaitFor(postgres);

await builder.Build().RunAsync();
