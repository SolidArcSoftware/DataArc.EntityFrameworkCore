using DataArc.Core;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBContexts;
using Microsoft.EntityFrameworkCore.Storage;

namespace DataArc.EntityFrameworkCore.Demo.Persistence.Seeding
{
    public class DatabaseCreator : IDatabaseCreator //EFCore provides the IDatabaseCreator interface to manage database creation and deletion. By implementing this interface, we can control how our databases are created, ensuring that they are set up according to our specific requirements.
    {
        private readonly IDatabaseBuilder _databaseBuilder;
        public DatabaseCreator(IDatabaseBuilder databaseBuilder)
        {
            _databaseBuilder = databaseBuilder;
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
                var db = _databaseBuilder
                        .UseContext<FinanceDbContext>()
                        .UseContext<HrDbContext>()
                        .UseContext<ItDbContext>()
                        .UseContext<OperationsDbContext>()
                        .Build(applyChanges: true, generateScripts: true); //Generates SQL scripts for the database changes and applies them to the databases. This is useful for debugging and auditing purposes.

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
                var db = _databaseBuilder
                      .UseContext<FinanceDbContext>()
                      .UseContext<HrDbContext>()
                      .UseContext<ItDbContext>()
                      .UseContext<OperationsDbContext>()
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