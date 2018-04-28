using System.Collections.Generic;
using System;
using System.IO;

using Microsoft.Extensions.Configuration;

using TradeBot.Repo;
using TradeBot.Models;
using static TradeBot.Utils.Enum.AppEnums;
using TradeBot.Utils.ExtensionMethods;
using TradeBot.Utils.Enum;
using TradeBot.Models.Broker.ETrade;
using Microsoft.Extensions.Logging;

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
            double currentPositionPrice = GetCurrentPrice(underlying, positionType, out expirationDate, out optionChain);

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

        private double GetCurrentPrice(string underlying, PositionType positionType, out DateTime expirationDate, out OptionChainResponse optionChain)
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

			throw new NotImplementedException();
		}

        public AccountPosition Change(AccountPosition accountPosition, TradeDirection tradeDirection, double changeAmount)
        {
            return _positionRepo.Change(accountPosition, tradeDirection, changeAmount);
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

        public Decision Evaluate(AccountPosition adjustedAccountPosition)
        {
            return _positionRepo.Evaluate(adjustedAccountPosition);
        }

        private Position CreateNewPosition(string underlying, OptionChainResponse optionChain, int numOfContracts, double currentPositionPrice, AppEnums.OptionType optionType)
        {
            return _positionRepo.CreateNewPosition(underlying, optionChain, numOfContracts, currentPositionPrice, optionType);
        }
    }
}
