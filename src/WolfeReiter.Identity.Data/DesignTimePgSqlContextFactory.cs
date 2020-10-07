using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;


namespace WolfeReiter.Identity.Data
{
    public class DesignTimePgSqlContextFactory : IDesignTimeDbContextFactory<PgSqlContext>
    {
        public PgSqlContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var connectionString = configuration.GetConnectionString("PgSqlConnection");
            var optionsBuilder = new DbContextOptionsBuilder<PgSqlContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new PgSqlContext(optionsBuilder.Options);
        }
    }
}