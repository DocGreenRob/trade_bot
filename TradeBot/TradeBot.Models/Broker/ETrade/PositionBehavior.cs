using System.Collections.Generic;

using static TradeBot.Utils.Enum.AppEnums;

namespace TradeBot.Models.Broker.ETrade
{
    public class PositionBehavior
    {
        public AccountPosition AccountPosition { get; set; }
        public List<Changes> Changes { get; set; }
    }
}
