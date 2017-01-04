using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DaemonizerService;
using Daemonizer;

namespace DaemonizerServiceTest
{
    [TestClass]
    public class DaemonizerServiceTests
    {
        [TestMethod]
        public void InBetweenDaysInclusive_WithValidDays_NoRollover_IsWithin()
        {
            // arrange
            DayOfWeek TestDay = DayOfWeek.Wednesday;
            DayOfWeek StartDay = DayOfWeek.Sunday;
            DayOfWeek EndDay = DayOfWeek.Saturday;
            bool result;
            ProcessService service = new ProcessService();

            // act
            result = service.InBetweenDaysInclusive(TestDay, StartDay, EndDay);

            // assert
            Assert.AreEqual(true, result, "Inclusive in between day not calculated correctly.");
        }

        [TestMethod]
        public void InBetweenDaysInclusive_WithValidDays_NoRollover_OnStart_IsWithin()
        {
            // arrange
            DayOfWeek TestDay = DayOfWeek.Saturday;
            DayOfWeek StartDay = DayOfWeek.Sunday;
            DayOfWeek EndDay = DayOfWeek.Saturday;
            bool result;
            ProcessService service = new ProcessService();

            // act
            result = service.InBetweenDaysInclusive(TestDay, StartDay, EndDay);

            // assert
            Assert.AreEqual(true, result, "Inclusive in between day not calculated correctly.");
        }

        [TestMethod]
        public void InBetweenDaysInclusive_WithValidDays_NoRollover_IsNotWithin()
        {
            // arrange
            DayOfWeek TestDay = DayOfWeek.Monday;
            DayOfWeek StartDay = DayOfWeek.Wednesday;
            DayOfWeek EndDay = DayOfWeek.Saturday;
            bool result;
            ProcessService service = new ProcessService();

            // act
            result = service.InBetweenDaysInclusive(TestDay, StartDay, EndDay);

            // assert
            Assert.AreEqual(false, result, "Inclusive in between day not calculated correctly.");
        }

        [TestMethod]
        public void InBetweenDaysInclusive_WithValidDays_Rollover_IsWithin()
        {
            // arrange
            DayOfWeek TestDay = DayOfWeek.Friday;
            DayOfWeek StartDay = DayOfWeek.Wednesday;
            DayOfWeek EndDay = DayOfWeek.Tuesday;
            bool result;
            ProcessService service = new ProcessService();

            // act
            result = service.InBetweenDaysInclusive(TestDay, StartDay, EndDay);

            // assert
            Assert.AreEqual(true, result, "Inclusive in between day with week rollover not calculated correctly.");
        }

        [TestMethod]
        public void InBetweenDaysInclusive_WithValidDays_Rollover_IsNotWithin()
        {
            // arrange
            DayOfWeek TestDay = DayOfWeek.Wednesday;
            DayOfWeek StartDay = DayOfWeek.Friday;
            DayOfWeek EndDay = DayOfWeek.Tuesday;
            bool result;
            ProcessService service = new ProcessService();

            // act
            result = service.InBetweenDaysInclusive(TestDay, StartDay, EndDay);

            // assert
            Assert.AreEqual(false, result, "Inclusive in between day with week rollover not calculated correctly.");
        }

        [TestMethod]
        public void InBetweenDaysInclusive_WithValidDays_SameDay_IsWithin()
        {
            // arrange
            DayOfWeek TestDay = DayOfWeek.Friday;
            DayOfWeek StartDay = DayOfWeek.Friday;
            DayOfWeek EndDay = DayOfWeek.Friday;
            bool result;
            ProcessService service = new ProcessService();

            // act
            result = service.InBetweenDaysInclusive(TestDay, StartDay, EndDay);

            // assert
            Assert.AreEqual(true, result, "Inclusive in between day with week rollover not calculated correctly.");
        }

        [TestMethod]
        public void InBetweenDaysInclusive_WithValidDays_SameDay_IsNotWithin()
        {
            // arrange
            DayOfWeek TestDay = DayOfWeek.Monday;
            DayOfWeek StartDay = DayOfWeek.Friday;
            DayOfWeek EndDay = DayOfWeek.Friday;
            bool result;
            ProcessService service = new ProcessService();

            // act
            result = service.InBetweenDaysInclusive(TestDay, StartDay, EndDay);

            // assert
            Assert.AreEqual(false, result, "Inclusive in between day not calculated correctly.");
        }
    }
}
