using DataArc.Core;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBModels;
using Microsoft.EntityFrameworkCore;

namespace DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBContexts
{
    public interface IOperationsDbContext : IExecutionContext
    {
    }

    internal class OperationsDbContext : DbContext, IOperationsDbContext
    {
        public OperationsDbContext(DbContextOptions<OperationsDbContext> dbContextOptions) : base(dbContextOptions) { }

        public DbSet<Employer>? Employer { get; set; }
        public DbSet<Employee>? Employee { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Employer>().Property(x => x.Description).HasColumnType("text");
        }
    }
}