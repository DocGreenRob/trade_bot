using System.Collections.Generic;
using TradeBot.Models.Broker.ETrade.Analyzer.Studies;

namespace TradeBot.Models.Broker.ETrade.Analyzer
{
    public class PriceActionBehavior
    {
        public bool IsDoubled { get; set; }
        public PnL PnL { get; set; }
        public List<Studies.Study> Studies { get; set; }
    }

    public class x
    {
        public x()
        {
            var y = new PriceActionBehavior
            {
                IsDoubled = false,
                PnL = new PnL(),
                Studies = new List<Studies.Study>
                {
                    new Stochastic
                    {
                        Interval = Enum.AppEnums.Interval.Min_15,
                        Type = Enum.AppEnums.StochType.Full_D,
                        Value = 1
                    },
                    new EMA
                    {

                    }
                }
            };
        }
    }
}
