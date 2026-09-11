namespace DataArc.EntityFrameworkCore.Demo.Application.Dtos
{
    public class EmployeeDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public decimal? Salary { get; set; }
    }
}