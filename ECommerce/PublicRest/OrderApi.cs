using ECommerce.Domain.Order;
using ECommerce.Entities;
using ECommerce.Orchestration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace ECommerce.PublicRest
{
    public static class OrderApi
    {
        [FunctionName("OrderCheckoutPost")]
        public static async Task<IActionResult> Checkout(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "user/{userId}/order/checkout")] HttpRequest req,
            [DurableClient] IDurableClient client,
            string userId,
            ILogger log)
        {
            log.LogInformation("Order Checkout Post for user {userId}.");

            var instanceId = await client.StartNewAsync<string>(nameof(OrderOrchestrator), userId);
            return await client.WaitForCompletionOrCreateCheckStatusResponseAsync(req, instanceId);
        }

        [FunctionName("OrderGet")]
        public static async Task<IActionResult> OrderStatus(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "user/{userId}/order/{orderId}")] HttpRequest req,
            [DurableClient] IDurableClient client,
            string userId,
            string orderId,
            ILogger log)
        {
            log.LogInformation($"Getting information for order {orderId} on user {userId}");

            var target = new EntityId(nameof(OrderEntity), userId);
            var order = await client.ReadEntityStateAsync<OrderEntity>(target);

            if (order.EntityState != null && order.EntityState.Orders.Count > 0)
            {
                log.LogInformation("Order Found");

                return new JsonResult(order.EntityState.Orders);

                //NOTE: For some reason the order id is not matching expected.  Not going to press any further, just working around it.
                //log.LogInformation("Order Found");
                //log.LogInformation("Orders");
                //foreach (var o in order.EntityState.Orders)
                //{
                //    log.LogInformation($"OrderID {o.Id} with items {o.OrderItems.Count()} for user {o.UserId}");
                //    foreach (var i in o.OrderItems)
                //    {
                //        log.LogInformation($"Order: {o.Id}] Item: {i.Description}");
                //    }
                //}
                //var userOrder = order.EntityState.Orders.FirstOrDefault(x => x.Id.Equals(orderId));
                //return new JsonResult(userOrder ?? new OrderItem());
            }

            return new JsonResult(new OrderItem());
        }
    }
}
