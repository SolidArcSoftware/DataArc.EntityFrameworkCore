using DataArc.EntityFrameworkCore.Demo.Application.Modules.Finance.Features.SalaryAdjustments.Dtos;

namespace DataArc.EntityFrameworkCore.Demo.Application.Modules.Finance.Services
{
    public interface IFinanceService
    {
        Task<int> ProcessEmployeeFinanceDataAsync(decimal salaryAdjustmentBaseRate, decimal salaryThreshold, int batchSize);
        Task<List<EmployeeDto>> GetTopRatedEmployeesAsync(double rating);
    }
}