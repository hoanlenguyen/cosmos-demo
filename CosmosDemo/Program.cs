using CosmosDemo.Enum;
using CosmosDemo.Repositories;
using CosmosDemo.Services;
using Microsoft.Azure.Cosmos.Fluent;

 

var builder = WebApplication.CreateBuilder(args);

// Add services 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IFoodDataRepository>(
    InitializeCosmosClientInstanceAsync(builder.Configuration.GetSection("Cosmos"), 
    CosmosDbValue.DatabaseName, 
    CosmosDbValue.ContainerName, 
    CosmosDbValue.PartitionKeyPath)
    .GetAwaiter().GetResult());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(config =>
    {
        config.DisplayRequestDuration();
    });
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