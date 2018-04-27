using static TradeBot.Utils.Enum.AppEnums;

namespace TradeBot.Models.Broker.ETrade
{
    public class Changes
    {
        public TradeDirection TradeDirection { get; set; }
        public double Change { get; set; }
    }
}