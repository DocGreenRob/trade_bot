using System.Collections.Generic;

using TradeBot.Models;
using TradeBot.Models.Broker.ETrade;
using TradeBot.Models.Broker.ETrade.Analyzer;
using TradeBot.Models.Enum;
using static TradeBot.Models.Enum.AppEnums;

namespace TradeBot.Repo
{
	public interface IPositionRepo
	{
        #region ETrade

        OptionChainResponse GetOptionChain(string underlying, OptionType chainType);
		double GetOrderPrice(OptionChainResponse optionChain);
		double GetOptionBuyingPower();
        Models.Position CreateNewPosition(string underlying, OptionChainResponse optionChain, int numOfContracts, double currentPositionPrice, AppEnums.OptionType optionType);
        AccountPosition Change(AccountPosition accountPosition, Change change);
        AccountPositionsResponse GetPositions(int accountId);
        bool Close(Trade trade);
        double GetStockPrice(string symbol);
        #endregion

    }
}
