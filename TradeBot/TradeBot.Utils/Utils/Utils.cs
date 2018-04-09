using System;

namespace TradeBot.Utils.Utils
{
	public static class Utils
	{
		public static DateTime GetExpirationDate()
		{
			return (DateTime.Now.DayOfWeek < DayOfWeek.Thursday) ? DateTime.Today.AddDays(((int)DateTime.Today.DayOfWeek - (int)DayOfWeek.Monday)) : DateTime.Today.AddDays(((int)DateTime.Today.DayOfWeek - (int)DayOfWeek.Friday) + 7);
		}
	}
}
