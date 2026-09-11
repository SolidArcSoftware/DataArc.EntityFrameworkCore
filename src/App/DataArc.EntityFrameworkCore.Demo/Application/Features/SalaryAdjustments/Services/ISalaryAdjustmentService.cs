namespace DataArc.EntityFrameworkCore.Demo.Application.Features.SalaryAdjustments.Services
{
    public interface ISalaryAdjustmentService
    {
        Task<int> ProcessEmployeeSalaryAdjustmentsAsync(decimal salaryAdjustmentBaseRate, double rating, int batchSize);
    }
}