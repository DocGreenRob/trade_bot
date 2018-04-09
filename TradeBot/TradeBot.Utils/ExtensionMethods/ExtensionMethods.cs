using System;
using System.Collections.Generic;
using System.Text;

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
	}
}
