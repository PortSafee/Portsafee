# Script para atualizar a API no Docker
# Execute este script sempre que fizer alterações no código

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  Atualizando API PortSafe no Docker" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Verificar se Docker está rodando
Write-Host "► Verificando se Docker está rodando..." -ForegroundColor Yellow
$dockerRunning = docker info 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Docker não está rodando!" -ForegroundColor Red
    Write-Host "  Por favor, inicie o Docker Desktop e execute este script novamente." -ForegroundColor Red
    exit 1
}
Write-Host "✓ Docker está rodando" -ForegroundColor Green
Write-Host ""

# Navegar para o diretório do Docker
Set-Location -Path $PSScriptRoot

# Parar containers atuais
Write-Host "► Parando containers atuais..." -ForegroundColor Yellow
docker-compose down
Write-Host "✓ Containers parados" -ForegroundColor Green
Write-Host ""

# Rebuild da API (sem cache para garantir que está atualizado)
Write-Host "► Fazendo rebuild da API..." -ForegroundColor Yellow
Write-Host "  (Isso pode levar alguns minutos...)" -ForegroundColor Gray
docker-compose build --no-cache api
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Erro ao fazer build da API!" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Build da API concluído" -ForegroundColor Green
Write-Host ""

# Iniciar containers
Write-Host "► Iniciando containers..." -ForegroundColor Yellow
docker-compose up -d
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Erro ao iniciar containers!" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Containers iniciados" -ForegroundColor Green
Write-Host "  (Migrations serão aplicadas automaticamente ao iniciar a API)" -ForegroundColor Gray
Write-Host ""

# Aguardar alguns segundos
Write-Host "► Aguardando containers iniciarem..." -ForegroundColor Yellow
Start-Sleep -Seconds 5

# Verificar status dos containers
Write-Host "► Status dos containers:" -ForegroundColor Yellow
docker-compose ps
Write-Host ""

# Mostrar logs da API
Write-Host "► Últimas linhas do log da API:" -ForegroundColor Yellow
docker-compose logs --tail=20 api
Write-Host ""

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "✓ Atualização concluída!" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "A API está disponível em: http://localhost:5000" -ForegroundColor Cyan
Write-Host "Swagger: http://localhost:5000/swagger" -ForegroundColor Cyan
Write-Host ""
Write-Host "Para ver os logs em tempo real:" -ForegroundColor Gray
Write-Host "  docker-compose logs -f api" -ForegroundColor White
Write-Host ""
