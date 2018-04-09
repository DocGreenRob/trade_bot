﻿using System.Collections.Generic;
using System;
using System.IO;

using Microsoft.Extensions.Configuration;

using TradeBot.Repo;
using TradeBot.Models;
using static TradeBot.Utils.Enum.AppEnums;
using TradeBot.Utils.ExtensionMethods;


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
			
			// 1. Need to Authenticate with Broker
		}

		public void OpenPosition(string underlying, PositionType positionType, TradeStrength tradeStrength)
        {
			DateTime expirationDate;
			List<Option> optionChain;

			// Create new option trade, this should:
			// 1. Check the current price of the Underlying (Get Option Chain)
			
			// positionPrice = THe price of the Position (the Trade).
			double positionPrice = GetCurrentPrice(underlying, positionType, out expirationDate, out optionChain);

			// 2. Check account value (to determine how many contracts to buy)
			int contracts = DetermineNumberOfContracts(positionPrice, tradeStrength);

			if(contracts > 0)
			{
				// 3. Place trade
				Position position = CreateNewPosition(underlying, optionChain, contracts);

				// 4. Return results of the above (order number, position number, position id)
			}

		}

		private double GetCurrentPrice(string underlying, PositionType positionType, out DateTime expirationDate, out List<Option> optionChain)
		{
			expirationDate = Utils.Utils.Utils.GetExpirationDate();

			// 1. Get the option chain for the underlying
			if(positionType == PositionType.Strangle)
			{
				// 1. need to know price of underlying so I can
				optionChain = _positionRepo.GetOptionChain(underlying, OptionType.CALLPUT);

				// Get the price of the order
				return _positionRepo.GetOrderPrice(optionChain);
			}

			throw new NotImplementedException();
		}

		private int DetermineNumberOfContracts(double positionPrice, TradeStrength tradeStrength)
		{
			// 1. Get account value
			double optionBuyingPower = _positionRepo.GetOptionBuyingPower();

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


			throw new NotImplementedException();
		}

	}
}
