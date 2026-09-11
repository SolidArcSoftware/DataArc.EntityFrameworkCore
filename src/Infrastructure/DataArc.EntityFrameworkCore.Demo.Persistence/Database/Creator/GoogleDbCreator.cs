using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBContexts;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace DataArc.EntityFrameworkCore.Demo.Persistence.Database.Creator
{
    public interface IGoogleDbCreator : IDatabaseCreator
    {
    }

    internal class GoogleDbCreator : IGoogleDbCreator
    {
        private readonly IDbContextFactory<GoogleDbContext> _googleDbContextFactory;

        public GoogleDbCreator(IDbContextFactory<GoogleDbContext> googleDbContextFactory)
        {
            _googleDbContextFactory = googleDbContextFactory;
        }

        public bool CanConnect()
        {
            using var dbContext = _googleDbContextFactory.CreateDbContext();
            return dbContext.Database.CanConnect();
        }

        public async Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
        {
            await using var dbContext = await _googleDbContextFactory.CreateDbContextAsync(cancellationToken);
            return await dbContext.Database.CanConnectAsync(cancellationToken);
        }

        public bool EnsureCreated()
        {
            using var dbContext = _googleDbContextFactory.CreateDbContext();
            return dbContext.Database.EnsureCreated();
        }

        public async Task<bool> EnsureCreatedAsync(CancellationToken cancellationToken = default)
        {
            await using var dbContext = await _googleDbContextFactory.CreateDbContextAsync(cancellationToken);
            return await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        }

        public bool EnsureDeleted()
        {
            using var dbContext = _googleDbContextFactory.CreateDbContext();
            return dbContext.Database.EnsureDeleted();
        }

        public async Task<bool> EnsureDeletedAsync(CancellationToken cancellationToken = default)
        {
            await using var dbContext = await _googleDbContextFactory.CreateDbContextAsync(cancellationToken);
            return await dbContext.Database.EnsureDeletedAsync(cancellationToken);
        }
    }
}