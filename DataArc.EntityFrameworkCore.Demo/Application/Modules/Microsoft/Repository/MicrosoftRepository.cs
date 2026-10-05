using DataArc.EntityFrameworkCore.Demo.Contracts.Application.Dtos;
using DataArc.EntityFrameworkCore.Demo.Contracts.Application.Modules.Microsoft.Repository;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBContexts;

using Microsoft.EntityFrameworkCore;

namespace DataArc.EntityFrameworkCore.Demo.Application.Modules.Microsoft.Repository
{
    internal class MicrosoftRepository : IMicrosoftRepository
    {
        private readonly IDbContextFactory<MicrosoftDbContext> _dbContextFactory;

        public MicrosoftRepository(
            IDbContextFactory<MicrosoftDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<List<EmployeeDto>> GetTopRatedEmployeesAsync(double rating)
        {
            await using var dbContext =
                await _dbContextFactory.CreateDbContextAsync();

            return await dbContext.Employee!
                .AsNoTracking()
                .Where(employee => employee.Rating > rating)
                .Select(employee => new EmployeeDto
                {
                    Id = employee.Id,
                    Name = employee.Name,
                    Surname = employee.Surname,
                    Salary = employee.Salary
                })
                .ToListAsync();
        }
    }
}