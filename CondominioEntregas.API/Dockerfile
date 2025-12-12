# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar o arquivo de projeto e restaurar dependências
COPY ["CondominioEntregas.API.csproj", "./"]
RUN dotnet restore "CondominioEntregas.API.csproj"

# Copiar todos os arquivos e fazer o build
COPY . .
RUN dotnet build "CondominioEntregas.API.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "CondominioEntregas.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Final - Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Instalar dotnet-ef tools para migrations
RUN dotnet tool install --global dotnet-ef --version 9.0.0
ENV PATH="${PATH}:/root/.dotnet/tools"

# Copiar arquivos publicados
COPY --from=publish /app/publish .

# Copiar arquivos de migration (necessário para aplicar migrations)
COPY --from=build /src/Data/Migrations ./Data/Migrations

# Copiar script de entrypoint
COPY docker-entrypoint.sh /app/docker-entrypoint.sh
RUN chmod +x /app/docker-entrypoint.sh

# Render usa a variável PORT, mas também expõe 8080
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}
EXPOSE ${PORT:-8080}

# Usar entrypoint resiliente com retry para migrations
ENTRYPOINT ["/app/docker-entrypoint.sh"]
