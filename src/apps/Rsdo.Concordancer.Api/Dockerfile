FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

# INSTALL NODE.JS
RUN curl -fsSL https://deb.nodesource.com/setup_16.x | bash - \
    && apt-get install -y \
        nodejs \
    && rm -rf /var/lib/apt/lists/*

# RESTORE PACKAGES (with project files only)
WORKDIR /src/apps
COPY src/apps/Rsdo.Concordancer.Api/Rsdo.Concordancer.Api.csproj Rsdo.Concordancer.Api/Rsdo.Concordancer.Api.csproj

WORKDIR /src/core
COPY src/core/Rsdo.Concordancer.Core/Rsdo.Concordancer.Core.csproj Rsdo.Concordancer.Core/Rsdo.Concordancer.Core.csproj 
COPY src/core/Rsdo.Concordancer.Data/Rsdo.Concordancer.Data.csproj Rsdo.Concordancer.Data/Rsdo.Concordancer.Data.csproj 
COPY src/core/Rsdo.Concordancer.Infrastructure/Rsdo.Concordancer.Infrastructure.csproj Rsdo.Concordancer.Infrastructure/Rsdo.Concordancer.Infrastructure.csproj 
COPY src/core/Rsdo.Concordancer.ServiceModel/Rsdo.Concordancer.ServiceModel.csproj Rsdo.Concordancer.ServiceModel/Rsdo.Concordancer.ServiceModel.csproj
COPY src/core/Rsdo.Concordancer.Services/Rsdo.Concordancer.Services.csproj Rsdo.Concordancer.Services/Rsdo.Concordancer.Services.csproj

WORKDIR /src/apps/Rsdo.Concordancer.Api
RUN dotnet restore Rsdo.Concordancer.Api.csproj

# COPY ALL FILES
WORKDIR /src/apps
COPY src/apps/Rsdo.Concordancer.Api/. Rsdo.Concordancer.Api/

WORKDIR /src/core
COPY src/core/Rsdo.Concordancer.Core/. Rsdo.Concordancer.Core/
COPY src/core/Rsdo.Concordancer.Data/. Rsdo.Concordancer.Data/
COPY src/core/Rsdo.Concordancer.Infrastructure/. Rsdo.Concordancer.Infrastructure/
COPY src/core/Rsdo.Concordancer.ServiceModel/. Rsdo.Concordancer.ServiceModel/
COPY src/core/Rsdo.Concordancer.Services/. Rsdo.Concordancer.Services/

# BUILD WEBAPP AND AFTER THAT API, WHICH INCLUDES WEBAPP
FROM build AS publish

WORKDIR /src/apps/Rsdo.Concordancer.Api/WebApp
ARG WEBAPP_PUBLIC_URL=''
ENV PUBLIC_URL=${WEBAPP_PUBLIC_URL}
RUN npm install && npm run build

WORKDIR /src/apps/Rsdo.Concordancer.Api
RUN dotnet publish "Rsdo.Concordancer.Api.csproj" -c Release -o /app/publish

# COPY TO FINAL
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Rsdo.Concordancer.Api.dll"]
