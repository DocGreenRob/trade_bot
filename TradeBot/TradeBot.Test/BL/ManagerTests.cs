using Microsoft.VisualStudio.TestTools.UnitTesting;
using TradeBot.BL.Managers;
using static TradeBot.Models.Enum.AppEnums;
using System.Collections.Generic;
using TradeBot.Models;
using TradeBot.Models.Broker.ETrade;
using System.Linq;
using TradeBot.Utils.ExtensionMethods;
using System.Globalization;
using System;
using TradeBot.Models.Broker.ETrade.Analyzer;
using TradeBot.Utils.Utils;

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
            //++ Arrange
            // -------
            PositionManager positionMgr = new PositionManager(positionRepo, Broker.ETrade);
            SetTestDefaults("TSLA", 300, OptionType.CALL, 5);
            // Create Position
            Models.MockModelDefaults.Default.Positions.Add(positionMgr.OpenPosition(Models.MockModelDefaults.Default.RootSymbol, PositionType.Naked, TradeStrength.Light, OptionType.CALL));
            // Set Description
            Models.MockModelDefaults.Default.Positions.ElementAt(0).Description = Models.MockModelDefaults.Default.SymbolName;

            Position position = Models.MockModelDefaults.Default.Positions.ElementAt(0);

            List<Broker> brokers = new List<Broker> { Broker.ETrade };

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
                //AccountPosition adjustedAccountPosition = positionMgr.Change(accountPosition, TradeDirection.Down, .02);

                DateTime marketTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 9, 31, 0);
                double currentStockPrice = position.UnderlyingPriceAtEntry + 1;
                // Now create the Trade() for Evaluation
                Trade trade = new Trade
                {
                    Time = marketTime,
                    StockPrice = currentStockPrice,
                    Positions = new List<Position>
                    {
                        position
                    },
                    BehaviorChanges = new Dictionary<TradeBehaviorChange, TradeBehaviorChange>
                    {

                    }
                };

                TradeBehaviorChange callTradeBehaviorChange = new TradeBehaviorChange
                {
                    PriceActionBehavior = new PriceActionBehavior
                    {
                        IsDoubled = Utils.Utils.Utils.IsDoubled(adjustedAccountPosition.CostBasis, adjustedAccountPosition.CurrentPrice),
                        PnL = Utils.Utils.Utils.GetPnL(adjustedAccountPosition, 0),
                        Studies = null
                    },
                    PositionBehavior = new PositionBehavior
                    {
                        AccountPosition = adjustedAccountPosition,
                        Change = new Change
                        {
                            Amount = .02,
                            DateTime = marketTime,
                            StockPrice = currentStockPrice,
                            TradeDirection = tradeDirection
                        }
                    }
                };

                trade.BehaviorChanges.Add(callTradeBehaviorChange, null);

                // Gain
                // Validate Change() with some Asserts
                Assert.IsTrue(trade.BehaviorChanges.ElementAt(0).Key.PriceActionBehavior.PnL.PercentChange < 2);
                Assert.IsTrue(trade.BehaviorChanges.ElementAt(0).Key.PriceActionBehavior.PnL.PercentChange > 1.9);

                #region math notes
                // 50 * 1.5 = 75
                // 50 * 1.0 = 50 ; so 1.0 = 100%
                // 50 * .20 = 10 ; so .20 = 20%
                // 50 * .02 = 1 ; so .02 = 2%
                #endregion

                //++ Act
                // ---
                // Evaluate Position
                Trade tradeResult = positionMgr.Evaluate(trade);

                //++ Assert
                // ------
                Assert.AreEqual(tradeResult.Decision, Decision.Wait);
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
            Models.MockModelDefaults.Default.Positions.ElementAt(1).Description = Models.MockModelDefaults.Default.SymbolName;

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
                Trade tradeResult = positionMgr.Evaluate(new Trade());

                // Assert
                // ------
                Assert.AreEqual(tradeResult.Decision, Decision.Wait);

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
            //++ Arrange
            PositionManager positionMgr;
            Tuple<AccountPositionsResponse, List<AccountPosition>> result = CreateStrangle("BA", 322.5, 320, 4.05, 3.4, out positionMgr);
            AccountPositionsResponse accountPositionsResponse = result.Item1;
            List<AccountPosition> positionsOfInterest = result.Item2;

            int controlVariable = Models.MockModelDefaults.Default.Positions.Count;

            List<List<Change>> changes = new List<List<Change>>();
            List<Change> _changes = new List<Change>();
            AccountPosition accountPosition = new AccountPosition();


            #region Create the list of Changes to be applied to the Trade for Evaluation()
            // Create the list of Changes to be applied to the Trade for Evaluation()
            // Output: List<Change> changes now has this list of Changes

            for (int i = 0; i < controlVariable; i++)
            {
                Position position = Models.MockModelDefaults.Default.Positions.ElementAt(i);

                // Set AccountPositionResponse
                position.AccountPositionsResponse = accountPositionsResponse;


                if (i == 0)
                {
                    _changes = new List<Change>
                    {
                        new Change
                        {
                            TradeDirection = TradeDirection.Down,
                            Amount =  .0741,
                            DateTime = position.EntryTime.AddMinutes(1),
                            StockPrice = 321.85
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Up,
                            Amount =  .0247,
                            DateTime = position.EntryTime.AddMinutes(2),
                            StockPrice = 322.38
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Up,
                            Amount =  .0494,
                            DateTime = position.EntryTime.AddMinutes(3),
                            StockPrice = 322.38
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Down,
                            Amount =  .0617,
                            DateTime = position.EntryTime.AddMinutes(4),
                            StockPrice = 322.28
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Sideways,
                            Amount =  0,
                            DateTime = position.EntryTime.AddMinutes(5),
                            StockPrice = 321.38
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Down,
                            Amount =  .1728,
                            DateTime = position.EntryTime.AddMinutes(6),
                            StockPrice = 320.99
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Down,
                            Amount =  .1432,
                            DateTime = position.EntryTime.AddMinutes(7),
                            StockPrice = 319.85
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Down,
                            Amount =  .1086,
                            DateTime = position.EntryTime.AddMinutes(8),
                            StockPrice = 318.71
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Down,
                            Amount =  .1704,
                            DateTime = position.EntryTime.AddMinutes(9),
                            StockPrice = 317.28
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Up,
                            Amount =  .0247,
                            DateTime = position.EntryTime.AddMinutes(10),
                            StockPrice = 313.56
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Up,
                            Amount =  .0741,
                            DateTime = position.EntryTime.AddMinutes(11),
                            StockPrice = 313.74
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Up,
                            Amount =  .0543,
                            DateTime = position.EntryTime.AddMinutes(12),
                            StockPrice = 315.83
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Down,
                            Amount =  .0370,
                            DateTime = position.EntryTime.AddMinutes(13),
                            StockPrice = 316.51
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Down,
                            Amount =  .0420,
                            DateTime = position.EntryTime.AddMinutes(14),
                            StockPrice = 315.73
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Up,
                            Amount =  .0716,
                            DateTime = position.EntryTime.AddMinutes(15),
                            StockPrice = 314.82
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Up,
                            Amount =  .0469,
                            DateTime = position.EntryTime.AddMinutes(16),
                            StockPrice = 316.43
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Up,
                            Amount =  .0296,
                            DateTime = position.EntryTime.AddMinutes(17),
                            StockPrice = 317.15
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Up,
                            Amount =  .1358,
                            DateTime = position.EntryTime.AddMinutes(18),
                            StockPrice = 317.33
                        }
                    };

                    changes.Add(_changes);
                }
                else
                {
                    _changes = new List<Change>
                    {
                        new Change
                        {
                            TradeDirection = TradeDirection.Down,
                            Amount =  .0247,
                            DateTime = position.EntryTime.AddMinutes(1),
                            StockPrice = 321.85
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Down,
                            Amount =  .0864,
                            DateTime = position.EntryTime.AddMinutes(2),
                            StockPrice = 322.38
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Down,
                            Amount =  .0469,
                            DateTime = position.EntryTime.AddMinutes(3),
                            StockPrice = 322.38
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Up,
                            Amount =  .0247,
                            DateTime = position.EntryTime.AddMinutes(4),
                            StockPrice = 322.28
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Down,
                            Amount =  .0074,
                            DateTime = position.EntryTime.AddMinutes(5),
                            StockPrice = 321.38
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Up,
                            Amount =  .1901,
                            DateTime = position.EntryTime.AddMinutes(6),
                            StockPrice = 320.99
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Up,
                            Amount =  .1728,
                            DateTime = position.EntryTime.AddMinutes(7),
                            StockPrice = 319.85
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Up,
                            Amount =  .1235,
                            DateTime = position.EntryTime.AddMinutes(8),
                            StockPrice = 318.71
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Up,
                            Amount =  .2469,
                            DateTime = position.EntryTime.AddMinutes(9),
                            StockPrice = 317.28
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Up,
                            Amount =  .7160,
                            DateTime = position.EntryTime.AddMinutes(10),
                            StockPrice = 313.56
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Down,
                            Amount =  .0370,
                            DateTime = position.EntryTime.AddMinutes(11),
                            StockPrice = 313.74
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Down,
                            Amount =  .2346,
                            DateTime = position.EntryTime.AddMinutes(12),
                            StockPrice = 315.83
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Down,
                            Amount =  .2840,
                            DateTime = position.EntryTime.AddMinutes(13),
                            StockPrice = 316.51
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Up,
                            Amount =  .1481,
                            DateTime = position.EntryTime.AddMinutes(14),
                            StockPrice = 315.73
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Up,
                            Amount =  .1605,
                            DateTime = position.EntryTime.AddMinutes(15),
                            StockPrice = 314.82
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Down,
                            Amount =  .2346,
                            DateTime = position.EntryTime.AddMinutes(16),
                            StockPrice = 316.43
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Down,
                            Amount =  .1975,
                            DateTime = position.EntryTime.AddMinutes(17),
                            StockPrice = 317.15
                        },
                        new Change
                        {
                            TradeDirection = TradeDirection.Down,
                            Amount =  .0864,
                            DateTime = position.EntryTime.AddMinutes(18),
                            StockPrice = 317.33
                        }
                    };

                    changes.Add(_changes);
                }

            }

            #endregion


            //accountPosition = positionsOfInterest.Where(pi => 
            //pi.Product.StrikePrice == position.OptionOrderResponse.OptionSymbol.StrikePrice 
            //&& position.OptionOrderResponse.OptionSymbol.OptionType == (i == 0 ? OptionType.CALL : OptionType.PUT)).FirstOrDefault();

            // Get correct account position

            /*
            accountPosition = positionsOfInterest.Where(pi =>
                pi.Product.StrikePrice == position.OptionOrderResponse.OptionSymbol.StrikePrice
                && position.OptionOrderResponse.OptionSymbol.OptionType == pi.Product.CallPut).FirstOrDefault();

            PositionBehavior positionBehavior = new PositionBehavior
            {
                AccountPosition = accountPosition,
                Change = changes
            };

            Models.MockModelDefaults.Default.Positions.ElementAt(i).PositionBehavior = positionBehavior;
            */

            // Act & Assert

            // Now run through the "Evaluator()"

            // it should evaluate the positions as a whole and respond to each message accordingly - but it must have the logic to output the appropriate Decision as per the price action
            int changesCount = changes.Count;

            Position callPosition = Models.MockModelDefaults.Default.Positions.Where(p => p.OptionOrderResponse.OptionSymbol.OptionType == OptionType.CALL).FirstOrDefault();
            Position putPosition = Models.MockModelDefaults.Default.Positions.Where(p => p.OptionOrderResponse.OptionSymbol.OptionType == OptionType.PUT).FirstOrDefault();

            //PositionBehavior callPositionBehavior = callPosition.PositionBehavior;
            //PositionBehavior putPositionBehavior = putPosition.PositionBehavior;

            #region Set Defaults on Trade()
            Trade trade = new Trade();
            trade.Positions = new List<Position>();
            trade.Positions.AddRange(new List<Position>{ callPosition,putPosition });
            trade.BehaviorChanges = new Dictionary<TradeBehaviorChange, TradeBehaviorChange>();
            
            //trade.BehaviorChanges.Add(
            //    new TradeBehaviorChange
            //    {
            //        PriceActionBehavior = new PriceActionBehavior { },
            //        PositionBehavior = new PositionBehavior
            //        {
            //            AccountPosition = new AccountPosition { },
            //            Change = new Change { }
            //        }
            //    },
            //    new TradeBehaviorChange
            //    {
            //        PriceActionBehavior = new PriceActionBehavior { },
            //        PositionBehavior = new PositionBehavior
            //        {
            //            AccountPosition = new AccountPosition { },
            //            Change = new Change { }
            //        }
            //    }
            //);
            #endregion

            for (int i = 0; i < changesCount; i++)
            {


                int n = i + 1;

                // Set the Market Time
                DateTime marketTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 9, 30, 0).AddMinutes(i);
                trade.Time = marketTime;

                // Simulate the Change
                // Call
                callPosition.PositionBehavior = new PositionBehavior
                {
                    Change = changes.ElementAt(0).ElementAt(i)
                };
                // Put
                putPosition.PositionBehavior = new PositionBehavior
                {
                    Change = changes.ElementAt(1).ElementAt(i)
                };

                //++ Act
                // Simulate Position Change
                // Simulates checking the position via the API (getting the most current status of the position)
                trade.StockPrice = callPosition.PositionBehavior.Change.StockPrice;

                // [x]Position.PositionBehavior.AccountPosition =
                // Call
                AccountPosition callAdjustedAccountPosition = positionMgr.Change(callPosition.AccountPositionsResponse.AccountPositions.Where(a => a.Product.CallPut == OptionType.CALL).FirstOrDefault(), callPosition.PositionBehavior.Change.TradeDirection, callPosition.PositionBehavior.Change.Amount);
                putPosition.PositionBehavior.AccountPosition = callAdjustedAccountPosition;

                // Put
                AccountPosition putAdjustedAccountPosition = positionMgr.Change(putPosition.AccountPositionsResponse.AccountPositions.Where(a => a.Product.CallPut == OptionType.PUT).FirstOrDefault(), callPosition.PositionBehavior.Change.TradeDirection, callPosition.PositionBehavior.Change.Amount);
                putPosition.PositionBehavior.AccountPosition = putAdjustedAccountPosition;

                // Call
                PriceActionBehavior callPriceActionBehavior = new PriceActionBehavior
                {
                    IsDoubled = Utils.Utils.Utils.IsDoubled(callAdjustedAccountPosition.CostBasis, callAdjustedAccountPosition.CurrentPrice),
                    // need to get the last lastPercentChange if the time is greater than 9:31, else, return 0
                    PnL = Utils.Utils.Utils.GetPnL(callAdjustedAccountPosition, 0),
                    Studies = null
                };

                // Put
                PriceActionBehavior putPriceActionBehavior = new PriceActionBehavior
                {
                    IsDoubled = Utils.Utils.Utils.IsDoubled(putAdjustedAccountPosition.CostBasis, putAdjustedAccountPosition.CurrentPrice),
                    // need to get the last lastPercentChange if the time is greater than 9:31, else, return 0
                    PnL = Utils.Utils.Utils.GetPnL(putAdjustedAccountPosition, 0),
                    Studies = null
                };

                // Call
                TradeBehaviorChange callTradeBehaviorChange = new TradeBehaviorChange
                {
                    PriceActionBehavior = callPriceActionBehavior,
                    PositionBehavior = callPosition.PositionBehavior
                };

                // Put
                TradeBehaviorChange putTradeBehaviorChange = new TradeBehaviorChange
                {
                    PriceActionBehavior = putPriceActionBehavior,
                    PositionBehavior = putPosition.PositionBehavior
                };

                trade.BehaviorChanges.Add(callTradeBehaviorChange, putTradeBehaviorChange);
                

                // Here we want to simulate getting a decision every minute.  So, since our object is fully built, and backward looking (we have mocked the data for the a future time) then here,
                // as we loop through our "changesCount" we will allow our decision maker utility to act as though it is real time.  Of course in real-time we will not know the future so it will be real time
                // I think that however I may change the signature but the guts should remain the same.

                //Tuple<PositionBehavior, List<Change>> callTuple = new Tuple<PositionBehavior, List<Change>>(callPositionBehavior, callPositionBehavior.Changes.Take(n).ToList());
                //Tuple<PositionBehavior, List<Change>> putTuple = new Tuple<PositionBehavior, List<Change>>(putPositionBehavior, putPositionBehavior.Changes.Take(n).ToList());

                //++ Act
                // ---
                // Evaluate Position
                //Decision decision = positionMgr.Evaluate(callTuple, putTuple);
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
