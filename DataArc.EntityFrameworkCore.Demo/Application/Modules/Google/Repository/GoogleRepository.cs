using DataArc.EntityFrameworkCore.Demo.Contracts.Application.Dtos;
using DataArc.EntityFrameworkCore.Demo.Contracts.Application.Modules.Google.Repository;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBContexts;
using Microsoft.EntityFrameworkCore;

internal class GoogleRepository : IGoogleRepository
{
    private readonly IDbContextFactory<GoogleDbContext> _dbContextFactory;

    public GoogleRepository(
        IDbContextFactory<GoogleDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<List<EmployeeDto>> GetTopRatedEmployeesAsync(
        double rating)
    {
        await using var dbContext =
            await _dbContextFactory.CreateDbContextAsync();

        return await dbContext!.Employee!
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