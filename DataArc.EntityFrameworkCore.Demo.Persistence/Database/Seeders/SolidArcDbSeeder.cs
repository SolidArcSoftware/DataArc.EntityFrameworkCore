using DataArc.Core;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBContexts;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBModels;
using DataArc.EntityFrameworkCore.Demo.Persistence.Utils;

namespace DataArc.EntityFrameworkCore.Demo.Persistence.Database.Seeders
{
    public interface ISolidArcDbSeeder : IDatabaseSeeder
    {

    }

    internal class SolidArcDbSeeder : ISolidArcDbSeeder
    {
        private readonly ICommandFactory _commandFactory;

        public SolidArcDbSeeder(ICommandFactory commandFactory)
        {
            _commandFactory = commandFactory;
        }

        public bool SeedDatabase()
        {
            try
            {
                var seedingCommand = _commandFactory.CreateCommand();

                var commandResult = seedingCommand
                    .UseDbExecutionContext<SolidArcDbContext>()
                    .Add(new Employer()
                    {
                        Name = "OpenAI",
                        Description = "OpenAI Company"
                    }).Execute();


                if (!commandResult.Success)
                    throw new Exception($"Exception occured in {nameof(SolidArcDbSeeder)}, {commandResult.Message}");

                return true;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> SeedDatabaseAsync()
        {
            try
            {
                var seedingCommand = await _commandFactory.CreateCommandAsync();

                var employer = new Employer()
                {
                    Name = "OpenAI",
                    Description = "OpenAI Company"
                };

                var commandResult = await seedingCommand
                    .UseDbExecutionContext<SolidArcDbContext>()
                    .Add(employer)
                    .ExecuteAsync();

                if (!commandResult.Success)
                    throw new Exception($"Exception occured in {nameof(SolidArcDbSeeder)}, {commandResult.Message}");

                var bulkSeedCommand = await _commandFactory.CreateCommandAsync();

                var bulkSeedCommandResult = await seedingCommand
                   .UseDbExecutionContext<SolidArcDbContext>()
                   .AddBulk(SeedDataGenerator.GenerateHrSeedData(100_000), 100_000)
                   .ExecuteAsync();

                if(!bulkSeedCommandResult.Success)
                    throw new Exception($"Exception occured in {nameof(SolidArcDbSeeder)}, {bulkSeedCommandResult.Message}");

                return true;
            }
            catch
            {
                throw;
            }
        }
    }
}