using Microsoft.Extensions.Hosting;

using DataArc.EntityFrameworkCore.Demo.Application.Modules.Finance.Features.SalaryAdjustments.Services;

namespace DataArc.EntityFrameworkCore.Demo.Application.Workers
{
    internal sealed class DemoWorkflowWorker : BackgroundService
    {
        private const int BatchSize = 100_000;

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
                decimal salaryAdjustmentBaseRate = 0.05m;
                decimal salaryThreshold = 10_000m;
                double ratingThreshold = 4.5;

                var processedCount = await _salaryAdjustmentService
                    .ProcessEmployeeSalaryAdjustmentsAsync(
                        salaryAdjustmentBaseRate,
                        salaryThreshold,
                        BatchSize);

                Console.WriteLine($"Processed salary adjustment records: {processedCount:N0}");

                if (processedCount > 0)
                {
                    var topRatedEmployees = await _employeePerformanceService
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
                Console.WriteLine("Demo workflow completed.");
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