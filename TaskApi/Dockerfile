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

# Install PostgreSQL support for Entity Framework
RUN dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL

# BCrypt error fix
RUN dotnet add package BCrypt.Net-Next

# Install Microsoft.EntityFrameworkCore.Design
RUN dotnet add package Microsoft.EntityFrameworkCore.Design

# Install required authentication & JWT packages
RUN dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.2
RUN dotnet add package Microsoft.IdentityModel.Tokens --version 8.3.1

# Install Data Protection package
RUN dotnet add package Microsoft.AspNetCore.DataProtection

# Install email libraries
RUN dotnet add package MimeKit
RUN dotnet add package MailKit

# Install DotNetEnv for Environment Variables
RUN dotnet add package DotNetEnv

# Fix JWT error
RUN dotnet add package System.IdentityModel.Tokens.Jwt --version 8.3.1

COPY .env /app/.env

# Build the project
RUN dotnet build --no-restore

# Set a default JWT secret key for build-time operations
ENV JwtSettings__Secret="YourVeryStrongSecretKeyWithAtLeast32Characters!"

# Start the app
ENTRYPOINT ["dotnet", "run"]
