using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

using DataArc.EntityFrameworkCore.Demo.Application.Modules.Finance.Features.SalaryAdjustments.Services;
using DataArc.EntityFrameworkCore.Demo.Application.Modules.Finance.Registration;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.Creator;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.Seeder;

const int batchSize = 100_000;

var serviceProvider = new ServiceCollection()
    .AddFinanceModule()
    .BuildServiceProvider();

var databaseCreators = new IDatabaseCreator[]
{
    serviceProvider.GetRequiredService<IFinanceDbCreator>(),
    serviceProvider.GetRequiredService<IHrDbCreator>(),
    serviceProvider.GetRequiredService<IItDbCreator>(),
    serviceProvider.GetRequiredService<IOperationsDbCreator>()
};

var databaseSeeders = new IDatabaseSeeder[]
{
    serviceProvider.GetRequiredService<IHrDbSeeder>(),
};
    
var salaryAdjustmentService = serviceProvider.GetRequiredService<ISalaryAdjustmentService>();
var employeePerformanceService = serviceProvider.GetRequiredService<IEmployeePerformanceService>();

await ResetDatabasesAsync(databaseCreators, databaseSeeders, batchSize);

decimal salaryAdjustmentBaseRate = 0.05m;
decimal salaryThreshold = 10_000m;
double ratingThreshold = 4.5;

var processedCount = await salaryAdjustmentService
    .ProcessEmployeeSalaryAdjustmentsAsync(
        salaryAdjustmentBaseRate,
        salaryThreshold,
        batchSize);

Console.WriteLine($"Processed salary adjustment records: {processedCount:N0}");

if (processedCount > 0)
{
    var topRatedEmployees = await employeePerformanceService
        .GetTopRatedEmployeesAsync(ratingThreshold);

    var topRatedEmployee = topRatedEmployees
        .OrderByDescending(employee => employee.Salary)
        .FirstOrDefault();

    Console.WriteLine();

    Console.WriteLine(topRatedEmployee is null
        ? "No top rated employees found."
        : $"Top Rated Employee: {topRatedEmployee.Name} {topRatedEmployee.Surname}, " +
          $"Salary: {topRatedEmployee.Salary:N2}, " +
          $"Number of top rated employees: {topRatedEmployees.Count:N0}");
}

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
        if(!await databaseSeeder.SeedDatabaseAsync(batchSize))
            throw new InvalidOperationException("Failed to seed the demo databases.");
    }

    Console.WriteLine("Databases seeded successfully.");
}