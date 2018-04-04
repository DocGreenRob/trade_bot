using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TradeBot.Tests.BusinessLogicTests
{
    [TestClass]
    public class ManagerTests
    {
        // TODO: What does it mean to evaluate a position?
        [TestMethod]
        public void Can_Evaluate_Position()
        {
            // Arrange
            // -------
            // Create Position
            Position position = new Position();
            position.CreatePosition();

            // Simulate Position Change
            position.Change(TradeDirection.Up, .02);

            // 50 * 1.5 = 75
            // 50 * 1.0 = 50 ; so 1.0 = 100%
            // 50 * .20 = 10 ; so .20 = 20%
            // 50 * .02 = 1 ; so .02 = 2%

            // Act
            // ---
            // Evaluate Position

            // Assert
            // ------

            // TBD
        }
    }
}
