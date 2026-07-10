# =============================================================
# Dockerfile de PRODUÇÃO do EM.Web
# Build a partir da RAIZ do repositório (contexto precisa alcançar
# EM.Domain e EM.Repository, referenciados pelo EM.Web):
#   docker build -f ProjetoAtual/EM.Web/Dockerfile -t projetoem-web .
# O compose em deploy/docker-compose.producao.yml já faz isso.
# =============================================================

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia csprojs primeiro para aproveitar o cache de restore
COPY ProjetoAtual/EM.Domain/EM.Domain.csproj       EM.Domain/
COPY ProjetoAtual/EM.Repository/EM.Repository.csproj EM.Repository/
COPY ProjetoAtual/EM.Web/EM.Web.csproj             EM.Web/
RUN dotnet restore EM.Web/EM.Web.csproj

COPY ProjetoAtual/EM.Domain/     EM.Domain/
COPY ProjetoAtual/EM.Repository/ EM.Repository/
COPY ProjetoAtual/EM.Web/        EM.Web/
RUN dotnet publish EM.Web/EM.Web.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Imagens aspnet do .NET 8+ escutam em 8080 por padrão; deixamos explícito
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "EM.Web.dll"]
