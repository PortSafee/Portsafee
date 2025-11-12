#!/bin/bash
set -e

echo "🔄 Aplicando migrations..."
dotnet ef database update --no-build

echo "🚀 Iniciando aplicação..."
exec dotnet CondominioEntregas.API.dll
