using System;
using System.Collections.Generic;
using static TradeBot.Utils.Enum.AppEnums;

namespace TradeBot.Models.Broker.ETrade
{
    /// <summary>
    /// The optionOrderResponse object from the JSON response/results from ETrade
    /// </summary>
    public class OptionOrderResponse
    {
        /// <summary>
        /// Gets or sets the account identifier.
        /// </summary>
        /// <value>
        /// The account identifier. (optionOrderReponse.accountId)
        /// </value>
        public int AccountId { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [all or none].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [all or none]; otherwise, <c>false</c>. (optionOrderReponse.allOrNone)
        /// </value>
        public bool AllOrNone { get; set; }
        /// <summary>
        /// Gets or sets the estimated commission.
        /// </summary>
        /// <value>
        /// The estimated commission. (optionOrderReponse.estimatedCommission)
        /// </value>
        public double EstimatedCommission { get; set; }
        /// <summary>
        /// Gets or sets the estimated total amount.
        /// </summary>
        /// <value>
        /// The estimated total amount. (optionOrderReponse.estimatedTotalAmount)
        /// </value>
        public double EstimatedTotalAmount { get; set; }
        /// <summary>
        /// Gets or sets the messages.
        /// </summary>
        /// <value>
        /// The messages. (optionOrderReponse.messageList{})
        /// </value>
        public List<Message> Messages { get; set; }
        /// <summary>
        /// Gets or sets the order number.
        /// </summary>
        /// <value>
        /// The order number.
        /// </value>
        public int OrderNumber { get; set; }
        /// <summary>
        /// Gets or sets the order time.
        /// </summary>
        /// <value>
        /// The order time.
        /// </value>
        public DateTime OrderTime { get; set; }
        /// <summary>
        /// Gets or sets the quantity.
        /// </summary>
        /// <value>
        /// The quantity.
        /// </value>
        public int Quantity { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [reserve order].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [reserve order]; otherwise, <c>false</c>.
        /// </value>
        public bool ReserveOrder { get; set; }
        /// <summary>
        /// Gets or sets the reserve quantity.
        /// </summary>
        /// <value>
        /// The reserve quantity.
        /// </value>
        public int ReserveQuantity { get; set; }
        /// <summary>
        /// Gets or sets the order term.
        /// </summary>
        /// <value>
        /// The order term.
        /// </value>
        public OrderTerm OrderTerm { get; set; }
        /// <summary>
        /// Gets or sets the limit price.
        /// </summary>
        /// <value>
        /// The limit price.
        /// </value>
        public double LimitPrice { get; set; }
        /// <summary>
        /// Gets or sets the option symbol.
        /// </summary>
        /// <value>
        /// The option symbol.
        /// </value>
        public OptionSymbol OptionSymbol { get; set; }
        /// <summary>
        /// Gets or sets the order action.
        /// </summary>
        /// <value>
        /// The order action.
        /// </value>
        public OrderAction OrderAction { get; set; }
    }
}
