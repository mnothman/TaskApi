using Microsoft.EntityFrameworkCore;
using TaskApi.Data;
using TaskApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Register SQLite Database Context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Controllers
builder.Services.AddControllers();

// Enable API Explorer and Swagger for testing
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// For TaskService (need to register in dependency injection):
builder.Services.AddScoped<TaskService>();

// Build after registering above dependencies
var app = builder.Build();

// Enable Swagger UI for API documentation in development mode
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

// Map Controller Endpoints
app.MapControllers();

var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Urls.Add($"http://0.0.0.0:{port}");

app.Run();
