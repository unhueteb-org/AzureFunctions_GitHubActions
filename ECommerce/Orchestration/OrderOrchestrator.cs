using ECommerce.Domain.Inventory;
using ECommerce.Domain.Order;
using ECommerce.Domain.ShoppingCart;
using ECommerce.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce.Orchestration
{
    public static class OrderOrchestrator
    {
        [FunctionName("OrderOrchestrator")]
        public static async Task<OrderItem> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var userId = context.GetInput<string>();
            var orderEntity = new EntityId(nameof(OrderEntity), userId);
            var shoppingCartEntity = new EntityId(nameof(ShoppingCartEntity), userId);
            var inventoryEntity = new EntityId(nameof(InventoryEntity), "onestore");

            var orderProxy = context.CreateEntityProxy<IOrder>(orderEntity);
            var shoppingCartProxy = context.CreateEntityProxy<IShoppingCart>(shoppingCartEntity);
            var inventoryProxy = context.CreateEntityProxy<IInventory>(inventoryEntity);

            var shoppingCartItems = await shoppingCartProxy.GetItemsAsync();
            var orderId = Guid.NewGuid().ToString();
            var orderItem = new OrderItem()
            {
                Id = orderId,
                Timestamp = DateTime.UtcNow,
                UserId = userId,
                Details = shoppingCartItems
            };

            var shoppingCart = await shoppingCartProxy.GetItemsAsync();

            var tasks = shoppingCart
                .Select(id => inventoryProxy.GetItemAsync(id))
                .ToList();
            await Task.WhenAll(tasks);

            var orderItems = tasks
                .Select(task => task.Result)
                .ToList();

            orderItem.OrderItems = orderItems;

            var groupedItems = orderItems.GroupBy(x => x.Description);

            var canSell = true;
            foreach (var item in groupedItems)
            {
                //ensure enough exist
                var enoughExist = item.Count() <= orderItems.FirstOrDefault(x => x.Description.Equals(item.Key))?.AvailableStock;
                if (!enoughExist) canSell = false;
            }

            if (canSell)
            {
                //add the order to the user's orders
                await orderProxy.AddAsync(orderItem);
                //clear the user shopping cart
                await shoppingCartProxy.ResetCart();

                return orderItem;
            }

            return new OrderItem();
        }

        //[FunctionName("OrderOrchestrator")]
        //public static async Task<bool> RunOrchestrator(
        //    [OrchestrationTrigger] IDurableOrchestrationContext context)
        //{
        //    var userId = context.GetInput<string>();
        //    var shoppingCartEntity = new EntityId(nameof(ShoppingCartEntity), userId);
        //    var inventoryEntity = new EntityId(nameof(InventoryEntity), "onestore");
        //    //order is for the user, remember that users can have multiple orders.
        //    var orderEntity = new EntityId(nameof(OrderEntity), userId);

        //    // Create a critical section to avoid race conditions.
        //    using (await context.LockAsync(inventoryEntity, orderEntity, shoppingCartEntity))
        //    {
        //        IShoppingCart shoppingCartProxy = 
        //            context.CreateEntityProxy<IShoppingCart>(shoppingCartEntity);
        //        IInventory inventoryProxy =
        //            context.CreateEntityProxy<IInventory>(inventoryEntity);
        //        IOrder orderProxy =
        //            context.CreateEntityProxy<IOrder>(orderEntity);

        //        var shoppingCartItems = await shoppingCartProxy.GetItemsAsync();
        //        var orderItem = new OrderItem()
        //        {
        //            Timestamp = DateTime.UtcNow,
        //            UserId = userId,
        //            Details = shoppingCartItems
        //        };

        //        var canSell = true;
        //        foreach (var inventoryItem in orderItem.Details)
        //        {
        //            if (await inventoryProxy.IsItemInInventory(inventoryItem))
        //            {
        //                await inventoryProxy.RemoveStockAsync(inventoryItem);
        //                await shoppingCartProxy.RemoveItemAsync(inventoryItem);
        //            }
        //            else
        //            {
        //                canSell = false;
        //                break;
        //            }
        //        }

        //        if (canSell)
        //        {
        //            await orderProxy.AddAsync(orderItem);
        //            // order placed successfully

        //            //TODO: Close out shopping cart
        //            await shoppingCartProxy.ResetCart();

        //            return true;
        //        }
        //        // the order failed due to insufficient stock
        //        return false;
        //    }
        //}
    }
}