using DisputeReconciliation.Core;
using Microsoft.EntityFrameworkCore;

namespace DisputeReconciliation.App.Data
{
    public class DisputeDAO
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DisputeDAO> _logger;

        public DisputeDAO(ApplicationDbContext context, ILogger<DisputeDAO> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IEnumerable<Dispute> GetAll() => _context.Disputes.ToList();

        public async Task<List<Dispute>> GetPageAsync(int page, int size)
        {
            try
            {
                return await _context.Disputes.Skip((page - 1) * size).Take(size).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged data");
                return new List<Dispute>();
            }
        }

        public async Task<Dispute?> GetByDisputeIdAsync(string disputeId)
        {
            return await _context.Disputes.FirstOrDefaultAsync(d => d.DisputeId == disputeId);
        }

        public async Task<Dispute?> GetByTransactionIdAsync(string transactionId)
        {
            return await _context.Disputes.FirstOrDefaultAsync(d => d.TransactionId == transactionId);
        }
    }
}
