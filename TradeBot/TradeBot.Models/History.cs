
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
            if (Entry == null)
                Entry = new Dictionary<DateTime, AccountPositionHistory>();

            Entry.Add(DateTime.Now, new AccountPositionHistory {
                AccountPosition = adjustedAccountPosition,
                ChangeInDollars = changeInDollars,
                Percent = percent
            });
        }
    }
}
