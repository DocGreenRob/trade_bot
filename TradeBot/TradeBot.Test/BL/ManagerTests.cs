using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TradeBot.BL.Managers;
using static TradeBot.Utils.Enum.AppEnums;
using System.Collections.Generic;
using TradeBot.Models;
using TradeBot.Models.Broker.ETrade;
using System.Linq;

namespace TradeBot.Test.BL
{
    [TestClass]
    public class ManagerTests
    {
        private Repo.IPositionRepo positionRepo;

        public ManagerTests()
        {
            positionRepo = new MockRepo.PositionRepo();
        }
		// TODO: What does it mean to evaluate a position?
		/// <summary>
		/// In this case, we will evaluate an Open Position and make the decision Not to Close the Position because we have Not met our Financial Goals (% or $) with this Trade.
		/// </summary>
		[TestMethod]
        public void Can_Evaluate_Single_Position_And_Not_Close_Position()
        {
			// Arrange
			// -------
			List<Broker> brokers = new List<Broker> { Broker.ETrade, Broker.TDAmeritrade };

			foreach(Broker broker in brokers)
			{
                if(broker == Broker.ETrade)
                {
                    // Create Position
                    PositionManager positionMgr = new PositionManager(positionRepo, broker);
                    Position position = positionMgr.OpenPosition("TSLA", PositionType.Strangle, TradeStrength.Light);
                    // TODO: Log

                    // Get Account Positions
                    int accountId = 999999999;
                    AccountPositionsResponse accountPositions = positionMgr.GetPositions(accountId);

                    // Get the Position
                    // Find my "position" in the "accountPositions"
                    List<AccountPosition> positionsOfInterest = accountPositions.AccountPositions.Where(a => a.Product.Symbol.ToLower() == position.Underlying.Name.ToLower()).ToList();

                    
                    foreach(AccountPosition accountPosition in positionsOfInterest)
                    {
                        TradeDirection tradeDirection;
                        if (accountPosition.Product.CallPut == OptionType.CALL)
                            tradeDirection = TradeDirection.Up;
                        else
                            tradeDirection = TradeDirection.Down;

                        // Simulate Position Change
                        // Simulates checking the position via the API (getting the most current status of the position)
                        AccountPosition adjustedAccountPosition = positionMgr.Change(accountPosition, tradeDirection, .02);

                        // 50 * 1.5 = 75
                        // 50 * 1.0 = 50 ; so 1.0 = 100%
                        // 50 * .20 = 10 ; so .20 = 20%
                        // 50 * .02 = 1 ; so .02 = 2%

                        // Act
                        // ---
                        // Evaluate Position
                        Decision decision = positionMgr.Evaluate(adjustedAccountPosition);

                        // Assert
                        // ------
                        Assert.AreEqual(decision, Decision.Wait);

                        // TBD
                    }

                }

            }

		}

        /// <summary>
        /// In this case, we will change the Position value such as to make the app start the monitoring process
        /// </summary>
        [TestMethod]
        public void Can_Evaluate_Position_And_Toggle_Monitor() { }

        /// <summary>
        /// In this case, we will evaluate the Position and becuase of its behavior we know its time to close the position
        /// </summary>
        [TestMethod]
        public void Can_Evaluate_Position_And_Close_Position() { }

		/// <summary>
		/// In this case, we want to Open a Position
		/// </summary>
		[TestMethod]
		public void Can_Open_Position() { }

		/// <summary>
		/// In this case, we want to Get a Position
		/// </summary>
		[TestMethod]
		public void Can_Get_Position() { }

		/// <summary>
		/// In this case, we want to Close a Position
		/// </summary>
		[TestMethod]
		public void Can_Close_Position() { }

		/// <summary>
		/// In this case, we want to Evaluate a Position
		/// </summary>
		[TestMethod]
		public void Can_Evaluate_Position() { }

		/// <summary>
		/// In this case, we want to Analyze the Price Action Chart for a specified time frame
		/// </summary>
		[TestMethod]
		public void Can_Evaluate_PriceActionChart() { }

		/// <summary>
		/// In this case, we want to Analyze a specific time frame's Candlestick
		/// </summary>
		[TestMethod]
		public void Can_Evaluate_Candlestick() { }

		/// <summary>
		/// In this case, we want to ensure that we can get the correct expiration date for Options based on Today's date
		/// </summary>
		[TestMethod]
		public void Can_Get_Correct_Expiration_Date() { }

	}
}
