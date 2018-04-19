using System;
using System.Collections.Generic;
using System.Text;
using TradeBot.Models.Interfaces;
using static TradeBot.Utils.Enum.AppEnums;

namespace TradeBot.Models
{
    public class Option : Instrument
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
