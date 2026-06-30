using DataArc.Core;
using Microsoft.EntityFrameworkCore.Storage;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBContexts;

namespace DataArc.EntityFrameworkCore.Demo.Persistence.Database.Creator
{
    public interface IItDbCreator : IDatabaseCreator
    {

    }

    public class ItDbCreator : IItDbCreator
    {
        private readonly IDatabaseFactory _databaseFactory;

        public ItDbCreator(IDatabaseFactory databaseFactory)
        {
            _databaseFactory = databaseFactory;
        }

        public bool CanConnect()
        {
            throw new NotImplementedException();
        }

        public Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public bool EnsureCreated()
        {
            var dbBuilder = _databaseFactory.CreateDatabaseBuilder();

            var db = dbBuilder
                .IncludeDbContext<ItDbContext>()
                .Build(generateScripts: true, applyChanges: true);

            db.ExecuteCreate();
            return true;
        }

        public Task<bool> EnsureCreatedAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public bool EnsureDeleted()
        {
            var dbBuilder = _databaseFactory.CreateDatabaseBuilder();

            var db = dbBuilder
                .IncludeDbContext<ItDbContext>()
                .Build(generateScripts: true, applyChanges: true);

            db.ExecuteDrop();
            return true;
        }

        public Task<bool> EnsureDeletedAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}