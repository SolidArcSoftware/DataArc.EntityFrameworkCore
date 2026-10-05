using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBContexts;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBModels;

using Microsoft.EntityFrameworkCore;

namespace DataArc.EntityFrameworkCore.Demo.Persistence.Database.Seeders
{
    public interface IMicrosoftDbSeeder : IDatabaseSeeder
    {
    }

    internal class MicrosoftDbSeeder : IMicrosoftDbSeeder
    {
        private readonly IDbContextFactory<MicrosoftDbContext> _dbContextFactory;

        public MicrosoftDbSeeder(
            IDbContextFactory<MicrosoftDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public bool SeedDatabase()
        {
            using var dbContext = _dbContextFactory.CreateDbContext();

            dbContext.Employer!.Add(
                new Employer
                {
                    Name = "Google",
                    Description = "Google Company"
                });

            dbContext.SaveChanges();

            return true;
        }

        public async Task<bool> SeedDatabaseAsync()
        {
            await using var dbContext =
                await _dbContextFactory.CreateDbContextAsync();

            await dbContext.Employer!.AddAsync(
                new Employer
                {
                    Name = "Google",
                    Description = "Google Company"
                });

            await dbContext.SaveChangesAsync();

            return true;
        }
    }
}