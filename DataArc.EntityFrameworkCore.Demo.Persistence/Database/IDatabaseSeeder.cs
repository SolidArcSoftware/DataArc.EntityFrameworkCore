namespace DataArc.EntityFrameworkCore.Demo.Persistence.Database
{
    public interface IDatabaseSeeder
    {
        bool SeedDatabase();
        Task<bool> SeedDatabaseAsync();
    }
}