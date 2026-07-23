using DotNetEnv;
using MongoDB.Driver;

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

var app = builder.Build();
app.MapControllers();

// Habilita o Swagger
app.UseSwagger();
app.UseSwaggerUI();

app.Run();