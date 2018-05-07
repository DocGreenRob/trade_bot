using System.Collections.Generic;
using TradeBot.Models.Broker.ETrade.Analyzer;
using static TradeBot.Models.Enum.AppEnums;

namespace TradeBot.Models.Broker.ETrade
{
    public class PositionBehavior
    {
        public AccountPosition AccountPosition { get; set; }
        public Change Change { get; set; }
        public List<Flag> Flags { get; set; }
        public Decision Decision { get; set; }
    }
}
