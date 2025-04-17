namespace DisputeReconciliation.App.Services
{
    public class ExchangeRateService
    {
        private readonly Dictionary<string, decimal> _exchangeRates = new() {
            { "USD", 1.0m },
            { "EUR", 1.1m },
            { "JPY", 0.009m },
        };

        public decimal ConvertToUSD(decimal amount, string currency)
        {
            return _exchangeRates.TryGetValue(currency.ToUpper(), out var rate)
                ? amount * rate
                : amount; // Default no conversion
        }
    }
}
