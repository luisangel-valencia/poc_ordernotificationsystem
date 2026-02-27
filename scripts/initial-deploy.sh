#!/bin/bash
# Initial deployment script - builds and uploads Lambda packages before CloudFormation

set -e

DEPLOYMENT_BUCKET="$1"
VERSION="${2:-initial}"

if [ -z "$DEPLOYMENT_BUCKET" ]; then
  echo "Usage: ./scripts/initial-deploy.sh <deployment-bucket> [version]"
  echo "Example: ./scripts/initial-deploy.sh my-lambda-deployments v1.0.0"
  exit 1
fi

echo "Building Lambda functions..."

# Build and package OrderLambda
dotnet publish src/OrderLambda/OrderLambda.csproj -c Release -r linux-x64 --self-contained false -o publish/order-lambda
cd publish/order-lambda && zip -r ../../order-lambda.zip . && cd ../..

# Build and package EmailLambda
dotnet publish src/EmailLambda/EmailLambda.csproj -c Release -r linux-x64 --self-contained false -o publish/email-lambda
cd publish/email-lambda && zip -r ../../email-lambda.zip . && cd ../..

# Build and package AuditLambda
dotnet publish src/AuditLambda/AuditLambda.csproj -c Release -r linux-x64 --self-contained false -o publish/audit-lambda
cd publish/audit-lambda && zip -r ../../audit-lambda.zip . && cd ../..

echo "Uploading Lambda packages to S3..."

# Upload to S3
aws s3 cp order-lambda.zip s3://${DEPLOYMENT_BUCKET}/lambda/order-lambda-${VERSION}.zip
aws s3 cp email-lambda.zip s3://${DEPLOYMENT_BUCKET}/lambda/email-lambda-${VERSION}.zip
aws s3 cp audit-lambda.zip s3://${DEPLOYMENT_BUCKET}/lambda/audit-lambda-${VERSION}.zip

echo "Lambda packages uploaded successfully!"
echo ""
echo "Now deploy CloudFormation stacks with Version=${VERSION}"
echo "Example:"
echo "  ./infrastructure/deploy-all.sh ${VERSION} ${DEPLOYMENT_BUCKET}"
