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
        public void Can_Open_Multiple_Positions_For_Same_Root_Symbol()
        {
            // Arrange
            // Act
            Tuple<AccountPositionsResponse, List<AccountPosition>> result = CreateStrangle("BA", 322.5, 320, 4.05, 3.4);

            // Assert
            AccountPositionsResponse accountPositionsResponse = result.Item1;
            Assert.AreEqual(2, accountPositionsResponse.AccountPositions.Count);

            List<AccountPosition> positionsOfInterest = result.Item2;
            Assert.AreEqual(2, positionsOfInterest.Count);

            Assert.AreEqual(2, Models.MockModelDefaults.Default.Positions.Count);
        }

        #region private methods
        private Tuple<AccountPositionsResponse, List<AccountPosition>> CreateStrangle(string symbol, double callStrike, double putStrike, double callCostBasis, double putCostBasis)
        {
            PositionManager positionMgr = new PositionManager(positionRepo, Broker.ETrade);

            // Create Position # 1 (CALL)
            SetTestDefaults(symbol, callStrike, OptionType.CALL, callCostBasis);
            Models.MockModelDefaults.Default.Positions.Add(positionMgr.OpenPosition(Models.MockModelDefaults.Default.RootSymbol, PositionType.Strangle, TradeStrength.Light, OptionType.CALL));

            // Create Position # 2 (PUT)
            SetTestDefaults(symbol, putStrike, OptionType.PUT, putCostBasis);
            Models.MockModelDefaults.Default.Positions.Add(positionMgr.OpenPosition(Models.MockModelDefaults.Default.RootSymbol, PositionType.Strangle, TradeStrength.Light, OptionType.PUT));

            // Get Account Positions
            AccountPositionsResponse accountPositionsResponse = positionMgr.GetPositions(Models.MockModelDefaults.Default.AccountNumber);

            // Find my positions of interest (i.e., the two I just opened above) in my list of AccountPositons returned within the AccountPositionResponse object
            List<Position> mockPosition = Models.MockModelDefaults.Default.Positions.Where(c => c.Underlying.Name.ToLower() == "ba").ToList();
            List<AccountPosition> positionsOfInterest = accountPositionsResponse.AccountPositions.Where(a => a.Product.Symbol.ToLower() == "ba").ToList();

            return new Tuple<AccountPositionsResponse, List<AccountPosition>>(accountPositionsResponse, positionsOfInterest);
        }
        #endregion

        [TestMethod]
        public void Can_Evaluate_Single_Strangle_And_Close_Position_Based_On_Decision_BA()
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
        /// Determines whether this instance [can evaluate strangle and close position based on decision ba].  Here we want to evaluate a Strange and close 1 side of the position because some conditions have been met, but keep the other side of the position going until some other condition is met.
        /// </summary>
        [TestMethod]
        public void Can_Evaluate_Strangle_And_Close_Position_Based_On_Decision_BA()
        {

            // Arrange
            Tuple<AccountPositionsResponse, List<AccountPosition>> result = CreateStrangle("BA", 322.5, 320, 4.05, 3.4);
            AccountPositionsResponse accountPositionsResponse = result.Item1;
            List<AccountPosition> positionsOfInterest = result.Item2;

            
            // 1 liner attempt

            //Models.MockModelDefaults.Default.Positions
            //    .ForEach(p => 
            //    p.PositionBehavior = new PositionBehavior
            //    {
            //        AccountPosition = ,
                    
            //    }
            //    );


            int controlVariable = Models.MockModelDefaults.Default.Positions.Count;
            for (int i = 0; i < controlVariable; i++)
            {
                
                // if CALL
                List<Changes> changes = new List<Changes>();
                AccountPosition accountPosition = new AccountPosition();
                if (i == 0)
                {
                    changes = new List<Changes>
                    {
                        new Changes
                        {
                            TradeDirection = TradeDirection.Down,
                            Change = 7.41
                        },
                        new Changes
                        {
                            TradeDirection = TradeDirection.Up,
                            Change = 2.47
                        },
                        new Changes
                        {
                            TradeDirection = TradeDirection.Up,
                            Change = 4.94
                        },
                        new Changes
                        {
                            TradeDirection = TradeDirection.Down,
                            Change = 6.17
                        },
                        new Changes
                        {
                            TradeDirection = TradeDirection.Sideways,
                            Change = 0
                        },
                        new Changes
                        {
                            TradeDirection = TradeDirection.Down,
                            Change = 17.28
                        },
                        new Changes
                        {
                            TradeDirection = TradeDirection.Down,
                            Change = 14.32
                        },
                        new Changes
                        {
                            TradeDirection = TradeDirection.Down,
                            Change = 10.86
                        },
                        new Changes
                        {
                            TradeDirection = TradeDirection.Down,
                            Change = 17.04
                        },
                        new Changes
                        {
                            TradeDirection = TradeDirection.Up,
                            Change = 2.47
                        },
                        new Changes
                        {
                            TradeDirection = TradeDirection.Up,
                            Change = 7.41
                        },
                        new Changes
                        {
                            TradeDirection = TradeDirection.Up,
                            Change = 5.43
                        },
                        new Changes
                        {
                            TradeDirection = TradeDirection.Down,
                            Change = 3.70
                        },
                        new Changes
                        {
                            TradeDirection = TradeDirection.Down,
                            Change = 4.20
                        },
                        new Changes
                        {
                            TradeDirection = TradeDirection.Up,
                            Change = 7.16
                        },
                        new Changes
                        {
                            TradeDirection = TradeDirection.Up,
                            Change = 4.69
                        },
                        new Changes
                        {
                            TradeDirection = TradeDirection.Up,
                            Change = 2.96
                        },
                        new Changes
                        {
                            TradeDirection = TradeDirection.Up,
                            Change = 13.58
                        }
                    };

                    Position p = Models.MockModelDefaults.Default.Positions.ElementAt(i);
                    accountPosition = positionsOfInterest.Where(pi => pi.Product.StrikePrice == p.OptionOrderResponse.OptionSymbol.StrikePrice && p.OptionOrderResponse.OptionSymbol.OptionType == OptionType.CALL).FirstOrDefault();
                }
                else
                {

                }

                
                PositionBehavior positionBehavior = new PositionBehavior
                {
                    AccountPosition = accountPosition,
                    Changes = changes
                };

                Models.MockModelDefaults.Default.Positions.ElementAt(i).PositionBehavior = positionBehavior;
            }

            foreach (AccountPosition accountPosition in positionsOfInterest)
            {
                //TradeDirection tradeDirection;
                //if (accountPosition.Product.CallPut == OptionType.CALL)
                //    tradeDirection = TradeDirection.Up;
                //else
                //    tradeDirection = TradeDirection.Down;

                //// Simulate Position Change
                //// Simulates checking the position via the API (getting the most current status of the position)
                //AccountPosition adjustedAccountPosition = positionMgr.Change(accountPosition, tradeDirection, .02);

                //// 50 * 1.5 = 75
                //// 50 * 1.0 = 50 ; so 1.0 = 100%
                //// 50 * .20 = 10 ; so .20 = 20%
                //// 50 * .02 = 1 ; so .02 = 2%

                //// Act
                //// ---
                //// Evaluate Position
                //Decision decision = positionMgr.Evaluate(adjustedAccountPosition);

                //// Assert
                //// ------
                //Assert.AreEqual(decision, Decision.Wait);

                // TBD
            }

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
