# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/BookWorm.Api/BookWorm.Api.csproj", "src/BookWorm.Api/"]
RUN dotnet restore "src/BookWorm.Api/BookWorm.Api.csproj"

COPY src/ src/
WORKDIR "/src/src/BookWorm.Api"
RUN dotnet publish "BookWorm.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime (non-root)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

USER $APP_UID

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BookWorm.Api.dll"]
