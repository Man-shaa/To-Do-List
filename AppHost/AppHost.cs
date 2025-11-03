using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithHostPort(15432)
    .WithPgWeb()
    .AddDatabase("todo-db");
    // .WithDataVolume()
    // .WithLifetime(ContainerLifetime.Persistent);

builder.AddProject<Presentation>("Presentation")
    .WithEnvironment("SwaggerEnabled", "true")
    .WithReference(postgres)
    .WaitFor(postgres);

await builder.Build().RunAsync();
