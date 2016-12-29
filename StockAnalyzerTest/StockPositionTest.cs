using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StockAnalyzer.StockPortfolio;

namespace StockAnalyzerTest
{
    [TestClass]
    public class StockPositionTest
    {
        public bool closeEventReceived = false;

        [TestMethod]
        public void TestPositionLongSimple()
        {
            closeEventReceived = false;
            int qty = 100;
            string name = "CAC40";
            DateTime openDate = DateTime.Today.AddMonths(-6);
            float orderFee = 10;
            float openValue = 4500;
            bool isShort = false;
           
            // Create Open order
            StockOrder openOrder = StockOrder.CreateExecutedOrder(name, StockOrder.OrderType.BuyAtMarketOpen, isShort, openDate, openDate, qty, openValue, orderFee);

            StockPosition position = new StockPosition(openOrder);
            position.OnPositionClosed += position_OnPositionClosed;

            Assert.AreEqual(qty, position.Number);
            Assert.AreEqual(name, position.StockName);
            Assert.AreEqual(openDate, position.OpenDate);
            Assert.AreEqual(openOrder, position.OpenOrder);
            Assert.AreEqual(isShort, position.IsShort);
            Assert.IsFalse(position.IsClosed);
            Assert.IsFalse(closeEventReceived);
            Assert.AreEqual(1, position.Orders.Count);
            Assert.AreEqual(orderFee, position.TotalFee);
            Assert.AreEqual(openOrder.UnitCost, position.AverageOpenPrice);
            Assert.AreEqual(0, position.TotalReturn);

            // Create closing order     
            DateTime closeDate = DateTime.Today.AddMonths(-3);
            float closeValue = 5000;
            StockOrder closeOrder = StockOrder.CreateExecutedOrder(name, StockOrder.OrderType.SellAtMarketOpen, isShort, closeDate, closeDate, qty, closeValue, orderFee);

            position.Add(closeOrder);

            Assert.AreEqual(0, position.Number);
            Assert.AreEqual(name, position.StockName);
            Assert.AreEqual(openDate, position.OpenDate);
            Assert.AreEqual(closeDate, position.CloseDate);
            Assert.AreEqual(openOrder, position.OpenOrder);
            Assert.AreEqual(isShort, position.IsShort);
            Assert.IsTrue(position.Orders.Contains(closeOrder));
            Assert.IsTrue(position.IsClosed);
            Assert.IsTrue(closeEventReceived);
            Assert.AreEqual(2, position.Orders.Count);
            Assert.AreEqual(orderFee * 2, position.TotalFee);
            Assert.AreEqual(openOrder.UnitCost, position.AverageOpenPrice);
            float expectedReturn = (closeValue - openValue) * qty - orderFee*2;
            Assert.AreEqual(Math.Round(expectedReturn), Math.Round(position.TotalReturn));
        }

        [TestMethod]
        public void TestPositionShortSimple()
        {
            closeEventReceived = false;
            int qty = 100;
            string name = "CAC40";
            DateTime openDate = DateTime.Today.AddMonths(-6);
            float orderFee = 10;
            float openValue = 4500;
            bool isShort = true;

            // Create Open order
            StockOrder openOrder = StockOrder.CreateExecutedOrder(name, StockOrder.OrderType.BuyAtMarketOpen, isShort, openDate, openDate, qty, openValue, orderFee);

            StockPosition position = new StockPosition(openOrder);
            position.OnPositionClosed += position_OnPositionClosed;

            Assert.AreEqual(qty, position.Number);
            Assert.AreEqual(name, position.StockName);
            Assert.AreEqual(openDate, position.OpenDate);
            Assert.AreEqual(openOrder, position.OpenOrder);
            Assert.AreEqual(isShort, position.IsShort);
            Assert.IsFalse(position.IsClosed);
            Assert.IsFalse(closeEventReceived);
            Assert.AreEqual(1, position.Orders.Count);
            Assert.AreEqual(orderFee, position.TotalFee);
            Assert.AreEqual(openOrder.UnitCost, position.AverageOpenPrice);
            Assert.AreEqual(0, position.TotalReturn);

            // Create closing order     
            DateTime closeDate = DateTime.Today.AddMonths(-3);
            float closeValue = 5000;
            StockOrder closeOrder = StockOrder.CreateExecutedOrder(name, StockOrder.OrderType.SellAtMarketOpen, isShort, closeDate, closeDate, qty, closeValue, orderFee);

            position.Add(closeOrder);

            Assert.AreEqual(0, position.Number);
            Assert.AreEqual(name, position.StockName);
            Assert.AreEqual(openDate, position.OpenDate);
            Assert.AreEqual(closeDate, position.CloseDate);
            Assert.AreEqual(openOrder, position.OpenOrder);
            Assert.AreEqual(isShort, position.IsShort);
            Assert.IsTrue(position.Orders.Contains(closeOrder));
            Assert.IsTrue(position.IsClosed);
            Assert.IsTrue(closeEventReceived);
            Assert.AreEqual(2, position.Orders.Count);
            Assert.AreEqual(orderFee * 2, position.TotalFee);
            Assert.AreEqual(openOrder.UnitCost, position.AverageOpenPrice);
            float expectedReturn = -(closeValue - openValue) * qty - orderFee * 2;
            Assert.AreEqual(Math.Round(expectedReturn), Math.Round(position.TotalReturn));
        }

        [TestMethod]
        public void TestPositionLongComplex()
        {
            closeEventReceived = false;
            int openQty1 = 125;
            string name = "CAC40";
            DateTime openDate1 = DateTime.Today.AddMonths(-6);
            float orderFee = 10;
            float openValue1 = 4500;
            bool isShort = false;

            // Create Open order1
            StockOrder openOrder1 = StockOrder.CreateExecutedOrder(name, StockOrder.OrderType.BuyAtMarketOpen, isShort, openDate1, openDate1, openQty1, openValue1, orderFee);

            StockPosition position = new StockPosition(openOrder1);
            position.OnPositionClosed += position_OnPositionClosed;

            Assert.AreEqual(openQty1, position.Number);
            Assert.AreEqual(name, position.StockName);
            Assert.AreEqual(openDate1, position.OpenDate);
            Assert.AreEqual(openOrder1, position.OpenOrder);
            Assert.AreEqual(isShort, position.IsShort);
            Assert.IsFalse(position.IsClosed);
            Assert.IsFalse(closeEventReceived);
            Assert.AreEqual(1, position.Orders.Count);
            Assert.AreEqual(orderFee, position.TotalFee);
            Assert.AreEqual(openOrder1.UnitCost, position.AverageOpenPrice);
            Assert.AreEqual(0, position.TotalReturn);

            // Create Open order2
            int openQty2 = 75;
            DateTime openDate2 = DateTime.Today.AddMonths(-4);
            float openValue2 = 4800;
            StockOrder openOrder2 = StockOrder.CreateExecutedOrder(name, StockOrder.OrderType.BuyAtMarketOpen, isShort,
                openDate2, openDate2, openQty2, openValue2, orderFee);

            position.Add(openOrder2);

            Assert.AreEqual(openQty1+openQty2, position.Number);
            Assert.AreEqual(name, position.StockName);
            Assert.AreEqual(openDate1, position.OpenDate);
            Assert.AreEqual(openOrder1, position.OpenOrder);
            Assert.AreEqual(isShort, position.IsShort);
            Assert.IsFalse(position.IsClosed);
            Assert.IsFalse(closeEventReceived);
            Assert.AreEqual(2, position.Orders.Count);
            Assert.AreEqual(orderFee*2, position.TotalFee);
            float expectedUnitCost = (openQty1 * openOrder1.UnitCost + openQty2 * openOrder2.UnitCost) / (openQty1 + openQty2);
            Assert.AreEqual(expectedUnitCost, position.AverageOpenPrice);
            Assert.AreEqual(0, position.TotalReturn);

            // Create closing order     
            DateTime closeDate1 = DateTime.Today.AddMonths(-3);
            float closeValue1 = 5000;
            int closeQty1 = 50;
            StockOrder closeOrder = StockOrder.CreateExecutedOrder(name, StockOrder.OrderType.SellAtMarketOpen, isShort,
                closeDate1, closeDate1, closeQty1, closeValue1, orderFee);

            position.Add(closeOrder);

            Assert.AreEqual(openQty1 + openQty2 - closeQty1, position.Number);
            Assert.AreEqual(name, position.StockName);
            Assert.AreEqual(openDate1, position.OpenDate);
            Assert.AreEqual(null, position.CloseDate);
            Assert.AreEqual(openOrder1, position.OpenOrder);
            Assert.AreEqual(isShort, position.IsShort);
            Assert.IsTrue(position.Orders.Contains(closeOrder));
            Assert.IsFalse(position.IsClosed);
            Assert.IsFalse(closeEventReceived);
            Assert.AreEqual(3, position.Orders.Count);
            Assert.AreEqual(orderFee * 3, position.TotalFee);
            Assert.AreEqual(expectedUnitCost, position.AverageOpenPrice);
            float expectedReturn = (closeValue1 - expectedUnitCost) * closeQty1 - orderFee;
            Assert.AreEqual(Math.Round(expectedReturn), Math.Round(position.TotalReturn));

            // Create closing order 2
            DateTime closeDate2 = DateTime.Today.AddMonths(-2);
            float closeValue2 = 5000;
            int closeQty2 = openQty1 + openQty2 - closeQty1;
            StockOrder closeOrder2 = StockOrder.CreateExecutedOrder(name, StockOrder.OrderType.SellAtMarketOpen, isShort,
                closeDate2, closeDate2, closeQty2, closeValue2, orderFee);

            position.Add(closeOrder2);

            Assert.AreEqual(0, position.Number);
            Assert.AreEqual(name, position.StockName);
            Assert.AreEqual(openDate1, position.OpenDate);
            Assert.AreEqual(closeDate2, position.CloseDate);
            Assert.AreEqual(openOrder1, position.OpenOrder);
            Assert.AreEqual(isShort, position.IsShort);
            Assert.IsTrue(position.Orders.Contains(closeOrder2));
            Assert.IsTrue(position.IsClosed);
            Assert.IsTrue(closeEventReceived);
            Assert.AreEqual(4, position.Orders.Count);
            Assert.AreEqual(orderFee * 4, position.TotalFee);
            Assert.AreEqual(expectedUnitCost, position.AverageOpenPrice);
            expectedReturn += (closeValue2 - expectedUnitCost) * closeQty2 - orderFee;
            Assert.AreEqual(Math.Round(expectedReturn), Math.Round(position.TotalReturn));
        }

        [TestMethod]
        public void TestPositionShortComplex()
        {
            closeEventReceived = false;
            int openQty1 = 125;
            string name = "CAC40";
            DateTime openDate1 = DateTime.Today.AddMonths(-6);
            float orderFee = 10;
            float openValue1 = 4500;
            bool isShort = true;

            // Create Open order1
            StockOrder openOrder1 = StockOrder.CreateExecutedOrder(name, StockOrder.OrderType.BuyAtMarketOpen, isShort, openDate1, openDate1, openQty1, openValue1, orderFee);

            StockPosition position = new StockPosition(openOrder1);
            position.OnPositionClosed += position_OnPositionClosed;

            Assert.AreEqual(openQty1, position.Number);
            Assert.AreEqual(name, position.StockName);
            Assert.AreEqual(openDate1, position.OpenDate);
            Assert.AreEqual(openOrder1, position.OpenOrder);
            Assert.AreEqual(isShort, position.IsShort);
            Assert.IsFalse(position.IsClosed);
            Assert.IsFalse(closeEventReceived);
            Assert.AreEqual(1, position.Orders.Count);
            Assert.AreEqual(orderFee, position.TotalFee);
            Assert.AreEqual(openOrder1.UnitCost, position.AverageOpenPrice);
            Assert.AreEqual(0, position.TotalReturn);

            // Create Open order2
            int openQty2 = 75;
            DateTime openDate2 = DateTime.Today.AddMonths(-4);
            float openValue2 = 4800;
            StockOrder openOrder2 = StockOrder.CreateExecutedOrder(name, StockOrder.OrderType.BuyAtMarketOpen, isShort,
                openDate2, openDate2, openQty2, openValue2, orderFee);

            position.Add(openOrder2);

            Assert.AreEqual(openQty1 + openQty2, position.Number);
            Assert.AreEqual(name, position.StockName);
            Assert.AreEqual(openDate1, position.OpenDate);
            Assert.AreEqual(openOrder1, position.OpenOrder);
            Assert.AreEqual(isShort, position.IsShort);
            Assert.IsFalse(position.IsClosed);
            Assert.IsFalse(closeEventReceived);
            Assert.AreEqual(2, position.Orders.Count);
            Assert.AreEqual(orderFee * 2, position.TotalFee);
            float expectedUnitCost = (openQty1 * openOrder1.UnitCost + openQty2 * openOrder2.UnitCost) / (openQty1 + openQty2);
            Assert.AreEqual(expectedUnitCost, position.AverageOpenPrice);
            Assert.AreEqual(0, position.TotalReturn);

            // Create closing order 1
            DateTime closeDate1 = DateTime.Today.AddMonths(-3);
            float closeValue1 = 5000;
            int closeQty1 = 50;
            StockOrder closeOrder = StockOrder.CreateExecutedOrder(name, StockOrder.OrderType.SellAtMarketOpen, isShort,
                closeDate1, closeDate1, closeQty1, closeValue1, orderFee);

            position.Add(closeOrder);

            Assert.AreEqual(openQty1 + openQty2 - closeQty1, position.Number);
            Assert.AreEqual(name, position.StockName);
            Assert.AreEqual(openDate1, position.OpenDate);
            Assert.AreEqual(null, position.CloseDate);
            Assert.AreEqual(openOrder1, position.OpenOrder);
            Assert.AreEqual(isShort, position.IsShort);
            Assert.IsTrue(position.Orders.Contains(closeOrder));
            Assert.IsFalse(position.IsClosed);
            Assert.IsFalse(closeEventReceived);
            Assert.AreEqual(3, position.Orders.Count);
            Assert.AreEqual(orderFee * 3, position.TotalFee);
            Assert.AreEqual(expectedUnitCost, position.AverageOpenPrice);
            float expectedReturn = -(closeValue1 - expectedUnitCost) * closeQty1 - orderFee;
            Assert.AreEqual(Math.Round(expectedReturn), Math.Round(position.TotalReturn));

            // Create closing order 2
            DateTime closeDate2 = DateTime.Today.AddMonths(-2);
            float closeValue2 = 5000;
            int closeQty2 = openQty1 + openQty2 - closeQty1;
            StockOrder closeOrder2 = StockOrder.CreateExecutedOrder(name, StockOrder.OrderType.SellAtMarketOpen, isShort,
                closeDate2, closeDate2, closeQty2, closeValue2, orderFee);

            position.Add(closeOrder2);

            Assert.AreEqual(0, position.Number);
            Assert.AreEqual(name, position.StockName);
            Assert.AreEqual(openDate1, position.OpenDate);
            Assert.AreEqual(closeDate2, position.CloseDate);
            Assert.AreEqual(openOrder1, position.OpenOrder);
            Assert.AreEqual(isShort, position.IsShort);
            Assert.IsTrue(position.Orders.Contains(closeOrder2));
            Assert.IsTrue(position.IsClosed);
            Assert.IsTrue(closeEventReceived);
            Assert.AreEqual(4, position.Orders.Count);
            Assert.AreEqual(orderFee * 4, position.TotalFee);
            Assert.AreEqual(expectedUnitCost, position.AverageOpenPrice);
            expectedReturn += -(closeValue2 - expectedUnitCost) * closeQty2 - orderFee;
            Assert.AreEqual(Math.Round(expectedReturn), Math.Round(position.TotalReturn));
        }

        void position_OnPositionClosed(StockPosition position)
        {
            closeEventReceived = true;
        }
    }
}
