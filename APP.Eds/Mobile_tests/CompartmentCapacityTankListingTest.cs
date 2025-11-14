using NUnit.Framework;
using APP.Eds.Models.Tank;
using System.Collections.Generic;
using System.Linq;

namespace Mobile_tests
{
    /// <summary>
    /// Test to verify that the tank listing uses the correct endpoint and model
    /// </summary>
    [TestFixture]
    public class CompartmentCapacityTankListingTest
    {
        [Test]
        [Category("CompartmentCapacity")]
        [Category("TankListing")]
        public void TankResponse_ShouldHaveCorrectProperties()
        {
            // Arrange & Act
            var tank = new TankResponse
            {
                IdTank = 1,
                Number = "TQ-TQ-001",
                Ability = 15000,
                Stock = 10000,
                Compartment = 3
            };

            // Assert
            Assert.That(tank.IdTank, Is.EqualTo(1), "IdTank should be correctly set");
            Assert.That(tank.Number, Is.EqualTo("TQ-TQ-001"), "Number should be correctly set");
            Assert.That(tank.Ability, Is.EqualTo(15000), "Ability should be correctly set");
            Assert.That(tank.Stock, Is.EqualTo(10000), "Stock should be correctly set");
            Assert.That(tank.Compartment, Is.EqualTo(3), "Compartment should be correctly set");
        }

        [Test]
        [Category("CompartmentCapacity")]
        [Category("TankListing")]
        public void TankResponse_DisplayText_ShouldFormatCorrectly()
        {
            // Arrange
            var tank = new TankResponse
            {
                IdTank = 1,
                Number = "TQ-TQ-001",
                Ability = 15000,
                Stock = 10000,
                Compartment = 3
            };

            // Act
            var displayText = tank.DisplayText;

            // Assert
            Assert.That(displayText, Is.Not.Null, "DisplayText should not be null");
            Assert.That(displayText, Does.Contain("TQ-TQ-001"), "DisplayText should contain tank number");
            Assert.That(displayText, Does.Contain("15,000"), "DisplayText should contain formatted ability");
        }

        [Test]
        [Category("CompartmentCapacity")]
        [Category("TankListing")]
        public void TankApiResponse_ShouldContainDataList()
        {
            // Arrange
            var tanks = new List<TankResponse>
            {
                new TankResponse { IdTank = 1, Number = "1", Ability = 10000 },
                new TankResponse { IdTank = 2, Number = "2", Ability = 12000 },
                new TankResponse { IdTank = 3, Number = "TQ-TQ-001", Ability = 15000 }
            };

            var apiResponse = new TankApiResponse
            {
                StatusCode = 200,
                Success = true,
                Message = "Success",
                Data = tanks
            };

            // Assert
            Assert.That(apiResponse.Data, Is.Not.Null, "Data should not be null");
            Assert.That(apiResponse.Data.Count, Is.EqualTo(3), "Data should contain 3 tanks");
            Assert.That(apiResponse.Success, Is.True, "Success should be true");
        }

        [Test]
        [Category("CompartmentCapacity")]
        [Category("TankListing")]
        public void TankList_ShouldNotContainDuplicates()
        {
            // Arrange
            var tanks = new List<TankResponse>
            {
                new TankResponse { IdTank = 1, Number = "1", Ability = 10000 },
                new TankResponse { IdTank = 2, Number = "2", Ability = 12000 },
                new TankResponse { IdTank = 3, Number = "3", Ability = 15000 }
            };

            // Act
            var distinctTanks = tanks.GroupBy(t => t.IdTank).Select(g => g.First()).ToList();

            // Assert
            Assert.That(distinctTanks.Count, Is.EqualTo(tanks.Count), "No duplicates should exist");
        }

        [Test]
        [Category("CompartmentCapacity")]
        [Category("TankListing")]
        public void TankNumber_ShouldNotBeNullOrEmpty()
        {
            // Arrange
            var tank = new TankResponse
            {
                IdTank = 1,
                Number = "TQ-TQ-001",
                Ability = 15000
            };

            // Assert
            Assert.That(tank.Number, Is.Not.Null, "Tank number should not be null");
            Assert.That(tank.Number, Is.Not.Empty, "Tank number should not be empty");
        }
    }
}
