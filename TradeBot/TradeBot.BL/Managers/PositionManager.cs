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
			if(optionBuyingPower <= 250)
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
            // Evaluate the currentPositionPrice versus the costBasis and see the difference in percent

            // CaptureData()

            // Naked Trade
            if (trade.Positions.Count == 1)
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

            if (trade.Positions.Count == 2)
            {
                // Strangle
                if (trade.Call() != null && trade.Put() != null)
                {
                    if (trade.Time.TradeMinutes() <= 3)
                    {
                        trade.Decision = AppEnums.Decision.Wait;
                        trade.Flags = new List<Flag>();
                        trade.Call().PositionBehavior.Flags = new List<Flag>();
                        trade.Put().PositionBehavior.Flags = new List<Flag>();

                        trade.Call().PositionBehavior.Decision = Decision.Null;
                        trade.Put().PositionBehavior.Decision = Decision.Null;

                        return trade;
                    }
                }
            }

            throw new Exception("Something went wrong!");
        }

        private Position CreateNewPosition(string underlying, OptionChainResponse optionChain, int numOfContracts, double currentPositionPrice, AppEnums.OptionType optionType)
        {
            return _positionRepo.CreateNewPosition(underlying, optionChain, numOfContracts, currentPositionPrice, optionType);
        }

        public AccountPosition Change(AccountPosition accountPosition, Change change)
        {
            return _positionRepo.Change(accountPosition, change);
        }
    }
}
