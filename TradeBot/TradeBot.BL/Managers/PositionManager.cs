using System.Collections.Generic;
using System;
using System.IO;

using Microsoft.Extensions.Configuration;

using TradeBot.Repo;
using TradeBot.Models;
using static TradeBot.Models.Enum.AppEnums;
using TradeBot.Utils.ExtensionMethods;
using TradeBot.Models.Enum;
using TradeBot.Models.Broker.ETrade;
using Microsoft.Extensions.Logging;
using System.Linq;
using TradeBot.Models.Broker.ETrade.Analyzer;
using TradeBot.Utils.Const;

namespace TradeBot.BL.Managers
{
    public class PositionManager
    {
        private IPositionRepo _positionRepo;

        private Broker broker;


        public PositionManager(IPositionRepo positionRepo, Broker broker)
        {
            this.broker = broker;

            if (positionRepo == null)
                throw new Exception("Repo cannot be null!");

            _positionRepo = positionRepo;

            var factory = new LoggerFactory();
            var logger = factory.CreateLogger("MyLog");
            logger.LogError(5, "Here is a test!!", "Here is a test");

            // 1. Need to Authenticate with Broker
        }

        public Position OpenPosition(string underlying, PositionType positionType, TradeStrength tradeStrength, OptionType optionType)
        {
            DateTime expirationDate;
            OptionChainResponse optionChain;

            // Create new option trade, this should:
            // 1. Check the current price of the Underlying (Get Option Chain)

            // currentPositionPrice = THe price of the Position (the Trade).
            double currentPositionPrice = GetCurrentPrice(underlying, positionType, optionType, out expirationDate, out optionChain);

            // 2. Check account value (to determine how many contracts to buy)
            int numOfContracts = DetermineNumberOfContracts(currentPositionPrice, tradeStrength);

            if (numOfContracts > 0)
            {
                // 3. Place trade &
                // 4. Return results of the above (order number, position number, position id)
                return CreateNewPosition(underlying, optionChain, numOfContracts, currentPositionPrice, optionType);
            }

            throw new Exception("Don't have enough funds to open a position of this size.");
        }

        public AccountPositionsResponse GetPositions(int accountId)
        {
            return _positionRepo.GetPositions(accountId);
        }

        private double GetCurrentPrice(string underlying, PositionType positionType, OptionType optionType, out DateTime expirationDate, out OptionChainResponse optionChain)
        {
            expirationDate = Utils.Utils.Utils.GetExpirationDate();

            // 1. Get the option chain for the underlying
            if (positionType == PositionType.Strangle)
            {
                // 1. need to know price of underlying so I can
                optionChain = _positionRepo.GetOptionChain(underlying, OptionType.CALLPUT);

                // Get the price of the order
                return _positionRepo.GetOrderPrice(optionChain);
            }

            if (positionType == PositionType.Naked)
            {
                // 1. need to know price of underlying so I can
                optionChain = _positionRepo.GetOptionChain(underlying, optionType);

                // Get the price of the order
                return _positionRepo.GetOrderPrice(optionChain);
            }

            throw new NotImplementedException();
        }

        private int DetermineNumberOfContracts(double positionPrice, TradeStrength tradeStrength) // ok
        {
            // 1. Get account value
            double optionBuyingPower = _positionRepo.GetOptionBuyingPower(); // ok

            // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?tabs=basicconfiguration

            // 2. If Account value > 
            double maxPosition = 0;
            if (optionBuyingPower <= 250)
            {
                maxPosition = 250;
            }
            if (optionBuyingPower <= 500)
            {
                maxPosition = 500;
            }
            if (optionBuyingPower <= 1000)
            {
                maxPosition = 1000;
            }
            if (optionBuyingPower <= 2000)
            {
                maxPosition = 2000;
            }
            if (optionBuyingPower <= 5000)
            {
                maxPosition = 5000;
            }
            if (optionBuyingPower <= 10000)
            {
                maxPosition = 10000;
            }
            if (optionBuyingPower > 10000)
            {
                maxPosition = 10000;
            }

            return (maxPosition / positionPrice).ToGetBase();
        }

        public Trade Evaluate(Trade trade)
        {
            // If I was previously MicroWatch[ing] then remove that Flag and process as normal. 
            // If we need to re-Flag it for MicroWatch, let the new logic handle that.
            if (trade.Flags.Contains(Flag.Micro_Watch))
            {
                trade.Flags.Remove(trade.Flags.Where(a => a.Equals(Flag.Micro_Watch)).FirstOrDefault());
            }

            // Evaluate the currentPositionPrice versus the costBasis and see the difference in percent

            // CaptureData()

            // Naked Trade
            if (trade.PositionType() == PositionType.Naked)
            {
                AccountPosition position = trade.BehaviorChanges.Keys.ElementAt(0).PositionBehavior.AccountPosition;
                double changeInDollars = Math.Round(position.CurrentPrice - position.CostBasis, 2);
                double percentAsDecimal = Math.Round(changeInDollars / position.CostBasis, 2);
                double percent = Math.Round(percentAsDecimal * 100, 2);

                // Add the evaluation to the history
                History.Add(position, changeInDollars, percent);

                // negative
                if (percent < 0)
                {
                    if (percent > -5)
                    {
                        trade.Decision = AppEnums.Decision.Wait;
                    }
                    if (percent < -5)
                    {

                        trade.Decision = AppEnums.Decision.Wait;
                    }
                    if (percent < -7)
                    {
                        trade.Decision = AppEnums.Decision.Start_To_Worry;
                    }
                    if (percent >= -7)
                    {
                        trade.Decision = AppEnums.Decision.Investigate;
                    }
                    if (percent < -10)
                    {
                        trade.Decision = AppEnums.Decision.Close;
                    }
                    if (percent > -15)
                    {
                        trade.Decision = AppEnums.Decision.Close;
                    }
                }
                else
                {
                    if (percent < 5)
                    {
                        trade.Decision = AppEnums.Decision.Wait;
                    }
                    if (percent > 5)
                    {
                        // What does 5% look like, what's the book value, (i.e., if 5% = $1,000) then we will NOT give back more than 1% of that 5%.
                        trade.Decision = AppEnums.Decision.Break_Even;
                        trade.Decision = AppEnums.Decision.Set_Least_Gain_2_Percent;
                    }
                    if (percent > 7)
                    {
                        trade.Decision = AppEnums.Decision.Set_Least_Gain_4_Percent;
                    }
                    if (percent > 10)
                    {
                        // allows room for fluctations
                        trade.Decision = AppEnums.Decision.Set_Least_Gain_6_Percent;
                    }
                    if (percent > 15)
                    {
                        trade.Decision = AppEnums.Decision.Close;
                    }
                    // positive
                }

                return trade;
            }

            if (trade.PositionType() == PositionType.Strangle)
            {
                // Strangle
                if (trade.Call() != null && trade.Put() != null)
                {
                    // Less than or equal to 3rd minute of trading day (9:33:00 AM EST)
                    if (trade.Time.First3Minutes())
                    {
                        if (trade.MaxLossAlert())
                        {
                            if (Close(trade))
                            {
                                trade.Reset();

                                return AssignDecision(trade, Decision.Close);
                            }
                        }
                        else
                        {
                            trade.Default();
                            Trade _trade = AssignDecision(trade, Decision.Wait);
                            return _trade;
                        }
                    }
                    else
                    {
                        if (trade.MaxLossAlert())
                        {
                            if (!trade.Flags.Any(Flag.Max_Loss_Percent_Triggered))
                            {
                                trade.Decision = Decision.Close_If_Worse;
                                trade.Flags.AddRange(new List<Flag> { Flag.Max_Loss_Percent_Triggered, Flag.Micro_Watch });
                                Microwatch(trade);
                                trade.MaxLossPercent = trade.Sum_Change.Last().PriceActionBehavior.PnL.Percent;
                                trade.MaxLossDollars = trade.Sum_Change.Last().PriceActionBehavior.PnL.Dollars;
                                return trade;
                            }
                            else
                            {
                                // if trade = maxLossPercent
                                if ((trade.PnL().Percent == trade.MaxLossPercent) &&
                                    !trade.Flags.Contains(Flag.Close_At_10_Percent))
                                {
                                    trade = AssignDecision(trade, Decision.Close_If_Worse);

                                    // here we don't add the flag Flag.Max_Loss_Percent_Triggered because we already have it
                                    trade.Flags.AddRange(new List<Flag> { Flag.Micro_Watch });
                                    Microwatch(trade);
                                    return trade;
                                }

                                // if trade < maxLossPercent OR Flag.Close_At_10_Percent then EXIT
                                if ((trade.PnL().Percent < trade.MaxLossPercent) ||
                                    trade.Flags.Contains(Flag.Close_At_10_Percent))
                                {
                                    if (Close(trade))
                                    {
                                        trade.Reset();
                                        return AssignDecision(trade, Decision.Close);
                                    }
                                }
                            }

                            // # 1
                            // if trade < 0 (in the red)
                            if (trade.PnL().Percent < 0)
                            {
                                if (trade.Flags.Contains(Flag.Max_Loss_Percent_Triggered))
                                {
                                    trade = AssignDecision(trade, Decision.Wait);

                                    // add flag
                                    if (trade.Flags.Add(Flag.Close_At_10_Percent, null))
                                    {
                                        return trade;
                                    }
                                    else
                                    {
                                        // TODO: Start here : 5.15.18
                                    }
                                } 
                            }
                            else // in the green
                            {
                                // Now we are in the green.  Now we hope to be able to monitor the trade and capture either (1) by closing the whole trade or (2) closing the profiting side of the trade.
                                // Because of this, here is the core logic used to determine the decisions while monitoring a profit???


                                // from "[x.1]" (the below "if" statement)
                                if (trade.Flags.Contains(Flag.Max_Loss_Percent_Triggered))
                                {
                                    // TODO: Start here : 5.14.18
                                }
                                else
                                {
                                    // TODO: Start here : 5.15.18
                                }

                                if (trade.Flags.Contains(Flag.Inspect_Stoch_15_Mins) // trade
                                    || trade.BehaviorChanges.Keys.Select(k => k.PositionBehavior).Any(b => b.Flags.Contains(Flag.Inspect_Stoch_15_Mins)) // call
                                    || trade.BehaviorChanges.Values.Select(k => k.PositionBehavior).Any(b => b.Flags.Contains(Flag.Inspect_Stoch_15_Mins))) // put
                                {
                                    trade = InspectStudy(Study.Stochastics, Interval.Min_15, trade);
                                }

                                if (trade.Flags.Contains(Flag.Red_Alert_15_SMA) // trade
                                    || trade.BehaviorChanges.Keys.Select(k => k.PositionBehavior).Any(b => b.Flags.Contains(Flag.Red_Alert_15_SMA)) // call
                                    || trade.BehaviorChanges.Values.Select(k => k.PositionBehavior).Any(b => b.Flags.Contains(Flag.Red_Alert_15_SMA))) // put
                                {
                                    trade = InspectPriceToEMA(EMALength._30, Interval.Min_15, trade);
                                }
                            }
                        }
                    }
                }
            }

            return trade;
            //throw new Exception("Something went wrong!");
        }

        private Trade InspectPriceToEMA(EMALength eMALength, Interval interval, Trade trade)
        {
            double ema = GetEMA(eMALength, interval);
            Underlying underlying = GetLastSession(Interval.Min_1);
            
            if (underlying.High >= ema)
            {
                return AssignDecision(trade, Decision.Close);
            }
            else
            {
                // Since this is a non actionable event for the trade, then render no decision - "Null"
                return AssignDecision(trade, Decision.Null);
            }
                
            throw new NotImplementedException();
        }

        private double GetEMA(EMALength eMALength, Interval interval)
        {
            // TODO: Start here : 5.15.18
            throw new NotImplementedException();
        }

        private Underlying GetLastSession(Interval min_1)
        {
            // TODO: Start here : 5.15.18
            throw new NotImplementedException();
        }

        private Trade InspectStudy(Study study, Interval interval, Trade trade)
        {
            // TODO: Start here : 5.15.18
            // Inspect 
            // 1. Trade
            // 2. Call
            // 3. Put
            throw new NotImplementedException();
        }

        private Trade AssignDecision(Trade trade, Decision decision, bool deep = true)
        {
            trade.Decision = decision;

            if (deep)
            {
                trade.Call().PositionBehavior.Decision = decision;
                trade.Put().PositionBehavior.Decision = decision;
            }

            return trade;
        }

        private bool Close(Trade trade)
        {
            return _positionRepo.Close(trade);
        }

        // TODO: Need to adjust for put or call.  Right now I only check 1 direction (if the stock price gets lower than the last check)
        // TODO: I think that I may need to set the direction or something like that.
        // TODO: 5.8.18
        // Need to know the impact of Price Action Behavior (Stock Price) on my PnL.Percent.
        // If the stock going UP has caused me to flag for MicroWatch, then if the Stock Price goes further  UP during a MicroWatch, Close()
        // conversely, if the stock going DOWN has caused me to flag for MicroWatch, then if the Stock Price goes further DOWN during a MicroWatch, Close()
        private void Microwatch(Trade trade)
        {
            if (trade.Flags.Contains(Flag.Micro_Watch))
            {
                if (trade.Decision == Decision.New_Request)
                    Close(trade);
                else
                {
                    trade = GetBias(trade);
                    if (trade.Sum_Change.LastOrDefault().Bias == Bias.Bullish)
                    {
                        if (trade.GetStockPrice() < trade.GetStockPrice(true))
                        {
                            Close(trade);
                        }
                    }

                    if (trade.Sum_Change.LastOrDefault().Bias == Bias.Bearish)
                    {
                        if (trade.GetStockPrice() > trade.GetStockPrice(true))
                        {
                            Close(trade);
                        }
                    }

                    System.Threading.Thread.Sleep(Constants.Microwatch_Trigger);

                    // Get current stock price
                    double currentStockPrice = _positionRepo.GetStockPrice(trade.GetUnderlying());

                    // create change object
                    trade.Sum_Change.Add(new TradeBehaviorChange
                    {
                        PositionBehavior = new PositionBehavior
                        {
                            Change = new Change
                            {
                                DateTime = DateTime.Now,
                                StockPrice = currentStockPrice
                            }
                        }
                    });

                    Microwatch(trade);
                }

                // call Microwatch
            }
            throw new NotImplementedException("Implement");
        }
        private Position CreateNewPosition(string underlying, OptionChainResponse optionChain, int numOfContracts, double currentPositionPrice, AppEnums.OptionType optionType)
        {
            return _positionRepo.CreateNewPosition(underlying, optionChain, numOfContracts, currentPositionPrice, optionType);
        }

        public AccountPosition Change(AccountPosition accountPosition, Change change)
        {
            return _positionRepo.Change(accountPosition, change);
        }

        public Trade GetBias(Trade trade)
        {
            if (trade.Sum_Change.Count == 0)
            {
                trade.Sum_Change.ElementAt(trade.Sum_Change.Count - 1).Bias = Bias.Null;
                return trade;
            }

            if (trade.PositionType() == PositionType.Strangle)
            {
                int count = trade.Sum_Change.Count - 1;

                // let the last one
                double currentProfitPercent = trade.Sum_Change.ElementAt(count).PriceActionBehavior.PnL.Percent;
                double currentStockPrice = trade.Sum_Change.ElementAt(count).PositionBehavior.Change.StockPrice;
                double currentProfitDollars = trade.Sum_Change.ElementAt(count).PriceActionBehavior.PnL.Dollars;
                double priorProfitPercent = 0;
                double priorStockPrice = 0;
                double priorProfitDollars = 0;
                Bias lastBias = Bias.Null;

                if (trade.Sum_Change.Count != 1)
                {
                    // get the one before the last one
                    count -= 1;
                    priorProfitPercent = trade.Sum_Change.ElementAt(count).PriceActionBehavior.PnL.Percent;
                    priorStockPrice = trade.Sum_Change.ElementAt(count).PositionBehavior.Change.StockPrice;
                    priorProfitDollars = trade.Sum_Change.ElementAt(count).PriceActionBehavior.PnL.Dollars;
                    lastBias = trade.Sum_Change.ElementAt(count).Bias;
                }

                if ((currentStockPrice < priorStockPrice) &&
                    (currentProfitDollars < priorProfitDollars)
                    )
                {
                    trade.Sum_Change.LastOrDefault().Bias = Bias.Bullish;
                }

                // opposite ^
                if ((currentStockPrice > priorStockPrice) &&
                    (currentProfitDollars > priorProfitDollars)
                    )
                {
                    trade.Sum_Change.LastOrDefault().Bias = Bias.Bullish;
                }

                if ((currentStockPrice < priorStockPrice) &&
                    (currentProfitDollars > priorProfitDollars)
                    )
                {
                    trade.Sum_Change.LastOrDefault().Bias = Bias.Bearish;
                }

                // if currentStockPrice > priorStockPrice && currentProfit < priorProfit then BEARISH
                if ((currentStockPrice > priorStockPrice) &&
                    currentProfitDollars < priorProfitDollars)
                {
                    trade.Sum_Change.LastOrDefault().Bias = Bias.Bearish;
                }

                // if currentStockPrice == priorStockPrice && currentProfit < priorProfit then LAST
                if (currentStockPrice == priorStockPrice)
                //if ((currentStockPrice == priorStockPrice) &&
                //    currentProfitDollars < priorProfitDollars)
                {
                    trade.Sum_Change.LastOrDefault().Bias = lastBias;
                }

                // if last Bias was bullish
            }

            return trade;
        }
    }
}
