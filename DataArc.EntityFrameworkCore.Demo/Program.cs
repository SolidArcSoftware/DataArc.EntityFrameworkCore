using DataArc.EntityFrameworkCore.Demo.Application.Modules.Finance.Features.SalaryAdjustments.Services;
using DataArc.EntityFrameworkCore.Demo.Application.Modules.Finance.Registration;
using DataArc.EntityFrameworkCore.Demo.Persistence.Seeding;

using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

const int batchSize = 100_000;

var serviceProvider = new ServiceCollection()
    .AddFinanceModule()
    .BuildServiceProvider();

var databaseCreator = serviceProvider.GetRequiredService<IDatabaseCreator>();
var databaseSeeder = serviceProvider.GetRequiredService<IDatabaseSeeder>();

var salaryAdjustmentService = serviceProvider.GetRequiredService<ISalaryAdjustmentService>();
var employeePerformanceService = serviceProvider.GetRequiredService<IEmployeePerformanceService>();

await ResetDatabasesAsync(databaseCreator, databaseSeeder, batchSize);

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
    IDatabaseCreator databaseCreator,
    IDatabaseSeeder databaseSeeder,
    int batchSize)
{
    if (!databaseCreator.EnsureDeleted())
        throw new InvalidOperationException("Failed to delete the demo databases.");

    Console.WriteLine("Databases deleted successfully.");

    if (!databaseCreator.EnsureCreated())
        throw new InvalidOperationException("Failed to create the demo databases.");

    Console.WriteLine("Databases created successfully.");

    if (!await databaseSeeder.SeedDatabaseAsync(batchSize))
        throw new InvalidOperationException("Failed to seed the demo databases.");

    Console.WriteLine("Databases seeded successfully.");
}