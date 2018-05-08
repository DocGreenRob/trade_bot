using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Logging;
using TradeBot.BL.Managers;
using TradeBot.Models;
using TradeBot.Models.Broker.ETrade;
using static TradeBot.Models.Enum.AppEnums;

namespace TradeBot.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private readonly ILogger _logger;
        private Repo.IPositionRepo positionRepo;

        public ValuesController()
        {
            var factory = new LoggerFactory();
            var logger = factory.CreateLogger("MyLog");
            logger.LogError(1, "...", "sdfs");
        }

        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        // GET api/values/1
        [HttpGet("{id}")]
        public void Test()
        {
            _logger.LogWarning("Request to index");
            // Arrange
            // -------
            List<Broker> brokers = new List<Broker> { Broker.ETrade, Broker.TDAmeritrade };

            foreach (Broker broker in brokers)
            {
                if (broker == Broker.ETrade)
                {
                    // Create Position
                    PositionManager positionMgr = new PositionManager(positionRepo, broker); // ok
                    Position position = positionMgr.OpenPosition(Models.MockModelDefaults.Default.RootSymbol, PositionType.Strangle, TradeStrength.Light, OptionType.CALL); // ok
                    //++ LOOK HERE!!!!!!!!!!!!!!!!!!!!!!!!!!! (See TODO Below!!!)
                    // TODO: Log

                    // Get Account Positions
                    AccountPositionsResponse accountPositions = positionMgr.GetPositions(Models.MockModelDefaults.Default.AccountNumber);  // ok

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
                        //AccountPosition adjustedAccountPosition = positionMgr.Change(accountPosition, tradeDirection, .02);

                        // 50 * 1.5 = 75
                        // 50 * 1.0 = 50 ; so 1.0 = 100%
                        // 50 * .20 = 10 ; so .20 = 20%
                        // 50 * .02 = 1 ; so .02 = 2%

                        // Act
                        // ---
                        // Evaluate Position
                        //Decision decision = positionMgr.Evaluate(new Models.Broker.ETrade.Analyzer.Trade());

                        // Assert
                        // ------


                        // TBD
                    }

                }

            }
        }
    }
}
