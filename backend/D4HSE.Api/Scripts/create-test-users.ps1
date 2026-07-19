# =============================================================================
# Create Test Users in Cognito User Pool (Dev Environment)
# =============================================================================
# Prerequisites:
#   - AWS CLI v2 installed and configured with dev account credentials
#   - User Pool ID set below (from CDK output or AWS Console)
#
# Usage:
#   .\create-test-users.ps1
#   .\create-test-users.ps1 -UserPoolId "us-east-1_XXXXXXX" -Region "us-east-1"
#
# Note: This script is for local development/testing only.
#       It creates users with temporary passwords that must be changed on first login.
# =============================================================================

param(
    [string]$UserPoolId = $env:COGNITO_USER_POOL_ID ?? "us-east-1_PLACEHOLDER",
    [string]$Region = $env:AWS_REGION ?? "us-east-1",
    [string]$TempPassword = "TempPass123!"
)

Write-Host "Creating test users in Cognito User Pool: $UserPoolId"
Write-Host "Region: $Region"
Write-Host "---"

function New-TestUser {
    param(
        [string]$Email,
        [string]$Role
    )

    Write-Host "Creating user: $Email with role: $Role"

    try {
        aws cognito-idp admin-create-user `
            --user-pool-id $UserPoolId `
            --username $Email `
            --user-attributes `
                "Name=email,Value=$Email" `
                "Name=email_verified,Value=true" `
                "Name=custom:role,Value=$Role" `
            --temporary-password $TempPassword `
            --message-action SUPPRESS `
            --region $Region

        if ($LASTEXITCODE -eq 0) {
            Write-Host "  + User created successfully" -ForegroundColor Green

            # Set permanent password (skip forced password change for dev)
            aws cognito-idp admin-set-user-password `
                --user-pool-id $UserPoolId `
                --username $Email `
                --password $TempPassword `
                --permanent `
                --region $Region

            if ($LASTEXITCODE -eq 0) {
                Write-Host "  + Password set as permanent" -ForegroundColor Green
            } else {
                Write-Host "  x Failed to set permanent password" -ForegroundColor Red
            }
        } else {
            Write-Host "  x Failed to create user" -ForegroundColor Red
        }
    }
    catch {
        Write-Host "  x Error: $_" -ForegroundColor Red
    }

    Write-Host ""
}

# Create test users for each role
New-TestUser -Email "testuser-hse-manager@example.com" -Role "hse-manager"
New-TestUser -Email "testuser-data-owner@example.com" -Role "hse-data-owner"
New-TestUser -Email "testuser-executive@example.com" -Role "executive"
New-TestUser -Email "testuser-admin@example.com" -Role "admin"

Write-Host "=== Test User Summary ==="
Write-Host ""
Write-Host "| Email                                | Role           | Password        |"
Write-Host "|--------------------------------------|----------------|-----------------|"
Write-Host "| testuser-hse-manager@example.com     | hse-manager    | $TempPassword |"
Write-Host "| testuser-data-owner@example.com      | hse-data-owner | $TempPassword |"
Write-Host "| testuser-executive@example.com       | executive      | $TempPassword |"
Write-Host "| testuser-admin@example.com           | admin          | $TempPassword |"
Write-Host ""
Write-Host "Set COGNITO_USER_POOL_ID and AWS_REGION environment variables before running."
Write-Host "Done."
