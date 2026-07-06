using DataArc.EntityFrameworkCore.Demo.Contracts.Application.Dtos;

namespace DataArc.EntityFrameworkCore.Demo.Contracts.Application.Modules.Google.Repository
{
    public interface IGoogleRepository
    {
        Task<List<EmployeeDto>> GetTopRatedEmployeesAsync(double rating);
    }
}