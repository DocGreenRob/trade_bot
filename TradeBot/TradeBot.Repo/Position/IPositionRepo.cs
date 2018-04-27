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

        OptionChainResponse GetOptionChain(string underlying, OptionType chainType);
		double GetOrderPrice(OptionChainResponse optionChain);
		double GetOptionBuyingPower();
        Models.Position CreateNewPosition(string underlying, OptionChainResponse optionChain, int numOfContracts, double currentPositionPrice, AppEnums.OptionType optionType);
        AccountPosition Change(AccountPosition accountPosition, TradeDirection tradeDirection, double changeAmount);
        AccountPositionsResponse GetPositions(int accountId);
        Decision Evaluate(AccountPosition adjustedAccountPosition);

        #endregion

    }
}
