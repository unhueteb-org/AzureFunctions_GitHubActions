using ECommerce.Domain.Inventory;
using ECommerce.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECommerce.PublicRest
{
    public static class StoreApi
    {
        [FunctionName("StoreGet")]
        public static async Task<IActionResult> StoreGet(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "store")] HttpRequest req,
            [DurableClient] IDurableClient client,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var target = new EntityId(nameof(InventoryEntity), "onestore");
            var store = await client.ReadEntityStateAsync<InventoryEntity>(target);

            return new JsonResult(store.EntityState?.Items ?? new List<InventoryItem>());
        }

        [FunctionName("StoreReset")]
        public static async Task<IActionResult> StoreReset(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "store/reset")] HttpRequest req,
            [DurableClient] IDurableClient client,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var target = new EntityId(nameof(InventoryEntity), "onestore");
            var store = await client.ReadEntityStateAsync<InventoryEntity>(target);

            if (store.EntityState != null && store.EntityState.Items.Count > 0)
            {
                await client.SignalEntityAsync<IInventory>(target, async x => await x.DeleteInventoryAsync());
            }
            
            return new JsonResult(store.EntityState?.Items ?? new List<InventoryItem>());
        }

        [FunctionName("StoreValidateProducts")]
        public static async Task<IActionResult> StoreValidateProducts([HttpTrigger(AuthorizationLevel.Function, "get", Route = "store/validateproducts")] HttpRequest req,
            [DurableClient] IDurableClient client,
            ExecutionContext context,
            ILogger log)
        {
            log.LogInformation("Starting the Store Validate Products method");

            var secretValue = Environment.GetEnvironmentVariable("ThirdPartyAPIFromKeyVault", EnvironmentVariableTarget.Process);

            //TODO: Use the secret - DO NOT display or log it.  The only reason this is here is to prove it's working for the challenge :)
            log.LogInformation($"Secret Value: {secretValue}");

            //Use the secret to validate the product:
            //FUTURE: Use the api secret to make a call to the third-party API for validation of products.
            //FORNOW: just return dummy product:
            var item = new InventoryItem("Validated Product", 9.99m, 2000, 500, 10000, Guid.NewGuid().ToString(), false);

            return new JsonResult(item);
        }
    }
}
