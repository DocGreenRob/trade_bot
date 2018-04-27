using static TradeBot.Utils.Enum.AppEnums;

namespace TradeBot.Models.Broker.ETrade
{
    public class AccountBalanceResponse
    {
        public int AccountId { get; set; }
        public AccountType AccountType { get; set; }
        public OptionLevel OptionLevel { get; set; }
        public AccountBalance AccountBalance { get; set; }
        public MarginAccountBalance MarginAccountBalance { get; set; }
    }
}