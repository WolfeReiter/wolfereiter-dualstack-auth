using System;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WolfeReiter.Identity.Data.Models;

namespace WolfeReiter.Identity.Data
{
    /// <summary>
    /// Configuration shared by all providers, pgsql and sqlserver.
    /// </summary>
    public abstract class SharedDbContext : DbContext, IDataProtectionKeyContext
    {
        public SharedDbContext(DbContextOptions options) : base(options)
        {
            Users                 = Set<User>();
            Roles                 = Set<Role>();
            UserRoles             = Set<UserRole>();
            DataTransformsHistory = Set<DataTransformsHistory>();
            DataProtectionKeys    = Set<DataProtectionKey>();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSnakeCaseNamingConvention();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasIndex(x => x.Name).IsUnique();

            modelBuilder.Entity<Role>().HasIndex(x => x.Name).IsUnique();

            modelBuilder.Entity<UserRole>()
                .HasOne<User>(x => x.User!)
                .WithMany(x => x.UserRoles)
                .HasForeignKey(x => x.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne<Role>(x => x.Role!)
                .WithMany(x => x.UserRoles)
                .HasForeignKey(x => x.RoleId);
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<DataTransformsHistory> DataTransformsHistory { get; set; }

        //implement IDataProtectionKeyContext for IDataProtectionBuilder.PersistKeysToDbContext<TContext>()
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
    }
}