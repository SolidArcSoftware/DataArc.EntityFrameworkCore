using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.Storage;

using DataArc.EntityFrameworkCore.Demo.Persistence.Database.Creator;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.Seeders;

namespace DataArc.EntityFrameworkCore.Demo.Persistence
{
    public static class DemoDatabaseInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var databaseCreators = new IDatabaseCreator[]
            {
                serviceProvider.GetRequiredService<IGoogleDbCreator>(),
                serviceProvider.GetRequiredService<IMicrosoftDbCreator>(),
                serviceProvider.GetRequiredService<IOpenAiDbCreator>(),
                serviceProvider.GetRequiredService<ISASDbCreator>()
            };

            var databaseSeeders = new IDatabaseSeeder[]
            {
                serviceProvider.GetRequiredService<IGoogleDBSeeder>(),
                serviceProvider.GetRequiredService<IMicrosoftDbSeeder>(),
                serviceProvider.GetRequiredService<IOpenAIDbSeeder>(),
                serviceProvider.GetRequiredService<ISolidArcDbSeeder>()
            };

            await ResetDatabasesAsync(
                databaseCreators,
                databaseSeeders);
        }

        private static async Task ResetDatabasesAsync(
            IReadOnlyCollection<IDatabaseCreator> databaseCreators,
            IReadOnlyCollection<IDatabaseSeeder> databaseSeeders)
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
                if (!await databaseSeeder.SeedDatabaseAsync())
                    throw new InvalidOperationException("Failed to seed the demo databases.");
            }

            Console.WriteLine("Databases seeded successfully.");
        }
    }
}