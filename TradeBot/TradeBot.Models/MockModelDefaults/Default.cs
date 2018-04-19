using System;

namespace TradeBot.Models.MockModelDefaults
{
    public static class Default
    {
        public static string RootSymbol { get; set; }
        public static double OptionPrice { get; set; }

        public static int ExpirationMonth { get; set; }
        public static int ExpirationDay { get; set; }
        public static int ExpirationYear { get; set; }

        public static double StrikePrice { get; set; }

        public static string SymbolName { get; set; }

        public static void SetExpirationDate(DateTime dateTime)
        {
            ExpirationMonth = dateTime.Month;
            ExpirationDay = dateTime.Day;
            ExpirationYear = dateTime.Year;
        }
    }
}
