namespace MauiApp.Models;

public class OrderResponse
{
    public string? OrderId { get; set; }
    public string? Message { get; set; }
    public string? CreatedAt { get; set; }
}

public class ErrorResponse
{
    public string? Error { get; set; }
    public string? Message { get; set; }
    public List<ValidationError>? Errors { get; set; }
}

public class ValidationError
{
    public string? Field { get; set; }
    public string? Message { get; set; }
}
