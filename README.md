# AWS Order Processing POC

Event-driven order processing system built with AWS Lambda, DynamoDB, SNS/SQS, and .NET 8.

## Architecture

This system demonstrates modern cloud-native patterns:
- **API Gateway** for REST endpoints
- **Lambda functions** (.NET 8) for serverless compute
- **DynamoDB** for NoSQL data persistence
- **SNS/SQS** for event-driven messaging with fan-out pattern
- **CloudFormation** for infrastructure as code
- **GitHub Actions** for CI/CD automation

## Project Structure

```
├── src/
│   ├── Shared/              # Common models and logging utilities
│   ├── OrderApi/            # Order submission Lambda
│   ├── EmailLambda/         # Email notification Lambda
│   └── AuditLambda/         # Audit logging Lambda
├── tests/                   # Unit and property-based tests
├── infrastructure/          # CloudFormation templates
└── mobile/                  # .NET MAUI mobile app
```

## Prerequisites

- .NET 8 SDK
- AWS CLI configured with credentials
- AWS account with appropriate permissions

## Getting Started

1. Restore dependencies:
   ```bash
   dotnet restore
   ```

2. Build the solution:
   ```bash
   dotnet build
   ```

3. Run tests:
   ```bash
   dotnet test
   ```

## Deployment

Deployment is automated via GitHub Actions. Push to `main` branch triggers:
1. Build all Lambda projects
2. Run unit and property-based tests
3. Package Lambda functions
4. Deploy CloudFormation stack
5. Update Lambda function code

## Local Development

See [docs/deployment.md](docs/deployment.md) for detailed setup instructions.

## License

MIT
