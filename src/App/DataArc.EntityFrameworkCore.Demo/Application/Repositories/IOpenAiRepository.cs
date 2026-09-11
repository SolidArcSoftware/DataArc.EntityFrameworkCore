using DataArc.EntityFrameworkCore.Demo.Application.Entities;

namespace DataArc.EntityFrameworkCore.Demo.Application.Repositories
{
    public interface IOpenAiRepository
    {
        Task<List<EmployeeEntity>> GetTopRatedOpenAIEmployeesAsync(double rating);
        Task<int> ProcessBulkOpenAIEmployeesAsync(IEnumerable<EmployeeEntity> employees, int batchSize);
    }
}