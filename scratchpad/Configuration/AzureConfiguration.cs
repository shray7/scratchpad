using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.SqlServer;

namespace WebApi.Configuration
{
    public class AzureConfiguration: DbConfiguration
    {
        public AzureConfiguration()
        {
            SetExecutionStrategy("System.Data.SqlClient", () => new SqlAzureExecutionStrategy(4, TimeSpan.FromSeconds(10)));
        }
    }
}