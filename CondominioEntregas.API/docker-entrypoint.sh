#!/bin/bash
set -e

echo "🚀 PortSafe - Iniciando aplicação..."

# Função para aguardar o banco de dados
wait_for_db() {
    echo "⏳ Aguardando banco de dados ficar disponível..."
    
    max_attempts=30
    attempt=0
    
    while [ $attempt -lt $max_attempts ]; do
        if dotnet ef database update --no-build 2>/dev/null; then
            echo "✅ Migrations aplicadas com sucesso!"
            return 0
        fi
        
        attempt=$((attempt + 1))
        echo "⚠️  Tentativa $attempt/$max_attempts falhou. Aguardando 2 segundos..."
        sleep 2
    done
    
    echo "❌ Não foi possível conectar ao banco de dados após $max_attempts tentativas."
    echo "⚠️  Continuando mesmo assim - a aplicação pode falhar se o BD não estiver disponível."
    return 1
}

# Aplicar migrations com retry
wait_for_db

# Iniciar a aplicação
echo "🎯 Iniciando PortSafe API..."
exec dotnet CondominioEntregas.API.dll
