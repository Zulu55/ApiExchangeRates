namespace ApiExchangeRates.Models
{
    using System.Data.Entity;

    public class DataContext : DbContext
    {
        public DataContext() : base("DefaultConnection")
        {
        }

        public DbSet<QueryHistory> QueryHistories { get; set; }

        public DbSet<Rate> Rates { get; set; }
    }
}