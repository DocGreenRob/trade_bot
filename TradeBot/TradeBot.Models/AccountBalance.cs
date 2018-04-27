namespace TradeBot.Models.Broker.ETrade
{
    public class AccountBalance
    {
        public double CashAvailableForWithdrawal { get; set; }
        public double CashCall { get; set; }
        public double FundsWithheldFromPurchasePower { get; set; }
        public double FundsWithheldFromWithdrawal { get; set; }
        public double NetAccountValue { get; set; }
        public double NetCash { get; set; }
        public double SweepDepositAmount { get; set; }
        public double TotalLongValue { get; set; }
        public double TotalSecuritiesMktValue { get; set; }
        public double TotalCash { get; set; }
    }
}