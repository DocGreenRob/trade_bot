using System;
using System.Collections.Generic;

using TradeBot.Models;
using TradeBot.Models.Broker.ETrade;
using TradeBot.Models.Broker.ETrade.Analyzer;
using TradeBot.Models.Enum;
using TradeBot.Utils.Utils;

namespace TradeBot.Repo.Position
{
	public class ETradePositionRepo : IPositionRepo
	{
        public OptionChainResponse GetOptionChain(string underlying, AppEnums.OptionType chainType)
		{
			// Get Expiration Date
			DateTime expirationDate = Utils.Utils.Utils.GetExpirationDate();

			// https://developer.etrade.com/ctnt/dev-portal/getDetail?contentUri=V0_Documentation-MarketAPI-GetOptionChains
			// GET https://etws.etrade.com/market/rest/optionchains?expirationMonth=04&expirationYear=2011&chainType=PUT&skipAdjusted=true&underlier=GOOG
			string protocol = "https://";
			string uri = $"{protocol}etws.etrade.com/market/rest/optionchains?expirationMonth={expirationDate.Month}&expirationYear={expirationDate.Month}&chainType={chainType}&skipAdjusted=true&underlier={underlying}";

			throw new NotImplementedException();
		}

        public double GetOrderPrice(OptionChainResponse optionChain)
		{
			// https://etws.etrade.com/order/rest/previewoptionorder
			// https://developer.etrade.com/ctnt/dev-portal/getDetail?contentUri=V0_Documentation-OrderAPI-PreviewOptionOrder

			throw new NotImplementedException();
		}

		public double GetOptionBuyingPower()
		{
			// https://developer.etrade.com/ctnt/dev-portal/getDetail?contentUri=V0_Documentation-AccountsAPI-GetAccountBalance
			throw new NotImplementedException();
		}

        public Models.Position CreateNewPosition(string underlying, OptionChainResponse optionChain, int numOfContracts, double currentPositionPrice, AppEnums.OptionType optionType)
        {
            // https://developer.etrade.com/ctnt/dev-portal/getDetail?contentUri=V0_Documentation-OrderAPI-PlaceOptionOrder
            throw new NotImplementedException();
        }

        public AccountPositionsResponse GetPositions(int accountId)
        {
            throw new NotImplementedException();
        }

        public Trade Evaluate(Trade trade)
        {
            throw new NotImplementedException();
        }

        public AccountPosition Change(AccountPosition accountPosition, Change change)
        {
            throw new NotImplementedException();
        }
    }
}
