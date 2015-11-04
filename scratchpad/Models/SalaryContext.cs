using System.Data.Entity;

namespace scratchpad.Models
{
    public class SalaryContext : DbContext
    {
        public DbSet<Salary> SalarySet { get; set; }
        public SalaryContext() : base("DefaultConnection")
        {}

        public virtual void Delete(Salary salary)
        {
            SalarySet.Attach(salary);
            SalarySet.Remove(salary);
            SaveChanges();
        }
    }
}