using System;
using System.Collections.Generic;
using static TradeBot.Models.Enum.AppEnums;

namespace TradeBot.Models.Broker.ETrade.Analyzer
{
    /// <summary>
    /// Returns a fully constructed Trade().
    /// </summary>
    public class Trade
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Trade"/> class.  Returns a fully constructed Trade.
        /// </summary>
        public Trade() : this(new Position(), new Position()) { }
        public Trade(Position callPosition, Position putPosition)
        {
            this.Positions = new List<Position>();
            this.Positions.AddRange(new List<Position> { callPosition, putPosition });
            this.BehaviorChanges = new Dictionary<TradeBehaviorChange, TradeBehaviorChange>();
            // this.Sum_Change = new List<TradeBehaviorChange> { new TradeBehaviorChange() };
            this.Sum_Change = new List<TradeBehaviorChange>();
        }
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
        public double MaxLossPercent { get; set; }
        public double MaxLossDollars { get; set; }
        public double MaxGainPercent { get; set; }
        public double MaxGainDollars { get; set; }

        public Bias Bias { get; set; }
    }
}