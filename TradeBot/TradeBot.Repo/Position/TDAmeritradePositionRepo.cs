
using System.Collections.Generic;

using TradeBot.Models;
using TradeBot.Models.Broker.ETrade;
using TradeBot.Models.Broker.ETrade.Analyzer;
using TradeBot.Models.Enum;

namespace TradeBot.Repo.Position
{
    public class TDAmeritradePositionRepo : IPositionRepo
    {
        public AccountPosition Change(AccountPosition accountPosition, Change change)
        {
            throw new System.NotImplementedException();
        }

        public Models.Position CreateNewPosition(string underlying, OptionChainResponse optionChain, int numOfContracts, double currentPositionPrice, AppEnums.OptionType optionType)
        {
            throw new System.NotImplementedException();
        }

        public AppEnums.Decision Evaluate(Trade trade)
        {
            throw new System.NotImplementedException();
        }

        public double GetOptionBuyingPower()
        {
            throw new System.NotImplementedException();
        }

        public OptionChainResponse GetOptionChain(string underlying, AppEnums.OptionType chainType)
        {
            throw new System.NotImplementedException();
        }

        public double GetOrderPrice(OptionChainResponse optionChain)
        {
            throw new System.NotImplementedException();
        }

        public AccountPositionsResponse GetPositions(int accountId)
        {
            throw new System.NotImplementedException();
        }

        Trade IPositionRepo.Evaluate(Trade trade)
        {
            throw new System.NotImplementedException();
        }
    }
}
