# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files
COPY ["RaceDayMVC.csproj", "./"]

# Restore packages
RUN dotnet restore "RaceDayMVC.csproj"

# Copy the remaining files
COPY . .

# Publish the application
RUN dotnet publish "RaceDayMVC.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "RaceDayMVC.dll"]