using DataArc.Core;

using DataArc.EntityFrameworkCore.Demo.Contracts.Application.Modules.Features.SalaryAdjustments.Services;
using DataArc.EntityFrameworkCore.Demo.Host;
using DataArc.EntityFrameworkCore.Demo.Persistence;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBContexts;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBModels;
using DataArc.EntityFrameworkCore.Parallel.Transactional;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataArc.EntityFrameworkCore.SqlServer.IntegrationTests;

public sealed class SqlServerParallelTransactionIntegrationTests
{
    private const int RecordCount = 100_000;
    private const int BatchSize = 100_000;

    private ServiceProvider _serviceProvider = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        var configuration = new ConfigurationManager();

        configuration
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables();

        var licenseKey = configuration["DataArc:LicenseKey"];

        var services = new ServiceCollection();

        services
            .AddDataArcCore(options =>
            {
                if (string.IsNullOrWhiteSpace(licenseKey))
                    options.UseServerKey();
                else
                    options.UseKey(licenseKey);
            })
            .ConfigureDataArc();

        services.AddBackgroundServices(configuration);

        _serviceProvider = services.BuildServiceProvider();

        await DemoDatabaseInitializer.InitializeAsync(
            _serviceProvider);
    }

    [Test]
    public async Task SalaryAdjustmentWorkflow_ShouldExecuteUsingParallelTransactions()
    {
        var service =
            _serviceProvider.GetRequiredService<
                ITransactionalSalaryAdjustmentService>();

        var affected =
            await service.ProcessEmployeeSalaryAdjustmentsAsync(
                salaryAdjustmentBaseRate: 0.05m,
                salaryThreshold: 0m,
                batchSize: 100_000);

        Assert.That(affected, Is.EqualTo(600_000));
    }

    [Test]
    public async Task ParallelTransaction_ShouldRollbackAllOperations_WhenExecutionFails()
    {
        var googleDbContextFactory =
            _serviceProvider.GetRequiredService<
                IDbContextFactory<GoogleDbContext>>();

        await using var googleDbContext =
            await googleDbContextFactory.CreateDbContextAsync();

        var employer = new Employer
        {
            Name = "Rollback Employer",
            Description = "This record must not survive the transaction"
        };

        var invalidEmployee = new Employee
        {
            Name = "Invalid",
            Surname = "Employee",
            Salary = 100_000m,
            EmployerId = int.MaxValue
        };

        var exception = Assert.CatchAsync<Exception>(async () =>
        {
            await googleDbContext
                .AsParallelTransaction()
                .Add(employer)
                .Add(invalidEmployee)
                .CommitTransactionParallelAsync();
        });

        Assert.That(
            exception,
            Is.Not.Null);

        Assert.That(
            exception!.InnerException,
            Is.TypeOf<DbUpdateException>());

        await using var verificationContext =
            await googleDbContextFactory.CreateDbContextAsync();

        var employerExists =
            await verificationContext.Employer!
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Name == "Rollback Employer");

        Assert.That(
            employerExists,
            Is.False);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _serviceProvider.Dispose();
    }
}