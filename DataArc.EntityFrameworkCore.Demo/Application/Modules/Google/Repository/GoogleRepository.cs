using DataArc.Core;
using DataArc.EntityFrameworkCore.Demo.Contracts.Application.Dtos;
using DataArc.EntityFrameworkCore.Demo.Contracts.Application.Modules.Finance.Repository;
using DataArc.EntityFrameworkCore.Demo.Persistence.Contracts;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBModels;

namespace DataArc.EntityFrameworkCore.Demo.Application.Modules.Google.Repository
{
    internal class GoogleRepository : IGoogleRepository
    {
        private readonly IQueryFactory _queryFactory;
        public GoogleRepository(IQueryFactory queryFactory)
        {
            _queryFactory = queryFactory;
        }

        public async Task<List<EmployeeDto>> GetTopRatedEmployeesAsync(double rating)
        {
            var topRatedEmployeesQuery = await _queryFactory.CreateQueryAsync();

            var topRatedEmployees = await topRatedEmployeesQuery
                .UseDbExecutionContext<IGoogleDbContext>()
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