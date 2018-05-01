using static TradeBot.Utils.Enum.AppEnums;

namespace TradeBot.Models.Broker.ETrade.Analyzer.Studies
{
    public abstract class Study
    {
        public Interval Interval { get; set; }
        public double Value { get; set; }
    }
}
