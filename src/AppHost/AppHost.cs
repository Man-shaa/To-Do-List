using AppHost.Configurations;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres();
var kafka = builder.AddKafka();

builder.AddTodoProject(postgres);

builder.AddKafkaConsumerResources(postgres, kafka); // ?

builder.AddECommerceEnrichers(kafka); // ?

await builder
    .Build()
    .RunAsync();
