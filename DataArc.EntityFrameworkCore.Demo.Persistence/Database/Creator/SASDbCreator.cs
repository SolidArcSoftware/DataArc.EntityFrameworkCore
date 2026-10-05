using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBContexts;

namespace DataArc.EntityFrameworkCore.Demo.Persistence.Database.Creator
{
    public interface ISASDbCreator : IDatabaseCreator
    {
    }

    public class SASDbCreator : ISASDbCreator
    {
        private readonly IDbContextFactory<SolidArcDbContext> _dbContextFactory;

        public SASDbCreator(
            IDbContextFactory<SolidArcDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public bool CanConnect()
        {
            using var dbContext =
                _dbContextFactory.CreateDbContext();

            return dbContext.Database.CanConnect();
        }

        public async Task<bool> CanConnectAsync(
            CancellationToken cancellationToken = default)
        {
            await using var dbContext =
                await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            return await dbContext.Database.CanConnectAsync(cancellationToken);
        }

        public bool EnsureCreated()
        {
            using var dbContext =
                _dbContextFactory.CreateDbContext();

            return dbContext.Database.EnsureCreated();
        }

        public async Task<bool> EnsureCreatedAsync(
            CancellationToken cancellationToken = default)
        {
            await using var dbContext =
                await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            return await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        }

        public bool EnsureDeleted()
        {
            using var dbContext =
                _dbContextFactory.CreateDbContext();

            return dbContext.Database.EnsureDeleted();
        }

        public async Task<bool> EnsureDeletedAsync(
            CancellationToken cancellationToken = default)
        {
            await using var dbContext =
                await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            return await dbContext.Database.EnsureDeletedAsync(cancellationToken);
        }
    }
}