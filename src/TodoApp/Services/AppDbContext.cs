using Microsoft.EntityFrameworkCore;

namespace TodoApp.Services
{
    public class AppDbContext : DbContext
    {
        public DbSet<DbUser> Users { get; protected set; } = null!;
        public DbSet<DbSession> Sessions { get; protected set; } = null!;

        public AppDbContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var user = modelBuilder.Entity<DbUser>();
            user.HasKey(e => e.Id);
            user.HasIndex(e => e.Name).IsUnique();
            user.HasIndex(e => e.Email);

            var session = modelBuilder.Entity<DbSession>();
            session.HasKey(e => e.Id);
            session.HasIndex(e => e.UserId);
        }
    }
}
