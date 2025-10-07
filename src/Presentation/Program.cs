using Application;
using Application.Common.Behaviors;
using FluentValidation;
using FluentValidation.AspNetCore;
using Infrastructure;
using Infrastructure.Repositories;
using Infrastructure.Repositories.Configurations;
using MediatR;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc.Formatters;
using Presentation.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure();

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
