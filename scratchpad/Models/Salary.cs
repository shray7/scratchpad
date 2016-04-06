using System.ComponentModel.DataAnnotations;

namespace scratchpad.Models
{
    public class Salary
    {
        [Key]
        public int SalaryId { get; set; }
        public string Name { get; set; }
        public string Title { get; set;}
        public string Department { get; set; }
        public string FTR { get; set; }
        public string GF { get; set; }
        public string Year { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}