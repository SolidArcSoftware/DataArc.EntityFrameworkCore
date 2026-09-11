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
        private readonly IDbContextFactory<MicrosoftDbContext> _microsoftDbContextFactory;

        public MicrosoftDbSeeder(IDbContextFactory<MicrosoftDbContext> microsoftDbContextFactory)
        {
            _microsoftDbContextFactory = microsoftDbContextFactory;
        }

        public async Task<bool> SeedDatabaseAsync()
        {
            await using var dbContext = await _microsoftDbContextFactory.CreateDbContextAsync();

            await dbContext.AddAsync(new Employer
            {
                Name = "Microsoft",
                Description = "Microsoft Company"
            });

            await dbContext.SaveChangesAsync();

            return true;
        }
    }
}