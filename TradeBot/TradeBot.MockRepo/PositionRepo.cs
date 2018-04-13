using System;
using System.Collections.Generic;
using TradeBot.Models;
using TradeBot.Repo;
using TradeBot.Utils.Enum;
using TradeBot.Models.Broker.ETrade;

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
                ProfitLossOpen = 0,
                OptionOrderResponse = new OptionOrderResponse
                {
                    AccountId = 999999999,
                    AllOrNone = false,
                    EstimatedCommission = 8.74,
                    EstimatedTotalAmount = 1008.78,
                    Messages = new List<Message>
                    {
                        new Message
                        {
                            Description = "Your order was successfully entered during market hours.",
                            Code = "1026"
                        }
                    },
                    OrderNumber = 257,
                    OrderTime = DateTime.Now.AddSeconds(-5),
                    Quantity = 1,
                    ReserveOrder = false,
                    ReserveQuantity = 0,
                    OrderTerm = AppEnums.OrderTerm.GOOD_FOR_DAY,
                    LimitPrice = 10,
                    OptionSymbol = new OptionSymbol
                    {
                        Symbol = "TSLA",
                        OptionType = AppEnums.OptionType.CALL,
                        StrikePrice = 300,
                        ExpirationYear = 2018,
                        ExpirationMonth = 4,
                        ExpirationDay = 20
                    },
                    OrderAction = AppEnums.OrderAction.BUY_TO_OPEN
                }
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
