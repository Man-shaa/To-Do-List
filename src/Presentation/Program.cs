using Application;
using Infrastructure;
using Presentation;
using Presentation.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.AddPresentation()
    .AddInfrastructure()
    .AddApplication();

var app = builder.Build();

app.MapTodoEndpoints();
app.MapOpenApi();
app.MapSwagger();
app.UseSwagger();
app.UseSwaggerUI();

await app.RunAsync();
