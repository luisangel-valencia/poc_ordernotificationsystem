# Order Processing MAUI Mobile App

This is a cross-platform mobile application built with .NET MAUI for submitting orders to the AWS Order Processing POC system.

## Features

- Cross-platform support (iOS and Android)
- Order entry form with validation
- Real-time validation feedback
- HTTP client with retry logic (2 retries with exponential backoff)
- 30-second timeout for API requests
- Success confirmation with Order ID
- Error handling for validation errors (HTTP 400) and server errors (HTTP 500)

## Prerequisites

- .NET 10 SDK or later
- Visual Studio 2022 (Windows) or Visual Studio for Mac with MAUI workload installed
- For Android: Android SDK
- For iOS: Xcode (Mac only)

## Configuration

Before running the app, you need to configure the API endpoint:

1. Open `MainPage.xaml.cs`
2. Update the `ApiEndpoint` constant with your actual API Gateway URL:

```csharp
private const string ApiEndpoint = "https://your-api-gateway-url.execute-api.us-east-1.amazonaws.com/dev";
```

You can find your API Gateway URL in:
- AWS Console: API Gateway → Your API → Stages → dev
- CloudFormation Outputs: `ApiEndpoint` value
- After deployment: Check the deployment logs

## Building the App

### Android

```bash
dotnet build mobile/MauiApp/MauiApp.csproj -f net10.0-android
```

### iOS (Mac only)

```bash
dotnet build mobile/MauiApp/MauiApp.csproj -f net10.0-ios
```

## Running the App

### Android Emulator

```bash
dotnet run --project mobile/MauiApp/MauiApp.csproj -f net10.0-android
```

### iOS Simulator (Mac only)

```bash
dotnet run --project mobile/MauiApp/MauiApp.csproj -f net10.0-ios
```

### Visual Studio

1. Open the solution in Visual Studio
2. Set `MauiApp` as the startup project
3. Select your target platform (Android or iOS)
4. Press F5 to run

## Using the App

1. **Enter Customer Information**:
   - Customer Name (minimum 2 characters)
   - Customer Email (valid email format)

2. **Enter Order Items**:
   - Product ID (required)
   - Product Name (optional)
   - Quantity (positive integer)
   - Price (positive decimal)

3. **Submit Order**:
   - Click "Submit Order" button
   - The app will validate input and show errors if any
   - On success, you'll see the Order ID
   - On validation error (HTTP 400), specific errors will be displayed
   - On server error (HTTP 500), an error message will be shown
   - The app will retry up to 2 times with exponential backoff for network errors

## Validation Rules

The app validates input according to the system requirements:

- **Customer Name**: Minimum 2 characters, maximum 100 characters
- **Customer Email**: Valid email format (RFC 5322)
- **Product ID**: Required, non-empty string
- **Quantity**: Positive integer greater than 0
- **Price**: Positive decimal greater than 0

## Error Handling

The app handles three types of responses:

1. **HTTP 200 (Success)**:
   - Displays success message with Order ID
   - Clears the form for next order
   - Shows confirmation alert

2. **HTTP 400 (Validation Error)**:
   - Displays validation errors from server
   - Shows which fields failed validation
   - User can correct and resubmit

3. **HTTP 500 (Server Error)**:
   - Displays error message
   - Automatically retries up to 2 times with exponential backoff
   - Shows final error if all retries fail

## Retry Logic

The app implements retry logic for network failures and server errors:

- **Max Retries**: 2
- **Timeout**: 30 seconds per request
- **Backoff Strategy**: Exponential (2s, 4s)
- **Retry Conditions**: HTTP 500, network errors, timeouts

## Project Structure

```
MauiApp/
├── Models/
│   ├── Order.cs              # Order and OrderItem models
│   └── OrderResponse.cs      # API response models
├── Services/
│   └── OrderApiClient.cs     # HTTP client with retry logic
├── Platforms/
│   ├── Android/              # Android-specific code
│   └── iOS/                  # iOS-specific code
├── Resources/
│   ├── Styles/               # XAML styles and colors
│   ├── Fonts/                # Custom fonts
│   └── Images/               # App images
├── App.xaml                  # Application resources
├── AppShell.xaml             # Shell navigation
├── MainPage.xaml             # Order entry UI
├── MainPage.xaml.cs          # Order submission logic
└── MauiProgram.cs            # App configuration

```

## Known Limitations

- Currently supports only one item per order (can be extended to support multiple items)
- No offline support (requires active internet connection)
- No order history or tracking
- No authentication/authorization

## Future Enhancements

- Support for multiple items per order
- Order history and tracking
- Offline mode with sync
- User authentication
- Push notifications for order status updates
- Barcode scanning for Product ID
- Camera integration for product images

## Troubleshooting

### "Cannot connect to API"
- Verify the API endpoint URL is correct
- Check your internet connection
- Ensure the API Gateway is deployed and accessible
- Check CloudWatch logs for API errors

### "Request timed out"
- Check your network connection
- Verify the Lambda functions are not cold starting (first request may be slow)
- Increase timeout if needed (currently 30 seconds)

### Build Errors
- Ensure .NET 8 SDK is installed
- Restore NuGet packages: `dotnet restore`
- Clean and rebuild: `dotnet clean && dotnet build`

## Support

For issues or questions, please refer to the main project README or create an issue in the repository.
