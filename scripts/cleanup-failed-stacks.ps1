# Cleanup script for failed CloudFormation stacks and orphaned resources

param(
    [string]$Region = "us-east-2",
    [string]$Environment = "dev"
)

Write-Host "Cleaning up failed stacks in region: $Region" -ForegroundColor Green

# Function to delete stack if in ROLLBACK_COMPLETE state
function Cleanup-Stack {
    param([string]$StackName)
    
    Write-Host "Checking $StackName..." -ForegroundColor Yellow
    
    try {
        $status = aws cloudformation describe-stacks --stack-name $StackName --region $Region --query 'Stacks[0].StackStatus' --output text 2>$null
        
        if ($status -eq "ROLLBACK_COMPLETE") {
            Write-Host "  $StackName is in ROLLBACK_COMPLETE state, deleting..." -ForegroundColor Red
            aws cloudformation delete-stack --stack-name $StackName --region $Region
            aws cloudformation wait stack-delete-complete --stack-name $StackName --region $Region 2>$null
            Write-Host "  $StackName deleted" -ForegroundColor Green
        }
        else {
            Write-Host "  $StackName status: $status (OK)" -ForegroundColor Green
        }
    }
    catch {
        Write-Host "  $StackName does not exist (OK)" -ForegroundColor Green
    }
}

# Function to cleanup orphaned IAM roles
function Cleanup-IAMRoles {
    Write-Host "Cleaning up orphaned IAM roles..." -ForegroundColor Yellow
    
    $roles = @("$Environment-OrderApiRole", "$Environment-EmailLambdaRole", "$Environment-AuditLambdaRole")
    
    foreach ($role in $roles) {
        try {
            $roleExists = aws iam get-role --role-name $role 2>$null
            if ($roleExists) {
                Write-Host "  Found orphaned role: $role" -ForegroundColor Red
                
                # Delete inline policies
                $policies = aws iam list-role-policies --role-name $role --query 'PolicyNames' --output text 2>$null
                if ($policies) {
                    foreach ($policy in $policies.Split()) {
                        Write-Host "    Deleting policy: $policy"
                        aws iam delete-role-policy --role-name $role --policy-name $policy 2>$null
                    }
                }
                
                # Detach managed policies
                $managed = aws iam list-attached-role-policies --role-name $role --query 'AttachedPolicies[].PolicyArn' --output text 2>$null
                if ($managed) {
                    foreach ($policyArn in $managed.Split()) {
                        Write-Host "    Detaching managed policy: $policyArn"
                        aws iam detach-role-policy --role-name $role --policy-arn $policyArn 2>$null
                    }
                }
                
                # Delete role
                Write-Host "    Deleting role: $role"
                aws iam delete-role --role-name $role 2>$null
            }
        }
        catch {
            # Role doesn't exist, continue
        }
    }
}

# Cleanup stacks in reverse order (opposite of creation)
Cleanup-Stack "order-processing-$Environment-api"
Cleanup-Stack "order-processing-$Environment-lambda"
Cleanup-Stack "order-processing-$Environment-iam"
Cleanup-Stack "order-processing-$Environment-messaging"
Cleanup-Stack "order-processing-$Environment-storage"

# Cleanup orphaned IAM roles
Cleanup-IAMRoles

Write-Host "Cleanup complete!" -ForegroundColor Green
