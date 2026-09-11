using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBContexts;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBModels;
using DataArc.EntityFrameworkCore.Demo.Persistence.Utils;

using Microsoft.EntityFrameworkCore;

namespace DataArc.EntityFrameworkCore.Demo.Persistence.Database.Seeders
{
    public interface ISolidArcDbSeeder : IDatabaseSeeder
    {
    }

    internal class SolidArcDbSeeder : ISolidArcDbSeeder
    {
        private readonly IDbContextFactory<SolidArcDbContext> _solidArcDbContextFactory;

        public SolidArcDbSeeder(IDbContextFactory<SolidArcDbContext> solidArcDbContextFactory)
        {
            _solidArcDbContextFactory = solidArcDbContextFactory;
        }

        public async Task<bool> SeedDatabaseAsync()
        {
            await using var dbContext = await _solidArcDbContextFactory.CreateDbContextAsync();

            var employer = new Employer
            {
                Name = "SolidArcSoftware",
                Description = "SolidArc Company"
            };

            await dbContext.AddAsync(employer);
            await dbContext.SaveChangesAsync();

            await dbContext.AddBulkAsync(
                SeedDataGenerator.GenerateHrSeedData(100_000),
                100_000);

            return true;
        }
    }
}