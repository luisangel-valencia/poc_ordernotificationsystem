#!/bin/bash

# Deploy all CloudFormation stacks in the correct order
# Usage: ./deploy-all.sh <environment> <version> <deployment-bucket>

set -e

ENVIRONMENT=${1:-dev}
VERSION=${2:-v1.0.0}
DEPLOYMENT_BUCKET=${3}

if [ -z "$DEPLOYMENT_BUCKET" ]; then
  echo "Error: Deployment bucket is required"
  echo "Usage: ./deploy-all.sh <environment> <version> <deployment-bucket>"
  exit 1
fi

echo "Deploying Order Processing POC infrastructure..."
echo "Environment: $ENVIRONMENT"
echo "Version: $VERSION"
echo "Deployment Bucket: $DEPLOYMENT_BUCKET"
echo ""

# 1. Deploy Storage Layer (DynamoDB tables)
echo "1/5 Deploying Storage Layer..."
aws cloudformation deploy \
  --template-file storage.yaml \
  --stack-name order-processing-$ENVIRONMENT-storage \
  --parameter-overrides Environment=$ENVIRONMENT \
  --tags Environment=$ENVIRONMENT Application=OrderProcessingPOC

# 2. Deploy Messaging Layer (SNS/SQS)
echo "2/5 Deploying Messaging Layer..."
aws cloudformation deploy \
  --template-file messaging.yaml \
  --stack-name order-processing-$ENVIRONMENT-messaging \
  --parameter-overrides Environment=$ENVIRONMENT \
  --tags Environment=$ENVIRONMENT Application=OrderProcessingPOC

# 3. Deploy IAM Roles
echo "3/5 Deploying IAM Roles..."
aws cloudformation deploy \
  --template-file iam-roles.yaml \
  --stack-name order-processing-$ENVIRONMENT-iam \
  --parameter-overrides Environment=$ENVIRONMENT \
  --capabilities CAPABILITY_NAMED_IAM \
  --tags Environment=$ENVIRONMENT Application=OrderProcessingPOC

# 4. Deploy Lambda Functions
echo "4/5 Deploying Lambda Functions..."
aws cloudformation deploy \
  --template-file lambda-functions.yaml \
  --stack-name order-processing-$ENVIRONMENT-lambda \
  --parameter-overrides \
    Environment=$ENVIRONMENT \
    Version=$VERSION \
    DeploymentBucket=$DEPLOYMENT_BUCKET \
  --tags Environment=$ENVIRONMENT Application=OrderProcessingPOC

# 5. Deploy API Gateway
echo "5/5 Deploying API Gateway..."
aws cloudformation deploy \
  --template-file api-gateway.yaml \
  --stack-name order-processing-$ENVIRONMENT-api \
  --parameter-overrides Environment=$ENVIRONMENT \
  --tags Environment=$ENVIRONMENT Application=OrderProcessingPOC

echo ""
echo "Deployment complete!"
echo ""
echo "Getting API endpoint..."
aws cloudformation describe-stacks \
  --stack-name order-processing-$ENVIRONMENT-api \
  --query 'Stacks[0].Outputs[?OutputKey==`ApiEndpoint`].OutputValue' \
  --output text
