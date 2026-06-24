# ---- build ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Сначала только манифесты — слой restore кэшируется, пока не меняются зависимости.
COPY global.json ./
COPY TaskManagement.sln ./
COPY src/TaskManagement.Api/TaskManagement.Api.csproj src/TaskManagement.Api/
COPY src/TaskManagement.Application/TaskManagement.Application.csproj src/TaskManagement.Application/
COPY src/TaskManagement.Infrastructure/TaskManagement.Infrastructure.csproj src/TaskManagement.Infrastructure/
RUN dotnet restore src/TaskManagement.Api/TaskManagement.Api.csproj

# Остальной код и публикация.
COPY src/ src/
RUN dotnet publish src/TaskManagement.Api/TaskManagement.Api.csproj -c Release -o /app --no-restore

# ---- runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app ./

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "TaskManagement.Api.dll"]
