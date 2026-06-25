using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using DataArc.EntityFrameworkCore.Demo.Application.Modules.Finance.Registration;
using DataArc.EntityFrameworkCore.Demo.Application.Modules.Finance.Services;
using DataArc.EntityFrameworkCore.Demo.Persistence.Seeding;

internal class Program
{
    static int batchSize = 100_000;

    //Infrastructure dependencies
    static readonly IDatabaseCreator _databaseCreator;
    static readonly IDatabaseSeeder _databaseSeeder;

    //Application dependencies
    static readonly IFinanceService _financeService;

    /// <summary>
    /// Initializes static resources for the Program type by configuring logging, building the service provider, and
    /// resolving required infrastructure and application services.
    /// </summary>
    /// <remarks>Runs once before the type is first used. Registers modules with the dependency-injection
    /// container, configures the console logger, builds the service provider, and retrieves IDatabaseCreator,
    /// IDatabaseSeeder, and IFinanceService instances.</remarks>
    static Program()
    {
        ILoggerFactory factory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });

        // Register modules and build the service provider
        var dataProvider = new ServiceCollection()
            .AddFinanceModule()
            .BuildServiceProvider();

        //Infrastructure dependencies
        _databaseCreator = dataProvider.GetRequiredService<IDatabaseCreator>();
        _databaseSeeder = dataProvider.GetRequiredService<IDatabaseSeeder>();

        //Application dependencies
        _financeService = dataProvider.GetRequiredService<IFinanceService>();
    }

    /// <summary>
    /// Sets up the database by creating it and seeding it with initial data.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    static async Task Setup()
    {
        try
        {
            if (_databaseCreator.EnsureCreated())
            {
                Console.WriteLine("Database created successfully.");
                if (await _databaseSeeder.SeedDatabaseAsync(batchSize))
                {
                    Console.WriteLine("Database seeded successfully.");
                }
                else
                {
                    Console.WriteLine("Failed to seed the database.");
                }
            }
            else
            {
                Console.WriteLine("Failed to create the database.");
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Deletes the database using _databaseCreator.EnsureDeleted and writes the outcome to the console.
    /// </summary>
    /// <remarks>Rethrows any exception encountered during deletion. Logs success or failure to the
    /// console.</remarks>
    /// <returns>A task that represents the asynchronous teardown operation.</returns>
    static async Task Teardown()
    {
        try
        {
            if (_databaseCreator.EnsureDeleted())
            {
                Console.WriteLine("Database deleted successfully.");
            }
            else
            {
                Console.WriteLine("Failed to delete the database.");
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Resets the database by performing teardown followed by setup asynchronously.
    /// </summary>
    /// <remarks>Exceptions from Teardown or Setup propagate to the caller. Intended for use in test or
    /// initialization scenarios.</remarks>
    /// <returns>A task that represents the asynchronous operation.</returns>
    static async Task ResetDatabases()
    {
        await Teardown();
        await Setup();
    }

    /// <summary>
    /// Performs the main execution flow of the application by resetting the database, processing employee finance data,
    /// and retrieving top-rated employees.
    /// </summary>
    /// <returns></returns>
    static async Task ExecuteMain()
    {
        await ResetDatabases();

        decimal salaryAdjustmentBaseRate = 0.05m;
        decimal salaryThreshold = 10000m;
        double ratingThreshold = 4.5;

        var processedCount = await _financeService.ProcessEmployeeFinanceDataAsync(salaryAdjustmentBaseRate, salaryThreshold, batchSize);
        Console.WriteLine($"Processed {processedCount} employees.");

        if (processedCount > 0) {
            var topRatedEmployees = await _financeService.GetTopRatedEmployeesAsync(ratingThreshold);
            foreach (var employee in topRatedEmployees)
            {
                Console.WriteLine($"Top Rated Employee: {employee.Name} {employee.Surname}, Salary: {employee.Salary}");
            }
        }

        Console.WriteLine("Press any key to exit");
        Console.ReadKey();
    }

    private static void Main(string[] args)
    {
        try
        {
            ExecuteMain().GetAwaiter().GetResult();
        }
        catch
        {
            throw;
        }
    }
}