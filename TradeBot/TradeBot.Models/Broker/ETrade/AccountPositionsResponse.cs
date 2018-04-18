using System.Collections.Generic;

namespace TradeBot.Models.Broker.ETrade
{
    public class AccountPositionsResponse
    {
        public int AccountId { get; set; }
        public int Count { get; set; }
        public List<AccountPosition> AccountPositions { get; set; }
    }
}
