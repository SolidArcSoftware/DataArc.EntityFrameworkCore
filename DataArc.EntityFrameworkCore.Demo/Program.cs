using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using DataArc.EntityFrameworkCore.Demo.Application.Modules.Finance.Registration;
using DataArc.EntityFrameworkCore.Demo.Application.Workers;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.Creator;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.Seeder;

const int batchSize = 100_000;

var host = Host
    .CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services
            .AddFinanceModule()
            .AddHostedService<DemoWorkflowWorker>();
    })
    .Build();

using var scope = host.Services.CreateScope();

var databaseCreators = new IDatabaseCreator[]
{
    scope.ServiceProvider.GetRequiredService<IFinanceDbCreator>(),
    scope.ServiceProvider.GetRequiredService<IHrDbCreator>(),
    scope.ServiceProvider.GetRequiredService<IItDbCreator>(),
    scope.ServiceProvider.GetRequiredService<IOperationsDbCreator>()
};

var databaseSeeders = new IDatabaseSeeder[]
{
    scope.ServiceProvider.GetRequiredService<IHrDbSeeder>()
};

await ResetDatabasesAsync(databaseCreators, databaseSeeders, batchSize);

await host.RunAsync();

Console.WriteLine();
Console.WriteLine("Press any key to exit.");
Console.ReadKey();

static async Task ResetDatabasesAsync(
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