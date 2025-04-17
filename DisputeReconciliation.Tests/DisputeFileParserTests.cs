using NUnit.Framework;
using Microsoft.Extensions.Logging;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using DisputeReconciliation.Parsers;
using DisputeReconciliation.Core;

namespace DisputeReconciliation.Tests.Parsers
{
    public class DisputeFileParserTests
    {
        private DisputeFileParser _parser;

        [SetUp]
        public void Setup()
        {
            ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<DisputeFileParser> logger = loggerFactory.CreateLogger<DisputeFileParser>();
            _parser = new DisputeFileParser(logger);
        }

        [Test]
        public async Task ParseCsvAsync_ValidCsv_ReturnsDisputes()
        {
            string csv = "DisputeId,TransactionId,Amount,Currency,Status,Reason\nD1,T1,100.00,USD,Open,Fraud";
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

            List<Dispute> list = await _parser.ParseCsvAsync(stream).ToListAsync();

            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("D1", list[0].DisputeId);
            Assert.AreEqual("T1", list[0].TransactionId);
            Assert.AreEqual(100.00m, list[0].Amount);
        }

        [Test]
        public async Task ParseCsvAsync_EmptyContent_ReturnsEmpty()
        {
            string csv = string.Empty;
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

            List<Dispute> list = await _parser.ParseCsvAsync(stream).ToListAsync();

            Assert.IsEmpty(list);
        }

        [Test]
        public async Task ParseXmlAsync_ValidXml_ReturnsDisputes()
        {
            string xml = @"<Root><Dispute><DisputeId>D2</DisputeId><TransactionId>T2</TransactionId><Amount>50.5</Amount><Currency>EUR</Currency><Status>Closed</Status><Reason>Chargeback</Reason></Dispute></Root>";
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));

            List<Dispute> list = await _parser.ParseXmlAsync(stream).ToListAsync();

            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("D2", list[0].DisputeId);
            Assert.AreEqual(50.5m, list[0].Amount);
        }

        [Test]
        public async Task ParseXmlAsync_MissingFields_DefaultsEmpty()
        {
            string xml = @"<Root><Dispute><DisputeId>D3</DisputeId><TransactionId>T3</TransactionId><Amount>0</Amount></Dispute></Root>";
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));

            List<Dispute> list = await _parser.ParseXmlAsync(stream).ToListAsync();

            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(string.Empty, list[0].Currency);
            Assert.AreEqual(string.Empty, list[0].Status);
            Assert.AreEqual(string.Empty, list[0].Reason);
        }

        [Test]
        public async Task ParseJsonAsync_ValidJson_ReturnsDisputes()
        {
            string json = "[ { \"DisputeId\": \"D4\", \"TransactionId\": \"T4\", \"Amount\": 75, \"Currency\": \"JPY\", \"Status\": \"Open\", \"Reason\": \"Duplicate\" } ]";
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

            List<Dispute> list = await _parser.ParseJsonAsync(stream).ToListAsync();

            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("D4", list[0].DisputeId);
            Assert.AreEqual(75m, list[0].Amount);
        }

        [Test]
        public async Task DetectFormatAndParseAsync_CsvExtension_UsesCsv()
        {
            string csv = "DisputeId,TransactionId,Amount,Currency,Status,Reason\nD5,T5,120.00,USD,Open,Error";
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

            List<Dispute> list = await _parser.DetectFormatAndParseAsync(stream, "file.csv").ToListAsync();

            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("D5", list[0].DisputeId);
        }

        [Test]
        public async Task DetectFormatAndParseAsync_XmlExtension_UsesXml()
        {
            string xml = @"<Root><Dispute><DisputeId>D6</DisputeId><TransactionId>T6</TransactionId><Amount>200</Amount><Currency>EUR</Currency><Status>Lost</Status><Reason>Delay</Reason></Dispute></Root>";
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));

            List<Dispute> list = await _parser.DetectFormatAndParseAsync(stream, "file.xml").ToListAsync();

            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("D6", list[0].DisputeId);
        }

        [Test]
        public async Task DetectFormatAndParseAsync_JsonExtension_UsesJson()
        {
            string json = "[ { \"DisputeId\": \"D7\", \"TransactionId\": \"T7\", \"Amount\": 300.5, \"Currency\": \"CAD\", \"Status\": \"Open\", \"Reason\": \"Test\" } ]";
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

            List<Dispute> list = await _parser.DetectFormatAndParseAsync(stream, "file.json").ToListAsync();

            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("T7", list[0].TransactionId);
        }

        [Test]
        public async Task DetectFormatAndParseAsync_UnsupportedExtension_ReturnsEmpty()
        {
            string data = "some data";
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(data));

            List<Dispute> list = await _parser.DetectFormatAndParseAsync(stream, "file.txt").ToListAsync();

            Assert.IsEmpty(list);
        }
    }

    public static class AsyncEnumerableExtensions
    {
        public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source)
        {
            List<T> result = new List<T>();
            await foreach (T item in source)
                result.Add(item);
            return result;
        }
    }
}
