using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBContexts;
using DataArc.EntityFrameworkCore.Demo.Persistence.Utils;

using Microsoft.EntityFrameworkCore;

namespace DataArc.EntityFrameworkCore.Demo.Persistence.Database.Seeders
{
    public interface ISolidArcDbSeeder : IDatabaseSeeder
    {
    }

    internal class SolidArcDbSeeder : ISolidArcDbSeeder
    {
        private const int SeedRecordCount = 100_000;

        private readonly IDbContextFactory<SolidArcDbContext> _dbContextFactory;

        public SolidArcDbSeeder(
            IDbContextFactory<SolidArcDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public bool SeedDatabase()
        {
            using var dbContext =
                _dbContextFactory.CreateDbContext();

            var seedData =
                SeedDataGenerator.GenerateHrSeedData(SeedRecordCount);

            dbContext.Employer!.AddRange(seedData.Employers);
            dbContext.SaveChanges();

            dbContext.Employee!.AddRange(seedData.Employees);
            dbContext.SaveChanges();

            return true;
        }

        public async Task<bool> SeedDatabaseAsync()
        {
            await using var dbContext =
                await _dbContextFactory.CreateDbContextAsync();

            var seedData =
                SeedDataGenerator.GenerateHrSeedData(SeedRecordCount);

            await dbContext.AddBulkAsync(
                seedData.Employers,
                SeedRecordCount);

            await dbContext.AddBulkAsync(
                seedData.Employees,
                SeedRecordCount);

            return true;
        }
    }
}