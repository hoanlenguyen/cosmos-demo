using System.Text.Json.Serialization;

namespace CosmosDemo.Models
{
    public abstract class BasePaging<T> where T : class
    {
        public BasePaging()
        {
        }

        public BasePaging(int totalItems, List<T> items)
        {
            TotalItems = totalItems;
            Items = items;
        }

        public int TotalItems { get; set; }
        public List<T> Items { get; set; }=new List<T>();
    }

    public class PagedResultDto<T> : BasePaging<T> where T : class
    {
        public PagedResultDto() : base()
        {
        }

        public PagedResultDto(int totalItems, List<T> items) : base(totalItems, items)
        {
        }
    }

    public class CountResult 
    {
        [JsonPropertyName("count")]
        public int Count { get; set; }
    }
}
