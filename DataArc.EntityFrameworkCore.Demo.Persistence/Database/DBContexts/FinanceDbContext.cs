using DataArc.Core;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBModels;
using Microsoft.EntityFrameworkCore;

namespace DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBContexts
{
    public interface IFinanceDbContext : IExecutionContext
    {
       
    }

    internal class FinanceDbContext : DbContext, IFinanceDbContext
    {
        public FinanceDbContext(DbContextOptions<FinanceDbContext> dbContextOptions) 
            : base(dbContextOptions) { }

        public DbSet<Employer>? Employer { get; set; }
        public DbSet<Employee>? Employee { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Employer>().Property(x => x.Description).HasColumnType("text");
        }
    }
}