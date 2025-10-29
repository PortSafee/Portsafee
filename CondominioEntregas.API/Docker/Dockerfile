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

# Stage 3: Final
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Instalar EF Core tools
RUN dotnet tool install --global dotnet-ef
ENV PATH="${PATH}:/root/.dotnet/tools"

# Copiar arquivos necessários para migrations
COPY --from=build /src/*.csproj ./
COPY --from=build /src/Data ./Data
COPY --from=build /src/Models ./Models
COPY --from=build /src/DTOs ./DTOs
COPY --from=build /src/Controllers ./Controllers
COPY --from=build /src/Services ./Services
COPY --from=build /src/Program.cs ./

# Copiar os binários publicados
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "CondominioEntregas.API.dll"]
