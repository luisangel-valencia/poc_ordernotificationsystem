# Delete all CloudFormation stacks in reverse order
# Usage: .\delete-all.ps1 -Environment dev

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet('dev', 'staging', 'prod')]
    [string]$Environment
)

$ErrorActionPreference = "Stop"

Write-Host "Deleting Order Processing POC infrastructure..." -ForegroundColor Cyan
Write-Host "Environment: $Environment"
Write-Host ""
Write-Warning "This will delete all resources."

$confirmation = Read-Host "Continue? (yes/no)"
if ($confirmation -ne "yes") {
    Write-Host "Cancelled." -ForegroundColor Yellow
    exit 0
}

# Delete in reverse order of dependencies
Write-Host "1/5 Deleting API Gateway..." -ForegroundColor Yellow
aws cloudformation delete-stack --stack-name "order-processing-$Environment-api"

Write-Host "Waiting for API Gateway stack deletion..." -ForegroundColor Gray
aws cloudformation wait stack-delete-complete --stack-name "order-processing-$Environment-api"

Write-Host "2/5 Deleting Lambda Functions..." -ForegroundColor Yellow
aws cloudformation delete-stack --stack-name "order-processing-$Environment-lambda"

Write-Host "Waiting for Lambda stack deletion..." -ForegroundColor Gray
aws cloudformation wait stack-delete-complete --stack-name "order-processing-$Environment-lambda"

Write-Host "3/5 Deleting IAM Roles..." -ForegroundColor Yellow
aws cloudformation delete-stack --stack-name "order-processing-$Environment-iam"

Write-Host "Waiting for IAM stack deletion..." -ForegroundColor Gray
aws cloudformation wait stack-delete-complete --stack-name "order-processing-$Environment-iam"

Write-Host "4/5 Deleting Messaging Layer..." -ForegroundColor Yellow
aws cloudformation delete-stack --stack-name "order-processing-$Environment-messaging"

Write-Host "Waiting for Messaging stack deletion..." -ForegroundColor Gray
aws cloudformation wait stack-delete-complete --stack-name "order-processing-$Environment-messaging"

Write-Host "5/5 Deleting Storage Layer..." -ForegroundColor Yellow
aws cloudformation delete-stack --stack-name "order-processing-$Environment-storage"

Write-Host "Waiting for Storage stack deletion..." -ForegroundColor Gray
aws cloudformation wait stack-delete-complete --stack-name "order-processing-$Environment-storage"

Write-Host ""
Write-Host "All stacks deleted successfully!" -ForegroundColor Green
