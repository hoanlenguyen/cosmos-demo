using CosmosDemo.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace CosmosDemo.Services
{
    public static class FoodDataService
    {
        public static void AddFoodDataService(this WebApplication app)
        {
            app.MapGet("food/all", [AllowAnonymous] async (IFoodDataRepository foodDataRepository) =>
            {
                return Results.Ok(await foodDataRepository.GetItemsAsync("SELECT * FROM c offset 0 limit 1"));
            });
        }
    }
}
