using System;
using System.Collections.Generic;
using System.Text;
using static TradeBot.Models.Enum.AppEnums;

namespace TradeBot.Models.Broker.ETrade
{
    /// <summary>
    /// https://developer.etrade.com/ctnt/dev-portal/getDetail?contentUri=V0_Documentation-MarketAPI-GetOptionChains
    /// </summary>
    public class Option
    {
        /// <summary>
        /// Gets or sets the type of the option. (i.e., CALL or PUT)
        /// </summary>
        /// <value>
        /// The type of the option.
        /// </value>
        public OptionType OptionType { get; set; }
        /// <summary>
        /// Gets or sets the root symbol. (i.e., GOOG)
        /// </summary>
        /// <value>
        /// The root symbol.
        /// </value>
        public string RootSymbol { get; set; }

        public DateTime ExpirationDate { get; set; }

        /// <summary>
        /// Gets or sets the type of the expiration. (i.e., MONTHLY)
        /// </summary>
        /// <value>
        /// The type of the expiration.
        /// </value>
        public ExpirationType ExpirationType { get; set; }

        public Product Product { get; set; }
        public double StrikePrice { get; set; }

        public double Bid { get; set; }
        public double Ask { get; set; }
        public double Last { get; set; }
        public double OpenInterest { get; set; }
        public int DaysToExpiration { get; set; }
        public string Name { get; set; }

        public OrderAction OrderAction { get; set; }
    }
}
