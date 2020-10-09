using System;
using Microsoft.EntityFrameworkCore;

namespace WolfeReiter.Identity.Data
{
    /// <summary>
    /// PostgreSQL-specific context. Configures PostgreSQL-specific features for entities.
    /// </summary>
    public class PgSqlContext : SharedDbContex
    {
        public PgSqlContext(DbContextOptions<PgSqlContext> options) : base(options)
        {
        }
    }
}