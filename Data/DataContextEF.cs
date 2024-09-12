using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class DataContextEF : DbContext
    {
        private readonly IConfiguration _config;
        public DataContextEF(IConfiguration config)
        {
            _config = config;
        }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserJobInfo> UserJobInfo { get; set; }
        public virtual DbSet<UserSalary> UserSalary { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(_config.GetConnectionString("DefaultConnection"),
                    optionsBuilder => optionsBuilder.EnableRetryOnFailure());
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("dbo");
            modelBuilder.Entity<User>()
                .ToTable("Users", "dbo")
                .HasKey(u => u.UserId);
                //.HasNoKey()

            modelBuilder.Entity<UserJobInfo>()
                .HasKey(u => u.UserId);

            modelBuilder.Entity<UserSalary>()
                .HasKey(u => u.UserId);
        }
    }
}
