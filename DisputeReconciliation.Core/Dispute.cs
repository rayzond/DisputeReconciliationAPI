using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace DisputeReconciliation.Core
{
    public class Dispute
    {
        public string DisputeId { get; set; }
        public string TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }
        public string Reason { get; set; }
    }

    // DbContext
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Dispute> Disputes { get; set; }
    }
}


