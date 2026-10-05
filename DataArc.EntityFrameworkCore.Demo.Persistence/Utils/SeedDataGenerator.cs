using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBModels;

namespace DataArc.EntityFrameworkCore.Demo.Persistence.Utils
{
    public sealed class HrSeedData
    {
        public List<Employer> Employers { get; init; } = [];
        public List<Employee> Employees { get; init; } = [];
    }

    public static class SeedDataGenerator
    {
        private static string GetRandomStatus(Random rand)
        {
            var statuses = new[] { "Active", "Inactive", "Pending" };
            return statuses[rand.Next(statuses.Length)];
        }

        public static HrSeedData GenerateHrSeedData(int count = 50)
        {
            var employers = new List<Employer>(count);
            var employees = new List<Employee>(count);

            var rand = new Random();
            var utcNow = DateTime.UtcNow;

            for (var i = 1; i <= count; i++)
            {
                employers.Add(
                    new Employer
                    {
                        Id = i,
                        Name = $"Employer{i}",
                        Description = $"Employer {i}"
                    });

                employees.Add(
                    new Employee
                    {
                        Id = i,
                        Name = $"Name{i}",
                        Surname = $"Surname{i}",
                        Salary = Math.Round(
                            (decimal)(rand.NextDouble() * (150000 - 30000) + 30000),
                            2),
                        EmployerId = i,
                        Order = rand.Next(1, 101),
                        IsArchived = rand.Next(0, 2) == 0,
                        CreatedUtc = utcNow.AddDays(-rand.Next(0, 1000)),
                        LastUpdatedUtc = utcNow,
                        Notes = $"This is a sample note for record {i}.",
                        Status = GetRandomStatus(rand),
                        Rating = Math.Round(
                            rand.NextDouble() * 4 + 1,
                            2)
                    });
            }

            return new HrSeedData
            {
                Employers = employers,
                Employees = employees
            };
        }
    }
}