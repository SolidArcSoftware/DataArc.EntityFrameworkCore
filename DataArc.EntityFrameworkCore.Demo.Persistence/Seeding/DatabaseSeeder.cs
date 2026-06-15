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
        private readonly ICommand _command;
        private readonly IAsyncCommand _asyncCommand;
        public DatabaseSeeder(ICommand command, IAsyncCommand asyncCommand)
        {
            _command = command;
            _asyncCommand = asyncCommand;
        }

        public bool SeedDatabase(int recordCount)
        {
            try
            {
                var seedingCommand = _command
                  .UseExecutionContext<HrDbContext>()
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
                var seedingCommand = await _asyncCommand
                    .UseExecutionContext<HrDbContext>()
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