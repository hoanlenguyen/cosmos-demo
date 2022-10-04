namespace CosmosDemo.Models
{
    using System.Text.Json.Serialization;

    public class FoodDetail
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("tags")]
        public List<Tag> Tags { get; set; }

        [JsonPropertyName("version")]
        public int Version { get; set; }

        [JsonPropertyName("foodGroup")]
        public string FoodGroup { get; set; }

        [JsonPropertyName("nutrients")]
        public List<Nutrient> Nutrients { get; set; }

        [JsonPropertyName("servings")]
        public List<Serving> Servings { get; set; }
    }

    public class Nutrient
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("nutritionValue")]
        public double NutritionValue { get; set; }

        [JsonPropertyName("units")]
        public string Units { get; set; }
    }

    public class Serving
    {
        [JsonPropertyName("amount")]
        public double Amount { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("weightInGrams")]
        public double WeightInGrams { get; set; }
    }

    public class Tag
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}