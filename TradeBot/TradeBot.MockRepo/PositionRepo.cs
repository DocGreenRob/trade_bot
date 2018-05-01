using System;
using System.Collections.Generic;
using TradeBot.Models;
using TradeBot.Repo;
using TradeBot.Utils.Enum;
using TradeBot.Models.Broker.ETrade;
using System.Linq;

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

        public Position CreateNewPosition(string underlying, OptionChainResponse optionChain, int numOfContracts, double currentPositionPrice, AppEnums.OptionType optionType)
        {
            return new Position
            {
                PositionId = 1,
                InstrumentType = AppEnums.InstrumentType.Option,
                EntryTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 9,30,0),
                UnderlyingPriceAtEntry = currentPositionPrice,
                Underlying = new Underlying
                {
                    Name = Models.MockModelDefaults.Default.RootSymbol
                },
                ProfitLossOpen = 0,
                OptionOrderResponse = new OptionOrderResponse
                {
                    AccountId = Models.MockModelDefaults.Default.AccountNumber,
                    AllOrNone = false,
                    EstimatedCommission = 5.95,
                    EstimatedTotalAmount = Models.MockModelDefaults.Default.CostBasis * 100,
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
                        OptionType = optionType,
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
            if (percent < 0)
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
            AccountBalanceResponse accountBalanceResponse = new AccountBalanceResponse
            {
                AccountId = Models.MockModelDefaults.Default.AccountNumber,
                AccountType = AppEnums.AccountType.MARGIN,
                OptionLevel = AppEnums.OptionLevel.LEVEL_4,
                AccountBalance = new AccountBalance
                {
                    CashAvailableForWithdrawal = 1000,
                    CashCall = 0,
                    FundsWithheldFromPurchasePower = 0,
                    FundsWithheldFromWithdrawal = 0,
                    NetAccountValue = 1000,
                    NetCash = 1000,
                    SweepDepositAmount = 0,
                    TotalLongValue = 0,
                    TotalSecuritiesMktValue = 0,
                    TotalCash = 1000
                },
                MarginAccountBalance = new MarginAccountBalance
                {
                    FedCall = 0,
                    MarginBalance = 1000,
                    MarginBalanceWithdrawal = 1000,
                    MarginEquity = 1000,
                    MarginEquityPct = 0,
                    MarginableSecurities = 2000,
                    MaxAvailableForWithdrawal = 1000,
                    MinEquityCall = 2500,
                    NonMarginableSecuritiesAndOptions = 1000,
                    TotalShortValue = 0,
                    ShortReserve = 0
                }
            };

            return accountBalanceResponse.AccountBalance.NetCash;
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
                                RootSymbol = Models.MockModelDefaults.Default.RootSymbol,
                                ExpirationDate = new DateTime(Models.MockModelDefaults.Default.ExpirationYear, Models.MockModelDefaults.Default.ExpirationMonth, Models.MockModelDefaults.Default.ExpirationDay),
                                ExpirationType = AppEnums.ExpirationType.MONTHLY,
                                Product = new Product
                                {
                                    ExchangeCode = AppEnums.ExchangeCode.CINC,
                                    Symbol = Models.MockModelDefaults.Default.SymbolName,
                                    TypeCode = AppEnums.TypeCode.OPTN
                                },
                                StrikePrice = Models.MockModelDefaults.Default.StrikePrice
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
                                RootSymbol = Models.MockModelDefaults.Default.RootSymbol,
                                ExpirationDate = new DateTime(Models.MockModelDefaults.Default.ExpirationYear, Models.MockModelDefaults.Default.ExpirationMonth, Models.MockModelDefaults.Default.ExpirationDay),
                                ExpirationType = AppEnums.ExpirationType.MONTHLY,
                                Product = new Product
                                {
                                    ExchangeCode = AppEnums.ExchangeCode.CINC,
                                    Symbol = Models.MockModelDefaults.Default.RootSymbol,
                                    TypeCode = AppEnums.TypeCode.OPTN
                                },
                                StrikePrice = Models.MockModelDefaults.Default.StrikePrice
                            }
                        }
                    }
                }
            };
        }

        public double GetOrderPrice(OptionChainResponse optionChain)
        {
            OptionOrderResponse optionOrderResponse = new OptionOrderResponse
            {
                AccountId = Models.MockModelDefaults.Default.AccountNumber,
                AllOrNone = false,
                EstimatedCommission = 5.95,
                EstimatedTotalAmount = 521.64,
                PreviewTime = DateTime.Now.AddSeconds(-5),
                PreviewId = 123,
                Quantity = 1,
                ReserveOrder = false,
                ReserveQuantity = 0,
                OrderTerm = AppEnums.OrderTerm.GOOD_FOR_DAY,
                OptionSymbol = new OptionSymbol
                {
                    Symbol = Models.MockModelDefaults.Default.RootSymbol,
                    OptionType = AppEnums.OptionType.CALL,
                    ExpirationYear = Models.MockModelDefaults.Default.ExpirationYear,
                    ExpirationMonth = Models.MockModelDefaults.Default.ExpirationMonth,
                    ExpirationDay = Models.MockModelDefaults.Default.ExpirationDay
                },
                OrderAction = AppEnums.OrderAction.BUY_TO_OPEN,
                PriceType = AppEnums.PriceType.MARKET
            };

            return optionOrderResponse.EstimatedTotalAmount;
        }

        public AccountPositionsResponse GetPositions(int accountId)  // ok
        {
            // determine number of positions to open
            int positions = Models.MockModelDefaults.Default.Positions.Count;

            AccountPositionsResponse accountPositionsResponse = new AccountPositionsResponse { AccountId = accountId, Count = positions, AccountPositions = new List<AccountPosition>() };

            for (int i = 0; i < positions; i++)
            {
                double costBasis = Models.MockModelDefaults.Default.Positions.ElementAt(i).OptionOrderResponse.EstimatedTotalAmount / 100;
                AccountPosition accountPosition = new AccountPosition
                {
                    CostBasis = costBasis,
                    Description = Models.MockModelDefaults.Default.Positions.ElementAt(i).Description,
                    LongOrShort = Models.MockModelDefaults.Default.Positions.ElementAt(i).OptionOrderResponse.OptionSymbol.OptionType == AppEnums.OptionType.CALL ? AppEnums.LongOrShort.LONG : AppEnums.LongOrShort.SHORT,
                    Product = new Product
                    {
                        Symbol = Models.MockModelDefaults.Default.Positions.ElementAt(i).OptionOrderResponse.OptionSymbol.Symbol,
                        TypeCode = AppEnums.TypeCode.OPTN,
                        CallPut = Models.MockModelDefaults.Default.Positions.ElementAt(i).OptionOrderResponse.OptionSymbol.OptionType,
                        StrikePrice = Models.MockModelDefaults.Default.Positions.ElementAt(i).OptionOrderResponse.OptionSymbol.StrikePrice,
                        ExpirationYear = Models.MockModelDefaults.Default.Positions.ElementAt(i).OptionOrderResponse.OptionSymbol.ExpirationYear,
                        ExpirationMonth = Models.MockModelDefaults.Default.Positions.ElementAt(i).OptionOrderResponse.OptionSymbol.ExpirationMonth,
                        ExpirationDay = Models.MockModelDefaults.Default.Positions.ElementAt(i).OptionOrderResponse.OptionSymbol.ExpirationDay
                    },
                    Quantity = 1,
                    CurrentPrice = costBasis,
                    MarketValue = costBasis * 100
                };

                accountPositionsResponse.AccountPositions.Add(accountPosition);
            }
            return accountPositionsResponse;
        }
    }
}
