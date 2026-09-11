using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBContexts;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBModels;

using Microsoft.EntityFrameworkCore;

namespace DataArc.EntityFrameworkCore.Demo.Persistence.Database.Seeders
{
    public interface IGoogleDBSeeder : IDatabaseSeeder
    {
    }

    internal class GoogleDBSeeder : IGoogleDBSeeder
    {
        private readonly IDbContextFactory<GoogleDbContext> _googleDbContextFactory;

        public GoogleDBSeeder(IDbContextFactory<GoogleDbContext> googleDbContextFactory)
        {
            _googleDbContextFactory = googleDbContextFactory;
        }

        public async Task<bool> SeedDatabaseAsync()
        {
            await using var dbContext = await _googleDbContextFactory.CreateDbContextAsync();

            await dbContext.AddAsync(new Employer
            {
                Name = "Google",
                Description = "Google Company"
            });

            await dbContext.SaveChangesAsync();

            return true;
        }
    }
}