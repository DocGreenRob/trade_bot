
using System;
using System.Collections.Generic;
using TradeBot.Models.Broker.ETrade;

namespace TradeBot.Models
{
    public static class History
    {
        public static Dictionary<DateTime, AccountPositionHistory> Entry;

        public static void Add(AccountPosition adjustedAccountPosition, double changeInDollars, double percent)
        {

            Entry.Add(DateTime.Now, new AccountPositionHistory {
                AccountPosition = adjustedAccountPosition,
                ChangeInDollars = changeInDollars,
                Percent = percent
            });

            throw new NotImplementedException();
        }
    }
}
