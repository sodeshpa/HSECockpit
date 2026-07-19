# HSECockpit Deployment Script
# Run from the repository root directory

$ErrorActionPreference = "Stop"

Write-Host "=== HSECockpit Deployment ===" -ForegroundColor Cyan

# 1. Publish Lambda
Write-Host "`n[1/4] Publishing D4HSE.Ingestion Lambda..." -ForegroundColor Yellow
dotnet publish backend/D4HSE.Ingestion/D4HSE.Ingestion.csproj -c Release -r linux-x64 -o backend/D4HSE.Ingestion/bin/Release/net8.0/publish
if ($LASTEXITCODE -ne 0) { Write-Error "Lambda publish failed"; exit 1 }
Write-Host "Lambda publish complete." -ForegroundColor Green

# 2. Build CDK
Write-Host "`n[2/4] Building CDK infrastructure project..." -ForegroundColor Yellow
dotnet build infrastructure/HSECockpit.Infra.sln
if ($LASTEXITCODE -ne 0) { Write-Error "CDK build failed"; exit 1 }
Write-Host "CDK build complete." -ForegroundColor Green

# 3. Delete stacks in ROLLBACK_COMPLETE state (cannot be updated, must be deleted first)
Write-Host "`n[3/4] Checking for stacks in ROLLBACK_COMPLETE state..." -ForegroundColor Yellow
$rollbackStacks = aws cloudformation list-stacks --stack-status-filter ROLLBACK_COMPLETE --query "StackSummaries[?contains(StackName,'HSECockpit') || contains(StackName,'Network') || contains(StackName,'Database') || contains(StackName,'Compute') || contains(StackName,'Frontend') || contains(StackName,'ApiGateway') || contains(StackName,'Observability') || contains(StackName,'Pipeline')].StackName" --output text 2>$null
if ($rollbackStacks) {
    foreach ($stackName in ($rollbackStacks -split "`t")) {
        if ($stackName.Trim()) {
            Write-Host "  Deleting failed stack: $stackName" -ForegroundColor Red
            aws cloudformation delete-stack --stack-name $stackName
            aws cloudformation wait stack-delete-complete --stack-name $stackName
            if ($LASTEXITCODE -ne 0) { Write-Error "Failed to delete stack $stackName"; exit 1 }
            Write-Host "  Deleted: $stackName" -ForegroundColor Green
        }
    }
} else {
    Write-Host "  No stacks in ROLLBACK_COMPLETE state." -ForegroundColor Green
}

# 4. Deploy
Write-Host "`n[4/4] Deploying all CDK stacks..." -ForegroundColor Yellow
Push-Location infrastructure/HSECockpit.Infra
try {
    cdk deploy --all -c env=dev
    if ($LASTEXITCODE -ne 0) { Write-Error "CDK deploy failed"; exit 1 }
}
finally {
    Pop-Location
}

Write-Host "`n=== Deployment Complete ===" -ForegroundColor Cyan
