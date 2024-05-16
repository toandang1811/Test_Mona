using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Data
{
    public class Employee
    {
        [Key]
        public string EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public DateTime DayOfBirth { get; set; }
    }
}
