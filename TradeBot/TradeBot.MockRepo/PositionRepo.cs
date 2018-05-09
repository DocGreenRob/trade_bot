using System;
using System.Collections.Generic;
using TradeBot.Models;
using TradeBot.Repo;
using TradeBot.Models.Enum;
using TradeBot.Models.Broker.ETrade;
using System.Linq;
using TradeBot.Models.Broker.ETrade.Analyzer;
using TradeBot.Utils.ExtensionMethods;
using static TradeBot.Models.Enum.AppEnums;

namespace TradeBot.MockRepo
{
    public class PositionRepo : IPositionRepo
    {
        public AccountPosition Change(AccountPosition accountPosition, Change change)
        {
            double currentOptionPrice = change.CallOptionPrice == 0 ? change.PutOptionPrice : change.CallOptionPrice;

            // Call/Put P/L % Open
            double percentOpen = Math.Round(((currentOptionPrice - accountPosition.CostBasis) / accountPosition.CostBasis) * 100, 2);

            // Relative Change
            // doubleRelativeChange = percentOpen - percentOpen.Previous()


            // Call/Put P/L $ Open
            double dollarsOpen = Math.Round((currentOptionPrice - accountPosition.CostBasis) * 100, 2);

            // Set
            accountPosition.CurrentPrice = currentOptionPrice;
            accountPosition.MarketValue = accountPosition.CurrentPrice * 100;

            return accountPosition;
        }

        public bool Close(Trade trade)
        {
            throw new NotImplementedException();
        }

        public Position CreateNewPosition(string underlying, OptionChainResponse optionChain, int numOfContracts, double currentPositionPrice, AppEnums.OptionType optionType)
        {
            double commission = 5.95;

            return new Position
            {
                PositionId = 1,
                InstrumentType = AppEnums.InstrumentType.Option,
                EntryTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 9, 30, 0),
                UnderlyingPriceAtEntry = currentPositionPrice,
                CostBasis = Models.MockModelDefaults.Default.CostBasis,
                Underlying = new Underlying
                {
                    Name = Models.MockModelDefaults.Default.RootSymbol
                },
                ProfitLossOpen = 0,
                OptionOrderResponse = new OptionOrderResponse
                {
                    AccountId = Models.MockModelDefaults.Default.AccountNumber,
                    AllOrNone = false,
                    EstimatedCommission = commission,
                    EstimatedTotalAmount = (Models.MockModelDefaults.Default.CostBasis * 100) + commission,
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
            double commission = 5.95;

            AccountPositionsResponse accountPositionsResponse = new AccountPositionsResponse { AccountId = accountId, Count = positions, AccountPositions = new List<AccountPosition>() };

            for (int i = 0; i < positions; i++)
            {
                double costBasis = (Models.MockModelDefaults.Default.Positions.ElementAt(i).OptionOrderResponse.EstimatedTotalAmount - commission) / 100;
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

        public double GetStockPrice(string symbol)
        {
            throw new NotImplementedException();
        }
    }
}
