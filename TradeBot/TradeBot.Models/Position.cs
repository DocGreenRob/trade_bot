using System;
using System.Collections.Generic;
using TradeBot.Models.Broker.ETrade;
using TradeBot.Models.Broker.ETrade.Analyzer;
using TradeBot.Models.Interfaces;
using static TradeBot.Models.Enum.AppEnums;

namespace TradeBot.Models
{
    public class Position
    {
        public int PositionId { get; set; }
        /// <summary>
        /// Gets or sets the underlying price at entry.
        /// </summary>
        /// <value>
        /// The underlying price at entry. (Which is an estimate a seconds could pass from the time of OrderPreview to Place[Option]Order --> This value is not returned from the API)
        /// </value>
        public double UnderlyingPriceAtEntry { get; set; }
        /// <summary>
        /// Gets or sets the type of the instrument.
        /// </summary>
        /// <value>
        /// The type of the instrument. (Stock, Bond, Mutual Fund, Option)
        /// </value>
        public InstrumentType InstrumentType { get; set; }
        /// <summary>
        /// Gets or sets the entry time.
        /// </summary>
        /// <value>
        /// The entry time.
        /// </value>
        public DateTime EntryTime { get; set; }
        /// <summary>
        /// Gets or sets the profit loss open.
        /// </summary>
        /// <value>
        /// The profit loss open.
        /// </value>
        public double ProfitLossOpen { get; set; }
        /// <summary>
        /// Gets or sets the underlying.
        /// </summary>
        /// <value>
        /// The underlying.
        /// </value>
        public Underlying Underlying { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public PriceActionBehavior PriceActionBehavior { get; set; }
        public List<Flag> Flags { get; set; }

        #region ETrade Properties        
        /// <summary>
        /// Gets or sets the option order response.
        /// </summary>
        /// <value>
        /// The option order response.
        /// </value>
        public OptionOrderResponse OptionOrderResponse { get; set; }

        public AccountPositionsResponse AccountPositionsResponse { get; set; }

        public PositionBehavior PositionBehavior { get; set; }

        public double CostBasis { get; set; }

        public string Description { get; set; }
        #endregion

    }
}
