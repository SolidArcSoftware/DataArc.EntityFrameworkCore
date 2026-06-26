using DataArc.Core;
using DataArc.EntityFrameworkCore.Demo.Application.Modules.Finance.Features.SalaryAdjustments.Dtos;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBContexts;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBModels;

namespace DataArc.EntityFrameworkCore.Demo.Application.Modules.Finance.Services
{
    public class FinanceService : IFinanceService
    {
        private readonly ICommandFactory _commandFactory;
        private readonly IQueryFactory _queryFactory;

        public FinanceService(IQueryFactory queryFactory, ICommandFactory commandFactory)
        {
            _queryFactory = queryFactory;
            _commandFactory = commandFactory;
        }

        public async Task<List<EmployeeDto>> GetTopRatedEmployeesAsync(double rating)
        {
            var topRatedEmployeesQuery = await _queryFactory.CreateQueryAsync();

            var topRatedEmployees = await topRatedEmployeesQuery
                .UseDbExecutionContext<IHrDbContext, Employee>(e => e.Rating > rating)
                    .Join<IFinanceDbContext, Employee>
                        (bag => bag.Get<Employee>()!.Id, f => f.Id)
                    .Join<IItDbContext, Employee>
                        (bag => bag.Get<Employee>()!.Id, i => i.Id)
                    .Join<IOperationsDbContext, Employee>(bag => bag.Get<Employee>()!.Id, o => o.Id)
                .Select(bag => new EmployeeDto()
                {
                    Id = bag.Get<Employee>()!.Id,
                    Name = bag.Get<Employee>()!.Name!,
                    Surname = bag.Get<Employee>()!.Surname!,
                    Salary = bag.Get<Employee>()!.Salary!

                }).ToListAsync();

            return topRatedEmployees;
        }

        public async Task<int> ProcessEmployeeFinanceDataAsync(
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

            var commandBuilder = await _commandFactory.CreateCommandBuilderAsync();

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
            var commandResult = await command.ExecuteParallelAsync();

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