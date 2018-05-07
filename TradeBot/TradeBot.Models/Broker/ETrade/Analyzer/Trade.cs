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
        /// <summary>
        /// The overall status of the trade.  Regardless of legs, the SUM of all positions in the Trade for the given Underlying (Root Symbol)
        /// </summary>
        /// <value>
        /// The sum change.
        /// </value>
        public List<TradeBehaviorChange> Sum_Change { get; set; }
    }
}