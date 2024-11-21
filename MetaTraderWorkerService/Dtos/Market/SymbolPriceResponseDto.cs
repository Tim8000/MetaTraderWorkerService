using System;
using System.Text.Json.Serialization;

namespace MetaTraderWorkerService.Dtos.Market
{
    public class SymbolPriceResponseDto
    {
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; }

        [JsonPropertyName("bid")]
        public decimal Bid { get; set; }

        [JsonPropertyName("ask")]
        public decimal Ask { get; set; }

        [JsonPropertyName("time")]
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime Time { get; set; }

        [JsonPropertyName("brokerTime")]
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime BrokerTime { get; set; }

        [JsonPropertyName("accountCurrencyExchangeRate")]
        public decimal AccountCurrencyExchangeRate { get; set; }

        [JsonPropertyName("profitTickValue")]
        public decimal ProfitTickValue { get; set; }

        [JsonPropertyName("lossTickValue")]
        public decimal LossTickValue { get; set; }

        [JsonPropertyName("timestamps")]
        public Timestamps Timestamps { get; set; }

        [JsonPropertyName("equity")]
        public decimal Equity { get; set; }
    }

    public class Timestamps
    {
        [JsonPropertyName("eventGenerated")]
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime EventGenerated { get; set; }

        [JsonPropertyName("serverProcessingStarted")]
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime ServerProcessingStarted { get; set; }
    }
}