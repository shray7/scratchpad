using System.Data.Entity;
using WebApi.Configuration;

namespace WebApi.Models.Context
{
    [DbConfigurationType(typeof(AzureConfiguration))]
    public class DepartmentContext : DbContext
    {
        public DbSet<Department> DepartmentSet { get; set; }
        public DepartmentContext(): base("DefaultConnection")
        {
            Database.SetInitializer<DepartmentContext>(null);
        }

        public virtual void Delete(Department department)
        {
            DepartmentSet.Attach(department);
            DepartmentSet.Remove(department);
            SaveChanges();
        }

    }
}