using DataArc.EntityFrameworkCore.Demo.Application.Entities;

namespace DataArc.EntityFrameworkCore.Demo.Application.Repositories
{
    public interface ISolidArcRepository
    {
        Task<List<EmployeeEntity>> GetTopRatedSolidArcEmployeesAsync(double rating);
    }
}