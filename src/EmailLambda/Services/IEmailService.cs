using EmailLambda.Models;

namespace EmailLambda.Services;

/// <summary>
/// Interface for email service operations
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an order confirmation email to the customer
    /// </summary>
    /// <param name="orderEvent">The order event containing customer and order details</param>
    /// <returns>Task representing the asynchronous operation</returns>
    Task SendOrderConfirmationAsync(OrderEvent orderEvent);
}
