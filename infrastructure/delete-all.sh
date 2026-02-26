#!/bin/bash

# Delete all CloudFormation stacks in reverse order
# Usage: ./delete-all.sh <environment>

set -e

ENVIRONMENT=${1:-dev}

echo "Deleting Order Processing POC infrastructure..."
echo "Environment: $ENVIRONMENT"
echo ""
echo "WARNING: This will delete all resources. Press Ctrl+C to cancel."
read -p "Continue? (yes/no): " confirm

if [ "$confirm" != "yes" ]; then
  echo "Cancelled."
  exit 0
fi

# Delete in reverse order of dependencies
echo "1/5 Deleting API Gateway..."
aws cloudformation delete-stack \
  --stack-name order-processing-$ENVIRONMENT-api

echo "Waiting for API Gateway stack deletion..."
aws cloudformation wait stack-delete-complete \
  --stack-name order-processing-$ENVIRONMENT-api

echo "2/5 Deleting Lambda Functions..."
aws cloudformation delete-stack \
  --stack-name order-processing-$ENVIRONMENT-lambda

echo "Waiting for Lambda stack deletion..."
aws cloudformation wait stack-delete-complete \
  --stack-name order-processing-$ENVIRONMENT-lambda

echo "3/5 Deleting IAM Roles..."
aws cloudformation delete-stack \
  --stack-name order-processing-$ENVIRONMENT-iam

echo "Waiting for IAM stack deletion..."
aws cloudformation wait stack-delete-complete \
  --stack-name order-processing-$ENVIRONMENT-iam

echo "4/5 Deleting Messaging Layer..."
aws cloudformation delete-stack \
  --stack-name order-processing-$ENVIRONMENT-messaging

echo "Waiting for Messaging stack deletion..."
aws cloudformation wait stack-delete-complete \
  --stack-name order-processing-$ENVIRONMENT-messaging

echo "5/5 Deleting Storage Layer..."
aws cloudformation delete-stack \
  --stack-name order-processing-$ENVIRONMENT-storage

echo "Waiting for Storage stack deletion..."
aws cloudformation wait stack-delete-complete \
  --stack-name order-processing-$ENVIRONMENT-storage

echo ""
echo "All stacks deleted successfully!"
