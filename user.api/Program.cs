using Persistence.Configuration.MongoDB;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IMongoDBSettings>
(
    new MongoDBSettings
    {
        ConnectionURI = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_URI"),
        DatabaseName = Environment.GetEnvironmentVariable("MONGODB_USER_DB_NAME"),
        CollectionName = Environment.GetEnvironmentVariable("MONGODB_USER_COLLECTION_NAME"),
    }
);

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

app.MapControllers();

app.UsePathBase(new PathString("/api"));

app.UseRouting();

app.Run();
