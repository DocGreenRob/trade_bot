using static TradeBot.Utils.Enum.AppEnums;

namespace TradeBot.Models.Broker.ETrade
{
    public class Change
    {
        public TradeDirection TradeDirection { get; set; }
        public double Amount { get; set; }
    }
}