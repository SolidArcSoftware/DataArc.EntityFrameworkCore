using DataArc.Core;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBContexts;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBModels;

namespace DataArc.EntityFrameworkCore.Demo.Persistence.Database.Seeders
{
    public interface IOpenAIDbSeeder : IDatabaseSeeder
    {

    }

    internal class OpenAIDbSeeder : IOpenAIDbSeeder
    {
        private readonly ICommandFactory _commandFactory;

        public OpenAIDbSeeder(ICommandFactory commandFactory)
        {
            _commandFactory = commandFactory;
        }

        public bool SeedDatabase()
        {
            try
            {
                var seedingCommand = _commandFactory.CreateCommand();

                var commandResult = seedingCommand
                    .UseDbExecutionContext<OpenAIDbContext>()
                    .Add(new Employer()
                    {
                        Name = "Microsoft",
                        Description = "Microsoft Company"
                    }).Execute();


                if (!commandResult.Success)
                    throw new Exception($"Exception occured in {nameof(OpenAIDbSeeder)}, {commandResult.Message}");

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
                    .UseDbExecutionContext<OpenAIDbContext>()
                    .Add(new Employer()
                    {
                        Name = "Microsoft",
                        Description = "Microsoft Company"
                    })
                    .ExecuteAsync();

                if (!commandResult.Success)
                    throw new Exception($"Exception occured in {nameof(OpenAIDbSeeder)}, {commandResult.Message}");

                return true;
            }
            catch
            {
                throw;
            }
        }
    }
}