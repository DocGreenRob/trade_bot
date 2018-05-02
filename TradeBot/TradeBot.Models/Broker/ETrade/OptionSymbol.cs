using static TradeBot.Models.Enum.AppEnums;

namespace TradeBot.Models.Broker.ETrade
{
    /// <summary>
    /// 
    /// </summary>
    public class OptionSymbol
    {
        /// <summary>
        /// Gets or sets the symbol.
        /// </summary>
        /// <value>
        /// The symbol.
        /// </value>
        public string Symbol { get; set; }
        /// <summary>
        /// Gets or sets the type of the option.
        /// </summary>
        /// <value>
        /// The type of the option.
        /// </value>
        public OptionType OptionType { get; set; }
        /// <summary>
        /// Gets or sets the strike price.
        /// </summary>
        /// <value>
        /// The strike price.
        /// </value>
        public double StrikePrice { get; set; }
        /// <summary>
        /// Gets or sets the expiration year.
        /// </summary>
        /// <value>
        /// The expiration year.
        /// </value>
        public int ExpirationYear { get; set; }
        /// <summary>
        /// Gets or sets the expiration month.
        /// </summary>
        /// <value>
        /// The expiration month.
        /// </value>
        public int ExpirationMonth { get; set; }
        /// <summary>
        /// Gets or sets the expiration day.
        /// </summary>
        /// <value>
        /// The expiration day.
        /// </value>
        public int ExpirationDay { get; set; }
    }
}