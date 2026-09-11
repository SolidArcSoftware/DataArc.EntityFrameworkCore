using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBContexts;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBModels;

using Microsoft.EntityFrameworkCore;

namespace DataArc.EntityFrameworkCore.Demo.Persistence.Database.Seeders
{
    public interface IOpenAIDbSeeder : IDatabaseSeeder
    {
    }

    internal class OpenAIDbSeeder : IOpenAIDbSeeder
    {
        private readonly IDbContextFactory<OpenAIDbContext> _openAIDbContextFactory;

        public OpenAIDbSeeder(IDbContextFactory<OpenAIDbContext> openAIDbContextFactory)
        {
            _openAIDbContextFactory = openAIDbContextFactory;
        }

        public async Task<bool> SeedDatabaseAsync()
        {
            await using var dbContext = await _openAIDbContextFactory.CreateDbContextAsync();

            await dbContext.AddAsync(new Employer
            {
                Name = "OpenAi",
                Description = "OpenAi Company"
            });

            await dbContext.SaveChangesAsync();

            return true;
        }
    }
}