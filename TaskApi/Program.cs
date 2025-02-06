using Microsoft.EntityFrameworkCore;
using TaskApi.Data;
using TaskApi.Services;
using TaskApi.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.DataProtection;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "DataSource=taskdb.sqlite";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

// For TaskService (need to register in dependency injection):
builder.Services.AddScoped<TaskService>();

// DO NOT NEED THIS ANYMORE I BELIEVE SINCE CUSTOMMIDDLEWARE NO LONGER EXPLCIITLY NEEDS TO BE REGISTERED
// Register Middleware (logging, error handling)
// builder.Services.AddTransient<CustomMiddleware>();

// Authentication (JWT Authentication) => JWT Secret is passed as env variable in docker run command
var jwtKey = builder.Configuration["JwtSettings:Secret"] ?? throw new ArgumentNullException("JWT Secret is missing!");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true
        };
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"JWT Authentication Failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                Console.WriteLine("JWT Challenge Triggered");
                return Task.CompletedTask;
            },
            OnMessageReceived = context =>
            {
                var token = context.Token ?? "(No token received)";
                Console.WriteLine($"JWT Token Received: {context.Token}");
                return Task.CompletedTask;
            }
        };
    });

// Authorization
builder.Services.AddAuthorization();

// Enable CORS (1/2)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // Vite dev server
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Enable API Explorer and Swagger for testing
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// // Need data protection to avoid error about data protection keys
// builder.Services.AddDataProtection()
//     .PersistKeysToFileSystem(new DirectoryInfo("/app/DataProtection-Keys")) // Store outside the container (fix data protection warning about persistence)
//     .SetApplicationName("TaskApi");

// Store keys persistently in production, but use ephemeral keys in development
if (builder.Environment.IsProduction())
{
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo("/app/DataProtection-Keys")) // Store keys persistently
        .SetApplicationName("TaskApi");
}
else
{
    builder.Services.AddDataProtection()
        .UseEphemeralDataProtectionProvider(); // Prevents unnecessary warnings in dev mode
}

// Register Controllers
builder.Services.AddControllers();

// Build after registering above dependencies
var app = builder.Build();

// Enable Swagger UI for API documentation in development mode
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// Middleware for logging and error handling
app.UseMiddleware<CustomMiddleware>();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Enable CORS (2/2)
app.UseCors("AllowFrontend");

// Map Controller Endpoints
app.MapControllers();

var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Urls.Add($"http://0.0.0.0:{port}");

app.Run();