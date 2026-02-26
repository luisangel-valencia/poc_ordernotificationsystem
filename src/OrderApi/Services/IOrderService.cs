using OrderApi.Models;

namespace OrderApi.Services;

public interface IOrderService
{
    Task SaveOrderAsync(Order order);
}
