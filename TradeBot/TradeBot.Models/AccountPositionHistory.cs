using TradeBot.Models.Broker.ETrade;

namespace TradeBot.Models
{
    public class AccountPositionHistory
    {
        public AccountPosition AccountPosition { get; set; }
        public double ChangeInDollars { get; set; }
        public double Percent { get; set; }
    }
}
