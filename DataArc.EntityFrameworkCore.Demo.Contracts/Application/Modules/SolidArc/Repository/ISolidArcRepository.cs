using DataArc.EntityFrameworkCore.Demo.Contracts.Application.Dtos;

namespace DataArc.EntityFrameworkCore.Demo.Contracts.Application.Modules.SolidArc.Repository
{
    public interface ISolidArcRepository
    {
        Task<List<EmployeeDto>> GetTopRatedEmployeesAsync(double rating);
    }
}