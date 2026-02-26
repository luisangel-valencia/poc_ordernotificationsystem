using Shared.Models;

namespace EmailLambda.Models;

/// <summary>
/// Model for deserializing order events from SQS messages
/// </summary>
public class OrderEvent
{
    public string OrderId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public List<OrderItemDto> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
}
