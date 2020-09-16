using ECommerce.Domain.Order;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace ECommerce.Entities
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class OrderEntity : IOrder
    {
        [JsonProperty]
        public List<OrderItem> Orders { get; set; } = new List<OrderItem>();

        public async Task<bool> AddAsync(OrderItem order)
        {
            Orders.Add(order);
            return await Task.FromResult(true);
        }

        public async Task<List<OrderItem>> Get()
        {
            return await Task.FromResult(Orders);
        }

        public async Task<OrderItem> GetOrder(string orderId)
        {
            return await Task.FromResult(Orders.SingleOrDefault(x => x.Id.ToString().Equals(orderId)));
        }

        // Boilerplate (entry point for the functions runtime)
        [FunctionName(nameof(OrderEntity))]
        public static async Task HandleEntityOperation([EntityTrigger] IDurableEntityContext context)
        {
            await context.DispatchAsync<OrderEntity>();
        }
    }
}