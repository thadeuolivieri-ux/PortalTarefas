# ==========================================
# ETAPA 1: BUILD E PUBLICAÇÃO (SDK)
# ==========================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar arquivos de projeto e restaurar dependências
COPY ["PortalTarefas.Web/PortalTarefas.Web.csproj", "PortalTarefas.Web/"]
RUN dotnet restore "PortalTarefas.Web/PortalTarefas.Web.csproj"

# Copiar o restante do código e publicar
COPY . .
WORKDIR "/src/PortalTarefas.Web"
RUN dotnet publish "PortalTarefas.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ==========================================
# ETAPA 2: RUNTIME LEVE E SEGURA
# ==========================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Instalar curl para execução do HealthCheck do Docker
USER root
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Alternar para usuário não privilegiado
USER app

# Copiar os artefatos compilados
COPY --from=build /app/publish .

# Configuração de rede e variáveis de ambiente
ENV ASPNETCORE_URLS=http://+:8080
ENV ConnectionStrings__DefaultConnection="Data Source=PortalTarefas.db"
EXPOSE 8080

# Health Check do Contêiner
HEALTHCHECK --interval=15s --timeout=5s --retries=3 --start-period=5s \
  CMD curl -f http://localhost:8080/health/live || exit 1

ENTRYPOINT ["dotnet", "PortalTarefas.Web.dll"]
