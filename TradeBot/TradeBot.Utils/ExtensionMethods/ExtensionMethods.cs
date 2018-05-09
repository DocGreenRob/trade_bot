using System;
using System.Collections.Generic;
using System.Linq;
using TradeBot.Models;
using TradeBot.Models.Broker.ETrade.Analyzer;
using TradeBot.Models.Enum;
using TradeBot.Utils.ExtensionMethods;
using static TradeBot.Models.Enum.AppEnums;

namespace TradeBot.Utils.ExtensionMethods
{
	public static class ExtensionMethods
	{
		public static int ToGetBase(this double value)
		{
			if (value.ToString().Contains("."))
			{
				return int.Parse(value.ToString().Split('.')[0]);
			}
			else
			{
				return int.Parse(value.ToString());
			}

		}

		public static string ToMonthAbrv(this int month)
		{
			switch (month)
			{
				case 1:
					return "Jan";
					break;
				case 2:
					return "Feb";
					break;
				case 3:
					return "Mar";
					break;
				case 4:
					return "Apr";
					break;
				case 5:
					return "May";
					break;
				case 6:
					return "Jun";
					break;
				case 7:
					return "Jul";
					break;
				case 8:
					return "Aug";
					break;
				case 9:
					return "Sep";
					break;
				case 10:
					return "Oct";
					break;
				case 11:
					return "Nov";
					break;
				case 12:
					return "Dec";
					break;
			}
			throw new Exception("Invalid Month!");
		}

		public static string ToYearAbrv(this int year)
		{
			return $"'{year.ToString().Substring(2)}";
		}

        /// <summary>
        /// Gets the Trades the minutes. (the number of minutes from 9:30)
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        public static int TradeMinutes(this DateTime dateTime)
        {
            DateTime today = DateTime.Today;
            // Get's the minutes from 9:30 
            TimeSpan difference = dateTime.TimeOfDay - new DateTime(today.Year, today.Month, today.Day, 9, 30, 0).TimeOfDay;
            return difference.Minutes;
        }

        public static bool First3Minutes(this DateTime dateTime)
        {
            return TradeMinutes(dateTime) <= 3;
        }

        /// <summary>
        /// Determines whether [is fifteenth minute].
        /// </summary>
        /// <param name="minute">The minute.</param>
        /// <returns>
        ///   <c>true</c> if [is fifteenth minute] [the specified minute]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsFifteenthMinute(this int minute)
        {
            return minute % 15 == 0;
        }

        public static Position Call(this Trade trade)
        {
            return trade.Positions.Where(p => p.OptionOrderResponse.OptionSymbol.OptionType == OptionType.CALL).FirstOrDefault();
        }

        public static Position Put(this Trade trade)
        {
            return trade.Positions.Where(p => p.OptionOrderResponse.OptionSymbol.OptionType == OptionType.PUT).FirstOrDefault();
        }

        public static bool MaxLossAlert(this Trade trade)
        {
            double maxLoss = -.125 * 100;
            return trade.Sum_Change.LastOrDefault().PriceActionBehavior.PnL.Percent < maxLoss;
        }

        public static Trade Default(this Trade trade)
        {
            trade.Decision = AppEnums.Decision.Wait;
            ResetFlags(trade);
            trade.Call().PositionBehavior.Decision = Decision.Null;
            trade.Put().PositionBehavior.Decision = Decision.Null;
            return trade;
        }

        public static Trade Reset(this Trade trade)
        {
            return Default(trade);
        }

        public static Trade ResetFlags(this Trade trade)
        {
            trade.Flags = new List<Flag>();
            trade.Call().PositionBehavior.Flags = new List<Flag>();
            trade.Put().PositionBehavior.Flags = new List<Flag>();
            return trade;
        }

        public static bool Any(this List<Flag> flags, Flag flag)
        {
            return flags.Any(f => f.Equals(flag));
        }

        public static double GetStockPrice(this Trade trade, bool previous = false)
        {
            if (!previous) // get the last one
                return trade.Sum_Change.LastOrDefault().PositionBehavior.Change.StockPrice;
            else
            {
                int changesCount = trade.Sum_Change.Count;
                return trade.Sum_Change.ElementAt(changesCount-1).PositionBehavior.Change.StockPrice;
            }
                
        }

        public static string GetUnderlying(this Trade trade)
        {
            return trade.Positions.FirstOrDefault().Underlying.Name;
        }
    }
}
