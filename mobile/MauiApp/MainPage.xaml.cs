using MauiApp.Models;
using MauiApp.Services;

namespace MauiApp;

public partial class MainPage : ContentPage
{
    private readonly OrderApiClient _apiClient;
    
    // TODO: Replace with your actual API Gateway endpoint URL
    private const string ApiEndpoint = "https://4kgjdf2avf.execute-api.us-east-2.amazonaws.com/dev";

    public MainPage()
    {
        InitializeComponent();
        _apiClient = new OrderApiClient(ApiEndpoint);
    }

    private void OnInputChanged(object sender, TextChangedEventArgs e)
    {
        // Clear validation errors when user starts typing
        ClearValidationErrors();
    }

    private void ClearValidationErrors()
    {
        CustomerNameError.IsVisible = false;
        CustomerEmailError.IsVisible = false;
        ProductIdError.IsVisible = false;
        QuantityError.IsVisible = false;
        PriceError.IsVisible = false;
        StatusLabel.Text = "";
        StatusLabel.TextColor = Colors.Black;
    }

    private bool ValidateInput()
    {
        bool isValid = true;
        ClearValidationErrors();

        // Validate customer name
        if (string.IsNullOrWhiteSpace(CustomerNameEntry.Text) || CustomerNameEntry.Text.Length < 2)
        {
            CustomerNameError.Text = "Customer name must be at least 2 characters";
            CustomerNameError.IsVisible = true;
            isValid = false;
        }

        // Validate customer email
        if (string.IsNullOrWhiteSpace(CustomerEmailEntry.Text) || !IsValidEmail(CustomerEmailEntry.Text))
        {
            CustomerEmailError.Text = "Please enter a valid email address";
            CustomerEmailError.IsVisible = true;
            isValid = false;
        }

        // Validate product ID
        if (string.IsNullOrWhiteSpace(ProductIdEntry.Text))
        {
            ProductIdError.Text = "Product ID is required";
            ProductIdError.IsVisible = true;
            isValid = false;
        }

        // Validate quantity
        if (!int.TryParse(QuantityEntry.Text, out int quantity) || quantity <= 0)
        {
            QuantityError.Text = "Quantity must be a positive number";
            QuantityError.IsVisible = true;
            isValid = false;
        }

        // Validate price
        if (!decimal.TryParse(PriceEntry.Text, out decimal price) || price <= 0)
        {
            PriceError.Text = "Price must be a positive number";
            PriceError.IsVisible = true;
            isValid = false;
        }

        return isValid;
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private async void OnSubmitClicked(object sender, EventArgs e)
    {
        // Validate input first
        if (!ValidateInput())
        {
            StatusLabel.Text = "Please fix the validation errors above";
            StatusLabel.TextColor = Colors.Red;
            return;
        }

        // Show loading indicator
        LoadingIndicator.IsRunning = true;
        LoadingIndicator.IsVisible = true;
        SubmitBtn.IsEnabled = false;
        StatusLabel.Text = "Submitting order...";
        StatusLabel.TextColor = Colors.Gray;

        try
        {
            // Build the order object
            var order = new Order
            {
                CustomerName = CustomerNameEntry.Text,
                CustomerEmail = CustomerEmailEntry.Text,
                Items = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ProductId = ProductIdEntry.Text,
                        ProductName = ProductNameEntry.Text,
                        Quantity = int.Parse(QuantityEntry.Text),
                        Price = decimal.Parse(PriceEntry.Text)
                    }
                }
            };

            // Submit the order
            var result = await _apiClient.SubmitOrderAsync(order);

            if (result.Success)
            {
                // Display success message with order ID (HTTP 200)
                StatusLabel.Text = $"✓ Order submitted successfully!\nOrder ID: {result.OrderId}";
                StatusLabel.TextColor = Colors.Green;
                
                // Clear the form
                ClearForm();
                
                // Show success alert
                await DisplayAlert("Success", $"Your order has been submitted successfully!\n\nOrder ID: {result.OrderId}", "OK");
            }
            else
            {
                // Check if there are validation errors (HTTP 400)
                if (result.ValidationErrors != null && result.ValidationErrors.Any())
                {
                    // Display validation errors from server
                    var errorMessage = string.Join("\n", result.ValidationErrors);
                    StatusLabel.Text = $"✗ Validation failed:\n{errorMessage}";
                    StatusLabel.TextColor = Colors.Red;
                    
                    await DisplayAlert("Validation Error", errorMessage, "OK");
                }
                else
                {
                    // Display error message (HTTP 500 or other errors)
                    StatusLabel.Text = $"✗ Error: {result.Message}";
                    StatusLabel.TextColor = Colors.Red;
                    
                    await DisplayAlert("Error", result.Message ?? "An error occurred while submitting the order", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            // Handle unexpected errors
            StatusLabel.Text = $"✗ Unexpected error: {ex.Message}";
            StatusLabel.TextColor = Colors.Red;
            
            await DisplayAlert("Error", $"An unexpected error occurred: {ex.Message}", "OK");
        }
        finally
        {
            // Hide loading indicator
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
            SubmitBtn.IsEnabled = true;
        }
    }

    private void ClearForm()
    {
        CustomerNameEntry.Text = string.Empty;
        CustomerEmailEntry.Text = string.Empty;
        ProductIdEntry.Text = string.Empty;
        ProductNameEntry.Text = string.Empty;
        QuantityEntry.Text = string.Empty;
        PriceEntry.Text = string.Empty;
        ClearValidationErrors();
    }
}
