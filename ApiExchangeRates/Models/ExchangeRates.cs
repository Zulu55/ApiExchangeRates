namespace ApiExchangeRates.Models
{
    using Newtonsoft.Json;

    public class ExchangeRates
    {
        [JsonProperty(PropertyName = "disclaimer")]
        public string Disclaimer { get; set; }

        [JsonProperty(PropertyName = "license")]
        public string License { get; set; }

        [JsonProperty(PropertyName = "timestamp")]
        public int TimeStamp { get; set; }

        [JsonProperty(PropertyName = "base")]
        public string Base { get; set; }

        [JsonProperty(PropertyName = "rates")]
        public Rates Rates { get; set; }
    }
}