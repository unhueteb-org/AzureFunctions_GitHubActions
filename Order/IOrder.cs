using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECommerce.Domain.Order
{
    public interface IOrder
    {
        Task<List<OrderItem>> Get();
        Task<OrderItem> GetOrder(string id);
        Task<bool> AddAsync(OrderItem order);
    }
}
