using FluentValidation;
using OrderApi.Models;

namespace OrderApi.Validators;

public class OrderValidator : AbstractValidator<Order>
{
    public OrderValidator()
    {
        // Customer Name validation
        RuleFor(x => x.CustomerName)
            .NotEmpty()
            .WithMessage("Customer name is required")
            .MinimumLength(2)
            .WithMessage("Customer name must be at least 2 characters")
            .MaximumLength(100)
            .WithMessage("Customer name must not exceed 100 characters");

        // Customer Email validation
        RuleFor(x => x.CustomerEmail)
            .NotEmpty()
            .WithMessage("Customer email is required")
            .EmailAddress()
            .WithMessage("Customer email must be a valid email address");

        // Items validation
        RuleFor(x => x.Items)
            .NotNull()
            .WithMessage("Order items are required")
            .NotEmpty()
            .WithMessage("Order must contain at least one item")
            .Must(items => items.Count <= 50)
            .WithMessage("Order cannot contain more than 50 items");

        // OrderItem validation
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId)
                .NotEmpty()
                .WithMessage("Product ID is required");

            item.RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithMessage("Quantity must be greater than 0")
                .LessThanOrEqualTo(1000)
                .WithMessage("Quantity cannot exceed 1000");

            item.RuleFor(x => x.Price)
                .GreaterThan(0)
                .WithMessage("Price must be greater than 0")
                .LessThanOrEqualTo(999999.99m)
                .WithMessage("Price cannot exceed 999999.99");
        });
    }
}
