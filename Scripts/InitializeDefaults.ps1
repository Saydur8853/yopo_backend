# PowerShell script to initialize default data
# Run this after starting your application

$baseUrl = "https://localhost:7000"  # Adjust to your API URL

Write-Host "Initializing Modules..." -ForegroundColor Green
try {
    $modulesResponse = Invoke-RestMethod -Uri "$baseUrl/api/modules/initialize" -Method POST
    Write-Host "Modules initialized: $($modulesResponse.message)" -ForegroundColor Yellow
} catch {
    Write-Host "Error initializing modules: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "Initializing Default User Types..." -ForegroundColor Green
try {
    $userTypesResponse = Invoke-RestMethod -Uri "$baseUrl/api/usertypes/initialize-defaults" -Method POST
    Write-Host "User Types initialized: $($userTypesResponse.message)" -ForegroundColor Yellow
} catch {
    Write-Host "Error initializing user types: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "Getting Super Admin user type..." -ForegroundColor Green
try {
    $superAdmin = Invoke-RestMethod -Uri "$baseUrl/api/usertypes/1" -Method GET
    Write-Host "Super Admin Details:" -ForegroundColor Cyan
    Write-Host "ID: $($superAdmin.id)" -ForegroundColor White
    Write-Host "Name: $($superAdmin.name)" -ForegroundColor White
    Write-Host "Description: $($superAdmin.description)" -ForegroundColor White
    Write-Host "Module IDs: $($superAdmin.moduleIds -join ', ')" -ForegroundColor White
    Write-Host "Module Names: $($superAdmin.moduleNames -join ', ')" -ForegroundColor White
} catch {
    Write-Host "Error getting Super Admin: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "Initialization completed!" -ForegroundColor Green
