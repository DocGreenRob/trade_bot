using System;
using System.Linq;
using TradeBot.Models;
using TradeBot.Models.Broker.ETrade.Analyzer;
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
    }
}
