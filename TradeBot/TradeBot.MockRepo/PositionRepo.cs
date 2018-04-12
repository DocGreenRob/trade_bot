using System;
using System.Collections.Generic;
using TradeBot.Models;
using TradeBot.Repo;
using TradeBot.Utils.Enum;

namespace TradeBot.MockRepo
{
    public class PositionRepo : IPositionRepo
    {
        public Position CreateNewPosition(string underlying, List<Option> optionChain, int numOfContracts, double currentPositionPrice)
        {
            return new Position
            {
                PositionId = 1,
                InstrumentType = AppEnums.InstrumentType.Option,
                EntryTime = DateTime.Now,
                UnderlyingPriceAtEntry = currentPositionPrice,
                Underlying = new Underlying {
                    Name = "TSLA"
                },
                ProfitLossOpen = 0
            };
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
    }
}
