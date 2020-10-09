using System;
using Microsoft.EntityFrameworkCore;

namespace WolfeReiter.Identity.Data
{
    /// <summary>
    /// SQL Server-specific context. Configures SQL Server-specific features for entities.
    /// </summary>
    public class SqlServerContext : SharedDbContext
    {
        public SqlServerContext(DbContextOptions<SqlServerContext> options) : base(options)
        {
        }
    }
}