# Deploy all CloudFormation stacks in the correct order
# Usage: .\deploy-all.ps1 -Environment dev -Version v1.0.0 -DeploymentBucket your-bucket

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet('dev', 'staging', 'prod')]
    [string]$Environment,
    
    [Parameter(Mandatory=$true)]
    [string]$Version,
    
    [Parameter(Mandatory=$true)]
    [string]$DeploymentBucket
)

$ErrorActionPreference = "Stop"

Write-Host "Deploying Order Processing POC infrastructure..." -ForegroundColor Cyan
Write-Host "Environment: $Environment"
Write-Host "Version: $Version"
Write-Host "Deployment Bucket: $DeploymentBucket"
Write-Host ""

# 1. Deploy Storage Layer (DynamoDB tables)
Write-Host "1/5 Deploying Storage Layer..." -ForegroundColor Yellow
aws cloudformation deploy `
    --template-file storage.yaml `
    --stack-name "order-processing-$Environment-storage" `
    --parameter-overrides Environment=$Environment `
    --tags Environment=$Environment Application=OrderProcessingPOC

if ($LASTEXITCODE -ne 0) {
    Write-Host "Storage deployment failed!" -ForegroundColor Red
    exit 1
}

# 2. Deploy Messaging Layer (SNS/SQS)
Write-Host "2/5 Deploying Messaging Layer..." -ForegroundColor Yellow
aws cloudformation deploy `
    --template-file messaging.yaml `
    --stack-name "order-processing-$Environment-messaging" `
    --parameter-overrides Environment=$Environment `
    --tags Environment=$Environment Application=OrderProcessingPOC

if ($LASTEXITCODE -ne 0) {
    Write-Host "Messaging deployment failed!" -ForegroundColor Red
    exit 1
}

# 3. Deploy IAM Roles
Write-Host "3/5 Deploying IAM Roles..." -ForegroundColor Yellow
aws cloudformation deploy `
    --template-file iam-roles.yaml `
    --stack-name "order-processing-$Environment-iam" `
    --parameter-overrides Environment=$Environment `
    --capabilities CAPABILITY_NAMED_IAM `
    --tags Environment=$Environment Application=OrderProcessingPOC

if ($LASTEXITCODE -ne 0) {
    Write-Host "IAM deployment failed!" -ForegroundColor Red
    exit 1
}

# 4. Deploy Lambda Functions
Write-Host "4/5 Deploying Lambda Functions..." -ForegroundColor Yellow
aws cloudformation deploy `
    --template-file lambda-functions.yaml `
    --stack-name "order-processing-$Environment-lambda" `
    --parameter-overrides `
        Environment=$Environment `
        Version=$Version `
        DeploymentBucket=$DeploymentBucket `
    --tags Environment=$Environment Application=OrderProcessingPOC

if ($LASTEXITCODE -ne 0) {
    Write-Host "Lambda deployment failed!" -ForegroundColor Red
    exit 1
}

# 5. Deploy API Gateway
Write-Host "5/5 Deploying API Gateway..." -ForegroundColor Yellow
aws cloudformation deploy `
    --template-file api-gateway.yaml `
    --stack-name "order-processing-$Environment-api" `
    --parameter-overrides Environment=$Environment `
    --tags Environment=$Environment Application=OrderProcessingPOC

if ($LASTEXITCODE -ne 0) {
    Write-Host "API Gateway deployment failed!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Deployment complete!" -ForegroundColor Green
Write-Host ""
Write-Host "Getting API endpoint..." -ForegroundColor Cyan
$apiEndpoint = aws cloudformation describe-stacks `
    --stack-name "order-processing-$Environment-api" `
    --query 'Stacks[0].Outputs[?OutputKey==`ApiEndpoint`].OutputValue' `
    --output text

Write-Host "API Endpoint: $apiEndpoint" -ForegroundColor Green
