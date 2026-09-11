using DataArc.EntityFrameworkCore.Demo.Application.Entities;
using DataArc.EntityFrameworkCore.Demo.Application.Repositories;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace DataArc.EntityFrameworkCore.Demo.Host.Repositories
{
    internal class SolidArcRepository : ISolidArcRepository
    {
        private readonly IDbContextFactory<SolidArcDbContext> _solidArcDbContextFactory;

        public SolidArcRepository(IDbContextFactory<SolidArcDbContext> solidArcDbContextFactory)
        {
            _solidArcDbContextFactory = solidArcDbContextFactory;
        }

        public async Task<List<EmployeeEntity>> GetTopRatedSolidArcEmployeesAsync(double rating)
        {
            await using var dbContext =
                await _solidArcDbContextFactory.CreateDbContextAsync();

            return await dbContext.Employee!
                .AsNoTracking()
                .Where(employee => employee.Rating > rating)
                .Select(employee => new EmployeeEntity
                {
                    Id = employee.Id,
                    Name = employee.Name,
                    Surname = employee.Surname,
                    Salary = employee.Salary,
                    EmployerId = employee.EmployerId,
                    Order = employee.Order,
                    IsArchived = employee.IsArchived,
                    CreatedUtc = employee.CreatedUtc,
                    LastUpdatedUtc = employee.LastUpdatedUtc,
                    Notes = employee.Notes,
                    Status = employee.Status,
                    Rating = employee.Rating
                })
                .ToListAsync();
        }
    }
}