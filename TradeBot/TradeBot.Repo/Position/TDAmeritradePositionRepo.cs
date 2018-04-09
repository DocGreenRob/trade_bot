
using System.Collections.Generic;
using TradeBot.Models;
using TradeBot.Utils.Enum;

namespace TradeBot.Repo.Position
{
	public class TDAmeritradePositionRepo : IPositionRepo
	{
		public double GetOptionBuyingPower()
		{
			throw new System.NotImplementedException();
		}

		public List<Option> GetOptionChain(string underlying, string chainType)
		{
			throw new System.NotImplementedException();
		}

		public List<Option> GetOptionChain(string underlying, AppEnums.OptionType chainType)
		{
			throw new System.NotImplementedException();
		}

		public double GetOrderPrice(List<Option> optionChain)
		{
			throw new System.NotImplementedException();
		}
	}
}
