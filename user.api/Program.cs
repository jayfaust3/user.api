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
using Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IUserContext, UserContext>();

//db
var openSearchNodeURIs = Environment.GetEnvironmentVariable("OPENSEARCH_NODE_URIS")?
    .Split(';') ??
    Array.Empty<string>();
var opensearchUserIndexName = Environment.GetEnvironmentVariable("OPENSEARCH_USER_INDEX_NAME");
if (
    openSearchNodeURIs.Length == 0 ||
    string.IsNullOrWhiteSpace(opensearchUserIndexName)
) throw new Exception("Missing OpenSearchCredentials");


builder.Services.AddSingleton<IOpenSearchSettings>
(
    new OpenSearchSettings
    (
        openSearchNodeURIs,
        opensearchUserIndexName
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
var isRabbitPortSet = int.TryParse(Environment.GetEnvironmentVariable("RABBITMQ_PORT"), out int rabbitPort);
var rabbitUser = Environment.GetEnvironmentVariable("RABBITMQ_USERNAME");
var rabbitPassword = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD");

if (
    string.IsNullOrWhiteSpace(rabbitHost) ||
    string.IsNullOrWhiteSpace(rabbitUser) ||
    string.IsNullOrWhiteSpace(rabbitPassword) ||
    !isRabbitPortSet
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
builder.Services.AddMassTransit(registrationConfigurator =>
{
    registrationConfigurator.AddBus(
        registrationContext =>
            Bus.Factory.CreateUsingRabbitMq(
                configure =>
                {
                    configure.Host(
                        new Uri(rabbitHost), hostConfigurator =>
                        {
                            hostConfigurator.Username(rabbitUser);
                            hostConfigurator.Password(rabbitPassword);
                        });
                        configure.ConfigureEndpoints(registrationContext);
                        configure.AutoDelete = true;
                    configure.UseRawJsonSerializer(isDefault: true);
                    configure.UseRawJsonDeserializer(isDefault: true);
                }
            )
    );

    registrationConfigurator.AddConsumers(Assembly.GetExecutingAssembly());
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

app.UseCors(builder =>
{
    builder.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader();
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseMiddleware<AuthMiddleware>();

app.MapControllers();

app.UsePathBase(new PathString("/api"));

app.UseRouting();

app.Run();
