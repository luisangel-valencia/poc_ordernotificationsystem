# CloudFormation Infrastructure Templates

This directory contains modular CloudFormation templates for the AWS Order Processing POC. Each template manages a specific layer of the infrastructure.

## Template Organization

### 1. **storage.yaml** - Storage Layer
- DynamoDB Orders table
- DynamoDB AuditLogs table with GSI
- Exports: Table names and ARNs

### 2. **messaging.yaml** - Messaging Layer
- SNS OrderEvents topic
- SQS queues (Email, Audit) with DLQs
- Queue policies and SNS subscriptions
- Exports: Topic ARN, Queue URLs and ARNs

### 3. **iam-roles.yaml** - IAM Roles
- OrderApiRole (DynamoDB + SNS permissions)
- EmailLambdaRole (SQS + SES permissions)
- AuditLambdaRole (SQS + DynamoDB permissions)
- Exports: Role ARNs

### 4. **lambda-functions.yaml** - Lambda Functions
- OrderApi Lambda (512 MB, 30s timeout)
- EmailLambda (256 MB, 60s timeout)
- AuditLambda (256 MB, 30s timeout)
- Event source mappings for SQS triggers
- Exports: Function ARNs and names

### 5. **api-gateway.yaml** - API Gateway
- REST API with /order endpoint
- POST method with Lambda proxy integration
- CORS configuration
- Exports: API endpoint URL

## Deployment Order

Templates must be deployed in this order due to dependencies:

1. Storage (no dependencies)
2. Messaging (no dependencies)
3. IAM Roles (depends on Storage + Messaging exports)
4. Lambda Functions (depends on IAM + Storage + Messaging exports)
5. API Gateway (depends on Lambda exports)

## Quick Deploy

Deploy all stacks:
```bash
chmod +x deploy-all.sh
./deploy-all.sh dev v1.0.0 your-deployment-bucket
```

Delete all stacks:
```bash
chmod +x delete-all.sh
./delete-all.sh dev
```

## Manual Deployment

Deploy individual stacks:

```bash
# 1. Storage
aws cloudformation deploy \
  --template-file storage.yaml \
  --stack-name order-processing-dev-storage \
  --parameter-overrides Environment=dev

# 2. Messaging
aws cloudformation deploy \
  --template-file messaging.yaml \
  --stack-name order-processing-dev-messaging \
  --parameter-overrides Environment=dev

# 3. IAM Roles
aws cloudformation deploy \
  --template-file iam-roles.yaml \
  --stack-name order-processing-dev-iam \
  --parameter-overrides Environment=dev \
  --capabilities CAPABILITY_NAMED_IAM

# 4. Lambda Functions
aws cloudformation deploy \
  --template-file lambda-functions.yaml \
  --stack-name order-processing-dev-lambda \
  --parameter-overrides \
    Environment=dev \
    Version=v1.0.0 \
    DeploymentBucket=your-bucket

# 5. API Gateway
aws cloudformation deploy \
  --template-file api-gateway.yaml \
  --stack-name order-processing-dev-api \
  --parameter-overrides Environment=dev
```

## Parameters

### Common Parameters
- **Environment**: `dev`, `staging`, or `prod`

### Lambda-Specific Parameters
- **Version**: Git commit SHA or version tag (e.g., `abc123` or `v1.0.0`)
- **DeploymentBucket**: S3 bucket containing Lambda ZIP files

## Stack Exports

Each stack exports values that other stacks import:

**Storage exports:**
- `{Environment}-OrdersTableName`
- `{Environment}-OrdersTableArn`
- `{Environment}-AuditLogsTableName`
- `{Environment}-AuditLogsTableArn`

**Messaging exports:**
- `{Environment}-OrderEventsTopicArn`
- `{Environment}-OrderEmailQueueUrl/Arn`
- `{Environment}-OrderAuditQueueUrl/Arn`

**IAM exports:**
- `{Environment}-OrderApiRoleArn`
- `{Environment}-EmailLambdaRoleArn`
- `{Environment}-AuditLambdaRoleArn`

**Lambda exports:**
- `{Environment}-OrderApiFunctionArn/Name`
- `{Environment}-EmailLambdaFunctionArn`
- `{Environment}-AuditLambdaFunctionArn`

**API Gateway exports:**
- `{Environment}-OrderApiEndpoint`
- `{Environment}-OrderApiId`

## Benefits of Modular Templates

1. **Independent Updates**: Update one layer without affecting others
2. **Faster Deployments**: Only deploy changed components
3. **Easier Testing**: Test individual layers in isolation
4. **Better Organization**: Clear separation of concerns
5. **Reusability**: Reuse templates across projects
6. **Reduced Blast Radius**: Errors affect only one layer

## Troubleshooting

### Export Not Found Error
If you see "Export X not found", ensure prerequisite stacks are deployed first.

### Stack Deletion Blocked
Delete stacks in reverse order (API → Lambda → IAM → Messaging → Storage).

### Permission Denied
Ensure you have `CAPABILITY_NAMED_IAM` for IAM role stack.
