namespace TradeBot.Utils.Enum
{
	public static class AppEnums
	{
		public enum TradeDirection
		{
			Up,
			Down,
			Sideways
		}

		/// <summary>
		/// 
		/// </summary>
		public enum PositionType
		{
			/// <summary>
			/// If "Underlying" is at $249.00 this means "Buy a Call" at the "Next Strike Price Up" ($250) and "Buy a Put" at the "Next Strike Price Down" ($247.50)
			/// </summary>
			Naked,
			Strangle,
			Vertical_Normal,
			Vertical_Personal,
			OppositeAfterGain,
			OppositeAfterSmallLoss,
			IronCondors
		}

		/// <summary>
		/// 
		/// </summary>
		public enum TradeStrength
		{
			/// <summary>
			/// 1 Contract or less than $500 on total trade (or something like this) or some % of account value
			/// </summary>
			Light,
			/// <summary>
			/// 1 Contract or less than $500 on total trade (or something like this)
			/// </summary>
			Medium,
			/// <summary>
			/// Max out on trade, X% (from config file) where X is the max of the account value percentage that can be traded at the current time. (or something like this)
			/// </summary>
			Strong
		}

		/// <summary>
		/// 
		/// </summary>
		public enum Broker
		{
			ETrade,
			TDAmeritrade

		}

		/// <summary>
		/// 
		/// </summary>
		public enum OptionType
		{
			CALL,
			PUT,
			CALLPUT
		}

		/// <summary>
		/// 
		/// </summary>
		public enum OrderAction
		{
			BUY_TO_OPEN,
			SELL_TO_OPEN,
			BUY_TO_CLOSE,
			SELL_TO_CLOSE
		}
	}
}
