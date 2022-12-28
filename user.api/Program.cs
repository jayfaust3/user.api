using Messaging.Bus;
using Messaging.Service;
using MassTransit;
using Persistence.Repositories;
using RabbitMQ.Client;
using Common.Models.Configuration;
using System.Reflection;
using Application.Services.Crud;
using Application.Services.Cache;
using Common.Models.Context;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IUserContext, UserContext>();

//db
builder.Services.AddSingleton<IOpenSearchSettings>
(
    new OpenSearchSettings
    (
        Environment.GetEnvironmentVariable("OPENSEARCH_NODE_URIS")?.Split(';') ?? Array.Empty<string>(),
        Environment.GetEnvironmentVariable("OPENSEARCH_USER_INDEX_NAME") ?? ""
    )
);
builder.Services.AddTransient<IUserRepository, UserRepository>();
//db

//services
builder.Services.AddTransient<ICacheService, MemoryCacheService>();
builder.Services.AddTransient<IUserCrudService, UserCrudService>();
//services

//messaging
var rabbitHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST");
var rabbitVirtualHost = Environment.GetEnvironmentVariable("RABBITMQ_VIRTUAL_HOST") ?? "/";
var rabbitPort = int.Parse(Environment.GetEnvironmentVariable("RABBITMQ_PORT") ?? "5672");
var rabbitUser = Environment.GetEnvironmentVariable("RABBITMQ_USERNAME");
var rabbitPassword = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD");

if (
    string.IsNullOrWhiteSpace(rabbitHost) ||
    string.IsNullOrWhiteSpace(rabbitUser) ||
    string.IsNullOrWhiteSpace(rabbitPassword)
) throw new Exception("Missing RabbitMQ Credentials");

builder.Services.AddTransient<IConnectionFactory>(_ => new ConnectionFactory
{
    HostName = rabbitHost.Split("//")[1],
    UserName = rabbitUser,
    Password = rabbitPassword,
    Port = rabbitPort,
    VirtualHost = rabbitVirtualHost
});
builder.Services.AddSingleton<IHostedService, BusService>();
builder.Services.AddTransient<IMessageService, MessageService>();
builder.Services.AddMassTransit(_ =>
{
    _.AddBus(registrationContext => Bus.Factory.CreateUsingRabbitMq(cfg =>
    {
        cfg.Host(new Uri(rabbitHost), h =>
        {
            h.Username(rabbitUser);
            h.Password(rabbitPassword);
        });
        cfg.ConfigureEndpoints(registrationContext);
        cfg.AutoDelete = true;
    }));

    _.AddConsumers(Assembly.GetExecutingAssembly());
});
//messaging

builder.Services.AddMemoryCache();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseMiddleware<AuthorizationMiddleware>();

app.UseCors(builder =>
{
    builder.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader();
});

app.MapControllers();

app.UsePathBase(new PathString("/api"));

app.UseRouting();

app.Run();
