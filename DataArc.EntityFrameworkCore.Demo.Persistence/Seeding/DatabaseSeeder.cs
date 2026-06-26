using DataArc.Core;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBContexts;
using DataArc.EntityFrameworkCore.Demo.Persistence.Utils;

namespace DataArc.EntityFrameworkCore.Demo.Persistence.Seeding
{
    public interface IDatabaseSeeder
    {
        bool SeedDatabase(int recordCount);
        Task<bool> SeedDatabaseAsync(int recordCount);
    }

    public class DatabaseSeeder : IDatabaseSeeder
    {
        private readonly ICommandFactory _commandFactory;
        
        public DatabaseSeeder(ICommandFactory commandFactory)
        {
            _commandFactory = commandFactory;
        }

        public bool SeedDatabase(int recordCount)
        {
            try
            {
                var seedingCommand = _commandFactory.CreateCommand();
                seedingCommand
                  .UseDbExecutionContext<HrDbContext>()
                  .AddBulk(SeedDataGenerator.GenerateHrSeedData(recordCount), recordCount)
                  .Execute();

                return true;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> SeedDatabaseAsync(int recordCount)
        {
            try
            {
                // Build and execute the seeding command
                var seedingCommand = await _commandFactory.CreateCommandAsync();
                await seedingCommand
                    .UseDbExecutionContext<HrDbContext>()
                    .AddBulk(SeedDataGenerator.GenerateHrSeedData(recordCount), recordCount)
                    .ExecuteAsync();

                return true;
            }
            catch
            {
                throw;
            }
        }
    }
}