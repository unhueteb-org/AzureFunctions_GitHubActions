using ECommerce.Domain.Inventory;
using System;
using System.Collections.Generic;

namespace ECommerce.Domain.Order
{
    public class OrderItem
    {
        //order ids are unique per order
        public string Id { get; set; }

        //users can have multiple orders.
        public string UserId { get; set; }

        public DateTime Timestamp { get; set; }

        public IEnumerable<string> Details { get; set; }

        public IEnumerable<InventoryItem> OrderItems { get; set; }
    }
}
