using DataArc.Core;
using DataArc.EntityFrameworkCore.Demo.Contracts.Application.Dtos;
using DataArc.EntityFrameworkCore.Demo.Contracts.Application.Modules.Operations.Repository;
using DataArc.EntityFrameworkCore.Demo.Persistence.Contracts;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBModels;

namespace DataArc.EntityFrameworkCore.Demo.Application.Modules.Microsoft.Repository
{
    internal class MicrosoftRepository : IMicrosoftRepository
    {
        private readonly IQueryFactory _queryFactory;
        public MicrosoftRepository(IQueryFactory queryFactory)
        {
            _queryFactory = queryFactory;
        }

        public async Task<List<EmployeeDto>> GetTopRatedEmployeesAsync(double rating)
        {
            var topRatedEmployeesQuery = await _queryFactory.CreateQueryAsync();

            var topRatedEmployees = await topRatedEmployeesQuery
                .UseDbExecutionContext<IMicrosoftDbContext>()
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