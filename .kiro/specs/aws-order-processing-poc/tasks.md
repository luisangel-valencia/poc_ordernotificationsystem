# Implementation Plan: AWS Order Processing POC

## Overview

This implementation plan breaks down the AWS event-driven order processing system into discrete coding tasks. The system uses .NET 8 Lambda functions, DynamoDB for data persistence, SNS/SQS for event-driven messaging, API Gateway for REST endpoints, and CloudFormation for infrastructure as code. Tasks are organized to build incrementally, with testing integrated throughout.

## Tasks

- [x] 1. Set up project structure and shared components
  - Create solution file and project structure following the design
  - Create Shared.csproj with common models (OrderDto) and StructuredLogger
  - Set up .gitignore for .NET projects
  - Configure NuGet package references for AWS SDK and Lambda libraries
  - _Requirements: 6.5, 9.6_

- [x] 2. Implement Order API Lambda function
  - [x] 2.1 Create Order API project structure and models
    - Create OrderApi.csproj with required NuGet packages (Amazon.Lambda.Core, Amazon.Lambda.APIGatewayEvents, AWSSDK.DynamoDBv2, AWSSDK.SimpleNotificationService, FluentValidation)
    - Implement Order.cs and OrderItem.cs models with all required properties
    - Implement ApiResponse.cs for success and error responses
    - _Requirements: 1.1, 10.1, 10.2, 10.3, 10.4, 10.5, 10.6_
  
  - [x] 2.2 Implement order validation with FluentValidation
    - Create OrderValidator.cs implementing AbstractValidator<Order>
    - Add validation rules for CustomerName (min 2 chars, max 100 chars)
    - Add validation rules for CustomerEmail (valid email format)
    - Add validation rules for Items (min 1 item, max 50 items)
    - Add validation rules for OrderItem fields (ProductId required, Quantity > 0, Price > 0)
    - _Requirements: 10.1, 10.2, 10.3, 10.4, 10.5, 10.6, 10.7_
  
  - [ ]* 2.3 Write property tests for order validation
    - **Property 2: Invalid Order Rejection**
    - **Property 8: Email Validation**
    - **Property 9: Customer Name Validation**
    - **Property 10: Order Items Validation**
    - **Property 11: Order Item Required Fields**
    - **Property 12: Quantity Validation**
    - **Property 13: Price Validation**
    - **Validates: Requirements 1.4, 10.1, 10.2, 10.3, 10.4, 10.5, 10.6, 10.7**
  
  - [x] 2.4 Implement OrderService for DynamoDB operations
    - Create IOrderService.cs interface with SaveOrderAsync method
    - Implement OrderService.cs with DynamoDB PutItem operation
    - Generate unique OrderId using Guid.NewGuid()
    - Add CreatedAt timestamp in ISO8601 format
    - Handle DynamoDB exceptions and log errors
    - _Requirements: 2.1, 2.2, 2.3, 9.1_
  
  - [ ]* 2.5 Write property tests for order persistence
    - **Property 1: Valid Order Processing (persistence part)**
    - **Property 3: Order ID Uniqueness**
    - **Property 4: Order Timestamp Presence**
    - **Validates: Requirements 2.1, 2.2, 2.3**
  
  - [x] 2.6 Implement EventPublisher for SNS operations
    - Create EventPublisher.cs with PublishOrderEventAsync method
    - Serialize order to JSON and publish to SNS topic
    - Handle SNS exceptions and log errors
    - _Requirements: 3.1, 3.2, 9.1_
  
  - [ ]* 2.7 Write property tests for event publishing
    - **Property 1: Valid Order Processing (SNS part)**
    - **Property 5: SNS Fan-Out Delivery**
    - **Validates: Requirements 3.1, 3.2, 3.4**
  
  - [x] 2.8 Implement Lambda Function handler
    - Create Function.cs with FunctionHandler method accepting APIGatewayProxyRequest
    - Deserialize request body to Order object
    - Validate order using OrderValidator
    - Return HTTP 400 with validation errors if invalid
    - Call OrderService.SaveOrderAsync to persist order
    - Call EventPublisher.PublishOrderEventAsync to publish event
    - Return HTTP 200 with order confirmation on success
    - Return HTTP 500 on DynamoDB or SNS errors
    - Implement structured logging for all operations
    - _Requirements: 1.2, 1.3, 1.4, 2.4, 3.3, 9.1, 9.2, 9.5_
  
  - [ ]* 2.9 Write unit tests for Lambda handler
    - Test successful order processing flow
    - Test validation error handling
    - Test DynamoDB error handling
    - Test SNS error handling
    - _Requirements: 1.3, 1.4, 2.4, 3.3_

- [x] 3. Checkpoint - Verify Order API implementation
  - Ensure all tests pass, ask the user if questions arise.

- [x] 4. Implement Email Lambda function
  - [x] 4.1 Create Email Lambda project structure and models
    - Create EmailLambda.csproj with required NuGet packages (Amazon.Lambda.Core, Amazon.Lambda.SQSEvents, AWSSDK.SimpleEmail)
    - Implement OrderEvent.cs model for deserializing SQS messages
    - Create HTML email template (OrderConfirmation.html) with order details
    - _Requirements: 4.1, 4.2_
  
  - [x] 4.2 Implement EmailService for sending emails
    - Create IEmailService.cs interface with SendOrderConfirmationAsync method
    - Implement EmailService.cs using Amazon SES
    - Generate email content from HTML template with order details
    - Handle SES exceptions and log errors
    - _Requirements: 4.3, 9.3_
  
  - [ ]* 4.3 Write property tests for email processing
    - **Property 6: Email Lambda Message Processing**
    - **Validates: Requirements 4.2, 4.3**
  
  - [x] 4.4 Implement Lambda Function handler for SQS events
    - Create Function.cs with FunctionHandler method accepting SQSEvent
    - Parse order details from SQS message body
    - Call EmailService.SendOrderConfirmationAsync
    - Implement structured logging for all operations
    - Throw exception on failure to trigger SQS retry
    - _Requirements: 4.1, 4.2, 4.3, 4.4, 9.3, 9.5_
  
  - [ ]* 4.5 Write unit tests for Email Lambda handler
    - Test successful email sending
    - Test email service error handling and retry behavior
    - Test message parsing errors
    - _Requirements: 4.3, 4.4, 4.5_

- [x] 5. Implement Audit Lambda function
  - [x] 5.1 Create Audit Lambda project structure and models
    - Create AuditLambda.csproj with required NuGet packages (Amazon.Lambda.Core, Amazon.Lambda.SQSEvents, AWSSDK.DynamoDBv2)
    - Implement AuditRecord.cs and OrderEvent.cs models
    - _Requirements: 5.1, 5.2_
  
  - [x] 5.2 Implement AuditService for DynamoDB operations
    - Create IAuditService.cs interface with CreateAuditRecordAsync method
    - Implement AuditService.cs with DynamoDB PutItem operation
    - Generate unique AuditId using Guid.NewGuid()
    - Add Timestamp in ISO8601 format
    - Set EventType to "ORDER_CREATED"
    - Handle DynamoDB exceptions and log errors
    - _Requirements: 5.2, 5.3, 9.4_
  
  - [ ]* 5.3 Write property tests for audit record creation
    - **Property 7: Audit Lambda Record Creation**
    - **Validates: Requirements 5.2, 5.3**
  
  - [x] 5.4 Implement Lambda Function handler for SQS events
    - Create Function.cs with FunctionHandler method accepting SQSEvent
    - Parse order details from SQS message body
    - Call AuditService.CreateAuditRecordAsync
    - Implement structured logging for all operations
    - Throw exception on failure to trigger SQS retry
    - _Requirements: 5.1, 5.2, 5.3, 5.4, 9.4, 9.5_
  
  - [ ]* 5.5 Write unit tests for Audit Lambda handler
    - Test successful audit record creation
    - Test DynamoDB error handling and retry behavior
    - Test message parsing errors
    - _Requirements: 5.3, 5.4, 5.5_

- [ ] 6. Checkpoint - Verify Lambda functions implementation
  - Ensure all tests pass, ask the user if questions arise.

- [ ] 7. Create CloudFormation infrastructure template
  - [ ] 7.1 Create CloudFormation template structure and parameters
    - Create infrastructure/template.yaml with AWSTemplateFormatVersion and Description
    - Define Parameters section (Environment, Version, DeploymentBucket)
    - _Requirements: 6.8_
  
  - [ ] 7.2 Define DynamoDB tables in CloudFormation
    - Define OrdersTable resource with OrderId as partition key
    - Configure on-demand billing, point-in-time recovery, and encryption
    - Define AuditLogsTable resource with AuditId (PK) and Timestamp (SK)
    - Add OrderIdIndex GSI to AuditLogsTable
    - _Requirements: 6.1, 6.2_
  
  - [ ] 7.3 Define SNS topic and SQS queues in CloudFormation
    - Define OrderEventsTopic SNS topic
    - Define OrderEmailQueue and OrderEmailDLQ with appropriate configuration
    - Define OrderAuditQueue and OrderAuditDLQ with appropriate configuration
    - Configure visibility timeout, message retention, and redrive policy
    - Create SNS subscriptions for both queues with RawMessageDelivery
    - Add SQS queue policies to allow SNS to send messages
    - _Requirements: 6.3, 6.4_
  
  - [ ] 7.4 Define IAM roles for Lambda functions
    - Define OrderApiRole with permissions for DynamoDB PutItem and SNS Publish
    - Define EmailLambdaRole with permissions for SQS operations and SES SendEmail
    - Define AuditLambdaRole with permissions for SQS operations and DynamoDB PutItem
    - Include AWSLambdaBasicExecutionRole managed policy for CloudWatch Logs
    - _Requirements: 6.7_
  
  - [ ] 7.5 Define Lambda functions in CloudFormation
    - Define OrderApiFunction with .NET 8 runtime, 512 MB memory, 30s timeout
    - Define EmailLambdaFunction with .NET 8 runtime, 256 MB memory, 60s timeout
    - Define AuditLambdaFunction with .NET 8 runtime, 256 MB memory, 30s timeout
    - Configure environment variables for each function
    - Create EventSourceMapping for Email and Audit Lambda functions
    - _Requirements: 6.5_
  
  - [ ] 7.6 Define API Gateway in CloudFormation
    - Define OrderApi REST API resource
    - Define /order resource and POST method
    - Configure Lambda proxy integration with OrderApiFunction
    - Create API deployment and stage
    - Enable CORS for mobile app
    - _Requirements: 6.6_
  
  - [ ] 7.7 Add CloudFormation outputs
    - Output API Gateway endpoint URL
    - Output DynamoDB table names
    - Output SNS topic ARN
    - Output SQS queue URLs
    - _Requirements: 6.8_

- [ ] 8. Implement CI/CD pipeline with GitHub Actions
  - [ ] 8.1 Create GitHub Actions workflow file
    - Create .github/workflows/deploy.yml
    - Define workflow triggers (push to main, pull requests)
    - Set environment variables (AWS_REGION, DOTNET_VERSION)
    - _Requirements: 7.1_
  
  - [ ] 8.2 Implement build and test job
    - Add checkout, setup .NET, and restore dependencies steps
    - Add build step for all Lambda projects in Release configuration
    - Add test step to execute unit tests for all projects
    - Configure job to fail if any test fails
    - _Requirements: 7.2, 7.3, 7.4_
  
  - [ ] 8.3 Implement package job
    - Add dotnet publish steps for all Lambda projects
    - Create ZIP archives for each Lambda function
    - Upload artifacts to GitHub Actions storage
    - _Requirements: 7.5_
  
  - [ ] 8.4 Implement deploy job
    - Add download artifacts step
    - Configure AWS credentials from GitHub Secrets
    - Upload Lambda packages to S3 deployment bucket
    - Deploy CloudFormation stack with parameters
    - Update Lambda function code with new versions
    - Configure job to run only on main branch
    - _Requirements: 7.6, 7.7, 7.8_

- [ ] 9. Implement MAUI mobile application
  - [ ] 9.1 Create MAUI project structure
    - Create MauiApp.csproj with .NET MAUI framework
    - Set up App.xaml and MainPage.xaml
    - Configure platform-specific settings (iOS, Android)
    - _Requirements: 1.1_
  
  - [ ] 9.2 Implement Order API client
    - Create OrderApiClient.cs with HttpClient
    - Implement SubmitOrderAsync method with POST to /order endpoint
    - Configure timeout (30 seconds) and retry logic (2 retries with exponential backoff)
    - Handle HTTP responses (200, 400, 500)
    - _Requirements: 1.1_
  
  - [ ] 9.3 Create order entry UI
    - Design MainPage.xaml with order entry form
    - Add input fields for customer name, email, and order items
    - Add validation feedback for user input
    - Add submit button with loading indicator
    - _Requirements: 1.1_
  
  - [ ] 9.4 Implement order submission logic
    - Wire up submit button to call OrderApiClient.SubmitOrderAsync
    - Display success message with order ID on HTTP 200
    - Display validation errors on HTTP 400
    - Display error message on HTTP 500
    - _Requirements: 1.3, 1.4_
  
  - [ ]* 9.5 Write unit tests for MAUI app
    - Test OrderApiClient request formatting
    - Test error handling for different HTTP responses
    - Test retry logic
    - _Requirements: 1.1, 1.3, 1.4_

- [ ] 10. Checkpoint - Verify end-to-end integration
  - Ensure all tests pass, ask the user if questions arise.

- [ ] 11. Implement monitoring and observability
  - [ ] 11.1 Add structured logging to all Lambda functions
    - Implement StructuredLogger.cs in Shared project
    - Add request ID logging to Order API
    - Add validation error logging to Order API
    - Add email attempt logging to Email Lambda
    - Add audit record logging to Audit Lambda
    - Add error logging with stack traces to all Lambda functions
    - _Requirements: 9.1, 9.2, 9.3, 9.4, 9.5, 9.6_
  
  - [ ]* 11.2 Write property tests for logging
    - **Property 14: Request Logging**
    - **Property 15: Validation Error Logging**
    - **Property 16: Lambda Error Logging**
    - **Property 17: Structured Logging Format**
    - **Validates: Requirements 9.1, 9.2, 9.5, 9.6**
  
  - [ ] 11.3 Add CloudWatch alarms to CloudFormation template
    - Define Lambda error rate alarms (> 5%)
    - Define DLQ message count alarms (> 0)
    - Define API Gateway 5xx error rate alarms (> 1%)
    - Define Lambda throttle alarms (> 0)
    - Create SNS topic for alarm notifications
    - _Requirements: 9.5_

- [ ] 12. Implement workflow independence verification
  - [ ]* 12.1 Write property tests for workflow independence
    - **Property 18: Workflow Independence**
    - **Validates: Requirements 8.2, 8.3, 8.4**
  
  - [ ]* 12.2 Write integration tests for concurrent processing
    - Test that Email Lambda failure does not affect Audit Lambda
    - Test that Audit Lambda failure does not affect Email Lambda
    - Test concurrent order submissions (at least 10 orders)
    - Verify no message loss during concurrent processing
    - _Requirements: 8.1, 8.2, 8.3, 8.4, 8.5_

- [ ] 13. Create documentation
  - [ ] 13.1 Create README.md
    - Add project overview and architecture diagram
    - Add prerequisites and setup instructions
    - Add deployment instructions
    - Add testing instructions
    - Add troubleshooting guide
    - _Requirements: 6.8, 7.8_
  
  - [ ] 13.2 Create API reference documentation
    - Document POST /order endpoint
    - Document request/response formats
    - Document error codes and messages
    - Add example requests and responses
    - _Requirements: 1.3, 1.4_
  
  - [ ] 13.3 Create deployment guide
    - Document AWS account setup requirements
    - Document GitHub Secrets configuration
    - Document CloudFormation stack deployment
    - Document rollback procedures
    - _Requirements: 7.6, 7.7, 7.8_

- [ ] 14. Final checkpoint - Complete system verification
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- Tasks marked with `*` are optional and can be skipped for faster MVP
- Each task references specific requirements for traceability
- Checkpoints ensure incremental validation throughout implementation
- Property tests validate universal correctness properties from the design document
- Unit tests validate specific examples and edge cases
- The implementation uses .NET 8 for all Lambda functions as specified in the design
- CloudFormation template defines all infrastructure as code for reproducibility
- GitHub Actions pipeline automates build, test, and deployment
- MAUI mobile app provides the client interface for order submission
