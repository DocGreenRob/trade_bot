namespace TradeBot.Models.Broker.ETrade.Analyzer
{
    public class TradeBehaviorChange
    {
        // The net change of the one or more Positions in the Trade
        public PriceActionBehavior PriceActionBehavior { get; set; }
        public PositionBehavior PositionBehavior { get; set; }
    }
}