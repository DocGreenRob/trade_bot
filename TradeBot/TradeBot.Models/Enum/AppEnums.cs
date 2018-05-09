namespace TradeBot.Models.Enum
{
    public static class AppEnums
    {
        public enum PriceType
        {
            MARKET,
            LIMIT
        }

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
            Null, // Default
            New_Request,
            Break_Even,
            /// <summary>
            /// The close
            /// </summary>
            Close,
            Close_If_Worse,
            Close_Profit_Gapping_Side_Of_Trade,
            Close_Trade_Percent_Decrease,
            Close_Trade_Price_Hit_15_Minute_30_SMA,
            HopeFor10PercentButTrap5Percent,
            Investigate,
            Look_For_Closing_Opportunity,
            Met_10_Percent_Target_Hopefully_Momentum,
            Met_15_Percent_Target_Hopefully_Momentum,
            Micro_Pull_Back_Stopped_Possible_Channel_Flag,
            Monitor_For_3_Micro_Pull_Backs_Watch_Possible_Reversal,
            Monitor_For_3_Micro_Pull_Backs_Watch_Possible_Reversal_Close_If_Reversal_Continues,
            Percent_Double_In_Favor_Monitor_Possible_Pop,
            Percent_Double_In_Favor_Monitor_Possible_Pop_10_Percent_Goal_Hit_One_Side,
            Percent_Double_In_Favor_Monitor_Possible_Pop_100_Percent_Goal_Hit_One_Side,
            Percent_Double_In_Favor_Monitor_Possible_Pop_40_Percent_Goal_Hit_One_Side,
            /// <summary>
            /// The reverse
            /// </summary>
            Reverse,
            /// <summary>
            /// The set least gain. (In the case where we are up 10% then we want to ensure that we gain at WORST 7.5%, for example)
            /// </summary>
            Set_Least_Gain,
            Set_Least_Gain_10_Percent_Met_15_Percent_Target_Hopefully_Momentum,
            Set_Least_Gain_15_Percent_Met_20_Percent_Target_Hopefully_Momentum,
            Set_Least_Gain_15_Percent_Met_20_Percent_Target_Maybe_Micro_Pull_Back,
            Set_Least_Gain_20_Percent_Met_25_Percent_Target_Hopefully_Momentum,
            Set_Least_Gain_25_Percent_Met_30_Percent_Target_Hopefully_Momentum,
            Set_Least_Gain_3_Percent,
            Set_Least_Gain_3_Percent_Maybe_Micro_Pull_Back_1,
            Set_Least_Gain_30_Percent_Met_35_Percent_Target_Hopefully_Momentum,
            Set_Least_Gain_30_Percent_Met_35_Percent_Target_Hopefully_Momentum_No_More_Micro_Pull_Back_Allowance,
            Set_Least_Gain_35_Percent_Met_40_Percent_Target_Hopefully_Momentum_No_More_Micro_Pull_Back_Allowance,
            Set_Least_Gain_40_Percent_Met_45_Percent_Target_Hopefully_Momentum,
            Set_Least_Gain_40_Percent_Met_45_Percent_Target_Hopefully_Momentum_No_More_Micro_Pull_Back_Allowance_Rehit_Max_Percent_Gain,
            Set_Least_Gain_45_Percent_Met_50_Percent_Target_Hopefully_Momentum_No_More_Micro_Pull_Back_Allowance,
            Start_To_Worry,
            Start_To_Worry_First_10_Minutes,
            Start_To_Worry_First_10_Minutes_Set_15_Percent_Max_Loss,
            Start_To_Worry_Invegtigate_Positions_Individually_Possible_Breakup_Trade,
            /// <summary>
            /// The set least gain 2 percent. In the case where we profit 7% but we don't want to allow the trade to go in the red, we say the minimum gain is 2%.
            /// </summary>
            Set_Least_Gain_2_Percent,
            /// <summary>
            /// 
            /// </summary>
            Set_Least_Gain_4_Percent,
            Set_Least_Gain_6_Percent,
            Trailing_Stop_1_Percent,
            View_Historicals,
            /// <summary>
            /// The wait
            /// </summary>
            Wait,
            Wait_Crossover_Identified_Watch_Stoch_And_30_SMA,
            Wait_Crossover_Identified_Watch_Stoch_And_30_SMA_Entering_Over_Bought,
            Wait_Crossover_Identified_Watch_Stoch_And_30_SMA_Hovering_Around_Over_Bought_30_SMA_Coming_Down_Slow_Coming_Up_Very_Close_About_To_Touch,
            Wait_Crossover_Identified_Watch_Stoch_And_30_SMA_In_Over_Bought_30_SMA_Coming_Down,
            Wait_Crossover_Identified_Watch_Stoch_And_30_SMA_Leaving_Over_Bought_30_SMA_Coming_Down_Slow_Coming_Up,
            Wait_Crossover_Identified_Watch_Stoch_And_30_SMA_Reentered_Over_Bought_2nd_Time_30_SMA_Coming_Down_Slow_Coming_Up_Very_Close_About_To_Touch_Watch_Price_For_Touch_Of_SMA,
            Wait_Crossover_Identified_Watch_Stoch_And_30_SMA_Reentering_Over_Bought_2nd_Time_30_SMA_Coming_Down_Slow_Coming_Up_Very_Close_About_To_Touch,
            Wait_For_15_StochFull_Fast_Look_For_Crossover,
            Wait_Watching_Price_For_Touch_Of_SMA,
            Watch_For_Change_Percent_Decrease_To_Close_And_Reverse_Analysis
        }

        public enum ExchangeCode
        {
            CINC
        }

        public enum ExpirationType
        {
            MONTHLY
        }

        public enum Month
        {
            Janurary,
            Feburary,
            March,
            April,
            May,
            June,
            July,
            August,
            September,
            October,
            November,
            December
        }

        public enum AccountType
        {
            MARGIN,
            CASH
        }

        public enum OptionLevel
        {
            LEVEL_1,
            LEVEL_2,
            LEVEL_3,
            LEVEL_4
        }

        public enum Interval
        {
            Min_1,
            Min_5,
            Min_15,
            Min_30,
            Hr_1,
            Hr_4,
            Day,
            Week
        }

        public enum StochType
        {
            /// <summary>
            /// Slow (Purple/Pinkish)
            /// </summary>
            Full_D,
            /// <summary>
            /// Fast (Off Red)
            /// </summary>
            Full_K
        }

        public enum Flag
        {
            Close_At_10_Percent,
            Exit_If_Decrease,
            Inspect_Stoch_15_Mins,
            Max_Loss_Percent_Triggered,
            Micro_Watch,
            New_Request
        }
    }
}
