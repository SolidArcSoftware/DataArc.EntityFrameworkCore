using DataArc.EntityFrameworkCore.Demo.Application.Features.EmployeePerformance.Services;
using DataArc.EntityFrameworkCore.Demo.Application.Features.SalaryAdjustments.Services;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;

namespace DataArc.EntityFrameworkCore.Demo.Host.Workers
{
    internal sealed class DemoWorkflowWorker : BackgroundService
    {
        private const int BatchSize = 250_000;

        private readonly ISalaryAdjustmentService _salaryAdjustmentService;
        private readonly IEmployeePerformanceService _employeePerformanceService;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;

        public DemoWorkflowWorker(
            ISalaryAdjustmentService salaryAdjustmentService,
            IEmployeePerformanceService employeePerformanceService,
            IHostApplicationLifetime hostApplicationLifetime)
        {
            _salaryAdjustmentService = salaryAdjustmentService;
            _employeePerformanceService = employeePerformanceService;
            _hostApplicationLifetime = hostApplicationLifetime;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();

                decimal salaryAdjustmentBaseRate = 0.05m;
                double qualifyingEmployeesRating = 0;

                var processedCount = await _salaryAdjustmentService
                    .ProcessEmployeeSalaryAdjustmentsAsync(
                        salaryAdjustmentBaseRate,
                        qualifyingEmployeesRating,
                        BatchSize);

                long salaryAdjustmentTimeElapsed = stopwatch.ElapsedMilliseconds;

                if (processedCount > 0)
                {
                    var topRatedEmployees = await _employeePerformanceService
                        .GetConsolidatedTopRatedEmployeesAsync(ratingThreshold);

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
                Console.WriteLine($"Processed {processedCount:N0} salary adjustment records in {salaryAdjustmentTimeElapsed}ms");
                Console.WriteLine($"Demo workflow completed in {stopwatch.ElapsedMilliseconds} ms.");
            }
            catch (Exception exception)
            {
                Console.WriteLine();
                Console.WriteLine($"Demo workflow failed: {exception.Message}");
                throw;
            }
            finally
            {
                _hostApplicationLifetime.StopApplication();
            }
        }
    }
}