# GitHub Actions CI/CD Pipeline

This directory contains the GitHub Actions workflow for automated build, test, and deployment of the AWS Order Processing POC.

## Workflow Overview

The `deploy.yml` workflow automates the complete CI/CD pipeline with two main jobs:

### 1. Build and Test Job
Runs on every push and pull request to the `main` branch.

**Steps:**
- Checkout code
- Setup .NET 8 SDK
- Restore NuGet dependencies for all Lambda projects
- Build all Lambda projects in Release configuration
- Run unit tests (if test projects exist)
- Package Lambda functions as ZIP archives
- Upload artifacts to GitHub Actions storage

**Duration:** ~3-4 minutes

### 2. Deploy Job
Runs only on pushes to the `main` branch (after successful build and test).

**Steps:**
- Download Lambda ZIP artifacts
- Configure AWS credentials from GitHub Secrets
- Upload Lambda packages to S3 deployment bucket
- Deploy CloudFormation stacks in correct order:
  1. Storage (DynamoDB tables)
  2. Messaging (SNS/SQS)
  3. IAM Roles
  4. Lambda Functions
  5. API Gateway
- Update Lambda function code with new versions

**Duration:** ~4-5 minutes

**Total Pipeline Duration:** ~7-9 minutes (well under the 10-minute requirement)

## Required GitHub Secrets

Before the pipeline can run successfully, configure these secrets in your GitHub repository:

### AWS Credentials
- **AWS_ACCESS_KEY_ID**: AWS access key with permissions to deploy CloudFormation and Lambda
- **AWS_SECRET_ACCESS_KEY**: AWS secret key corresponding to the access key

### Deployment Configuration
- **DEPLOYMENT_BUCKET**: S3 bucket name for storing Lambda deployment packages (e.g., `my-lambda-deployments`)

### Setting Up Secrets

1. Go to your GitHub repository
2. Navigate to **Settings** → **Secrets and variables** → **Actions**
3. Click **New repository secret**
4. Add each secret with its corresponding value

## AWS IAM Permissions

The AWS credentials used in the pipeline need the following permissions:

### CloudFormation
- `cloudformation:CreateStack`
- `cloudformation:UpdateStack`
- `cloudformation:DescribeStacks`
- `cloudformation:DescribeStackEvents`
- `cloudformation:GetTemplate`

### Lambda
- `lambda:CreateFunction`
- `lambda:UpdateFunctionCode`
- `lambda:UpdateFunctionConfiguration`
- `lambda:GetFunction`
- `lambda:PublishVersion`

### S3
- `s3:PutObject`
- `s3:GetObject`
- `s3:ListBucket` (on deployment bucket)

### IAM
- `iam:CreateRole`
- `iam:PutRolePolicy`
- `iam:AttachRolePolicy`
- `iam:GetRole`
- `iam:PassRole`

### DynamoDB, SNS, SQS, API Gateway
- Full permissions for creating and managing these resources

**Recommended:** Create a dedicated IAM user or role for CI/CD with these permissions.

## Deployment Bucket Setup

Before running the pipeline, create an S3 bucket for Lambda deployments:

```bash
aws s3 mb s3://your-deployment-bucket --region us-east-1
```

Enable versioning (recommended):
```bash
aws s3api put-bucket-versioning \
  --bucket your-deployment-bucket \
  --versioning-configuration Status=Enabled
```

## Workflow Triggers

### Automatic Triggers
- **Push to main**: Runs build, test, and deploy
- **Pull request to main**: Runs build and test only (no deploy)

### Manual Trigger
You can manually trigger the workflow from the GitHub Actions tab.

## Monitoring Pipeline Execution

1. Go to the **Actions** tab in your GitHub repository
2. Click on the latest workflow run
3. View logs for each job and step
4. Check for any errors or warnings

## Troubleshooting

### Build Failures
- Check that all .NET projects compile locally
- Verify NuGet package versions are compatible
- Review build logs for specific errors

### Test Failures
- Run tests locally: `dotnet test`
- Check test logs in the workflow output
- Fix failing tests before merging

### Deployment Failures

**CloudFormation Stack Errors:**
- Check CloudFormation console for detailed error messages
- Verify stack dependencies are deployed in correct order
- Ensure IAM permissions are sufficient

**Lambda Update Errors:**
- Verify Lambda functions exist (created by CloudFormation)
- Check S3 bucket permissions
- Ensure ZIP files are uploaded successfully

**Missing Secrets:**
- Verify all required secrets are configured
- Check secret names match exactly (case-sensitive)

### Common Issues

**Issue:** "Export X not found"
- **Solution:** Deploy CloudFormation stacks in the correct order (Storage → Messaging → IAM → Lambda → API Gateway)

**Issue:** "Access Denied" errors
- **Solution:** Verify AWS credentials have sufficient permissions

**Issue:** "Bucket does not exist"
- **Solution:** Create the S3 deployment bucket and add its name to GitHub Secrets

## Pipeline Optimization

### Caching
The workflow uses GitHub Actions caching for:
- NuGet packages (implicit in setup-dotnet action)
- Build artifacts between jobs

### Parallel Execution
- Build and test steps run in parallel where possible
- Multiple CloudFormation stacks could be deployed in parallel (future enhancement)

### Artifact Management
- Lambda ZIP files are uploaded as artifacts
- Artifacts are retained for 90 days by default
- Deploy job downloads artifacts instead of rebuilding

## Future Enhancements

- Add code coverage reporting
- Implement staging environment deployment with manual approval
- Add integration tests to the pipeline
- Implement blue/green deployment strategy
- Add automated rollback on deployment failure
- Add Slack/email notifications for pipeline status

## Related Documentation

- [Infrastructure README](../../infrastructure/README.md) - CloudFormation templates documentation
- [Main README](../../README.md) - Project overview and setup instructions
