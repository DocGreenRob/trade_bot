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

        public static bool IsDoubled(double originalPrice, double currentPrice, double offsetPercent = 0)
        {
            if (offsetPercent == 0)
                return currentPrice > (originalPrice * 2);
            else
            {
                currentPrice = currentPrice * 2;
                currentPrice = currentPrice * offsetPercent;

                return currentPrice > (originalPrice - currentPrice);
            }
        }

        public static PnL GetPnL(AccountPosition accountPosition, double lastPercentChange)
        {
            double dollarsPnL = Math.Round((Math.Round(accountPosition.CurrentPrice, 2) - Math.Round(accountPosition.CostBasis, 2)) * 100,2);
            double percentPnL = Math.Round((dollarsPnL / Math.Round(accountPosition.CostBasis, 2)), 2);
            double percentChange = Math.Round(percentPnL - lastPercentChange, 2);

            return new PnL
            {
                Dollars = dollarsPnL,
                Percent = percentPnL,
                PercentChange = percentChange
            };
        }
    }
}
