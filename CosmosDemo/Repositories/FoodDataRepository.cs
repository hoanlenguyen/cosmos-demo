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

        Task<PagedResultDto<FoodDetail>> QueryWithContinuationTokens(int rowsPerPage, int skipCount, string partitionKey);
        Task<PagedResultDto<FoodDetail>> QueryWithOffsetLimit(int rowsPerPage, int skipCount, string partitionKey);
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

        public async Task<PagedResultDto<FoodDetail>> QueryWithContinuationTokens(int rowsPerPage, int skipCount, string partitionKey)
        {
            var result = new PagedResultDto<FoodDetail>();

            var queryCountStr = string.IsNullOrEmpty(partitionKey) ?
                        $"SELECT COUNT(1) as 'count' FROM c" :
                        $"SELECT COUNT(1) as 'count' FROM c where c.foodGroup = '{partitionKey}'";
            var queryCount = container.GetItemQueryIterator<CountResult>(new QueryDefinition(queryCountStr));
            while (queryCount.HasMoreResults)
            {
                var response = await queryCount.ReadNextAsync();
                var countResult = response.FirstOrDefault();
                result.TotalItems = countResult?.Count ?? 0;
            }


            var queryStr = string.IsNullOrEmpty(partitionKey) ?
                        $"SELECT c.id,c.foodGroup,c.description FROM c" :
                        $"SELECT c.id,c.foodGroup,c.description FROM c where c.foodGroup= '{partitionKey}'";

            QueryDefinition query = new QueryDefinition(queryStr);
            string continuation = null;
 
            using (FeedIterator<FoodDetail> resultSetIterator = container.GetItemQueryIterator<FoodDetail>(
                query,
                requestOptions: new QueryRequestOptions()
                {
                    MaxItemCount = rowsPerPage
                }))
            {
                // Execute query and get 1 item in the results. Then, get a continuation token to resume later
                while (resultSetIterator.HasMoreResults)
                {
                    if (skipCount > 0)
                    {
                        skipCount--;
                        continue;
                    }
                    FeedResponse<FoodDetail> response = await resultSetIterator.ReadNextAsync();                    
                    result.Items.AddRange(response);
                   
                    // Get continuation token once we've gotten > 0 results. 
                    if (response.Count > 0)
                    {
                        continuation = response.ContinuationToken;
                        break;
                    }
                }
            }

            // Check if query has already been fully drained
            if (continuation == null)
            {
                return result;
            }

            //// Resume query using continuation token
            //using (FeedIterator<FoodDetail> resultSetIterator = container.GetItemQueryIterator<FoodDetail>(
            //        query,
            //        requestOptions: new QueryRequestOptions()
            //        {
            //            MaxItemCount = rowsPerPage
            //        },
            //        continuationToken: continuation))
            //{
            //    while (resultSetIterator.HasMoreResults)
            //    {
            //        FeedResponse<FoodDetail> response = await resultSetIterator.ReadNextAsync();

            //        result.Items.AddRange(response);                    
            //    }
            //}
            return result;
        }

        public async Task<PagedResultDto<FoodDetail>> QueryWithOffsetLimit(int rowsPerPage, int skipCount, string partitionKey)
        {
            var result = new PagedResultDto<FoodDetail>();

            var queryCountStr = string.IsNullOrEmpty(partitionKey) ?
                        $"SELECT COUNT(1) as 'count' FROM c" :
                        $"SELECT COUNT(1) as 'count' FROM c where c.foodGroup = '{partitionKey}'";
            var queryCount = container.GetItemQueryIterator<CountResult>(new QueryDefinition(queryCountStr));
            while (queryCount.HasMoreResults)
            {
                var response = await queryCount.ReadNextAsync();
                var countResult = response.FirstOrDefault();
                result.TotalItems = countResult?.Count??0;
            }

            var queryStr = string.IsNullOrEmpty(partitionKey) ?
                        $"SELECT c.id, c.foodGroup, c.description FROM c OFFSET {skipCount} LIMIT {rowsPerPage}" :
                        $"SELECT c.id,c.foodGroup,c.description FROM c where c.foodGroup= '{partitionKey}' OFFSET {skipCount} LIMIT {rowsPerPage}";

            stopwatch.Start();
            var query = container.GetItemQueryIterator<FoodDetail>(new QueryDefinition(queryStr));
            double requestCharge = 0;
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                result.Items.AddRange(response.ToList());
                requestCharge += response.RequestCharge;
            }
            stopwatch.Stop();
            Console.WriteLine($"Total Request Units consumed: {requestCharge} RUs");
            Console.WriteLine($"Total time: {stopwatch.Elapsed.TotalMilliseconds} ms");
            return result;
        }

    }
}
