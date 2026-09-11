using DataArc.EntityFrameworkCore.Demo.Application.Entities;

namespace DataArc.EntityFrameworkCore.Demo.Application.Repositories
{
    public interface IGoogleRepository
    {
        Task<List<EmployeeEntity>> GetTopRatedGoogleEmployeesAsync(double rating);
        Task<int> ProcessBulkGoogleEmployeesAsync(IEnumerable<EmployeeEntity> employees, int batchSize);
    }
}