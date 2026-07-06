namespace DataArc.EntityFrameworkCore.Demo.Contracts.Application.Modules.Features.SalaryAdjustments.Services
{
    public interface ISalaryAdjustmentService
    {
        Task<int> ProcessEmployeeSalaryAdjustmentsAsync(decimal salaryAdjustmentBaseRate, decimal salaryThreshold, int batchSize);
    }
}