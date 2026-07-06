using DataArc.Core;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBContexts;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBModels;

namespace DataArc.EntityFrameworkCore.Demo.Persistence.Database.Seeders
{
    public interface IMicrosoftDbSeeder : IDatabaseSeeder
    {

    }

    internal class MicrosoftDbSeeder : IMicrosoftDbSeeder
    {
        private readonly ICommandFactory _commandFactory;

        public MicrosoftDbSeeder(ICommandFactory commandFactory)
        {
            _commandFactory = commandFactory;
        }

        public bool SeedDatabase()
        {
            try
            {
                var seedingCommand = _commandFactory.CreateCommand();

                var commandResult = seedingCommand
                    .UseDbExecutionContext<MicrosoftDbContext>()
                    .Add(new Employer()
                    {
                        Name = "Google",
                        Description = "Google Company"
                    }).Execute();


                if (!commandResult.Success)
                    throw new Exception($"Exception occured in {nameof(MicrosoftDbSeeder)}, {commandResult.Message}");

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

                var commandResult = await seedingCommand
                    .UseDbExecutionContext<MicrosoftDbContext>()
                    .Add(new Employer() 
                    { 
                        Name = "Google",
                        Description = "Google Company"
                    })
                    .ExecuteAsync();

                if (!commandResult.Success)
                    throw new Exception($"Exception occured in {nameof(MicrosoftDbSeeder)}, {commandResult.Message}");

                return true;
            }
            catch
            {
                throw;
            }
        }
    }
}