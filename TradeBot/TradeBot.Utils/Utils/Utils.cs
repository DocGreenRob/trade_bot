using System;
using TradeBot.Models.Broker.ETrade;
using TradeBot.Models.Broker.ETrade.Analyzer;

namespace TradeBot.Utils.Utils
{
    public static class Utils
    {
        public static DateTime GetExpirationDate()
        {
            return (DateTime.Now.DayOfWeek < DayOfWeek.Thursday) ? DateTime.Today.AddDays(((int)DateTime.Today.DayOfWeek - (int)DayOfWeek.Monday)) : DateTime.Today.AddDays(((int)DateTime.Today.DayOfWeek - (int)DayOfWeek.Friday) + 7);
        }

        public static bool IsDoubled(double lastPrice, double currentPrice, double offsetPercent = 10)
        {
            double twoTimesLastPrice = lastPrice < 0 ? lastPrice * -1 : lastPrice + lastPrice;
            twoTimesLastPrice = Math.Round(twoTimesLastPrice - (twoTimesLastPrice * .1), 2);

            if (offsetPercent == 10)
            {
                bool result = currentPrice >= twoTimesLastPrice;
                return result;
            }
            else
            {
                // reason we "-" subtract here is because we want to add a little room so that 14.71 % to 29.41 %, is considered doubled
                lastPrice = lastPrice - (lastPrice * offsetPercent);

                return currentPrice > (lastPrice - currentPrice);
            }
        }

        public static PnL GetPnL(AccountPosition accountPosition, double lastPercentOpen)
        {
            double dollarsPnL = Math.Round((Math.Round(accountPosition.CurrentPrice, 2) - Math.Round(accountPosition.CostBasis, 2)) * 100, 2);
            double percentPnL = Math.Round((dollarsPnL / Math.Round(accountPosition.CostBasis, 2)), 2);
            double percentChange = Math.Round(percentPnL - lastPercentOpen, 2);

            return new PnL
            {
                Dollars = dollarsPnL,
                Percent = percentPnL,
                PercentChange = percentChange
            };
        }
    }
}
