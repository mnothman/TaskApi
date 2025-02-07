using Microsoft.EntityFrameworkCore;
using TaskApi.Data;
using TaskApi.Services;
using TaskApi.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.DataProtection;
using System.IO;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add rate limiting, completed for get task and create task
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("get-tasks", limiterOptions =>
    {
        limiterOptions.PermitLimit = 20; 
        limiterOptions.Window = TimeSpan.FromMinutes(1);
    });

    options.AddFixedWindowLimiter("create-task", limiterOptions =>
    {
        limiterOptions.PermitLimit = 5;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
    });
});


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

// modify JWT authentication to read token from HttpOnly cookie, ASP.NET Core by default reads token from Authorization header so we tell it to read from cookie instead
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // Only disabled for local development for now
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
        
        // allow the token to be read from cookies
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.ContainsKey("AuthToken"))
                {
                    context.Token = context.Request.Cookies["AuthToken"]; // Get token from cookie
                }
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
              .AllowAnyHeader()
              .AllowCredentials();
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

// Enable rate limiting globally, this is only for global limiting only (don't use we fine tuned get-task and create-task)
// app.UseRateLimiter();

// Standard
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