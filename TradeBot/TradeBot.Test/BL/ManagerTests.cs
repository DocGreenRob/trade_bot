using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TradeBot.BL.Managers;
using static TradeBot.Utils.Enum.AppEnums;

namespace TradeBot.Test.BL
{
    [TestClass]
    public class ManagerTests
    {
        // TODO: What does it mean to evaluate a position?
        [TestMethod]
        public void Can_Evaluate_Position_And_Not_Close_Position()
        {
            // Arrange
            // -------
            // Create Position
            PositionManager position = new PositionManager();
            position.CreatePosition("TSLA");

            // Simulate Position Change
            position.Change(TradeDirection.Up, .02);

            // 50 * 1.5 = 75
            // 50 * 1.0 = 50 ; so 1.0 = 100%
            // 50 * .20 = 10 ; so .20 = 20%
            // 50 * .02 = 1 ; so .02 = 2%

            // Act
            // ---
            // Evaluate Position
            position.Evaluate();

            // Assert
            // ------

            // TBD
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
    }
}
