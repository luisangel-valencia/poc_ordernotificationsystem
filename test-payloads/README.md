# API Gateway Test Payloads

This directory contains test payloads for testing the Order Processing API.

## How to Test

### Option 1: AWS Console (API Gateway Test)

1. Go to AWS Console → API Gateway
2. Find your API: `order-processing-dev-api`
3. Click on **Resources** → `/order` → `POST`
4. Click **Test** button
5. Copy one of the JSON payloads below into the **Request Body**
6. Click **Test**

### Option 2: cURL Command

First, get your API endpoint:
```bash
aws cloudformation describe-stacks \
  --stack-name order-processing-dev-api \
  --region us-east-2 \
  --query 'Stacks[0].Outputs[?OutputKey==`ApiEndpoint`].OutputValue' \
  --output text
```

Then test with cURL:
```bash
# Valid order
curl -X POST https://YOUR-API-ID.execute-api.us-east-2.amazonaws.com/dev/order \
  -H "Content-Type: application/json" \
  -d @test-payloads/valid-order.json

# Invalid order (missing email)
curl -X POST https://YOUR-API-ID.execute-api.us-east-2.amazonaws.com/dev/order \
  -H "Content-Type: application/json" \
  -d @test-payloads/invalid-order-missing-email.json
```

### Option 3: PowerShell

```powershell
# Get API endpoint
$apiEndpoint = aws cloudformation describe-stacks `
  --stack-name order-processing-dev-api `
  --region us-east-2 `
  --query 'Stacks[0].Outputs[?OutputKey==`ApiEndpoint`].OutputValue' `
  --output text

# Test valid order
$body = Get-Content test-payloads/valid-order.json -Raw
Invoke-RestMethod -Uri "$apiEndpoint/order" -Method Post -Body $body -ContentType "application/json"
```

## Test Payloads

### 1. valid-order.json ✅
**Expected Result**: HTTP 200 with order confirmation
- Valid customer name and email
- 2 items with proper quantities and prices
- Total amount calculated correctly

### 2. invalid-order-missing-email.json ❌
**Expected Result**: HTTP 400 with validation error
- Missing customer email (empty string)
- Should trigger validation error

### 3. invalid-order-no-items.json ❌
**Expected Result**: HTTP 400 with validation error
- Empty items array
- Should trigger "at least 1 item required" validation

### 4. large-order.json ✅
**Expected Result**: HTTP 200 with order confirmation
- 5 different items
- Higher total amount
- Tests system with more complex order

## Validation Rules

Based on the requirements, the API validates:
- **Customer Name**: Required, min 2 characters, max 100 characters
- **Customer Email**: Required, valid email format
- **Items**: Required, at least 1 item, max 50 items
- **Product ID**: Required for each item
- **Quantity**: Required, must be > 0
- **Price**: Required, must be > 0

## Expected Responses

### Success (200)
```json
{
  "orderId": "550e8400-e29b-41d4-a716-446655440000",
  "message": "Order received successfully",
  "createdAt": "2026-02-27T15:30:00Z"
}
```

### Validation Error (400)
```json
{
  "error": "Validation failed",
  "errors": [
    {
      "field": "customerEmail",
      "message": "Invalid email format"
    }
  ]
}
```

### Server Error (500)
```json
{
  "error": "Internal server error",
  "message": "Failed to process order",
  "requestId": "abc-123-def-456"
}
```

## What Happens After Successful Order

1. Order is saved to DynamoDB `dev-Orders` table
2. Event is published to SNS topic `dev-OrderEvents`
3. SNS fans out to two SQS queues:
   - `dev-OrderEmailQueue` → Email Lambda sends confirmation email
   - `dev-OrderAuditQueue` → Audit Lambda logs to `dev-AuditLogs` table

## Monitoring

Check CloudWatch Logs for each Lambda:
- `/aws/lambda/dev-OrderApi`
- `/aws/lambda/dev-EmailLambda`
- `/aws/lambda/dev-AuditLambda`

Check DynamoDB tables:
- `dev-Orders` - Should contain your order
- `dev-AuditLogs` - Should contain audit record

Check SQS Dead Letter Queues (should be empty):
- `dev-OrderEmailDLQ`
- `dev-OrderAuditDLQ`
