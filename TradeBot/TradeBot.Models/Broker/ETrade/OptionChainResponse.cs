using System.Collections.Generic;

namespace TradeBot.Models.Broker.ETrade
{
    public class OptionChainResponse
    {
        public int OptionPairCount { get; set; }
        public List<OptionPairCount> OptionPairs { get; set; }
    }
}
