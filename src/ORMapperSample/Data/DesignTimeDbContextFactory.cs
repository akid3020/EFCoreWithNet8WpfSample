using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace ORMapperSample.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            var databaseProvider = configuration["DatabaseProvider"];
            var connectionString = configuration.GetConnectionString(databaseProvider);

            switch (databaseProvider)
            {
                case "MySQL":
                    var serverVersion = ServerVersion.AutoDetect(connectionString);
                    optionsBuilder.UseMySql(connectionString, serverVersion);
                    break;
                case "SQLite":
                default:
                    optionsBuilder.UseSqlite(connectionString);
                    break;
            }

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}