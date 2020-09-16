using ECommerce.Domain.Inventory;
using ECommerce.Domain.ShoppingCart;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECommerce.Entities
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class ShoppingCartEntity : IShoppingCart
    {
        [JsonProperty]
        public List<string> Items { get; set; } = new List<string>();
        
        public async Task AddItemAsync(string itemId)
        {
            await Task.Run(() => Items.Add(itemId));
        }

        public async Task RemoveItemAsync(string itemId)
        {
            if (Items.Contains(itemId))
            {
                await Task.Run(() => Items.Remove(itemId));
            }
        }

        public async Task<IEnumerable<string>> GetItemsAsync()
        {
            return await Task.Run(() => Task.FromResult<IEnumerable<string>>(this.Items));
        }

        public async Task ResetCart()
        {
            await Task.Run(() => Items = new List<string>());
        }

        // Boilerplate (entry point for the functions runtime)
        [FunctionName(nameof(ShoppingCartEntity))]
        public static Task HandleEntityOperation([EntityTrigger] IDurableEntityContext context)
        {
            return context.DispatchAsync<ShoppingCartEntity>();
        }
    }
}