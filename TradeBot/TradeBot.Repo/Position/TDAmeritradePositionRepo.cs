
using System.Collections.Generic;
using TradeBot.Models;
using TradeBot.Models.Broker.ETrade;
using TradeBot.Utils.Enum;

namespace TradeBot.Repo.Position
{
    public class TDAmeritradePositionRepo : IPositionRepo
    {
        public AccountPosition Change(AccountPosition accountPosition, AppEnums.TradeDirection tradeDirection, double changeAmount)
        {
            throw new System.NotImplementedException();
        }

        public Models.Position CreateNewPosition(string underlying, List<Option> optionChain, int numOfContracts, double currentPositionPrice)
        {
            throw new System.NotImplementedException();
        }

        public AppEnums.Decision Evaluate(AccountPosition adjustedAccountPosition)
        {
            throw new System.NotImplementedException();
        }

        public double GetOptionBuyingPower()
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

        public AccountPositionsResponse GetPositions(int accountId)
        {
            throw new System.NotImplementedException();
        }
    }
}
