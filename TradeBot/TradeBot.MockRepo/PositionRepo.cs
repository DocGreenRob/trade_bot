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
        public AccountPosition Change(AccountPosition accountPosition, AppEnums.TradeDirection tradeDirection, double changeAmount)
        {
            switch (tradeDirection)
            {
                case AppEnums.TradeDirection.Down:
                    accountPosition.CurrentPrice = accountPosition.CurrentPrice - (accountPosition.CurrentPrice * changeAmount);
                    accountPosition.MarketValue = accountPosition.MarketValue - (accountPosition.CurrentPrice * changeAmount);
                    break;
                case AppEnums.TradeDirection.Sideways:
                    // TODO: Investigate
                    // Deduct time? Right?  I think so, because time is the only change in that case.  Here we want to siumulate, effectively, no change in price, which says only a change in time has occured!
                    break;
                case AppEnums.TradeDirection.Up:
                    accountPosition.CurrentPrice = accountPosition.CurrentPrice + (accountPosition.CurrentPrice * changeAmount);
                    accountPosition.MarketValue = accountPosition.MarketValue + (accountPosition.CurrentPrice * changeAmount);
                    break;
            }
            return accountPosition;
        }

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
                    EstimatedCommission = 5.95,
                    EstimatedTotalAmount = 521.64,
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

        public AppEnums.Decision Evaluate(AccountPosition adjustedAccountPosition)
        {
            // Evaluate the currentPositionPrice versus the costBasis and see the difference in percent

            double changeInDollars = adjustedAccountPosition.CurrentPrice - adjustedAccountPosition.CostBasis;
            double percentAsDecimal = changeInDollars / adjustedAccountPosition.CostBasis;
            double percent = percentAsDecimal * 100;

            // Add the evaluation to the history
            History.Add(adjustedAccountPosition, changeInDollars, percent);

            // negative
            if(percent < 0)
            {
                if (percent > -5)
                {
                    return AppEnums.Decision.Wait;
                }
                if (percent < -5)
                {

                    return AppEnums.Decision.Wait;
                }
                if (percent < -7)
                {
                    return AppEnums.Decision.Start_To_Worry;
                }
                if (percent >= -7)
                {
                    return AppEnums.Decision.Investigate;
                }
                if (percent < -10)
                {
                    return AppEnums.Decision.Close;
                }
                if (percent > -15)
                {
                    return AppEnums.Decision.Close;
                }
            }
            else
            {
                if (percent > 5)
                {
                    // What does 5% look like, what's the book value, (i.e., if 5% = $1,000) then we will NOT give back more than 1% of that 5%.
                    return AppEnums.Decision.Break_Even;
                    return AppEnums.Decision.Set_Least_Gain_2_Percent;
                }
                if (percent > 7)
                {
                    return AppEnums.Decision.Set_Least_Gain_4_Percent;
                }
                if (percent > 10)
                {
                    // allows room for fluctations
                    return AppEnums.Decision.Set_Least_Gain_6_Percent;
                }
                if (percent > 15)
                {
                    return AppEnums.Decision.Close;
                }
                // positive
            }

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
            return new AccountPositionsResponse
            {
                AccountId = accountId,
                Count = 1,
                AccountPositions = new List<AccountPosition> {
                    new AccountPosition {
                        CostBasis = 5.1572,
                        Description = "TSLA Apr 20 '18 $300 Call",
                        LongOrShort = AppEnums.LongOrShort.LONG,
                        Product = new Product
                        {
                            Symbol = "TSLA",
                            TypeCode = AppEnums.TypeCode.OPTN,
                            CallPut = AppEnums.OptionType.CALL,
                            StrikePrice = 300,
                            ExpirationYear = 2018,
                            ExpirationMonth = 4,
                            ExpirationDay = 20
                        },
                        Quantity = 1,
                        CurrentPrice = 5.1572,
                        MarketValue = 515.69
                    }
                }
            };
        }
    }
}
