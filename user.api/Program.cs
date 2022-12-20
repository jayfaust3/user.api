using Messaging.Bus;
using Messaging.Service;
using Microsoft.Extensions.Configuration;
using MassTransit;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using Persistence.Repositories;
using RabbitMQ.Client;
using Common.Models.Configuration;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

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

//messaging
var rabbitHost = Environment.GetEnvironmentVariable("RabbitMQ_Host");
var rabbitUser = Environment.GetEnvironmentVariable("RabbitMQ_Username");
var rabbitPassword = Environment.GetEnvironmentVariable("RabbitMQ_Password");

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
    VirtualHost = "/"
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
