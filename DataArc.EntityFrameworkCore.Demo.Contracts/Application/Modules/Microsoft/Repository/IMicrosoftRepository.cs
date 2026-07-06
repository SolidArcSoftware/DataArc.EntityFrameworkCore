using DataArc.EntityFrameworkCore.Demo.Contracts.Application.Dtos;

namespace DataArc.EntityFrameworkCore.Demo.Contracts.Application.Modules.Microsoft.Repository
{
    public interface IMicrosoftRepository
    {
        Task<List<EmployeeDto>> GetTopRatedEmployeesAsync(double rating);
    }
}