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

DotNetEnv.Env.Load();

// Rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            key => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
});

// Use PostgreSQL instead of SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// For TaskService (need to register in dependency injection):
builder.Services.AddScoped<TaskService>();

// DO NOT NEED THIS ANYMORE I BELIEVE SINCE CUSTOMMIDDLEWARE NO LONGER EXPLCIITLY NEEDS TO BE REGISTERED
// Register Middleware (logging, error handling)
// builder.Services.AddTransient<CustomMiddleware>();

// Authentication (JWT Authentication) => JWT Secret is passed as env variable in docker run command
var jwtKey = builder.Configuration["JwtSettings:Secret"] ?? throw new ArgumentNullException("JWT Secret is missing!");
Console.WriteLine($"🔑 JWT Secret Loaded: {jwtKey}");

// modify JWT authentication to read token from HttpOnly cookie, ASP.NET Core by default reads token from Authorization header so we tell it to read from cookie instead
// builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//     .AddJwtBearer(options =>
//     {
//         options.RequireHttpsMetadata = false; // Only disabled for local development for now
//         options.SaveToken = true;
//         options.TokenValidationParameters = new TokenValidationParameters
//         {
//             ValidateIssuerSigningKey = true,
//             IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
//             ValidateIssuer = false,
//             ValidateAudience = false,
//             ValidateLifetime = true,
//             ClockSkew = TimeSpan.Zero
//         };
        
        // allow the token to be read from cookies
        // options.Events = new JwtBearerEvents
        // {
        //     OnMessageReceived = context =>
        //     {
        //         var tokenFromCookie = context.Request.Cookies.ContainsKey("AuthToken") ? context.Request.Cookies["AuthToken"] : null;
        //         Console.WriteLine($"📌 Debug: Token from Cookie: {tokenFromCookie}");

        //         var tokenFromHeader = context.Request.Headers.ContainsKey("Authorization") ? context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "") : null;
        //         Console.WriteLine($"📌 Debug: Token from Header: {tokenFromHeader}");

        //         if (!string.IsNullOrEmpty(tokenFromCookie))
        //         {
        //             context.Token = tokenFromCookie;
        //         }
        //         else if (!string.IsNullOrEmpty(tokenFromHeader))
        //         {
        //             context.Token = tokenFromHeader;
        //         }

        //         Console.WriteLine($"📌 Final Token Being Used: {context.Token}");

        //         return Task.CompletedTask;
        //     }
        // };


        // options.Events = new JwtBearerEvents
        // {
        //     OnMessageReceived = context =>
        //     {
        //         if (context.Request.Cookies.ContainsKey("AuthToken"))
        //         {
        //             context.Token = context.Request.Cookies["AuthToken"]; // Get token from cookie
        //             Console.WriteLine($"📌 Debug: Retrieved token from cookie: {context.Token}");
        //         }

        //         if (string.IsNullOrEmpty(context.Token) && context.Request.Headers.ContainsKey("Authorization"))
        //         {
        //             context.Token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        //         }

        //         return Task.CompletedTask;
        //     }
        // };



// var key = Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"]);
var key = Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"]);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Cookies["AuthToken"];
                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token;
                    Console.WriteLine($"📌 Debug: Retrieved token from cookie: {context.Token}");
                }
                return Task.CompletedTask;
            }
        };
    });

// Bind Email Settings from Environment Variables
// Load environment variables (for Docker)
builder.Configuration["EmailSettings:SmtpHost"] = Environment.GetEnvironmentVariable("SMTP_HOST") 
    ?? builder.Configuration["EmailSettings:SmtpHost"];

builder.Configuration["EmailSettings:SmtpPort"] = Environment.GetEnvironmentVariable("SMTP_PORT") 
    ?? builder.Configuration["EmailSettings:SmtpPort"];

builder.Configuration["EmailSettings:SmtpUser"] = Environment.GetEnvironmentVariable("SMTP_USER") 
    ?? builder.Configuration["EmailSettings:SmtpUser"];

builder.Configuration["EmailSettings:SmtpPass"] = Environment.GetEnvironmentVariable("SMTP_PASS") 
    ?? builder.Configuration["EmailSettings:SmtpPass"];

builder.Configuration["EmailSettings:FromEmail"] = Environment.GetEnvironmentVariable("FROM_EMAIL") 
    ?? builder.Configuration["EmailSettings:FromEmail"];

builder.Configuration["EmailSettings:FromName"] = Environment.GetEnvironmentVariable("FROM_NAME") 
    ?? builder.Configuration["EmailSettings:FromName"];

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
// app.UseMiddleware<CustomMiddleware>();

// // Rate limiting
// app.UseRateLimiter(); // Enables IP based rate limiting

app.UseHttpsRedirection();



// Standard

app.UseAuthentication();
app.UseAuthorization();

// Enable CORS (2/2)
app.UseCors("AllowFrontend");

app.UseMiddleware<CustomMiddleware>();


// Rate limiting
app.UseRateLimiter(); // Enables IP based rate limiting

// Map Controller Endpoints, has to be after Authentication and Authorization
app.MapControllers();

var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Urls.Add($"http://0.0.0.0:{port}");

app.Run();