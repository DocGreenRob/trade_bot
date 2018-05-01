using Microsoft.VisualStudio.TestTools.UnitTesting;
using TradeBot.BL.Managers;
using static TradeBot.Utils.Enum.AppEnums;
using System.Collections.Generic;
using TradeBot.Models;
using TradeBot.Models.Broker.ETrade;
using System.Linq;
using TradeBot.Utils.ExtensionMethods;
using System.Globalization;
using System;

namespace TradeBot.Test.BL
{
    [TestClass]
    public class ManagerTests
    {
        private Repo.IPositionRepo positionRepo;

        public ManagerTests()
        {
            positionRepo = new MockRepo.PositionRepo();
            Models.MockModelDefaults.Default.AccountNumber = 999999999;
            Models.MockModelDefaults.Default.SetExpirationDate(Utils.Utils.Utils.GetExpirationDate());
            Models.MockModelDefaults.Default.Positions = new List<Position>();
        }

        private void SetTestDefaults(
            string _rootSymbol,
            double _strikePrice,
            OptionType _optionType,
            double _costBasis
            )
        {
            Models.MockModelDefaults.Default.RootSymbol = _rootSymbol;
            Models.MockModelDefaults.Default.StrikePrice = _strikePrice;
            Models.MockModelDefaults.Default.OptionType = _optionType;
            Models.MockModelDefaults.Default.CostBasis = _costBasis;
            TextInfo optionType = CultureInfo.CurrentCulture.TextInfo;

            Models.MockModelDefaults.Default.SymbolName = $"{Models.MockModelDefaults.Default.RootSymbol} {Models.MockModelDefaults.Default.ExpirationMonth.ToMonthAbrv()} {Models.MockModelDefaults.Default.ExpirationDay} {Models.MockModelDefaults.Default.ExpirationYear.ToYearAbrv()} ${Models.MockModelDefaults.Default.StrikePrice} {optionType.ToTitleCase(Models.MockModelDefaults.Default.OptionType.ToString().ToLower())}";
        }

        [TestMethod]
        public void Resolve_All_ToDo_s()
        {
            // TODO: Add Logging
            bool is_Logging_Added = false;
            Assert.IsTrue(is_Logging_Added);
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
            SetTestDefaults("TSLA", 300, OptionType.CALL, 5);

            List<Broker> brokers = new List<Broker> { Broker.ETrade, Broker.TDAmeritrade };

            foreach (Broker broker in brokers)
            {
                if (broker == Broker.ETrade)
                {
                    // Create Position
                    PositionManager positionMgr = new PositionManager(positionRepo, broker);
                    Position position = positionMgr.OpenPosition(Models.MockModelDefaults.Default.RootSymbol, PositionType.Strangle, TradeStrength.Light, OptionType.CALL);

                    // Get Account Positions
                    AccountPositionsResponse accountPositions = positionMgr.GetPositions(Models.MockModelDefaults.Default.AccountNumber);

                    // Get the Position
                    // Find my "position" in the "accountPositions"
                    List<AccountPosition> positionsOfInterest = accountPositions.AccountPositions.Where(a => a.Product.Symbol.ToLower() == position.Underlying.Name.ToLower()).ToList();


                    foreach (AccountPosition accountPosition in positionsOfInterest)
                    {
                        TradeDirection tradeDirection;
                        if (accountPosition.Product.CallPut == OptionType.CALL)
                            tradeDirection = TradeDirection.Up;
                        else
                            tradeDirection = TradeDirection.Down;

                        // Simulate Position Change
                        // Simulates checking the position via the API (getting the most current status of the position)
                        AccountPosition adjustedAccountPosition = positionMgr.Change(accountPosition, tradeDirection, .02);

                        #region math notes
                        // 50 * 1.5 = 75
                        // 50 * 1.0 = 50 ; so 1.0 = 100%
                        // 50 * .20 = 10 ; so .20 = 20%
                        // 50 * .02 = 1 ; so .02 = 2%
                        #endregion

                        // Act
                        // ---
                        // Evaluate Position
                        Decision decision = positionMgr.Evaluate(adjustedAccountPosition);

                        // Assert
                        // ------
                        Assert.AreEqual(decision, Decision.Wait);
                    }

                }

            }

        }

        /// <summary>
        /// Determines whether this instance [can open multiple positions for same root symbol (i.e., Strangle)].
        /// </summary>
        [TestMethod]
        public void Can_Create_Strangle()
        {
            // Arrange
            PositionManager positionManager;
            // Act
            Tuple<AccountPositionsResponse, List<AccountPosition>> result = CreateStrangle("BA", 322.5, 320, 4.05, 3.4, out positionManager);

            // Assert
            AccountPositionsResponse accountPositionsResponse = result.Item1;
            Assert.AreEqual(2, accountPositionsResponse.AccountPositions.Count);

            List<AccountPosition> positionsOfInterest = result.Item2;
            Assert.AreEqual(2, positionsOfInterest.Count);

            Assert.AreEqual(2, Models.MockModelDefaults.Default.Positions.Count);

            // Validate the Mocks
            // Strike Price
            Assert.AreEqual(322.5, Models.MockModelDefaults.Default.Positions.ElementAt(0).OptionOrderResponse.OptionSymbol.StrikePrice);
            // Option Type (CALL / PUT)
            Assert.AreEqual(OptionType.CALL, Models.MockModelDefaults.Default.Positions.ElementAt(0).OptionOrderResponse.OptionSymbol.OptionType);
            // Price
            Assert.AreEqual(4.05, (Models.MockModelDefaults.Default.Positions.ElementAt(0).OptionOrderResponse.EstimatedTotalAmount / 100));

            // Strike Price
            Assert.AreEqual(320, Models.MockModelDefaults.Default.Positions.ElementAt(1).OptionOrderResponse.OptionSymbol.StrikePrice);
            // Option Type (CALL / PUT)
            Assert.AreEqual(OptionType.PUT, Models.MockModelDefaults.Default.Positions.ElementAt(1).OptionOrderResponse.OptionSymbol.OptionType);
            // Price
            Assert.AreEqual(3.4, (Models.MockModelDefaults.Default.Positions.ElementAt(1).OptionOrderResponse.EstimatedTotalAmount / 100));


            // Account Positions
            // Strike Price
            Assert.AreEqual(322.5, accountPositionsResponse.AccountPositions.ElementAt(0).Product.StrikePrice);
            // Option Type (CALL / PUT)
            Assert.AreEqual(OptionType.CALL, accountPositionsResponse.AccountPositions.ElementAt(0).Product.CallPut);
            // Price
            Assert.AreEqual(4.05, accountPositionsResponse.AccountPositions.ElementAt(0).CostBasis);

            // Strike Price
            Assert.AreEqual(320, accountPositionsResponse.AccountPositions.ElementAt(1).Product.StrikePrice);
            // Option Type (CALL / PUT)
            Assert.AreEqual(OptionType.PUT, accountPositionsResponse.AccountPositions.ElementAt(1).Product.CallPut);
            // Price
            Assert.AreEqual(3.4, accountPositionsResponse.AccountPositions.ElementAt(1).CostBasis);
        }

        #region private methods
        private Tuple<AccountPositionsResponse, List<AccountPosition>> CreateStrangle(string symbol, double callStrike, double putStrike, double callCostBasis, double putCostBasis, out PositionManager positionManager)
        {
            PositionManager positionMgr = new PositionManager(positionRepo, Broker.ETrade);
            positionManager = positionMgr;

            // Create Position # 1 (CALL)
            SetTestDefaults(symbol, callStrike, OptionType.CALL, callCostBasis);
            Models.MockModelDefaults.Default.Positions.Add(positionMgr.OpenPosition(Models.MockModelDefaults.Default.RootSymbol, PositionType.Strangle, TradeStrength.Light, OptionType.CALL));
            Models.MockModelDefaults.Default.Positions.ElementAt(0).Description = Models.MockModelDefaults.Default.SymbolName;

            // Create Position # 2 (PUT)
            SetTestDefaults(symbol, putStrike, OptionType.PUT, putCostBasis);
            Models.MockModelDefaults.Default.Positions.Add(positionMgr.OpenPosition(Models.MockModelDefaults.Default.RootSymbol, PositionType.Strangle, TradeStrength.Light, OptionType.PUT));

            // Get Account Positions
            AccountPositionsResponse accountPositionsResponse = positionMgr.GetPositions(Models.MockModelDefaults.Default.AccountNumber);

            // Find my positions of interest (i.e., the two I just opened above) in my list of AccountPositons returned within the AccountPositionResponse object
            List<Position> mockPosition = Models.MockModelDefaults.Default.Positions.Where(c => c.Underlying.Name.ToLower() == symbol.ToLower()).ToList();
            List<AccountPosition> positionsOfInterest = accountPositionsResponse.AccountPositions.Where(a => a.Product.Symbol.ToLower() == symbol.ToLower()).ToList();

            return new Tuple<AccountPositionsResponse, List<AccountPosition>>(accountPositionsResponse, positionsOfInterest);
        }
        #endregion

        [TestMethod]
        public void Can_Evaluate_Naked_Option_And_Close_Position_Based_On_Decision_BA()
        {
            SetTestDefaults("BA", 322.5, OptionType.CALL, 4.05);

            // Arrange

            // Create Position
            PositionManager positionMgr = new PositionManager(positionRepo, Broker.ETrade); // ok
            Position position = positionMgr.OpenPosition(Models.MockModelDefaults.Default.RootSymbol, PositionType.Strangle, TradeStrength.Light, OptionType.CALL); // ok

            // Get Account Positions
            AccountPositionsResponse accountPositions = positionMgr.GetPositions(Models.MockModelDefaults.Default.AccountNumber);  // ok

            // Get the Position
            // Find my "position(s)" in the "accountPositions"
            List<AccountPosition> positionsOfInterest = accountPositions.AccountPositions.Where(a => a.Product.Symbol.ToLower() == position.Underlying.Name.ToLower()).ToList();

            Dictionary<TradeDirection, double> changePercents = new Dictionary<TradeDirection, double>();
            changePercents.Add(TradeDirection.Down, 7.41);
            changePercents.Add(TradeDirection.Up, 2.47);
            changePercents.Add(TradeDirection.Up, 4.94);
            changePercents.Add(TradeDirection.Down, 6.17);
            changePercents.Add(TradeDirection.Sideways, 0);
            changePercents.Add(TradeDirection.Down, 17.28);
            changePercents.Add(TradeDirection.Down, 14.32);
            changePercents.Add(TradeDirection.Down, 7.41);
            changePercents.Add(TradeDirection.Down, 10.86);
            changePercents.Add(TradeDirection.Down, 17.04);
            changePercents.Add(TradeDirection.Up, 2.47);
            changePercents.Add(TradeDirection.Up, 7.41);
            changePercents.Add(TradeDirection.Up, 5.43);
            changePercents.Add(TradeDirection.Down, 3.70);
            changePercents.Add(TradeDirection.Down, 4.20);
            changePercents.Add(TradeDirection.Up, 7.16);
            changePercents.Add(TradeDirection.Up, 4.69);
            changePercents.Add(TradeDirection.Up, 2.96);
            changePercents.Add(TradeDirection.Up, 13.58);

            foreach (AccountPosition accountPosition in positionsOfInterest)
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

        /// <summary>
        /// Determines whether this instance [can evaluate strangle and close position based on decision ba].  Here we want to evaluate a Strange and close 1 side of the position because some conditions have been met, but keep the other side of the position going until some other condition is met and then close it.
        /// Link: https://docs.google.com/spreadsheets/d/1IDjvIR7H8q15eeP0awyHCkjbuLwf1xnlNSUnzhdbTGI/edit#gid=1396774657
        /// </summary>
        [TestMethod]
        public void Can_Evaluate_Strangle_And_Close_Position_Based_On_Decision_BA()
        {
            // Arrange
            PositionManager positionMgr;
            Tuple<AccountPositionsResponse, List<AccountPosition>> result = CreateStrangle("BA", 322.5, 320, 4.05, 3.4, out positionMgr);
            AccountPositionsResponse accountPositionsResponse = result.Item1;
            List<AccountPosition> positionsOfInterest = result.Item2;

            int controlVariable = Models.MockModelDefaults.Default.Positions.Count;
            for (int i = 0; i < controlVariable; i++)
            {
                Position position = Models.MockModelDefaults.Default.Positions.ElementAt(i);

                // if CALL
                List<Change> changes = new List<Change>();
                AccountPosition accountPosition = new AccountPosition();
                if (i == 0)
                {
                    changes = new List<Change>
                    {
                        new Change
                        {
                            TradeDirection = TradeDirection.Down,
                            Amount =  7.41,
                            DateTime = position.EntryTime.AddMinutes(1)
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Up,
                            Amount =  2.47,
                            DateTime = position.EntryTime.AddMinutes(2)
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Up,
                            Amount =  4.94,
                            DateTime = position.EntryTime.AddMinutes(3)
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Down,
                            Amount =  6.17,
                            DateTime = position.EntryTime.AddMinutes(4)
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Sideways,
                            Amount =  0,
                            DateTime = position.EntryTime.AddMinutes(5)
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Down,
                            Amount =  17.28,
                            DateTime = position.EntryTime.AddMinutes(6)
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Down,
                            Amount =  14.32,
                            DateTime = position.EntryTime.AddMinutes(7)
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Down,
                            Amount =  10.86,
                            DateTime = position.EntryTime.AddMinutes(8)
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Down,
                            Amount =  17.04,
                            DateTime = position.EntryTime.AddMinutes(9)
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Up,
                            Amount =  2.47,
                            DateTime = position.EntryTime.AddMinutes(10)
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Up,
                            Amount =  7.41,
                            DateTime = position.EntryTime.AddMinutes(11)
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Up,
                            Amount =  5.43,
                            DateTime = position.EntryTime.AddMinutes(12)
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Down,
                            Amount =  3.70,
                            DateTime = position.EntryTime.AddMinutes(13)
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Down,
                            Amount =  4.20,
                            DateTime = position.EntryTime.AddMinutes(14)
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Up,
                            Amount =  7.16,
                            DateTime = position.EntryTime.AddMinutes(15)
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Up,
                            Amount =  4.69,
                            DateTime = position.EntryTime.AddMinutes(16)
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Up,
                            Amount =  2.96,
                            DateTime = position.EntryTime.AddMinutes(17)
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Up,
                            Amount =  13.58,
                            DateTime = position.EntryTime.AddMinutes(18)
                        }
                    };
                }
                else
                {
                    changes = new List<Change>
                    {
                        new Change
                        {
                            TradeDirection = TradeDirection.Down,
                            Amount =  2.47,
                            DateTime = position.EntryTime.AddMinutes(1)
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Down,
                            Amount =  8.64,
                            DateTime = position.EntryTime.AddMinutes(2)
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Down,
                            Amount =  4.69,
                            DateTime = position.EntryTime.AddMinutes(3)
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Up,
                            Amount =  2.47,
                            DateTime = position.EntryTime.AddMinutes(4)
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Down,
                            Amount =  0.74,
                            DateTime = position.EntryTime.AddMinutes(5)
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Up,
                            Amount =  19.01,
                            DateTime = position.EntryTime.AddMinutes(6)
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Up,
                            Amount =  17.28,
                            DateTime = position.EntryTime.AddMinutes(7)
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Up,
                            Amount =  12.35,
                            DateTime = position.EntryTime.AddMinutes(8)
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Up,
                            Amount =  24.69,
                            DateTime = position.EntryTime.AddMinutes(9)
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Up,
                            Amount =  71.60,
                            DateTime = position.EntryTime.AddMinutes(10)
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Down,
                            Amount =  3.70,
                            DateTime = position.EntryTime.AddMinutes(11)
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Down,
                            Amount =  23.46,
                            DateTime = position.EntryTime.AddMinutes(12)
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Down,
                            Amount =  28.40,
                            DateTime = position.EntryTime.AddMinutes(13)
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Up,
                            Amount =  14.81,
                            DateTime = position.EntryTime.AddMinutes(14)
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Up,
                            Amount =  16.05,
                            DateTime = position.EntryTime.AddMinutes(15)
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Down,
                            Amount =  23.46,
                            DateTime = position.EntryTime.AddMinutes(16)
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Down,
                            Amount =  19.75,
                            DateTime = position.EntryTime.AddMinutes(17)
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Down,
                            Amount =  8.64,
                            DateTime = position.EntryTime.AddMinutes(18)
                        }
                    };
                }


                accountPosition = positionsOfInterest.Where(pi => pi.Product.StrikePrice == position.OptionOrderResponse.OptionSymbol.StrikePrice && position.OptionOrderResponse.OptionSymbol.OptionType == (i == 0 ? OptionType.CALL : OptionType.PUT)).FirstOrDefault();

                PositionBehavior positionBehavior = new PositionBehavior
                {
                    AccountPosition = accountPosition,
                    Changes = changes
                };

                Models.MockModelDefaults.Default.Positions.ElementAt(i).PositionBehavior = positionBehavior;
            }

            // Act & Assert

            // Now run through the "PositionAnalyzer"

            // it should evaluate the positions as a whole and respond to each message accordingly - but it must have the logic to output the appropriate Decision as per the price action
            int changesCount = Models.MockModelDefaults.Default.Positions.FirstOrDefault().PositionBehavior.Changes.Count;

            Position callPosition = Models.MockModelDefaults.Default.Positions.ElementAt(0);
            Position putPosition = Models.MockModelDefaults.Default.Positions.ElementAt(1);

            PositionBehavior callPositionBehavior = Models.MockModelDefaults.Default.Positions.ElementAt(0).PositionBehavior;
            PositionBehavior putPositionBehavior = Models.MockModelDefaults.Default.Positions.ElementAt(1).PositionBehavior;


            for (int i = 0; i < changesCount; i++)
            {
                int n = i + 1;

                // Simulate Position Change
                // Simulates checking the position via the API (getting the most current status of the position)
                Change callChange = callPositionBehavior.Changes.ElementAt(i);
                AccountPosition callAdjustedAccountPosition = positionMgr.Change(callPosition.AccountPositionsResponse.AccountPositions.FirstOrDefault(), callChange.TradeDirection, callChange.Amount);

                // Here we want to simulate getting a decision every minute.  So, since our object is fully built, and backward looking (we have mocked the data for the a future time) then here,
                // as we loop through our "changesCount" we will allow our decision maker utility to act as though it is real time.  Of course in real-time we will not know the future so it will be real time
                // I think that however I may change the signature but the guts should remain the same.
                Tuple<PositionBehavior, List<Change>> callTuple = new Tuple<PositionBehavior, List<Change>>(callPositionBehavior, callPositionBehavior.Changes.Take(n).ToList());
                Tuple<PositionBehavior, List<Change>> putTuple = new Tuple<PositionBehavior, List<Change>>(putPositionBehavior, putPositionBehavior.Changes.Take(n).ToList());

                positionMgr.GetDecision(callTuple, putTuple);
            }

            Assert.Equals(true, false);
        }

        //      /// <summary>
        //      /// In this case, we will change the Position value such as to make the app start the monitoring process
        //      /// </summary>
        //      [TestMethod]
        //      public void Can_Evaluate_Position_And_Toggle_Monitor() { }

        //      /// <summary>
        //      /// In this case, we will evaluate the Position and becuase of its behavior we know its time to close the position
        //      /// </summary>
        //      [TestMethod]
        //      public void Can_Evaluate_Position_And_Close_Position() { }

        ///// <summary>
        ///// In this case, we want to Open a Position
        ///// </summary>
        //[TestMethod]
        //public void Can_Open_Position() { }

        ///// <summary>
        ///// In this case, we want to Get a Position
        ///// </summary>
        //[TestMethod]
        //public void Can_Get_Position() { }

        ///// <summary>
        ///// In this case, we want to Close a Position
        ///// </summary>
        //[TestMethod]
        //public void Can_Close_Position() { }

        ///// <summary>
        ///// In this case, we want to Evaluate a Position
        ///// </summary>
        //[TestMethod]
        //public void Can_Evaluate_Position() { }

        ///// <summary>
        ///// In this case, we want to Analyze the Price Action Chart for a specified time frame
        ///// </summary>
        //[TestMethod]
        //public void Can_Evaluate_PriceActionChart() { }

        ///// <summary>
        ///// In this case, we want to Analyze a specific time frame's Candlestick
        ///// </summary>
        //[TestMethod]
        //public void Can_Evaluate_Candlestick() { }

        ///// <summary>
        ///// In this case, we want to ensure that we can get the correct expiration date for Options based on Today's date
        ///// </summary>
        //[TestMethod]
        //public void Can_Get_Correct_Expiration_Date() { }

    }
}
