using AspireConfiguration;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres(AspireResourcesName.Postgres)
    .WithPgWeb()
    .WithDataVolume()
    .AddDatabase(AspireResourcesName.TodoDatabase);

builder.AddProject<Presentation>(AspireResourcesName.Presentation)
    .WithEnvironment("SwaggerEnabled", "true")
    .WithReference(postgres)
    .WaitFor(postgres);

await builder.Build().RunAsync();
