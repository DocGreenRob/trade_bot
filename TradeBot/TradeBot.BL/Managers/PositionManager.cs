using System;
using static TradeBot.Utils.Enum.AppEnums;

namespace TradeBot.BL.Managers
{
    public class PositionManager
    {
        public void OpenPosition(string underlying, PositionType positionType, TradeStrength tradeStrength)
        {
            // Create new option trade, this should:
            // 1. Check the current price of the Underlying
            // 2. Check account value (to determine how many contracts to buy)
        }
    }
}
