using Microsoft.Extensions.DependencyInjection;

using DataArc.EntityFrameworkCore.Demo.Persistence.Database.Creator;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.Seeder;
using Microsoft.EntityFrameworkCore.Storage;

namespace DataArc.EntityFrameworkCore.Demo.Persistence
{
    public static class DemoDatabaseInitializer
    {
        private const int BatchSize = 100_000;

        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var databaseCreators = new IDatabaseCreator[]
            {
                serviceProvider.GetRequiredService<IFinanceDbCreator>(),
                serviceProvider.GetRequiredService<IHrDbCreator>(),
                serviceProvider.GetRequiredService<IItDbCreator>(),
                serviceProvider.GetRequiredService<IOperationsDbCreator>()
            };

            var databaseSeeders = new IDatabaseSeeder[]
            {
                serviceProvider.GetRequiredService<IHrDbSeeder>()
            };

            await ResetDatabasesAsync(
                databaseCreators,
                databaseSeeders,
                BatchSize);
        }

        private static async Task ResetDatabasesAsync(
            IReadOnlyCollection<IDatabaseCreator> databaseCreators,
            IReadOnlyCollection<IDatabaseSeeder> databaseSeeders,
            int batchSize)
        {
            foreach (var databaseCreator in databaseCreators)
            {
                if (!databaseCreator.EnsureDeleted())
                    throw new InvalidOperationException($"Failed to delete database using {databaseCreator.GetType().Name}.");
            }

            Console.WriteLine("Databases deleted successfully.");

            foreach (var databaseCreator in databaseCreators)
            {
                if (!databaseCreator.EnsureCreated())
                    throw new InvalidOperationException($"Failed to create database using {databaseCreator.GetType().Name}.");
            }

            Console.WriteLine("Databases created successfully.");

            foreach (var databaseSeeder in databaseSeeders)
            {
                if (!await databaseSeeder.SeedDatabaseAsync(batchSize))
                    throw new InvalidOperationException("Failed to seed the demo databases.");
            }

            Console.WriteLine("Databases seeded successfully.");
        }
    }
}