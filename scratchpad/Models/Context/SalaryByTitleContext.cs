using System.Data.Entity;

namespace scratchpad.Models.Context
{
    public class SalaryByTitleContext : DbContext
    {
        protected DbSet<SalaryByTitle> SalaryByTitleSet { get; set; }
        public SalaryByTitleContext() : base("DefaultConnection") { }

        public virtual void Delete(SalaryByTitle model)
        {
            SalaryByTitleSet.Attach(model);
            SalaryByTitleSet.Remove(model);
            SaveChanges();
        }
    }
}