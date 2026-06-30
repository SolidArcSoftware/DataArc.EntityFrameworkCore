namespace DataArc.EntityFrameworkCore.Demo.Persistence.Database.Seeder
{
    public interface IDatabaseSeeder
    {
        bool SeedDatabase(int recordCount);
        Task<bool> SeedDatabaseAsync(int recordCount);
    }
}