using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBModels;

namespace DataArc.EntityFrameworkCore.Demo.Persistence.Utils
{
    public static class SeedDataGenerator
    {
        private static string GetRandomStatus(Random rand)
        {
            var statuses = new[] { "Active", "Inactive", "Pending" };
            return statuses[rand.Next(statuses.Length)];
        }

        public static List<Employee> GenerateHrSeedData(int count = 50)
        {
            var list = new List<Employee>();
            var rand = new Random();

            for (int i = 1; i <= count; i++)
            {
                var entity = new Employee
                {
                    Id = i,
                    Name = $"Name{i}",
                    Surname = $"Surname{i}",
                    Salary = Math.Round((decimal)(rand.NextDouble() * (150000 - 30000) + 30000), 2),
                    EmployerId = 1,
                    Order = rand.Next(1, 101),
                    IsArchived = rand.Next(0, 2) == 0,
                    CreatedUtc = DateTime.UtcNow.AddDays(-rand.Next(0, 1000)),
                    LastUpdatedUtc = DateTime.UtcNow,
                    Notes = $"This is a sample note for record {i}.",
                    Status = GetRandomStatus(rand),
                    Rating = Math.Round(rand.NextDouble() * 4 + 1, 2)
                };

                list.Add(entity);
            }

            return list;
        }
    }
}
