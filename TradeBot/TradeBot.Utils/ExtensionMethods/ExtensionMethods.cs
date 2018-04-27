using System;

using TradeBot.Utils.ExtensionMethods;

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
	}
}
