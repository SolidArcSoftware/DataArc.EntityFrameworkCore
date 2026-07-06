using DataArc.EntityFrameworkCore.Demo.Contracts.Application.Dtos;

namespace DataArc.EntityFrameworkCore.Demo.Contracts.Application.Modules.Features.EmployeePerformance.Services
{
    public interface IEmployeePerformanceService
    {
        Task<List<EmployeeDto>> GetConsolidatedTopRatedEmployeesAsync(double rating);
    }
}