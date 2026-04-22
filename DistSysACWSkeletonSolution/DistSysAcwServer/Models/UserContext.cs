using Microsoft.EntityFrameworkCore;

namespace DistSysAcwServer.Models
{
    public class UserContext : DbContext
    {
        public UserContext() : base() { }

        public DbSet<User> Users { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<LogArchive> LogArchives { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=DistSysAcw;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Task 13: Define the relationship explicitly
            // This ensures that when we query a User, EF knows where to find their Logs
            modelBuilder.Entity<Log>()
                .HasOne<User>()
                .WithMany(u => u.Logs)
                .HasForeignKey(l => l.UserApiKey);

            base.OnModelCreating(modelBuilder);
        }
    }
}