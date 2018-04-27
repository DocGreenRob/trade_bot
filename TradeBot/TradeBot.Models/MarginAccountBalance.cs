namespace TradeBot.Models.Broker.ETrade
{
    public class MarginAccountBalance
    {
        public double FedCall { get; set; }
        public double MarginBalance { get; set; }
        public double MarginBalanceWithdrawal { get; set; }
        public double MarginEquity { get; set; }
        public double MarginEquityPct { get; set; }
        public double MarginableSecurities { get; set; }
        public double MaxAvailableForWithdrawal { get; set; }
        public double MinEquityCall { get; set; }
        public double NonMarginableSecuritiesAndOptions { get; set; }
        public double TotalShortValue { get; set; }
        public double ShortReserve { get; set; }
    }
}