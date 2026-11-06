using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Gajyun.Data;

public sealed class GajyunDbContextFactory : IDesignTimeDbContextFactory<GajyunDbContext>
{
    public GajyunDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("GAJYUN_DESIGN_CONNECTION")
            ?? @"Server=(localdb)\MSSQLLocalDB;Database=GuesthouseGajyunDb;Trusted_Connection=True;TrustServerCertificate=True;";
        var options = new DbContextOptionsBuilder<GajyunDbContext>().UseSqlServer(connectionString).Options;
        return new GajyunDbContext(options);
    }
}
