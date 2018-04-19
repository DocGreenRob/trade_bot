using static TradeBot.Utils.Enum.AppEnums;

namespace TradeBot.Models.Broker.ETrade
{
    public class Product
    {
        /// <summary>
        /// (i.e., GOOG Jan 07 '11 $540 Call w)
        /// </summary>
        public string Symbol { get; set; }
        public TypeCode TypeCode { get; set; }
        public OptionType CallPut { get; set; }
        public double StrikePrice { get; set; }
        public int ExpirationYear { get; set; }
        public int ExpirationMonth { get; set; }
        public int ExpirationDay { get; set; }
        /// <summary>
        /// (i.e., CINC)
        /// </summary>
        public ExchangeCode ExchangeCode { get; set; }
    }
}