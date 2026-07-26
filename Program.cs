using DotNetEnv;
using MongoDB.Driver;
using TaskProcessor.Repositories;
using TaskProcessor.Services;
using TaskProcessor.Settings;
using TaskProcessor.Messaging.Publishers;
using TaskProcessor.Messaging.Consumers;

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

// Configuração do RabbitMQ
var portValue = Environment.GetEnvironmentVariable("RABBITMQ_PORT");
if (!int.TryParse(portValue, out var port))
{
    throw new InvalidOperationException("RABBITMQ_PORT não é um número válido.");
}

var rabbitMqSettings = new RabbitMqSettings
{
    HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? 
    throw new InvalidOperationException("RABBITMQ_HOST não está definido."),

    Port = port,
    UserName = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? 
    throw new InvalidOperationException("RABBITMQ_USER não está definido."),

    Password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? 
    throw new InvalidOperationException("RABBITMQ_PASSWORD não está definido."),

    QueueName = Environment.GetEnvironmentVariable("RABBITMQ_QUEUE") ?? 
    throw new InvalidOperationException("RABBITMQ_QUEUE não está definido.")
};

builder.Services.AddSingleton(rabbitMqSettings);
builder.Services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
builder.Services.AddHostedService<ProcessTaskConsumer>();
builder.Services.AddHostedService<EmailTaskConsumer>();

// Dependências de injeção para repositórios e serviços
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ITaskPublisher, TaskPublisher>();

var app = builder.Build();
app.MapControllers();

// Habilita o Swagger
app.UseSwagger();
app.UseSwaggerUI();

app.Run();