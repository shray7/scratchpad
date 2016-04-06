using scratchpad.Models;
using System.Data.Entity;
using WebApi.Configuration;

namespace WebApi.Models.Context
{
    [DbConfigurationType(typeof(AzureConfiguration))]
    public class AllSalariesContext : DbContext
    {
        public DbSet<Salary> SalarySet { get; set; }
        public AllSalariesContext() : base("DefaultConnection")
        { }

        public virtual void Delete(Salary salary)
        {
            SalarySet.Attach(salary);
            SalarySet.Remove(salary);
            SaveChanges();
        }
    }
}