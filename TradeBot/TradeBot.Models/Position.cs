using System;
using TradeBot.Models.Interfaces;

namespace TradeBot.Models
{
    public class Position
    {
        public int PositionId { get; set; }
        public double UnderlyingPriceAtEntry { get; set; }
        public IPosition PositionType { get; set; }
        public DateTime EntryTime { get; set; }
        public double ProfitLossOpen { get; set; }
        public Underlying Underlying { get; set; }
    }
}
