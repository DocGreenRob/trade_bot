using static TradeBot.Models.Enum.AppEnums;

namespace TradeBot.Models.Broker.ETrade
{
    public class AccountPosition
    {
        public double CostBasis { get; set; }
        public string Description { get; set; }
        public LongOrShort LongOrShort { get; set; }
        public bool Marginable { get; set; }
        public Product Product { get; set; }
        public int Quantity { get; set; }
        public double CurrentPrice { get; set; }
        public double MarketValue { get; set; }

    }
}
