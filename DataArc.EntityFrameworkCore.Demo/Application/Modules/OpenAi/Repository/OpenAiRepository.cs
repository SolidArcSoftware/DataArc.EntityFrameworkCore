using DataArc.Core;
using DataArc.EntityFrameworkCore.Demo.Contracts.Application.Dtos;
using DataArc.EntityFrameworkCore.Demo.Contracts.Application.Modules.OpenAi.Repository;
using DataArc.EntityFrameworkCore.Demo.Persistence.Contracts;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBModels;

namespace DataArc.EntityFrameworkCore.Demo.Application.Modules.OpenAi.Repository
{
    internal class OpenAiRepository : IOpenAiRepository
    {
        private readonly IQueryFactory _queryFactory;
        public OpenAiRepository(IQueryFactory queryFactory)
        {
            _queryFactory = queryFactory;
        }

        public async Task<List<EmployeeDto>> GetTopRatedEmployeesAsync(double rating)
        {
            var topRatedEmployeesQuery = await _queryFactory.CreateQueryAsync();

            var topRatedEmployees = await topRatedEmployeesQuery
                .UseDbExecutionContext<IOpenAIDbContext>()
                .ReadWhereAsync<Employee>(e => e.Rating > rating);

            return topRatedEmployees
                .Select(emp => new EmployeeDto()
                {
                    Id = emp.Id,
                    Name = emp.Name,
                    Surname = emp.Surname,
                    Salary = emp.Salary,
                }).ToList();
        }
    }
}