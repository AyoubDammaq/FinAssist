# Consultez https://aka.ms/customizecontainer pour savoir comment personnaliser votre conteneur de débogage et comment Visual Studio utilise ce Dockerfile pour générer vos images afin d'accélérer le débogage.

# Cet index est utilisé lors de l'exécution à partir de VS en mode rapide (par défaut pour la configuration de débogage)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8082
EXPOSE 8083

# Création d'un utilisateur non-root pour la sécurité (compatible Debian)
RUN useradd -m -s /bin/bash appuser
USER appuser

# Cette phase est utilisée pour générer le projet de service
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
		
# Copier les fichiers .csproj et restaurer les dépendances (meilleure utilisation du cache Docker)
COPY ["TransactionService.API/TransactionService.API.csproj", "TransactionService.API/"]
COPY ["TransactionService.Infrastructure/TransactionService.Infrastructure.csproj", "TransactionService.Infrastructure/"]
COPY ["TransactionService.Application/TransactionService.Application.csproj", "TransactionService.Application/"]
COPY ["TransactionService.Domain/TransactionService.Domain.csproj", "TransactionService.Domain/"]
RUN dotnet restore "TransactionService.API/TransactionService.API.csproj"

# Copier tout le reste du code source
COPY . .

# Build du projet
WORKDIR "/src/TransactionService.API"
RUN dotnet build "TransactionService.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Cette étape permet de publier le projet de service à copier dans la phase finale
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "TransactionService.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Cette phase est utilisée en production ou lors de l'exécution à partir de VS en mode normal (par défaut quand la configuration de débogage n'est pas utilisée)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Healthcheck pour vérifier la santé du conteneur (sans curl qui n'est pas installé par défaut)
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD dotnet --info > /dev/null || exit 1

# Variables d'environnement pour la configuration
ENV ASPNETCORE_URLS=http://+:8082

ENTRYPOINT ["dotnet", "TransactionService.API.dll"]