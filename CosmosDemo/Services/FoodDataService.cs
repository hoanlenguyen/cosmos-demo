using CosmosDemo.Models;
using CosmosDemo.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace CosmosDemo.Services
{
    public static class FoodDataService
    {
        public static void AddFoodDataService(this WebApplication app)
        {
            app.MapGet("food/all", async (IFoodDataRepository foodDataRepository, [FromQuery] int? size) =>
            {
                if (size is null) size = 10;
                return Results.Ok(await foodDataRepository.GetItemsAsync($"SELECT * FROM c offset 0 limit {size}"));
            });

            app.MapGet("food",
            async (IFoodDataRepository foodDataRepository,
            [FromQuery] string id,
            [FromQuery] string partitionKey) =>
            {
                return Results.Ok(await foodDataRepository.GetItemAsync(id, partitionKey));
            });

            app.MapGet("food/GetByQuery",
            async (IFoodDataRepository foodDataRepository,
            [FromQuery] string query) =>
            {
                return Results.Ok(await foodDataRepository.GetItemsAsync(query));
            });

            app.MapPut("food",
            async (IFoodDataRepository foodDataRepository,
            [FromBody] FoodDetail input) =>
            {
                await foodDataRepository.UpdateItemAsync(input);
                return Results.Ok();
            });

            app.MapPut("food/UpdatePartial",
            async (IFoodDataRepository foodDataRepository,
            [FromBody] FoodDetail input) =>
            {
                await foodDataRepository.UpdatePartialAsync(input);
                return Results.Ok();
            });

            app.MapGet("food/paging",
            async (
            IFoodDataRepository foodDataRepository,
            [FromQuery] int page,
            [FromQuery] int rowsPerPage,
            [FromQuery] string? partitionKey
            ) =>
            {
                if (page <= 0) page = 1;
                var skipCount = (page - 1) * rowsPerPage;

                var result = await foodDataRepository.QueryWithContinuationTokens(rowsPerPage, skipCount, partitionKey);
                //var result = await foodDataRepository.QueryWithOffsetLimit(rowsPerPage,skipCount,partitionKey);
                return Results.Ok(result);
            });
        }
    }
}