using DataArc.Core;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBModels;
using Microsoft.EntityFrameworkCore;

namespace DataArc.EntityFrameworkCore.Demo.Persistence.Contracts
{
    public interface IGoogleDbContext : IExecutionContext
    {
        DbSet<Employer>? Employer { get; set; }
        DbSet<Employee>? Employee { get; set; }
    }
}