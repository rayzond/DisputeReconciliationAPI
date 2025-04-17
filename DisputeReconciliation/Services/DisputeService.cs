using DisputeReconciliation.App.Data;
using DisputeReconciliation.App.Services;
using DisputeReconciliation.Core;
using DisputeReconciliation.Parsers;
using System.Collections.Concurrent;
using System.Xml.Linq;

public class DisputeService
{
    private readonly DisputeDAO _dao;
    private readonly ExchangeRateService _exchange;
    private readonly DisputeFileParser _parser;
    private readonly ILogger<DisputeService> _logger;

    public DisputeService(DisputeDAO dao, ExchangeRateService exchange, DisputeFileParser parser, ILogger<DisputeService> logger)
    {
        _dao = dao;
        _exchange = exchange;
        _parser = parser;
        _logger = logger;
    }

    public async Task<List<Dispute>> GetPagedInternalDataAsync(int page, int size) =>
        await _dao.GetPageAsync(page, size);

    public async Task<string> CompareDisputeFileAsync(Stream stream, string fileName)
    {
        var list = new List<Dispute>();
        await foreach (var d in _parser.DetectFormatAndParseAsync(stream, fileName))
            list.Add(d);
        return await CompareDisputesAsync(list);
    }

    public async Task<string> CompareDisputesAsync(IEnumerable<Dispute> incoming)
    {
        List<string> audit = new();
        List<string> alerts = new();
        await collectAuditAndAlerts(incoming, audit, alerts);

        // Generate report file
        string timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        string fileName = $"DisputeReport_{timestamp}.txt";
        string folder = Path.Combine(AppContext.BaseDirectory, "Reports");
        Directory.CreateDirectory(folder);
        string fullPath = Path.Combine(folder, fileName);

        var lines = new List<string> { "===== AUDIT =====" };
        lines.AddRange(audit);
        lines.Add("\n===== ALERTS =====");
        lines.AddRange(alerts);

        await File.WriteAllLinesAsync(fullPath, lines);
        return fileName;
    }

    private async Task collectAuditAndAlerts(IEnumerable<Dispute> incoming, List<string> audit, List<string> alerts)
    {
        foreach (var inc in incoming)
        {
            // Always log the incoming transaction
            audit.Add($"AUDIT: {inc.DisputeId} | {inc.TransactionId} | {inc.Amount:F2} {inc.Currency} | {inc.Status}");

            // Fetch match sequentially (safe for DbContext)
            Dispute? match = await _dao.GetByDisputeIdAsync(inc.DisputeId)
                             ?? await _dao.GetByTransactionIdAsync(inc.TransactionId);

            if (match == null)
            {
                alerts.Add($"⚠️ **HIGH**: {inc.DisputeId}/{inc.TransactionId} not found");
                continue;
            }

            // Build list of issues
            List<string> issues = new();
            if (!inc.DisputeId.Equals(match.DisputeId, StringComparison.OrdinalIgnoreCase))
                issues.Add($"[{inc.DisputeId}/{inc.TransactionId}] 🆔 ID mismatch: {inc.DisputeId} vs {match.DisputeId}");

            if (!inc.TransactionId.Equals(match.TransactionId, StringComparison.OrdinalIgnoreCase))
                issues.Add($"[{inc.DisputeId}/{inc.TransactionId}] 🔁 Transaction ID mismatch: {inc.TransactionId} vs {match.TransactionId}");

            decimal incUsd = _exchange.ConvertToUSD(inc.Amount, inc.Currency);
            decimal matchUsd = _exchange.ConvertToUSD(match.Amount, match.Currency);
            if (incUsd != matchUsd)
            {
                decimal diff = Math.Abs(matchUsd - incUsd);
                string sev = diff > 100 ? "**HIGH**" : "MEDIUM";
                issues.Add($"[{inc.DisputeId}/{inc.TransactionId}] 💰 Amount mismatch: {matchUsd:F2} vs {incUsd:F2} [{sev}]");
            }

            if (!inc.Status.Equals(match.Status, StringComparison.OrdinalIgnoreCase))
                issues.Add($"[{inc.DisputeId}/{inc.TransactionId}] 🔄 Status mismatch: {match.Status} vs {inc.Status}");

            if (!issues.Any() && !match.Status.Equals("Open", StringComparison.OrdinalIgnoreCase))
                issues.Add($"[{inc.DisputeId}/{inc.TransactionId}] ✅ Already resolved");

            if (issues.Any())
                alerts.Add(string.Join(" | ", issues));
        }
    }
}