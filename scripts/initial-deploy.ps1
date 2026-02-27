# Initial deployment script - builds and uploads Lambda packages before CloudFormation

param(
    [Parameter(Mandatory=$true)]
    [string]$DeploymentBucket,
    
    [Parameter(Mandatory=$false)]
    [string]$Version = "initial"
)

$ErrorActionPreference = "Stop"

Write-Host "Building Lambda functions..." -ForegroundColor Green

# Build and package OrderLambda
dotnet publish src/OrderLambda/OrderLambda.csproj -c Release -r linux-x64 --self-contained false -o publish/order-lambda
Push-Location publish/order-lambda
Compress-Archive -Path * -DestinationPath ../../order-lambda.zip -Force
Pop-Location

# Build and package EmailLambda
dotnet publish src/EmailLambda/EmailLambda.csproj -c Release -r linux-x64 --self-contained false -o publish/email-lambda
Push-Location publish/email-lambda
Compress-Archive -Path * -DestinationPath ../../email-lambda.zip -Force
Pop-Location

# Build and package AuditLambda
dotnet publish src/AuditLambda/AuditLambda.csproj -c Release -r linux-x64 --self-contained false -o publish/audit-lambda
Push-Location publish/audit-lambda
Compress-Archive -Path * -DestinationPath ../../audit-lambda.zip -Force
Pop-Location

Write-Host "Uploading Lambda packages to S3..." -ForegroundColor Green

# Upload to S3
aws s3 cp order-lambda.zip "s3://$DeploymentBucket/lambda/order-lambda-$Version.zip"
aws s3 cp email-lambda.zip "s3://$DeploymentBucket/lambda/email-lambda-$Version.zip"
aws s3 cp audit-lambda.zip "s3://$DeploymentBucket/lambda/audit-lambda-$Version.zip"

Write-Host "Lambda packages uploaded successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "Now deploy CloudFormation stacks with Version=$Version" -ForegroundColor Yellow
Write-Host "Example:" -ForegroundColor Yellow
Write-Host "  .\infrastructure\deploy-all.ps1 -Version $Version -DeploymentBucket $DeploymentBucket" -ForegroundColor Cyan
