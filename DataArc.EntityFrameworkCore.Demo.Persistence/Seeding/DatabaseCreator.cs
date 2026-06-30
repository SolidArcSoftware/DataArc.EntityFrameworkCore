using DataArc.Core;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBContexts;
using Microsoft.EntityFrameworkCore.Storage;

namespace DataArc.EntityFrameworkCore.Demo.Persistence.Seeding
{
    public class DatabaseCreator : IDatabaseCreator //EFCore provides the IDatabaseCreator interface to manage database creation and deletion. By implementing this interface, we can control how our databases are created, ensuring that they are set up according to our specific requirements.
    {
        private readonly IDatabaseFactory _databaseFactory;
        public DatabaseCreator(IDatabaseFactory databaseFactory)
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
            try
            {
                //Synchronize the database schemas with the current model definitions. This ensures that any changes made to the model classes are reflected in the database structure.
                var databaseBuilder = _databaseFactory.CreateDatabaseBuilder();
                var db = databaseBuilder
                        .IncludeDbContext<FinanceDbContext>()
                        .IncludeDbContext<HrDbContext>()
                        .IncludeDbContext<ItDbContext>()
                        .IncludeDbContext<OperationsDbContext>()
                        .Build(applyChanges:true);

                // Drop and create databases
                db.ExecuteCreate();

                return true;
            }
            catch
            {
                throw;
            }
        }

        public Task<bool> EnsureCreatedAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public bool EnsureDeleted()
        {
            try
            {
                // Drop the databases if they exist. This is useful for resetting the state of the databases during development or testing.
                var databaseBuilder = _databaseFactory.CreateDatabaseBuilder();
                var db = databaseBuilder
                      .IncludeDbContext<FinanceDbContext>()
                      .IncludeDbContext<HrDbContext>()
                      .IncludeDbContext<ItDbContext>()
                      .IncludeDbContext<OperationsDbContext>()
                      .Build(applyChanges: true, generateScripts: true);

                db.ExecuteDrop();
                return true;
            }
            catch 
            { 
                throw;
            }
        }

        public Task<bool> EnsureDeletedAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}