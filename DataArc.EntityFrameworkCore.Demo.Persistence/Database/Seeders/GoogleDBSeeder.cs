using DataArc.Core;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBContexts;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBModels;

namespace DataArc.EntityFrameworkCore.Demo.Persistence.Database.Seeders
{
    public interface IGoogleDBSeeder : IDatabaseSeeder
    {

    }

    internal class GoogleDBSeeder : IGoogleDBSeeder
    {
        private readonly ICommandFactory _commandFactory;
        public GoogleDBSeeder(ICommandFactory commandFactory)
        {
            _commandFactory = commandFactory;
        }

        public bool SeedDatabase()
        {
            try
            {
                var seedingCommand = _commandFactory.CreateCommand();

                var commandResult = seedingCommand
                    .UseDbExecutionContext<GoogleDbContext>()
                    .Add(new Employer()
                    {
                        Name = "Solid Arc Software",
                        Description = "Software Company"
                    }).Execute();


                if (!commandResult.Success)
                    throw new Exception($"Exception occured in {nameof(GoogleDBSeeder)}, {commandResult.Message}");

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
                    .UseDbExecutionContext<GoogleDbContext>()
                    .Add(new Employer()
                    {
                        Name = "Solid Arc Software",
                        Description = "Software Company"
                    })
                    .ExecuteAsync();

                if (!commandResult.Success)
                    throw new Exception($"Exception occured in {nameof(GoogleDBSeeder)}, {commandResult.Message}");

                return true;
            }
            catch
            {
                throw;
            }
        }
    }
}