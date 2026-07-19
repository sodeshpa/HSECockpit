#!/bin/bash
# =============================================================================
# Create Test Users in Cognito User Pool (Dev Environment)
# =============================================================================
# Prerequisites:
#   - AWS CLI v2 installed and configured with dev account credentials
#   - User Pool ID set below (from CDK output or AWS Console)
#
# Usage:
#   chmod +x create-test-users.sh
#   ./create-test-users.sh
#
# Note: This script is for local development/testing only.
#       It creates users with temporary passwords that must be changed on first login.
# =============================================================================

# Configuration — replace with actual User Pool ID from your dev environment
USER_POOL_ID="${COGNITO_USER_POOL_ID:-us-east-1_PLACEHOLDER}"
TEMP_PASSWORD="TempPass123!"
REGION="${AWS_REGION:-us-east-1}"

echo "Creating test users in Cognito User Pool: ${USER_POOL_ID}"
echo "Region: ${REGION}"
echo "---"

# Function to create a user with a specified role
create_test_user() {
    local email=$1
    local role=$2

    echo "Creating user: ${email} with role: ${role}"

    aws cognito-idp admin-create-user \
        --user-pool-id "${USER_POOL_ID}" \
        --username "${email}" \
        --user-attributes \
            Name=email,Value="${email}" \
            Name=email_verified,Value=true \
            Name="custom:role",Value="${role}" \
        --temporary-password "${TEMP_PASSWORD}" \
        --message-action SUPPRESS \
        --region "${REGION}"

    if [ $? -eq 0 ]; then
        echo "  ✓ User created successfully"

        # Set permanent password (skip forced password change for dev)
        aws cognito-idp admin-set-user-password \
            --user-pool-id "${USER_POOL_ID}" \
            --username "${email}" \
            --password "${TEMP_PASSWORD}" \
            --permanent \
            --region "${REGION}"

        if [ $? -eq 0 ]; then
            echo "  ✓ Password set as permanent"
        else
            echo "  ✗ Failed to set permanent password"
        fi
    else
        echo "  ✗ Failed to create user"
    fi

    echo ""
}

# Create test users for each role
create_test_user "testuser-hse-manager@example.com" "hse-manager"
create_test_user "testuser-data-owner@example.com" "hse-data-owner"
create_test_user "testuser-executive@example.com" "executive"
create_test_user "testuser-admin@example.com" "admin"

echo "=== Test User Summary ==="
echo ""
echo "| Email                                | Role           | Password        |"
echo "|--------------------------------------|----------------|-----------------|"
echo "| testuser-hse-manager@example.com     | hse-manager    | ${TEMP_PASSWORD} |"
echo "| testuser-data-owner@example.com      | hse-data-owner | ${TEMP_PASSWORD} |"
echo "| testuser-executive@example.com       | executive      | ${TEMP_PASSWORD} |"
echo "| testuser-admin@example.com           | admin          | ${TEMP_PASSWORD} |"
echo ""
echo "Set COGNITO_USER_POOL_ID and AWS_REGION environment variables before running."
echo "Done."
