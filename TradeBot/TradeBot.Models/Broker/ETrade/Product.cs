using static TradeBot.Utils.Enum.AppEnums;

namespace TradeBot.Models.Broker.ETrade
{
    public class Product
    {
        public string Symbol { get; set; }
        public TypeCode TypeCode { get; set; }
        public OptionType CallPut { get; set; }
        public double StrikePrice { get; set; }
        public int ExpirationYear { get; set; }
        public int ExpirationMonth { get; set; }
        public int ExpirationDay { get; set; }
    }
}