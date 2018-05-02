using System;
using System.Collections.Generic;

namespace TradeBot.Models.Broker.ETrade.Analyzer
{
    public class Trade
    {
        public DateTime Time { get; set; }
        public double StockPrice { get; set; }
        public List<Position> Positions { get; set; }
        public List<TradeBehaviorChange> BehaviorChanges { get; set; }
        public List<Flag> Flags { get; set; }
    }
}