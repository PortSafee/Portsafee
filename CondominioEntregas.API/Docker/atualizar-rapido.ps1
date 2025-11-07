# Script rápido para atualização (mantém o cache)
# Use este quando fizer pequenas alterações no código

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  Atualização Rápida - PortSafe API" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

Set-Location -Path $PSScriptRoot

Write-Host "► Parando API..." -ForegroundColor Yellow
docker-compose stop api

Write-Host "► Rebuild rápido (com cache)..." -ForegroundColor Yellow
docker-compose build api

Write-Host "► Reiniciando containers..." -ForegroundColor Yellow
docker-compose up -d

Write-Host ""
Write-Host "✓ Atualização rápida concluída!" -ForegroundColor Green
Write-Host ""
Write-Host "Logs:" -ForegroundColor Yellow
docker-compose logs --tail=15 api
