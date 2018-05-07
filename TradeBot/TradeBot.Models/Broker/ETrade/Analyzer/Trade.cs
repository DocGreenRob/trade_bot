using System;
using System.Collections.Generic;
using static TradeBot.Models.Enum.AppEnums;

namespace TradeBot.Models.Broker.ETrade.Analyzer
{
    public class Trade
    {
        public DateTime Time { get; set; }
        public double StockPrice { get; set; }
        public List<Position> Positions { get; set; }
        public Dictionary<TradeBehaviorChange, TradeBehaviorChange> BehaviorChanges { get; set; }
        public List<Flag> Flags { get; set; }
        public Decision Decision { get; set; }
    }
}