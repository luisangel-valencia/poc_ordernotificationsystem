using OrderLambda.Models;

namespace OrderLambda.Services;

public interface IOrderService
{
    Task SaveOrderAsync(Order order);
}
