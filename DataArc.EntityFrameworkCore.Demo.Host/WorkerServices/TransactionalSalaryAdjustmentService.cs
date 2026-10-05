using Microsoft.EntityFrameworkCore;

using DataArc.EntityFrameworkCore.Demo.Contracts.Application.Modules.Features.SalaryAdjustments.Services;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBContexts;

using DataArc.EntityFrameworkCore.Parallel.Transactional;

namespace DataArc.EntityFrameworkCore.Demo.Host.WorkerServices
{
    internal sealed class TransactionalSalaryAdjustmentService
       : ITransactionalSalaryAdjustmentService
    {
        private readonly IDbContextFactory<SolidArcDbContext> _solidArcDbContextFactory;
        private readonly IDbContextFactory<GoogleDbContext> _googleDbContextFactory;
        private readonly IDbContextFactory<MicrosoftDbContext> _microsoftDbContextFactory;
        private readonly IDbContextFactory<OpenAIDbContext> _openAiDbContextFactory;

        public TransactionalSalaryAdjustmentService(
            IDbContextFactory<SolidArcDbContext> solidArcDbContextFactory,
            IDbContextFactory<GoogleDbContext> googleDbContextFactory,
            IDbContextFactory<MicrosoftDbContext> microsoftDbContextFactory,
            IDbContextFactory<OpenAIDbContext> openAiDbContextFactory)
        {
            _solidArcDbContextFactory = solidArcDbContextFactory;
            _googleDbContextFactory = googleDbContextFactory;
            _microsoftDbContextFactory = microsoftDbContextFactory;
            _openAiDbContextFactory = openAiDbContextFactory;
        }

        public async Task<int> ProcessEmployeeSalaryAdjustmentsAsync(
            decimal salaryAdjustmentBaseRate,
            decimal salaryThreshold,
            int batchSize)
        {
            await using var solidArcDbContext =
                await _solidArcDbContextFactory.CreateDbContextAsync();

            var employers = await solidArcDbContext.Employer!
                .AsNoTracking()
                .ToListAsync();

            var employees = await solidArcDbContext.Employee!
                .AsNoTracking()
                .Where(employee => employee.Salary > salaryThreshold)
                .ToListAsync();

            foreach (var employee in employees)
            {
                employee.Salary +=
                    employee.Salary * salaryAdjustmentBaseRate;
            }

            await using var googleDbContext =
                await _googleDbContextFactory.CreateDbContextAsync();

            await using var microsoftDbContext =
                await _microsoftDbContextFactory.CreateDbContextAsync();

            await using var openAiDbContext =
                await _openAiDbContextFactory.CreateDbContextAsync();

            var results = await Task.WhenAll(
                googleDbContext
                    .AsParallelTransaction()
                    .AddBulk(employers, batchSize)
                    .AddBulk(employees, batchSize)
                    .CommitTransactionParallelAsync(),

                microsoftDbContext
                    .AsParallelTransaction()
                    .AddBulk(employers, batchSize)
                    .AddBulk(employees, batchSize)
                    .CommitTransactionParallelAsync(),

                openAiDbContext
                    .AsParallelTransaction()
                    .AddBulk(employers, batchSize)
                    .AddBulk(employees, batchSize)
                    .CommitTransactionParallelAsync());

            return results.Sum();
        }
    }
}
