using DataArc.EntityFrameworkCore.Demo.Contracts.Application.Dtos;

namespace DataArc.EntityFrameworkCore.Demo.Contracts.Application.Modules.OpenAi.Repository
{
    public interface IOpenAiRepository
    {
        Task<List<EmployeeDto>> GetTopRatedEmployeesAsync(double rating);
    }
}