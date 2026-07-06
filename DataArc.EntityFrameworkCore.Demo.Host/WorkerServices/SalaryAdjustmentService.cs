using DataArc.Core;
using DataArc.EntityFrameworkCore.Demo.Contracts.Application.Modules.Features.SalaryAdjustments.Services;
using DataArc.EntityFrameworkCore.Demo.Persistence.Contracts;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBModels;

namespace DataArc.EntityFrameworkCore.Demo.Host.BackgroundServices
{
    internal class SalaryAdjustmentService : ISalaryAdjustmentService
    {
        private readonly ICommandFactory _commandFactory;
        private readonly IQueryFactory _queryFactory;

        public SalaryAdjustmentService(
            ICommandFactory commandFactory, IQueryFactory queryFactory)
        {
            _commandFactory = commandFactory;
            _queryFactory = queryFactory;
        }

        public async Task<int> ProcessEmployeeSalaryAdjustmentsAsync(
            decimal salaryAdjustmentBaseRate, 
            decimal salaryThreshold, 
            int batchSize)
        {
            var employeesQuery = await _queryFactory.CreateQueryAsync();
            var employees = await employeesQuery
                .UseDbExecutionContext<ISolidArcDbContext>()
                    .ReadWhereAsync<Employee>(e => e.Salary > salaryThreshold);

            foreach (var employee in employees)
            {
                employee.Salary += employee.Salary * salaryAdjustmentBaseRate;
            }

            var commandBuilder = await _commandFactory
                .CreateCommandBuilderAsync();

            commandBuilder
                .UseDbExecutionContext<IGoogleDbContext>()
                .AddBulk(employees, batchSize);

            commandBuilder
                .UseDbExecutionContext<IMicrosoftDbContext>()
                .AddBulk(employees, batchSize);

            commandBuilder
                .UseDbExecutionContext<IOpenAIDbContext>()
                .AddBulk(employees, batchSize);

            var command = await commandBuilder.BuildAsync();
            var commandResult = await command.ExecuteAsync();

            if (!commandResult.Success) {
                throw new InvalidOperationException($"Failed to process salary adjustments. {commandResult?.Exception?.Message}");
            }

            return commandResult.TotalAffected;
        }
    }
}