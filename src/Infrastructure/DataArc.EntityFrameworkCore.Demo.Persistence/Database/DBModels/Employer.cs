using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBModels
{
    [Table("Employers", Schema = "employers")]
    public class Employer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public ICollection<Employee>? Employee { get; set; }
    }
}