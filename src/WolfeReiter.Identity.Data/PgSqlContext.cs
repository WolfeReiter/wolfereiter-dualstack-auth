using System;
using Microsoft.EntityFrameworkCore;
using WolfeReiter.Identity.Data.Models;

namespace WolfeReiter.Identity.Data
{
    /// <summary>
    /// PostgreSQL-specific context. Configures PostgreSQL-specific features for entities.
    /// </summary>
    public class PgSqlContext : SharedDbContext
    {
        public PgSqlContext(DbContextOptions<PgSqlContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}