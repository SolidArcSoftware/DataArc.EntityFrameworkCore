namespace DataArc.EntityFrameworkCore.Demo.Application.Modules.Finance.Features.SalaryAdjustments.Services
{
    public interface ISalaryAdjustmentService
    {
        Task<int> ProcessEmployeeSalaryAdjustmentsAsync(decimal salaryAdjustmentBaseRate, decimal salaryThreshold, int batchSize);
    }
}