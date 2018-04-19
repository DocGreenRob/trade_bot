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

        /// <summary>
        /// 
        /// </summary>
        public enum InstrumentType
        {
            Stock,
            Bond,
            Option,
            MutualFund
        }

        /// <summary>
        /// 
        /// </summary>
        public enum OrderTerm
        {
            GOOD_FOR_DAY,
            GOOD_TILL_CANCEL
        }

        /// <summary>
        /// 
        /// </summary>
        public enum TypeCode
        {
            /// <summary>
            /// Equity
            /// </summary>
            EQ,
            /// <summary>
            /// Option
            /// </summary>
            OPTN,
            /// <summary>
            /// Index
            /// </summary>
            INDX,
            /// <summary>
            /// Mutual Fund
            /// </summary>
            MF,
            /// <summary>
            /// Fixed Income
            /// </summary>
            FI
        }

        /// <summary>
        /// 
        /// </summary>
        public enum LongOrShort
        {
            /// <summary>
            /// Bullish
            /// </summary>
            LONG,
            /// <summary>
            /// Bearish
            /// </summary>
            SHORT
        }

        /// <summary>
        /// 
        /// </summary>
        public enum Decision
        {
            /// <summary>
            /// The close
            /// </summary>
            Close,
            /// <summary>
            /// The wait
            /// </summary>
            Wait,
            /// <summary>
            /// The reverse
            /// </summary>
            Reverse,
            Investigate,
            Start_To_Worry,
            View_Historicals,
            /// <summary>
            /// The set least gain. (In the case where we are up 10% then we want to ensure that we gain at WORST 7.5%, for example)
            /// </summary>
            Set_Least_Gain,
            Break_Even,
            Trailing_Stop_1_Percent,
            /// <summary>
            /// The set least gain 2 percent. In the case where we profit 7% but we don't want to allow the trade to go in the red, we say the minimum gain is 2%.
            /// </summary>
            Set_Least_Gain_2_Percent,
            /// <summary>
            /// 
            /// </summary>
            Set_Least_Gain_4_Percent,
            Set_Least_Gain_6_Percent,
            HopeFor10PercentButTrap5Percent
        }

        public enum ExchangeCode
        {
            CINC
        }

        public enum ExpirationType
        {
            MONTHLY
        }

	}
}
