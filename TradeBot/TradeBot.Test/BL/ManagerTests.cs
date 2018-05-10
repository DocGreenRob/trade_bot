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
using System.IO;

namespace TradeBot.Test.BL
{
    [TestClass]
    public class ManagerTests
    {
        private Repo.IPositionRepo positionRepo;

        private FileStream ostrm;
        private StreamWriter writer;

        public void Log(string text)
        {

            TextWriter oldOut = Console.Out;
            try
            {
                ostrm = new FileStream(@"C:\_dev\_git\trade_bot\Logs\Log.txt", FileMode.OpenOrCreate, FileAccess.Write);
                writer = new StreamWriter(ostrm);
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot open Redirect.txt for writing");
                Console.WriteLine(e.Message);
                return;
            }
            Console.SetOut(writer);
            Console.WriteLine($"{DateTime.Now}");
            Console.WriteLine($"{text}");
            Console.SetOut(oldOut);
            writer.Close();
            ostrm.Close();
            Console.WriteLine("Done");

        }

        public void AppendLog(string text)
        {
            string path = @"C:\_dev\_git\trade_bot\Logs\Log.txt";
            // This text is added only once to the file.
            if (!File.Exists(path))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(path))
                {
                    sw.WriteLine($"{DateTime.Now} >>>>> {text}");
                }
            }

            // This text is always added, making the file longer over time
            // if it is not deleted.
            using (StreamWriter sw = File.AppendText(path))
            {
                sw.WriteLine($"{DateTime.Now} >>>>> {text}");
            }

            // Open the file to read from.
            using (StreamReader sr = File.OpenText(path))
            {
                string s = "";
                while ((s = sr.ReadLine()) != null)
                {
                    Console.WriteLine(s);
                }
            }
        }

        public ManagerTests()
        {
            // Logger

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
                Change change = new Change
                {
                    CallOptionPrice = 4.8,
                    DateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 9, 31, 0),
                    StockPrice = 350
                };
                AccountPosition adjustedAccountPosition = positionMgr.Change(accountPosition, change);
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
                // Simulate Position Change
                // Simulates checking the position via the API (getting the most current status of the position)
                Change change = new Change
                {
                    CallOptionPrice = 4.8,
                    DateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 9, 31, 0),
                    StockPrice = 350
                };
                AccountPosition adjustedAccountPosition = positionMgr.Change(accountPosition, change);

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

        private void BuildStrangle(string symbol, double callStrike, double putStrike, double callOptionPrice, double putOptionPrice, out int changesCount, out Position callPosition, out Position putPosition, out List<List<Change>> changes, out PositionManager positionMgr)
        {
            Tuple<AccountPositionsResponse, List<AccountPosition>> result = CreateStrangle(symbol, callStrike, putStrike, callOptionPrice, putOptionPrice, out positionMgr);  //CreateStrangle("BA", 322.5, 320, 4.05, 3.4, out positionMgr);
            
            AccountPositionsResponse accountPositionsResponse = result.Item1;
            List<AccountPosition> positionsOfInterest = result.Item2;

            int controlVariable = Models.MockModelDefaults.Default.Positions.Count;

            changes = new List<List<Change>>();
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
                            DateTime = position.EntryTime.AddMinutes(1),
                            StockPrice = 321.85,
                            CallOptionPrice = 3.75
                        },
                        new Change
                        {
                            DateTime = position.EntryTime.AddMinutes(2),
                            StockPrice = 322.38,
                            CallOptionPrice = 3.85
                        },
                        new Change
                        {
                            DateTime = position.EntryTime.AddMinutes(3),
                            StockPrice = 322.38,
                            CallOptionPrice = 4.05
                        },
                        new Change
                        {
                            DateTime = position.EntryTime.AddMinutes(4),
                            StockPrice = 322.28,
                            CallOptionPrice = 3.8
                        },
                        new Change
                        {
                            DateTime = position.EntryTime.AddMinutes(5),
                            StockPrice = 321.38,
                            CallOptionPrice = 3.8
                        },
                        new Change
                        {
                            DateTime = position.EntryTime.AddMinutes(6),
                            StockPrice = 320.99,
                            CallOptionPrice = 3.1
                        },
                        new Change
                        {
                            DateTime = position.EntryTime.AddMinutes(7),
                            StockPrice = 319.85,
                            CallOptionPrice = 2.52
                        },
                        new Change
                        {
                            DateTime = position.EntryTime.AddMinutes(8),
                            StockPrice = 318.71,
                            CallOptionPrice = 2.22
                        },
                        new Change
                        {
                            DateTime = position.EntryTime.AddMinutes(9),
                            StockPrice = 317.28,
                            CallOptionPrice = 1.78
                        },
                        new Change
                        {
                            DateTime = position.EntryTime.AddMinutes(10),
                            StockPrice = 313.56,
                            CallOptionPrice = 1.09
                        },
                        new Change
                        {
                            DateTime = position.EntryTime.AddMinutes(11),
                            StockPrice = 313.74,
                            CallOptionPrice = 1.19
                        },
                        new Change
                        {
                            DateTime = position.EntryTime.AddMinutes(12),
                            StockPrice = 315.83,
                            CallOptionPrice = 1.55
                        },
                        new Change
                        {
                            DateTime = position.EntryTime.AddMinutes(13),
                            StockPrice = 316.51,
                            CallOptionPrice = 1.77
                        },
                        new Change
                        {
                            DateTime = position.EntryTime.AddMinutes(14),
                            StockPrice = 315.73,
                            CallOptionPrice = 1.62
                        },
                        new Change
                        {
                            DateTime = position.EntryTime.AddMinutes(15),
                            StockPrice = 314.82,
                            CallOptionPrice = 1.45
                        },
                        new Change
                        {
                            DateTime = position.EntryTime.AddMinutes(16),
                            StockPrice = 316.43,
                            CallOptionPrice = 1.74
                        },
                        new Change
                        {
                            DateTime = position.EntryTime.AddMinutes(17),
                            StockPrice = 317.15,
                            CallOptionPrice = 1.93
                        },
                        new Change
                        {
                            DateTime = position.EntryTime.AddMinutes(18),
                            StockPrice = 317.33,
                            CallOptionPrice = 2.05
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
                            DateTime = position.EntryTime.AddMinutes(1),
                            StockPrice = 321.85,
                            PutOptionPrice = 3.3
                        },
                        new Change
                        {
                            DateTime = position.EntryTime.AddMinutes(2),
                            StockPrice = 322.38,
                            PutOptionPrice = 2.95
                        },
                        new Change
                        {
                            DateTime = position.EntryTime.AddMinutes(3),
                            StockPrice = 322.38,
                            PutOptionPrice = 2.76
                        },
                        new Change
                        {
                            DateTime = position.EntryTime.AddMinutes(4),
                            StockPrice = 322.28,
                            PutOptionPrice = 2.86
                        },
                        new Change
                        {
                            DateTime = position.EntryTime.AddMinutes(5),
                            StockPrice = 321.38,
                            PutOptionPrice = 2.83
                        },
                        new Change
                        {
                            DateTime = position.EntryTime.AddMinutes(6),
                            StockPrice = 320.99,
                            PutOptionPrice = 3.6
                        },
                        new Change
                        {
                            DateTime = position.EntryTime.AddMinutes(7),
                            StockPrice = 319.85,
                            PutOptionPrice = 4.3
                        },
                        new Change
                        {
                            DateTime = position.EntryTime.AddMinutes(8),
                            StockPrice = 318.71,
                            PutOptionPrice = 4.8
                        },
                        new Change
                        {
                            DateTime = position.EntryTime.AddMinutes(9),
                            StockPrice = 317.28,
                            PutOptionPrice = 5.8
                        },
                        new Change
                        {
                            DateTime = position.EntryTime.AddMinutes(10),
                            StockPrice = 313.56,
                            PutOptionPrice = 8.7
                        },
                        new Change
                        {
                            DateTime = position.EntryTime.AddMinutes(11),
                            StockPrice = 313.74,
                            PutOptionPrice = 8.55
                        },
                        new Change
                        {
                            DateTime = position.EntryTime.AddMinutes(12),
                            StockPrice = 315.83,
                            PutOptionPrice = 7.6
                        },
                        new Change
                        {
                            DateTime = position.EntryTime.AddMinutes(13),
                            StockPrice = 316.51,
                            PutOptionPrice = 6.45
                        },
                        new Change
                        {
                            DateTime = position.EntryTime.AddMinutes(14),
                            StockPrice = 315.73,
                            PutOptionPrice = 7.05
                        },
                        new Change
                        {
                            DateTime = position.EntryTime.AddMinutes(15),
                            StockPrice = 314.82,
                            PutOptionPrice = 7.7
                        },
                        new Change
                        {
                            DateTime = position.EntryTime.AddMinutes(16),
                            StockPrice = 316.43,
                            PutOptionPrice = 6.75
                        },
                        new Change
                        {
                            DateTime = position.EntryTime.AddMinutes(17),
                            StockPrice = 317.15,
                            PutOptionPrice = 5.95
                        },
                        new Change
                        {
                            DateTime = position.EntryTime.AddMinutes(18),
                            StockPrice = 317.33,
                            PutOptionPrice = 5.6
                        }
                    };

                    changes.Add(_changes);
                }
            }

            #endregion


            // Act & Assert

            // Now run through the "Evaluator()"

            // it should evaluate the positions as a whole and respond to each message accordingly - but it must have the logic to output the appropriate Decision as per the price action
            changesCount = changes.FirstOrDefault().Count;

            /****************************************************************************************************************/
            /****************************************************************************************************************/
            /****************************************************************************************************************/

            //+ Here is the actual trade (positions) that I place with CBOE. (the options contracts)
            callPosition = Models.MockModelDefaults.Default.Positions.Where(p => p.OptionOrderResponse.OptionSymbol.OptionType == OptionType.CALL).FirstOrDefault();
            putPosition = Models.MockModelDefaults.Default.Positions.Where(p => p.OptionOrderResponse.OptionSymbol.OptionType == OptionType.PUT).FirstOrDefault();
            /****************************************************************************************************************/
            /****************************************************************************************************************/
            /****************************************************************************************************************/
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
            List<List<Change>> changes;
            Position callPosition;
            Position putPosition;
            int changesCount;

            BuildStrangle("BA", 322.5, 320, 4.05, 3.4, out changesCount, out callPosition, out putPosition, out changes, out positionMgr);
            
            Trade trade = BuildTrade(changesCount, callPosition, putPosition, changes, positionMgr);

            string outputLog = "";
            AppendLog($"{Environment.NewLine} -------- NEW TEST -------- {Environment.NewLine}");

            for (int i = 0; i < changesCount; i++)
            {

                int n = i + 1;

                AppendLog($"i = {i} ");

                // Call
                if (i >= 1)
                {
                    PriceActionBehavior prior_call_priceActionBehavior = trade.BehaviorChanges.ElementAt(i - 1).Key.PriceActionBehavior;
                    PriceActionBehavior this_call_priceActionBehavior = trade.BehaviorChanges.ElementAt(i).Key.PriceActionBehavior;

                    AppendLog($"CALL : i = {i} prior.change = {prior_call_priceActionBehavior.PnL.PercentChange}  ?? this.change {this_call_priceActionBehavior.PnL.PercentChange}");

                    this_call_priceActionBehavior.IsDoubled = Utils.Utils.Utils.IsDoubled(prior_call_priceActionBehavior.PnL.PercentChange, this_call_priceActionBehavior.PnL.PercentChange);
                }
                else
                {
                    PriceActionBehavior this_call_priceActionBehavior = trade.BehaviorChanges.ElementAt(i).Key.PriceActionBehavior;
                    AppendLog($"CALL : i = {i} prior.change = {0}  ?? this.change {this_call_priceActionBehavior.PnL.PercentChange}");

                    if (i == 1)
                    {
                        this_call_priceActionBehavior.IsDoubled = Utils.Utils.Utils.IsDoubled(0, this_call_priceActionBehavior.PnL.PercentChange);
                    }
                    else
                    {
                        trade.BehaviorChanges.ElementAt(i).Key.PriceActionBehavior.IsDoubled = false;
                    }
                }

                // Put
                if (i >= 1)
                {
                    PriceActionBehavior prior_put_priceActionBehavior = trade.BehaviorChanges.ElementAt(i - 1).Value.PriceActionBehavior;
                    PriceActionBehavior this_put_priceActionBehavior = trade.BehaviorChanges.ElementAt(i).Value.PriceActionBehavior;

                    AppendLog($"PUT : i = {i} prior.change = {prior_put_priceActionBehavior.PnL.PercentChange}  ?? this.change {this_put_priceActionBehavior.PnL.PercentChange}");

                    trade.BehaviorChanges.ElementAt(i).Value.PriceActionBehavior.IsDoubled = Utils.Utils.Utils.IsDoubled(prior_put_priceActionBehavior.PnL.PercentChange, this_put_priceActionBehavior.PnL.PercentChange);
                }
                else
                {
                    PriceActionBehavior this_put_priceActionBehavior = trade.BehaviorChanges.ElementAt(i).Value.PriceActionBehavior;
                    AppendLog($"PUT : i = {i} prior.change = {0}  ?? this.change {this_put_priceActionBehavior.PnL.PercentChange}");

                    if (i == 1)
                    {
                        this_put_priceActionBehavior.IsDoubled = Utils.Utils.Utils.IsDoubled(0, this_put_priceActionBehavior.PnL.PercentChange);
                    }
                    else
                    {
                        trade.BehaviorChanges.ElementAt(i).Value.PriceActionBehavior.IsDoubled = false;
                    }
                }
                //++ Act
                // ---

                //+ Log
                AppendLog($"{trade.Time} ${trade.Sum_Change.LastOrDefault().PriceActionBehavior.PnL.Dollars} <----> {trade.Sum_Change.LastOrDefault().PriceActionBehavior.PnL.Percent}% <<=======>> ${trade.BehaviorChanges.ElementAt(i).Key.PriceActionBehavior.PnL.Dollars} ... {trade.BehaviorChanges.ElementAt(i).Key.PriceActionBehavior.PnL.Percent}% ... {trade.BehaviorChanges.ElementAt(i).Key.PriceActionBehavior.PnL.PercentChange}% ({trade.BehaviorChanges.ElementAt(i).Key.PriceActionBehavior.IsDoubled}) -------- ${trade.BehaviorChanges.ElementAt(i).Value.PriceActionBehavior.PnL.Dollars} ... {trade.BehaviorChanges.ElementAt(i).Value.PriceActionBehavior.PnL.Percent}% ... {trade.BehaviorChanges.ElementAt(i).Value.PriceActionBehavior.PnL.PercentChange}% ({trade.BehaviorChanges.ElementAt(i).Value.PriceActionBehavior.IsDoubled})");

                AppendLog(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");

                //+ Evaluate Position
                trade = positionMgr.Evaluate(trade);

                switch (i)
                {
                    case 0: // 9:32
                        Assert.AreEqual(Decision.Wait, trade.Decision);
                        // Trade Flags
                        Assert.AreEqual(0, trade.Flags.Count);
                        //+ Call
                        // Flags
                        Assert.AreEqual(0, trade.Call().PositionBehavior.Flags.Count);
                        // Decision
                        Assert.AreEqual(Decision.Wait, trade.Call().PositionBehavior.Decision);

                        //+ Put
                        // Flags
                        Assert.AreEqual(0, trade.Put().PositionBehavior.Flags.Count);
                        // Decision
                        Assert.AreEqual(Decision.Wait, trade.Put().PositionBehavior.Decision);

                        break;
                    case 1: // 9:33
                        Assert.AreEqual(Decision.Wait, trade.Decision);
                        // Trade Flags
                        Assert.AreEqual(0, trade.Flags.Count);
                        //+ Call
                        // Flags
                        Assert.AreEqual(0, trade.Call().PositionBehavior.Flags.Count);
                        // Decision
                        Assert.AreEqual(Decision.Wait, trade.Call().PositionBehavior.Decision);

                        //+ Put
                        // Flags
                        Assert.AreEqual(0, trade.Put().PositionBehavior.Flags.Count);
                        // Decision
                        Assert.AreEqual(Decision.Wait, trade.Put().PositionBehavior.Decision);
                        break;
                    //default:
                    //    throw new Exception("Something went wrong!");
                }

            }
            //Log(outputLog);
            Assert.Equals(true, false);
        }

        private double GetLastPercentageOpen(Trade trade, OptionType optionType)
        {
            if (trade.BehaviorChanges.Count == 0)
                return 0;

            if (optionType == OptionType.CALL)
                return trade.BehaviorChanges.LastOrDefault().Key.PriceActionBehavior.PnL.Percent;

            if (optionType == OptionType.PUT)
                return trade.BehaviorChanges.LastOrDefault().Value.PriceActionBehavior.PnL.Percent;

            return 0;
        }

        [TestMethod]
        public void Can_Develop_Bias()
        {
            // Arrange
            PositionManager positionMgr;
            List<List<Change>> changes;
            Position callPosition;
            Position putPosition;
            int changesCount;

            BuildStrangle("BA", 322.5, 320, 4.05, 3.4, out changesCount, out callPosition, out putPosition, out changes, out positionMgr);

            Trade trade = BuildTrade(changesCount, callPosition, putPosition, changes, positionMgr);

            // Act
            positionMgr.GetBias(trade);

            for(var i=0; i<trade.Sum_Change.Count; i++)
            {
                switch (i)
                {
                    case 0:
                        Assert.AreEqual(Bias.Null, trade.Bias);
                        break;
                }
            }

            // Assert
        }

        private Trade BuildTrade(
            int changesCount,
            Position callPosition,
            Position putPosition,
            List<List<Change>> changes,
            PositionManager positionMgr)
        {
            PositionBehavior tradePositionBehavior;

            Trade trade = new Trade(callPosition, putPosition);
            double totalTradeCost = Math.Round(callPosition.CostBasis + putPosition.CostBasis, 2);

            string outputLog = "";
            AppendLog($"{Environment.NewLine} -------- NEW TEST -------- {Environment.NewLine}");

            for (int i = 0; i < changesCount; i++)
            {

                int n = i + 1;

                // Set the Market Time
                DateTime marketTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 9, 31, 0).AddMinutes(n);
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

                // Stock Price
                trade.StockPrice = callPosition.PositionBehavior.Change.StockPrice;

                // [x]Position.PositionBehavior.AccountPosition =

                //+ Change
                // Call
                AccountPosition callAdjustedAccountPosition = positionMgr.Change(callPosition.AccountPositionsResponse.AccountPositions.Where(a => a.Product.CallPut == OptionType.CALL).FirstOrDefault(), callPosition.PositionBehavior.Change);

                callPosition.PositionBehavior.AccountPosition = callAdjustedAccountPosition;

                // Put
                AccountPosition putAdjustedAccountPosition = positionMgr.Change(putPosition.AccountPositionsResponse.AccountPositions.Where(a => a.Product.CallPut == OptionType.PUT).FirstOrDefault(), putPosition.PositionBehavior.Change);

                putPosition.PositionBehavior.AccountPosition = putAdjustedAccountPosition;

                // Call
                PriceActionBehavior callPriceActionBehavior = new PriceActionBehavior
                {
                    // need to get the last lastPercentChange if the time is greater than 9:31, else, return 0
                    PnL = Utils.Utils.Utils.GetPnL(callAdjustedAccountPosition, GetLastPercentageOpen(trade, OptionType.CALL)),
                    Studies = null
                };

                // Put
                PriceActionBehavior putPriceActionBehavior = new PriceActionBehavior
                {
                    // need to get the last lastPercentChange if the time is greater than 9:31, else, return 0
                    PnL = Utils.Utils.Utils.GetPnL(putAdjustedAccountPosition, GetLastPercentageOpen(trade, OptionType.PUT)),
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

                // Calculate Sum Change for the Trade
                double dollarsPnL = Math.Round(callTradeBehaviorChange.PriceActionBehavior.PnL.Dollars + putTradeBehaviorChange.PriceActionBehavior.PnL.Dollars, 2);
                // (cost / dollarsPnL) * 100
                double cost = Math.Round(callTradeBehaviorChange.PositionBehavior.AccountPosition.CostBasis + putTradeBehaviorChange.PositionBehavior.AccountPosition.CostBasis, 2);
                double current = Math.Round(callTradeBehaviorChange.PositionBehavior.AccountPosition.CurrentPrice + putTradeBehaviorChange.PositionBehavior.AccountPosition.CurrentPrice, 2);
                double percentPnL = Math.Round(dollarsPnL / cost, 2);
                double percentChange = Math.Round(callTradeBehaviorChange.PriceActionBehavior.PnL.PercentChange + putTradeBehaviorChange.PriceActionBehavior.PnL.PercentChange, 2);

                var callPrice = changes.ElementAt(0).ElementAt(i).CallOptionPrice;
                var putPrice = changes.ElementAt(1).ElementAt(i).PutOptionPrice;
                var x = changes.ElementAt(0).ElementAt(i).DateTime;

                // Generate the overall PositionBehavior
                tradePositionBehavior = new PositionBehavior
                {
                    Change = new Change
                    {
                        Amount = Math.Round((callPrice + putPrice) - totalTradeCost, 2) * 100,
                        CallOptionPrice = changes.ElementAt(0).ElementAt(i).CallOptionPrice,
                        PutOptionPrice = changes.ElementAt(1).ElementAt(i).PutOptionPrice,
                        DateTime = changes.ElementAt(0).ElementAt(i).DateTime,
                        StockPrice = changes.ElementAt(0).ElementAt(i).StockPrice
                    }
                };

                trade.Sum_Change.Add(new TradeBehaviorChange { PositionBehavior = tradePositionBehavior, PriceActionBehavior = new PriceActionBehavior { PnL = new PnL { Dollars = dollarsPnL, Percent = percentPnL, PercentChange = percentChange } } });

            }

            return trade;
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
