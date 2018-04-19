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

        public Position CreateNewPosition(string underlying, OptionChainResponse optionChain, int numOfContracts, double currentPositionPrice)
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
                        Symbol = Models.MockModelDefaults.Default.RootSymbol,
                        OptionType = AppEnums.OptionType.CALL,
                        StrikePrice = Models.MockModelDefaults.Default.StrikePrice,
                        ExpirationYear = Models.MockModelDefaults.Default.ExpirationYear,
                        ExpirationMonth = Models.MockModelDefaults.Default.ExpirationMonth,
                        ExpirationDay = Models.MockModelDefaults.Default.ExpirationDay
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
                if (percent < 5)
                {
                    return AppEnums.Decision.Wait;
                }
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

            throw new Exception("Something went wrong!");

        }

        public double GetOptionBuyingPower()
        {
            throw new System.NotImplementedException();
        }

        public OptionChainResponse GetOptionChain(string underlying, AppEnums.OptionType chainType)
        {
            return new OptionChainResponse
            {
                OptionPairCount = 2,
                OptionPairs = new List<OptionPairCount>
                {
                    new OptionPairCount
                    {
                        OptionPairs = new List<Models.Broker.ETrade.Option>
                        {
                            new Models.Broker.ETrade.Option
                            {
                                OptionType = AppEnums.OptionType.CALL,
                                RootSymbol = "TSLA",
                                ExpirationDate = new DateTime(Models.MockModelDefaults.Date.Year, Models.MockModelDefaults.Date.Month, Models.MockModelDefaults.Date.Day),
                                ExpirationType = AppEnums.ExpirationType.MONTHLY,
                                Product = new Product
                                {
                                    ExchangeCode = AppEnums.ExchangeCode.CINC,
                                    Symbol = Models.MockModelDefaults.Symbol.Name,
                                    TypeCode = AppEnums.TypeCode.OPTN
                                },
                                StrikePrice = Models.MockModelDefaults.StrikePrice.Price
                            }
                        }
                    },
                    new OptionPairCount
                    {
                        OptionPairs = new List<Models.Broker.ETrade.Option>
                        {
                            new Models.Broker.ETrade.Option
                            {
                                OptionType = AppEnums.OptionType.PUT,
                                RootSymbol = "TSLA",
                                ExpirationDate = new DateTime(Models.MockModelDefaults.Date.Year, Models.MockModelDefaults.Date.Month, Models.MockModelDefaults.Date.Day),
                                ExpirationType = AppEnums.ExpirationType.MONTHLY,
                                Product = new Product
                                {
                                    ExchangeCode = AppEnums.ExchangeCode.CINC,
                                    Symbol = Models.MockModelDefaults.Symbol.Name,
                                    TypeCode = AppEnums.TypeCode.OPTN
                                },
                                StrikePrice = Models.MockModelDefaults.StrikePrice.Price
                            }
                        }
                    }
                }
            };
        }

        public double GetOrderPrice(OptionChainResponse optionChain)
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
                        Description = Models.MockModelDefaults.Symbol.Name,
                        LongOrShort = AppEnums.LongOrShort.LONG,
                        Product = new Product
                        {
                            Symbol = "TSLA",
                            TypeCode = AppEnums.TypeCode.OPTN,
                            CallPut = AppEnums.OptionType.CALL,
                            StrikePrice = Models.MockModelDefaults.StrikePrice.Price,
                            ExpirationYear = Models.MockModelDefaults.Date.Year,
                            ExpirationMonth = Models.MockModelDefaults.Date.Month,
                            ExpirationDay = Models.MockModelDefaults.Date.Day
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
