using System.Collections.Generic;

using static TradeBot.Models.Enum.AppEnums;

namespace TradeBot.Models.Broker.ETrade
{
    public class PositionBehavior
    {
        public AccountPosition AccountPosition { get; set; }
        public List<Change> Changes { get; set; }
    }
}
