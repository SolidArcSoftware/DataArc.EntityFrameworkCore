using DataArc.EntityFrameworkCore.Demo.Application.Entities;

namespace DataArc.EntityFrameworkCore.Demo.Application.Repositories
{
    public interface IMicrosoftRepository
    {
        Task<List<EmployeeEntity>> GetTopRatedMicrosoftEmployeesAsync(double rating);
        Task<int> ProcessBulkMicrosoftEmployeesAsync(IEnumerable<EmployeeEntity> employees, int batchSize);
    }
}