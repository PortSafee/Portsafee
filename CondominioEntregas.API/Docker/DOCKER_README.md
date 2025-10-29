# Docker Setup - PortSafee API

## 📋 Pré-requisitos

- Docker Desktop instalado
- Docker Compose instalado

## 🚀 Como usar

### 1. Iniciar os containers

```bash
docker-compose up -d
```

Este comando irá:
- Criar e iniciar o container PostgreSQL na porta 5432
- Criar e iniciar o container da API na porta 5000
- Criar uma rede Docker para comunicação entre os containers

### 2. Verificar se os containers estão rodando

```bash
docker-compose ps
```

### 3. Ver logs

Para ver os logs da API:
```bash
docker-compose logs -f api
```

Para ver os logs do PostgreSQL:
```bash
docker-compose logs -f postgres
```

Para ver todos os logs:
```bash
docker-compose logs -f
```

### 4. Acessar a aplicação

- API: http://localhost:5000
- Swagger: http://localhost:5000/swagger (se estiver em modo Development)

### 5. Executar migrations

Após os containers subirem pela primeira vez, você precisará executar as migrations:

```bash
docker-compose exec api dotnet ef database update
```

Ou você pode executar dentro do container:
```bash
docker exec -it portsafee-api dotnet ef database update
```

### 6. Parar os containers

```bash
docker-compose down
```

Para parar e remover os volumes (⚠️ isso apagará os dados do banco):
```bash
docker-compose down -v
```

## 🔧 Comandos úteis

### Rebuild da API (após mudanças no código)

```bash
docker-compose build api
docker-compose up -d
```

### Acessar o shell do container da API

```bash
docker exec -it portsafee-api /bin/bash
```

### Acessar o PostgreSQL

```bash
docker exec -it portsafee-postgres psql -U postgres -d portsafee
```

### Restaurar backup do banco

```bash
docker cp portsafee_backup.dump portsafee-postgres:/tmp/
docker exec -it portsafee-postgres pg_restore -U postgres -d portsafee /tmp/portsafee_backup.dump
```

## 📝 Estrutura

- **postgres**: Container PostgreSQL 16 Alpine
  - Porta: 5432
  - Database: portsafee
  - Usuário: postgres
  - Senha: Pedro2005
  - Volume persistente: postgres-data

- **api**: Container da API .NET 9.0
  - Porta: 5000 (mapeada para 8080 interno)
  - Ambiente: Development
  - Depende do PostgreSQL (aguarda healthcheck)

## 🔐 Segurança

⚠️ **IMPORTANTE**: As credenciais neste docker-compose são para desenvolvimento local. 

Para produção:
1. Use variáveis de ambiente
2. Altere as senhas
3. Use secrets do Docker
4. Configure HTTPS corretamente

## 🐛 Troubleshooting

### A API não consegue conectar ao banco

- Verifique se o PostgreSQL está saudável: `docker-compose ps`
- Verifique os logs: `docker-compose logs postgres`

### Porta já em uso

Se a porta 5000 ou 5432 já estiver em uso, edite o `docker-compose.yml` e altere a porta externa:

```yaml
ports:
  - "5001:8080"  # Para a API
  # ou
  - "5433:5432"  # Para o PostgreSQL
```

### Rebuild completo

```bash
docker-compose down -v
docker-compose build --no-cache
docker-compose up -d
```
