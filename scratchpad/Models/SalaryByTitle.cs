using System.ComponentModel.DataAnnotations;

namespace scratchpad.Models
{
    public class SalaryByTitle
    {
        [Key]
        public int SalaryByTitleId { get; set; }
        public string NumberPeopleWithTitle { get; set; }
        public string MaxSalary { get; set; }
        public string MinSalary { get; set; }
        public string AvgSalary { get; set; }
    }
}