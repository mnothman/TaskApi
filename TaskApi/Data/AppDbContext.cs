using Microsoft.EntityFrameworkCore;
using TaskApi.Models;

namespace TaskApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext() {}

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

        // Add OnConfiguring to ensure EF always knows what database to use
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("DataSource=taskdb.sqlite"); // Default fallback for migrations
            }
        }

        public DbSet<TaskModel> Tasks { get; set; }
    }
}
