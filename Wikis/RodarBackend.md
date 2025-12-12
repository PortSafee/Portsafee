# PortSafee API - Guia de Instalação

API REST para gerenciamento de entregas em condomínios.

## 🚀 Pré-requisitos

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [PostgreSQL 16+](https://www.postgresql.org/download/) ou [Docker](https://www.docker.com/)

## 📦 Como Rodar

### Opção 1: Com Docker (Recomendado)

1. Clone o repositório e navegue até a pasta Docker:

```bash
git clone https://github.com/PortSafee/Portsafee.git
cd Portsafee/CondominioEntregas.API/Docker
```

2. Suba os containers:

```bash
docker-compose up -d
```

3. Acesse:
   - API: <http://localhost:5000>
   - Swagger: <http://localhost:5000/swagger>

### Opção 2: Local (sem Docker)

1. Clone o repositório:

```bash
git clone https://github.com/PortSafee/Portsafee.git
cd Portsafee/CondominioEntregas.API
```

2. Configure o banco de dados no `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=portsafee;Username=postgres;Password=SUA_SENHA"
}
```

3. Aplique as migrations:

```bash
dotnet ef database update
```

4. Execute a aplicação:

```bash
dotnet run
```

5. Acesse:
   - API: <http://localhost:5000>
   - Swagger: <http://localhost:5000/swagger>

## ⚙️ Variáveis de Ambiente (Opcional)

Substitui configurações do `appsettings.json`:

- `DATABASE_URL` - Connection string do PostgreSQL
- `GMAIL_EMAIL` - Email para notificações
- `GMAIL_APP_PASSWORD` - Senha de aplicativo do Gmail

## 🛠️ Comandos Úteis

```bash
# Restaurar pacotes
dotnet restore

# Build
dotnet build

# Criar migration
dotnet ef migrations add NomeDaMigration

# Atualizar banco
dotnet ef database update

# Executar com hot reload
dotnet watch run
```

## 🔧 Tecnologias

- .NET 9.0
- Entity Framework Core
- PostgreSQL
- JWT Authentication
- Swagger/OpenAPI
- Google Gemini AI
- MailKit
