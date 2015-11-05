using System.Data.Entity;
using WebApi.Configuration;

namespace scratchpad.Models
{
    [DbConfigurationType(typeof(AzureConfiguration))]
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