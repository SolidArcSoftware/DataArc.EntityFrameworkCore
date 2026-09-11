namespace DataArc.EntityFrameworkCore.Demo.Application.Entities
{
    public sealed class EmployeeEntity
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public decimal Salary { get; set; }
        public int EmployerId { get; set; }
        public int? Order { get; set; }
        public bool IsArchived { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime? LastUpdatedUtc { get; set; }
        public string? Notes { get; set; }
        public string? Status { get; set; }
        public double? Rating { get; set; }
    }
}