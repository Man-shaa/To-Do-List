using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc.Formatters;
using Todo.Application.Common.Behaviors;
using Todo.Infrastructure.Repositories;
using Todo.Infrastructure.Repositories.Configurations;
using Todo.Presentation.Endpoints;
using Todo.Application;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<TodoService>();
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly, includeInternalTypes: true);
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblyContaining<AssemblyMarker>());
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddOptions<SettingsOptions>()
    .Bind(builder.Configuration.GetSection(SettingsOptions.ConfigurationSectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services
    .AddSingleton<IValidateOptions<SettingsOptions>, ValidateSettingsOptions>();
builder.Services.AddControllers(options =>
    {
        options.InputFormatters.RemoveType<SystemTextJsonInputFormatter>();
    })
    .AddNewtonsoftJson();

var app = builder.Build();

app.MapTodoEndpoints();

await app.RunAsync();
