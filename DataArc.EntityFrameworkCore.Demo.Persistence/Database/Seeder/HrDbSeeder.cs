using DataArc.Core;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBContexts;
using DataArc.EntityFrameworkCore.Demo.Persistence.Utils;

namespace DataArc.EntityFrameworkCore.Demo.Persistence.Database.Seeder
{
    public interface IHrDbSeeder : IDatabaseSeeder
    {
        
    }

    internal class HrDbSeeder : IHrDbSeeder
    {
        private readonly ICommandFactory _commandFactory;

        public HrDbSeeder(ICommandFactory commandFactory)
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