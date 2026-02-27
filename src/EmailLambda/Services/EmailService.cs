using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using EmailLambda.Models;
using Microsoft.Extensions.Logging;
using System.Text;

namespace EmailLambda.Services;

/// <summary>
/// Service for sending emails using Amazon SES
/// </summary>
public class EmailService : IEmailService
{
    private readonly IAmazonSimpleEmailService _sesClient;
    private readonly ILogger<EmailService> _logger;
    private readonly string _fromEmail;
    private readonly string _htmlTemplate;

    public EmailService(IAmazonSimpleEmailService sesClient, ILogger<EmailService> logger)
    {
        _sesClient = sesClient;
        _logger = logger;
        _fromEmail = Environment.GetEnvironmentVariable("EMAIL_FROM") ?? "luisangel.valencia@globant.com";
        
        // Load HTML template
        var templatePath = Path.Combine(AppContext.BaseDirectory, "Templates", "OrderConfirmation.html");
        _htmlTemplate = File.Exists(templatePath) 
            ? File.ReadAllText(templatePath)
            : GetDefaultTemplate();
    }

    public async Task SendOrderConfirmationAsync(OrderEvent orderEvent)
    {
        try
        {
            _logger.LogInformation("Sending order confirmation email for OrderId: {OrderId} to {Email}", 
                orderEvent.OrderId, orderEvent.CustomerEmail);

            var emailBody = GenerateEmailContent(orderEvent);
            var subject = $"Order Confirmation - Order #{orderEvent.OrderId}";

            var sendRequest = new SendEmailRequest
            {
                Source = _fromEmail,
                Destination = new Destination
                {
                    ToAddresses = new List<string> { orderEvent.CustomerEmail }
                },
                Message = new Message
                {
                    Subject = new Content(subject),
                    Body = new Body
                    {
                        Html = new Content
                        {
                            Charset = "UTF-8",
                            Data = emailBody
                        }
                    }
                }
            };

            var response = await _sesClient.SendEmailAsync(sendRequest);
            
            _logger.LogInformation("Email sent successfully for OrderId: {OrderId}, MessageId: {MessageId}", 
                orderEvent.OrderId, response.MessageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email for OrderId: {OrderId} to {Email}", 
                orderEvent.OrderId, orderEvent.CustomerEmail);
            throw;
        }
    }

    private string GenerateEmailContent(OrderEvent orderEvent)
    {
        var orderItemsHtml = new StringBuilder();
        foreach (var item in orderEvent.Items)
        {
            orderItemsHtml.AppendLine($@"
                <tr>
                    <td>{item.ProductName}</td>
                    <td>{item.Quantity}</td>
                    <td>${item.Price:F2}</td>
                    <td>${item.Subtotal:F2}</td>
                </tr>");
        }

        var emailContent = _htmlTemplate
            .Replace("{{CustomerName}}", orderEvent.CustomerName)
            .Replace("{{OrderId}}", orderEvent.OrderId)
            .Replace("{{CreatedAt}}", orderEvent.CreatedAt)
            .Replace("{{OrderItems}}", orderItemsHtml.ToString())
            .Replace("{{TotalAmount}}", orderEvent.TotalAmount.ToString("F2"));

        return emailContent;
    }

    private static string GetDefaultTemplate()
    {
        return @"
<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; }
        .header { background-color: #4CAF50; color: white; padding: 20px; text-align: center; }
        .content { padding: 20px; }
        table { width: 100%; border-collapse: collapse; }
        th, td { padding: 10px; text-align: left; border-bottom: 1px solid #ddd; }
    </style>
</head>
<body>
    <div class='header'><h1>Order Confirmation</h1></div>
    <div class='content'>
        <p>Dear {{CustomerName}},</p>
        <p>Thank you for your order!</p>
        <p><strong>Order ID:</strong> {{OrderId}}</p>
        <p><strong>Order Date:</strong> {{CreatedAt}}</p>
        <h3>Order Items:</h3>
        <table>
            <tr><th>Product</th><th>Quantity</th><th>Price</th><th>Subtotal</th></tr>
            {{OrderItems}}
        </table>
        <p><strong>Total: ${{TotalAmount}}</strong></p>
    </div>
</body>
</html>";
    }
}
