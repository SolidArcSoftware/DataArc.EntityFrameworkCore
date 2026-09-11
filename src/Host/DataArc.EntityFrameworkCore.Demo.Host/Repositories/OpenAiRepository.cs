using DataArc.EntityFrameworkCore.Demo.Application.Entities;
using DataArc.EntityFrameworkCore.Demo.Application.Repositories;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBContexts;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBModels;
using Microsoft.EntityFrameworkCore;

namespace DataArc.EntityFrameworkCore.Demo.Host.Repositories
{
    internal class OpenAiRepository : IOpenAiRepository
    {
        private readonly IDbContextFactory<OpenAIDbContext> _openAIDbContextFactory;

        public OpenAiRepository(IDbContextFactory<OpenAIDbContext> openAIDbContextFactory)
        {
            _openAIDbContextFactory = openAIDbContextFactory;
        }

        public async Task<int> ProcessBulkOpenAIEmployeesAsync(IEnumerable<EmployeeEntity> employees, int batchSize)
        {
            await using var dbContext = await _openAIDbContextFactory.CreateDbContextAsync();

            var bulkEmployees = employees
               .Select(employee => new Employee
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
               .ToList();

            await dbContext.AddBulkAsync(bulkEmployees, batchSize);

            return bulkEmployees.Count;
        }

        public async Task<List<EmployeeEntity>> GetTopRatedOpenAIEmployeesAsync(double rating)
        {
            await using var dbContext =
                await _openAIDbContextFactory.CreateDbContextAsync();

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