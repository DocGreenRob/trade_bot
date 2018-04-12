using System.Collections.Generic;
using TradeBot.Models;
using static TradeBot.Utils.Enum.AppEnums;

namespace TradeBot.Repo
{
	public interface IPositionRepo
	{
		System.Collections.Generic.List<Option> GetOptionChain(string underlying, OptionType chainType);
		double GetOrderPrice(List<Option> optionChain);
		double GetOptionBuyingPower();
        Models.Position CreateNewPosition(string underlying, List<Option> optionChain, int numOfContracts, double currentPositionPrice);
    }
}
