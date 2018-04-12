using System;
using System.Collections.Generic;
using System.Text;
using TradeBot.Models.Interfaces;
using static TradeBot.Utils.Enum.AppEnums;

namespace TradeBot.Models
{
	public class Option : Instrument
	{
		public double StrikePrice { get; set; }
		public double Bid { get; set; }
		public double Ask { get; set; }
		public double Last { get; set; }
		public double OpenInterest { get; set; }
		public int DaysToExpiration { get; set; }
		public string Name { get; set; }
		public OptionType OptionType { get; set; }
		public OrderAction OrderAction { get; set; }
	}
}
