using AppHost.Configurations;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres();
var kafka = builder.AddKafka();

builder.AddTodoProject(postgres, kafka);

await builder
    .Build()
    .RunAsync();
