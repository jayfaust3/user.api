using Common.Models.Configuration;
using Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IOpenSearchSettings>
(
    new OpenSearchSettings
    (
        Environment.GetEnvironmentVariable("OPENSEARCH_NODE_URIS")?.Split(';') ?? Array.Empty<string>(),
        Environment.GetEnvironmentVariable("OPENSEARCH_USER_INDEX_NAME") ?? ""
    )
);

builder.Services.AddTransient<IUserRepository, UserRepository>();

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
