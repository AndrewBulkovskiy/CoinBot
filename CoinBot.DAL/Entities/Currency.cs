using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CoinBot.DAL.Entities
{
    public class Currency
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "symbol")]
        public string Symbol { get; set; }
        [JsonProperty(PropertyName = "rank")]
        public int Rank { get; set; }
        [JsonProperty(PropertyName = "price_usd")]
        public double PriceUsd { get; set; }
        [JsonProperty(PropertyName = "price_btc")]
        public double PriceBtc { get; set; }
        [JsonProperty(PropertyName = "24h_volume_usd")]
        public double Volume24hUsd { get; set; }
        [JsonProperty(PropertyName = "market_cap_usd")]
        public double MarketCapUsd { get; set; }
        [JsonProperty(PropertyName = "available_supply")]
        public double AvailableSupply { get; set; }
        [JsonProperty(PropertyName = "total_supply")]
        public double TotalSupply { get; set; }
        [JsonProperty(PropertyName = "max_supply")]
        public double MaxSupply { get; set; }
        [JsonProperty(PropertyName = "percent_change_1h")]
        public double PercentChange1h { get; set; }
        [JsonProperty(PropertyName = "percent_change_24h")]
        public double PercentChange24h { get; set; }
        [JsonProperty(PropertyName = "percent_change_7d")]
        public double PercentChange7d { get; set; }
        [JsonProperty(PropertyName = "last_updated")]
        public double LastUpdated { get; set; }
    }
}
