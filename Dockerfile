# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy all source code
COPY . ./
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app
COPY --from=build /app/publish .

# Set environment variable for port
ENV ASPNETCORE_URLS=http://+:8080

# Expose port
EXPOSE 8080

# Expose persistent volume for SQLite
RUN mkdir -p /data
VOLUME /data

# Entry point
ENTRYPOINT ["dotnet", "Botyo.dll"]
