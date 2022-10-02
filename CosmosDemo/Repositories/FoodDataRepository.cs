using CosmosDemo.Models;
using Microsoft.Azure.Cosmos;

namespace CosmosDemo.Repositories
{
    public interface IFoodDataRepository
    {
        Task<IEnumerable<FoodDetail>> GetItemsAsync(string stringQuery);

        Task<FoodDetail> GetItemAsync(string id);

        Task AddItemAsync(FoodDetail item);

        Task UpdateItemAsync(FoodDetail item);

        Task DeleteItemAsync(string id);
    }
    public class FoodDataRepository : IFoodDataRepository
    {
        private Container _container;
        public FoodDataRepository(
            CosmosClient dbClient,
            string databaseName,
            string containerName)
        {
            _container = dbClient.GetContainer(databaseName, containerName);
        }
        public async Task<IEnumerable<FoodDetail>> GetItemsAsync(string queryString)
        {
            var query = this._container.GetItemQueryIterator<FoodDetail>(new QueryDefinition(queryString));
            List<FoodDetail> results = new List<FoodDetail>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();

                results.AddRange(response.ToList());
            }

            return results;
        }

        public async Task<FoodDetail> GetItemAsync(string id)
        {
            if (id == null) return null;
            try
            {
                ItemResponse<FoodDetail> response = await this._container.ReadItemAsync<FoodDetail>(id, new PartitionKey(id));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task AddItemAsync(FoodDetail item)
        {
            item.Id = Guid.NewGuid().ToString();
            await this._container.CreateItemAsync<FoodDetail>(item, new PartitionKey(item.Id));
        }

        public async Task UpdateItemAsync(FoodDetail item)
        {
            await this._container.UpsertItemAsync<FoodDetail>(item, new PartitionKey(item.Id));
        }

        public async Task DeleteItemAsync(string id)
        {
            if (id == null) return;

            var item = await GetItemAsync(id);

            if (item != null)
            {
                await this._container.DeleteItemAsync<FoodDetail>(id, new PartitionKey(id));
            }
            else
            {
                throw new ArgumentException("Can not find item with this id");
            }
        }
    }
}
