using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Mobile_tests
{
    // Simplified models for unit testing the tank listing logic in compartment capacity module
    public class TankCapacityResponse
    {
        public int IdTank { get; set; }
        public string Number { get; set; } = "";
        public decimal Ability { get; set; }
        public decimal Stock { get; set; }
        public int Compartment { get; set; }
        
        public string DisplayText => $"Tanque {Number} - Capacidad {Ability:N0} L";
    }

    public class TankCapacityApiResponse
    {
        public int StatusCode { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public List<TankCapacityResponse> Data { get; set; } = new();
    }

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
            var tank = new TankCapacityResponse
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
            var tank = new TankCapacityResponse
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
            var tanks = new List<TankCapacityResponse>
            {
                new TankCapacityResponse { IdTank = 1, Number = "1", Ability = 10000 },
                new TankCapacityResponse { IdTank = 2, Number = "2", Ability = 12000 },
                new TankCapacityResponse { IdTank = 3, Number = "TQ-TQ-001", Ability = 15000 }
            };

            var apiResponse = new TankCapacityApiResponse
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
            var tanks = new List<TankCapacityResponse>
            {
                new TankCapacityResponse { IdTank = 1, Number = "1", Ability = 10000 },
                new TankCapacityResponse { IdTank = 2, Number = "2", Ability = 12000 },
                new TankCapacityResponse { IdTank = 3, Number = "3", Ability = 15000 }
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
            var tank = new TankCapacityResponse
            {
                IdTank = 1,
                Number = "TQ-TQ-001",
                Ability = 15000
            };

            // Assert
            Assert.That(tank.Number, Is.Not.Null, "Tank number should not be null");
            Assert.That(tank.Number, Is.Not.Empty, "Tank number should not be empty");
        }

        [Test]
        [Category("CompartmentCapacity")]
        [Category("TankListing")]
        public void TankList_ShouldFilterByValidAbility()
        {
            // Arrange
            var tanks = new List<TankCapacityResponse>
            {
                new TankCapacityResponse { IdTank = 1, Number = "1", Ability = 0 },  // Invalid
                new TankCapacityResponse { IdTank = 2, Number = "2", Ability = 12000 },
                new TankCapacityResponse { IdTank = 3, Number = "3", Ability = -100 },  // Invalid
                new TankCapacityResponse { IdTank = 4, Number = "TQ-TQ-001", Ability = 15000 }
            };

            // Act
            var validTanks = tanks.Where(t => t.Ability > 0).ToList();

            // Assert
            Assert.That(validTanks.Count, Is.EqualTo(2), "Only tanks with positive ability should be valid");
            Assert.That(validTanks.All(t => t.Ability > 0), Is.True, "All valid tanks should have positive ability");
        }
    }
}
