namespace DataArc.EntityFrameworkCore.Demo.Persistence.Database
{
    public interface IDatabaseSeeder
    {
        Task<bool> SeedDatabaseAsync();
    }
}