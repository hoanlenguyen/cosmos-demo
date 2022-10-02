namespace CosmosDemo.Models
{
    using Newtonsoft.Json;

    public class FoodDetail
    {
        [JsonProperty("id")]
        public string Id { get; set; }= Guid.NewGuid().ToString();

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("tags")]
        public List<Tag> Tags { get; set; }

        [JsonProperty("version")]
        public int Version { get; set; }

        [JsonProperty("foodGroup")]
        public string FoodGroup { get; set; }

        [JsonProperty("nutrients")]
        public List<Nutrient> Nutrients { get; set; }

        [JsonProperty("servings")]
        public List<Serving> Servings { get; set; }
    }

    public class Nutrient
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("nutritionValue")]
        public double NutritionValue { get; set; }

        [JsonProperty("units")]
        public string Units { get; set; }
    }

    public class Serving
    {
        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("weightInGrams")]
        public double WeightInGrams { get; set; }
    }

    public class Tag
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}