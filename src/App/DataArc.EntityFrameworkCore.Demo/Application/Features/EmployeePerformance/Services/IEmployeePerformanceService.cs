using DataArc.EntityFrameworkCore.Demo.Application.Dtos;

namespace DataArc.EntityFrameworkCore.Demo.Application.Features.EmployeePerformance.Services
{
    public interface IEmployeePerformanceService
    {
        Task<List<EmployeeDto>> GetConsolidatedTopRatedEmployeesAsync(double rating);
    }
}