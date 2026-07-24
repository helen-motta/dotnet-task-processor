using DotNetEnv;
using MongoDB.Driver;
using TaskProcessor.Repositories;
using TaskProcessor.Services;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

builder.Services.AddControllers();

// Configuração do Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Configuração do MongoDB
var mongoUri = Environment.GetEnvironmentVariable("MONGODB_URI");

builder.Services.AddSingleton<IMongoClient>(
    new MongoClient(mongoUri));

builder.Services.AddSingleton(serviceProvider =>
{
    var client = serviceProvider.GetRequiredService<IMongoClient>();

    return client.GetDatabase("taskprocessor");
});


// Dependências de injeção para repositórios e serviços
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ITaskService, TaskService>();

var app = builder.Build();
app.MapControllers();

// Habilita o Swagger
app.UseSwagger();
app.UseSwaggerUI();

app.Run();