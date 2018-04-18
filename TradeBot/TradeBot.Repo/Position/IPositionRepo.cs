using System.Collections.Generic;
using TradeBot.Models;
using TradeBot.Models.Broker.ETrade;
using TradeBot.Utils.Enum;
using static TradeBot.Utils.Enum.AppEnums;

namespace TradeBot.Repo
{
	public interface IPositionRepo
	{
        #region ETrade

        List<Option> GetOptionChain(string underlying, OptionType chainType);
		double GetOrderPrice(List<Option> optionChain);
		double GetOptionBuyingPower();
        Models.Position CreateNewPosition(string underlying, List<Option> optionChain, int numOfContracts, double currentPositionPrice);
        AccountPosition Change(AccountPosition accountPosition, TradeDirection tradeDirection, double changeAmount);
        AccountPositionsResponse GetPositions(int accountId);
        Decision Evaluate(AccountPosition adjustedAccountPosition);

        #endregion

    }
}
