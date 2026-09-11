using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBModels
{
    [Table("Employees", Schema = "employees")]
    public class Employee
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("EmployeeName", Order = 1, TypeName = "varchar(50)")]
        public string? Name { get; set; }

        [Column("EmployeeNameSurname", Order = 2, TypeName = "varchar(50)")]
        public string? Surname { get; set; }

        [Column("EmployeeSalary", Order = 3, TypeName = "decimal(33,2)")]
        public decimal Salary { get; set; }

        [ForeignKey(nameof(Employer))]
        [Column("EmployerId", Order = 4)]
        public int EmployerId { get; set; }

        public Employer Employer { get; set; } = new();

        [Column("PositionOrder", Order = 5)]
        public int? Order { get; set; }

        [Column("IsArchived", Order = 6)]
        public bool IsArchived { get; set; }

        [Column("CreatedUtc", Order = 7)]
        public DateTime CreatedUtc { get; set; }

        [Column("LastUpdatedUtc", Order = 8)]
        public DateTime? LastUpdatedUtc { get; set; }

        [Column("Notes", Order = 9, TypeName = "nvarchar(500)")]
        public string? Notes { get; set; }

        [Column("Status", Order = 10, TypeName = "varchar(20)")]
        public string? Status { get; set; }

        [Column("Rating", Order = 11, TypeName = "float")]
        public double? Rating { get; set; }
    }
}