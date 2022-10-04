using CosmosDemo.Enum;
using CosmosDemo.Models;
using Microsoft.Azure.Cosmos;
using System.Diagnostics;
using System.Net;

namespace CosmosDemo.Repositories
{
    public interface IFoodDataRepository
    {
        Task<IEnumerable<FoodDetail>> GetItemsAsync(string stringQuery);

        Task<FoodDetail> GetItemAsync(string id, string partitionKey);

        Task AddItemAsync(FoodDetail item);

        Task UpdateItemAsync(FoodDetail item);

        Task DeleteItemAsync(string id, string partitionKey);
        Task UpdatePartialAsync(FoodDetail item);
    }
    public class FoodDataRepository : IFoodDataRepository
    {
        private Container container;
        private readonly Stopwatch stopwatch = new  Stopwatch();
        public FoodDataRepository(
            CosmosClient dbClient,
            string databaseName,
            string containerName)
        {
            container = dbClient.GetContainer(databaseName, containerName);
        }
        public async Task<IEnumerable<FoodDetail>> GetItemsAsync(string queryString)
        {
            Console.WriteLine(queryString);
            stopwatch.Start();
            var query = container.GetItemQueryIterator<FoodDetail>(new QueryDefinition(queryString));
            List<FoodDetail> results = new List<FoodDetail>();
            double requestCharge = 0;
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                results.AddRange(response.ToList());
                requestCharge += response.RequestCharge;
            }
            stopwatch.Stop();
            Console.WriteLine($"Total Request Units consumed: { requestCharge} RUs");
            Console.WriteLine($"Total time: {stopwatch.Elapsed.TotalMilliseconds} ms");
            return results;
        }

        public async Task<FoodDetail> GetItemAsync(string id, string partitionKey)
        {
            if (string.IsNullOrEmpty(id)) return null;
            try
            {
                stopwatch.Start();
                ItemResponse<FoodDetail> response = await container.ReadItemAsync<FoodDetail>(id, new PartitionKey(partitionKey));
                stopwatch.Stop();
                Console.WriteLine($"Total Request Units consumed: { response.RequestCharge} RUs");
                Console.WriteLine($"Total time: {stopwatch.Elapsed.TotalMilliseconds} ms");
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode ==  HttpStatusCode.NotFound)
            {
                return null;
            }
            
        }

        public async Task AddItemAsync(FoodDetail item)
        {
            stopwatch.Start();
            item.Id = Guid.NewGuid().ToString();
            var response =  await container.CreateItemAsync<FoodDetail>(item, new PartitionKey(item.FoodGroup));
            stopwatch.Stop();
            Console.WriteLine($"Total Request Units consumed: { response.RequestCharge} RUs");
            Console.WriteLine($"Total time: {stopwatch.Elapsed.TotalMilliseconds} ms");
        }

        public async Task UpdateItemAsync(FoodDetail item)
        {
            stopwatch.Start();
            Console.WriteLine($"PartitionKey {item.FoodGroup}");
            var response = await container.UpsertItemAsync<FoodDetail>(item, new PartitionKey(item.FoodGroup));
            stopwatch.Stop();
            Console.WriteLine($"Total Request Units consumed: { response.RequestCharge} RUs");
            Console.WriteLine($"Total time: {stopwatch.Elapsed.TotalMilliseconds} ms");
        }

        public async Task DeleteItemAsync(string id, string partitionKey)
        {
             
            try
            {
                await container.DeleteItemAsync<FoodDetail>(id, new PartitionKey(partitionKey));
            }
            catch(Exception ex)
            {
                //throw new ArgumentException("Can not find item with this id");
                return ;
            }
        }
        public async Task UpdatePartialAsync(FoodDetail item)
        {
            var patchOperations = new List<PatchOperation>();
            //patchOperations.Add(PatchOperation.Add("/nonExistentParent/Child", "bar"));
            //patchOperations.Add(PatchOperation.Remove("/cost"));
            //patchOperations.Add(PatchOperation.Increment("/taskNum", 6));
            patchOperations.Add(PatchOperation.Set("/description", item.Description));
            stopwatch.Start();
            var response= await container.PatchItemAsync<FoodDetail>(
                            id: item.Id,
                            partitionKey: new PartitionKey(item.FoodGroup),
                            patchOperations: patchOperations);
            stopwatch.Stop();
            Console.WriteLine($"Total Request Units consumed: { response.RequestCharge} RUs");
            Console.WriteLine($"Total time: {stopwatch.Elapsed.TotalMilliseconds} ms");
        }
    }
}
