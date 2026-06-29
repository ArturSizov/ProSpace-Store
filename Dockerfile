# 1. Runtime base layer
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# 2. Build Layer (SDK)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy the solution and project files to restore dependencies.
COPY ["ProSpace-Store.sln", "./"]
COPY ["ProSpace.Api/ProSpace.Api.csproj", "ProSpace.Api/"]
COPY ["ProSpace.Application/ProSpace.Application.csproj", "ProSpace.Application/"]
COPY ["ProSpace.Contracts/ProSpace.Contracts.csproj", "ProSpace.Contracts/"]
COPY ["ProSpace.Domain/ProSpace.Domain.csproj", "ProSpace.Domain/"]
COPY ["ProSpace.Infrastructure/ProSpace.Infrastructure.csproj", "ProSpace.Infrastructure/"]

RUN dotnet restore "ProSpace.Api/ProSpace.Api.csproj"

# Copy the source code and compile it.
COPY . .
WORKDIR "/src/ProSpace.Api"
RUN dotnet build -c $BUILD_CONFIGURATION -o /app/build

# 3. Publication layer
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# 4. Final launch layer (copying results from `publish` to `base`)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProSpace.Api.dll"]
