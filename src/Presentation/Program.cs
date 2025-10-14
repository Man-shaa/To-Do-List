using Application;
using Infrastructure;
using Presentation;
using Presentation.Endpoints;
using Presentation.ExceptionHandlers;

var builder = WebApplication.CreateBuilder(args);

builder.AddPresentation()
    .AddInfrastructure()
    .AddApplication();

var app = builder.Build();

app.UseApplicationExceptionHandling();

app.MapTodoEndpoints();
app.MapOpenApi();
app.MapSwagger();
app.UseSwagger();
app.UseSwaggerUI();

await app.RunAsync();
