using System;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WolfeReiter.Identity.Data.Models;
using WolfeReiter.Identity.Data.Models.SqlServer;

namespace WolfeReiter.Identity.Data
{
    /// <summary>
    /// SQL Server-specific context. Configures SQL Server-specific features for entities.
    /// </summary>
    public class SqlServerContext : SharedDbContext, IDataProtectionKeyContext
    {
        public SqlServerContext(DbContextOptions<SqlServerContext> options) : base(options)
        {
            Cache = Set<Cache>();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Cache> Cache { get; set; }
    }
}