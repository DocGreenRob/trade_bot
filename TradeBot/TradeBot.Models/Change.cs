using System;
using static TradeBot.Models.Enum.AppEnums;

namespace TradeBot.Models.Broker.ETrade
{
    public class Change
    {
        public TradeDirection TradeDirection { get; set; }
        public double Amount { get; set; }
        public DateTime DateTime { get; set; }
        public double StockPrice { get; set; }
    }
}