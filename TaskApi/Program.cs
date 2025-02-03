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
            ValidateAudience = false
        };
    });

// Authorization
builder.Services.AddAuthorization();

// Cors FOR NOW WE WILL ALLOW ALL REQUESTS, CHANGE AFTER (Config this later when add frontend 1/2)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
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

// Enable Cors (config this later when add frontend 2/2)
// app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.UseCors("AllowAll");

// Map Controller Endpoints
app.MapControllers();

var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Urls.Add($"http://0.0.0.0:{port}");

app.Run();