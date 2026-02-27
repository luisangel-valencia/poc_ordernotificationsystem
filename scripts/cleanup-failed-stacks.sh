#!/bin/bash
# Cleanup script for failed CloudFormation stacks and orphaned resources

REGION="${1:-us-east-2}"
ENV="${2:-dev}"

echo "Cleaning up failed stacks in region: $REGION"

# Function to delete stack if in ROLLBACK_COMPLETE state
cleanup_stack() {
  STACK_NAME=$1
  echo "Checking $STACK_NAME..."
  
  STATUS=$(aws cloudformation describe-stacks --stack-name $STACK_NAME --region $REGION --query 'Stacks[0].StackStatus' --output text 2>/dev/null || echo "DOES_NOT_EXIST")
  
  if [ "$STATUS" = "ROLLBACK_COMPLETE" ]; then
    echo "  $STACK_NAME is in ROLLBACK_COMPLETE state, deleting..."
    aws cloudformation delete-stack --stack-name $STACK_NAME --region $REGION
    aws cloudformation wait stack-delete-complete --stack-name $STACK_NAME --region $REGION 2>/dev/null || true
    echo "  $STACK_NAME deleted"
  elif [ "$STATUS" = "DOES_NOT_EXIST" ]; then
    echo "  $STACK_NAME does not exist (OK)"
  else
    echo "  $STACK_NAME status: $STATUS (OK)"
  fi
}

# Function to cleanup orphaned IAM roles
cleanup_iam_roles() {
  echo "Cleaning up orphaned IAM roles..."
  
  for ROLE in "${ENV}-OrderApiRole" "${ENV}-EmailLambdaRole" "${ENV}-AuditLambdaRole"; do
    # Check if role exists
    if aws iam get-role --role-name $ROLE 2>/dev/null >/dev/null; then
      echo "  Found orphaned role: $ROLE"
      
      # Delete inline policies
      POLICIES=$(aws iam list-role-policies --role-name $ROLE --query 'PolicyNames' --output text 2>/dev/null || echo "")
      for POLICY in $POLICIES; do
        echo "    Deleting policy: $POLICY"
        aws iam delete-role-policy --role-name $ROLE --policy-name $POLICY 2>/dev/null || true
      done
      
      # Detach managed policies
      MANAGED=$(aws iam list-attached-role-policies --role-name $ROLE --query 'AttachedPolicies[].PolicyArn' --output text 2>/dev/null || echo "")
      for POLICY_ARN in $MANAGED; do
        echo "    Detaching managed policy: $POLICY_ARN"
        aws iam detach-role-policy --role-name $ROLE --policy-arn $POLICY_ARN 2>/dev/null || true
      done
      
      # Delete role
      echo "    Deleting role: $ROLE"
      aws iam delete-role --role-name $ROLE 2>/dev/null || true
    fi
  done
}

# Cleanup stacks in reverse order (opposite of creation)
cleanup_stack "order-processing-${ENV}-api"
cleanup_stack "order-processing-${ENV}-lambda"
cleanup_stack "order-processing-${ENV}-iam"
cleanup_stack "order-processing-${ENV}-messaging"
cleanup_stack "order-processing-${ENV}-storage"

# Cleanup orphaned IAM roles
cleanup_iam_roles

echo "Cleanup complete!"
