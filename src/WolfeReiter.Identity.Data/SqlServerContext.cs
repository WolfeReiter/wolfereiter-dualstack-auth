using System;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WolfeReiter.Identity.Data.Models;

namespace WolfeReiter.Identity.Data
{
    /// <summary>
    /// SQL Server-specific context. Configures SQL Server-specific features for entities.
    /// </summary>
    public class SqlServerContext : SharedDbContext, IDataProtectionKeyContext
    {
        public SqlServerContext(DbContextOptions<SqlServerContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}