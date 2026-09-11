using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBContexts;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace DataArc.EntityFrameworkCore.Demo.Persistence.Database.Creator
{
    public interface IOpenAiDbCreator : IDatabaseCreator
    {
    }

    internal class OpenAiDbCreator : IOpenAiDbCreator
    {
        private readonly IDbContextFactory<OpenAIDbContext> _openAIDbContextFactory;

        public OpenAiDbCreator(IDbContextFactory<OpenAIDbContext> openAIDbContextFactory)
        {
            _openAIDbContextFactory = openAIDbContextFactory;
        }

        public bool CanConnect()
        {
            using var dbContext = _openAIDbContextFactory.CreateDbContext();
            return dbContext.Database.CanConnect();
        }

        public async Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
        {
            await using var dbContext = await _openAIDbContextFactory.CreateDbContextAsync(cancellationToken);
            return await dbContext.Database.CanConnectAsync(cancellationToken);
        }

        public bool EnsureCreated()
        {
            using var dbContext = _openAIDbContextFactory.CreateDbContext();
            return dbContext.Database.EnsureCreated();
        }

        public async Task<bool> EnsureCreatedAsync(CancellationToken cancellationToken = default)
        {
            await using var dbContext = await _openAIDbContextFactory.CreateDbContextAsync(cancellationToken);
            return await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        }

        public bool EnsureDeleted()
        {
            using var dbContext = _openAIDbContextFactory.CreateDbContext();
            return dbContext.Database.EnsureDeleted();
        }

        public async Task<bool> EnsureDeletedAsync(CancellationToken cancellationToken = default)
        {
            await using var dbContext = await _openAIDbContextFactory.CreateDbContextAsync(cancellationToken);
            return await dbContext.Database.EnsureDeletedAsync(cancellationToken);
        }
    }
}