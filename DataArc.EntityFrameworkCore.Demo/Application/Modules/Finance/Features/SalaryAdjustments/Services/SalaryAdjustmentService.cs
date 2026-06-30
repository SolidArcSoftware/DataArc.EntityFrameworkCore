using DataArc.Core;

using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBContexts;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBModels;

namespace DataArc.EntityFrameworkCore.Demo.Application.Modules.Finance.Features.SalaryAdjustments.Services
{
    internal class SalaryAdjustmentService : ISalaryAdjustmentService
    {
        private readonly ICommandFactory _commandFactory;
        private readonly IQueryFactory _queryFactory;

        public SalaryAdjustmentService(ICommandFactory commandFactory, IQueryFactory queryFactory)
        {
            _commandFactory = commandFactory;
            _queryFactory = queryFactory;
        }

        public async Task<int> ProcessEmployeeSalaryAdjustmentsAsync(
            decimal salaryAdjustmentBaseRate, 
            decimal salaryThreshold, 
            int batchSize)
        {
            // Build a query to read employee data from the HR database context based on the salary threshold
            var employeesQuery = await _queryFactory.CreateQueryAsync();
            var employees = await employeesQuery
                .UseDbExecutionContext<IHrDbContext>()
                    .ReadWhereAsync<Employee>(e => e.Salary > salaryThreshold);

            //Adjust salaries
            foreach (var employee in employees)
            {
                employee.Salary += employee.Salary * salaryAdjustmentBaseRate;
            }

            var commandBuilder = await _commandFactory
                .CreateCommandBuilderAsync();

            commandBuilder
                .UseDbExecutionContext<IFinanceDbContext>()
                .AddBulk(employees, batchSize);

            commandBuilder
                .UseDbExecutionContext<IItDbContext>()
                .AddBulk(employees, batchSize);

            commandBuilder
                .UseDbExecutionContext<IOperationsDbContext>()
                .AddBulk(employees, batchSize);

            // Build the command builder pipeline and execute the command in parallel across the different contexts
            var command = await commandBuilder.BuildAsync();
            var commandResult = await command.ExecuteAsync();

            // Check the command result for success and handle any errors or exceptions
            if (!commandResult.Success) {
                // Handle command failure (e.g., log errors, throw exceptions, etc.)
                throw new InvalidOperationException($"Failed to process salary adjustments. {commandResult?.Exception?.Message}");
            }

            // Return the total number of affected records across all contexts
            return commandResult.TotalAffected;
        }
    }
}