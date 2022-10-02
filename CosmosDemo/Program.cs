using CosmosDemo.Repositories;
using CosmosDemo.Services;
using Microsoft.Azure.Cosmos.Fluent;

const string databaseName = "DemoDB";
const string containerName = "NutritionFoods";
const string partitionKeyPath = "/foodGroup";

var builder = WebApplication.CreateBuilder(args);

// Add services 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IFoodDataRepository>(
    InitializeCosmosClientInstanceAsync(builder.Configuration.GetSection("Cosmos"), databaseName, containerName, partitionKeyPath).GetAwaiter().GetResult());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.AddFoodDataService();

app.Run();


async Task<IFoodDataRepository> InitializeCosmosClientInstanceAsync(IConfigurationSection configurationSection, string databaseName, string containerName, string partitionKeyPath)
{
    string account = configurationSection.GetSection("Account").Value;
    string key = configurationSection.GetSection("Key").Value;
    var clientBuilder = new CosmosClientBuilder(account, key);
    var client = clientBuilder.WithConnectionModeDirect().Build();
    var cosmosDbService = new FoodDataRepository(client, databaseName, containerName);
    var database = await client.CreateDatabaseIfNotExistsAsync(databaseName);
    await database.Database.CreateContainerIfNotExistsAsync(containerName, partitionKeyPath);

    return cosmosDbService;
}