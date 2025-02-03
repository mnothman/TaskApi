# Use the official .NET SDK image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Install dotnet-ef tool globally
RUN dotnet tool install --global dotnet-ef
ENV PATH="/root/.dotnet/tools:$PATH"

# Set working directory
WORKDIR /app

# Copy project files
COPY . .

# Restore dependencies
RUN dotnet restore

# Install Microsoft.EntityFrameworkCore.Sqlite
RUN dotnet add package Microsoft.EntityFrameworkCore.Sqlite

# Install Microsoft.EntityFrameworkCore.Design
RUN dotnet add package Microsoft.EntityFrameworkCore.Design

# Build the project
RUN dotnet build --no-restore

# Run EF Core migrations to update the database
RUN dotnet ef database update

# Start the app
ENTRYPOINT ["dotnet", "run"]
