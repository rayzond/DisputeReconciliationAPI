using Microsoft.Extensions.Logging;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using DisputeReconciliation.Core;

namespace DisputeReconciliation.Parsers
{
    public class DisputeFileParser
    {
        private readonly ILogger<DisputeFileParser> _logger;
        public DisputeFileParser(ILogger<DisputeFileParser> logger) => _logger = logger;

        public async IAsyncEnumerable<Dispute> ParseCsvAsync(Stream fileStream)
        {
            using var reader = new StreamReader(fileStream);
            string? header = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(header)) yield break;
            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split(',');
                if (parts.Length < 6) continue;
                yield return new Dispute
                {
                    DisputeId = parts[0],
                    TransactionId = parts[1],
                    Amount = decimal.Parse(parts[2], CultureInfo.InvariantCulture),
                    Currency = parts[3],
                    Status = parts[4],
                    Reason = parts[5],
                };
            }
        }

        public async IAsyncEnumerable<Dispute> ParseXmlAsync(Stream fileStream)
        {
            using var reader = new StreamReader(fileStream);
            var xml = await reader.ReadToEndAsync();
            var doc = XDocument.Parse(xml);
            foreach (var el in doc.Descendants("Dispute"))
            {
                yield return new Dispute
                {
                    DisputeId = el.Element("DisputeId")?.Value ?? string.Empty,
                    TransactionId = el.Element("TransactionId")?.Value ?? string.Empty,
                    Amount = decimal.Parse(el.Element("Amount")?.Value ?? "0", CultureInfo.InvariantCulture),
                    Currency = el.Element("Currency")?.Value ?? string.Empty,
                    Status = el.Element("Status")?.Value ?? string.Empty,
                    Reason = el.Element("Reason")?.Value ?? string.Empty,
                };
            }
        }

        public async IAsyncEnumerable<Dispute> ParseJsonAsync(Stream fileStream)
        {
            var list = await JsonSerializer.DeserializeAsync<List<Dispute>>(fileStream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                        ?? new List<Dispute>();
            foreach (var d in list) yield return d;
        }

        public async IAsyncEnumerable<Dispute> DetectFormatAndParseAsync(Stream stream, string fileName)
        {
            stream.Position = 0;
            if (fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                await foreach (var d in ParseCsvAsync(stream)) yield return d;
            else if (fileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                await foreach (var d in ParseXmlAsync(stream)) yield return d;
            else if (fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                await foreach (var d in ParseJsonAsync(stream)) yield return d;
            else
            {
                _logger.LogWarning("Unsupported format: {FileName}", fileName);
                yield break;
            }
        }
    }
}