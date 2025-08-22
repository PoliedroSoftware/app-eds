using NUnit.Framework;
using System.Collections.ObjectModel;
using System.Linq;

namespace Mobile_tests
{
    // Simplified models for unit testing the compartment validation logic
    public class TestCompartimentResponse
    {
        public int IdCompartment { get; set; }
        public int Number { get; set; }
        public int IdTank { get; set; }
    }

    public class TestTankResponse
    {
        public int IdTank { get; set; }
        public string Number { get; set; } = "";
        public int Compartment { get; set; }
    }

    [TestFixture]
    public class CompartmentValidationTest
    {
        private ObservableCollection<TestCompartimentResponse> _compartmentList;
        private TestTankResponse? _selectedTank;

        [SetUp]
        public void Setup()
        {
            _compartmentList = new ObservableCollection<TestCompartimentResponse>();
        }

        [Test]
        [Category("CompartmentValidation")]
        public void ShouldDetectDuplicateCompartment()
        {
            // Arrange
            _compartmentList.Add(new TestCompartimentResponse { IdCompartment = 1, Number = 1, IdTank = 10 });
            _compartmentList.Add(new TestCompartimentResponse { IdCompartment = 2, Number = 2, IdTank = 10 });
            _selectedTank = new TestTankResponse { IdTank = 10, Number = "Tank-001", Compartment = 5 };
            int newCompartmentNumber = 1; // This already exists

            // Act - Simulate the validation logic from CompartimentService
            var existingCompartment = _compartmentList.FirstOrDefault(c => 
                c.Number == newCompartmentNumber && c.IdTank == _selectedTank.IdTank);

            // Assert
            Assert.That(existingCompartment, Is.Not.Null, "Should detect duplicate compartment");
        }

        [Test]
        [Category("CompartmentValidation")]
        public void ShouldDetectTankFull()
        {
            // Arrange
            _compartmentList.Add(new TestCompartimentResponse { IdCompartment = 1, Number = 1, IdTank = 20 });
            _compartmentList.Add(new TestCompartimentResponse { IdCompartment = 2, Number = 2, IdTank = 20 });
            _compartmentList.Add(new TestCompartimentResponse { IdCompartment = 3, Number = 3, IdTank = 20 });
            _selectedTank = new TestTankResponse { IdTank = 20, Number = "Tank-002", Compartment = 3 };
            int newCompartmentNumber = 4; // New compartment, but tank is full

            // Act - Simulate the validation logic from CompartimentService
            var existingCompartment = _compartmentList.FirstOrDefault(c => 
                c.Number == newCompartmentNumber && c.IdTank == _selectedTank.IdTank);
            var existingCompartmentsCount = _compartmentList.Count(c => c.IdTank == _selectedTank.IdTank);

            // Assert
            Assert.That(existingCompartment, Is.Null, "Should not find existing compartment");
            Assert.That(existingCompartmentsCount, Is.GreaterThanOrEqualTo(_selectedTank.Compartment), "Tank should be full");
        }

        [Test]
        [Category("CompartmentValidation")]
        public void ShouldAllowValidAddition()
        {
            // Arrange
            _compartmentList.Add(new TestCompartimentResponse { IdCompartment = 1, Number = 1, IdTank = 30 });
            _compartmentList.Add(new TestCompartimentResponse { IdCompartment = 2, Number = 2, IdTank = 30 });
            _selectedTank = new TestTankResponse { IdTank = 30, Number = "Tank-003", Compartment = 5 };
            int newCompartmentNumber = 3; // New compartment, tank has space

            // Act - Simulate the validation logic from CompartimentService
            var existingCompartment = _compartmentList.FirstOrDefault(c => 
                c.Number == newCompartmentNumber && c.IdTank == _selectedTank.IdTank);
            var existingCompartmentsCount = _compartmentList.Count(c => c.IdTank == _selectedTank.IdTank);

            // Assert
            Assert.That(existingCompartment, Is.Null, "Should not find existing compartment");
            Assert.That(existingCompartmentsCount, Is.LessThan(_selectedTank.Compartment), "Tank should have space for more compartments");
        }

        [Test]
        [Category("CompartmentValidation")]
        public void ShouldHandleEmptyCompartmentList()
        {
            // Arrange
            _selectedTank = new TestTankResponse { IdTank = 40, Number = "Tank-004", Compartment = 5 };
            int newCompartmentNumber = 1;

            // Act - Simulate the validation logic from CompartimentService
            var existingCompartment = _compartmentList.FirstOrDefault(c => 
                c.Number == newCompartmentNumber && c.IdTank == _selectedTank.IdTank);
            var existingCompartmentsCount = _compartmentList.Count(c => c.IdTank == _selectedTank.IdTank);

            // Assert
            Assert.That(existingCompartment, Is.Null, "Should not find existing compartment in empty list");
            Assert.That(existingCompartmentsCount, Is.EqualTo(0), "Should have zero existing compartments");
            Assert.That(existingCompartmentsCount, Is.LessThan(_selectedTank.Compartment), "Should allow adding to empty tank");
        }

        [Test]
        [Category("CompartmentValidation")]
        public void ShouldHandleMultipleTanks()
        {
            // Arrange - Add compartments for different tanks
            _compartmentList.Add(new TestCompartimentResponse { IdCompartment = 1, Number = 1, IdTank = 50 });
            _compartmentList.Add(new TestCompartimentResponse { IdCompartment = 2, Number = 1, IdTank = 60 }); // Same number, different tank
            _selectedTank = new TestTankResponse { IdTank = 50, Number = "Tank-005", Compartment = 3 };
            int newCompartmentNumber = 2; // Should be valid for tank 50

            // Act - Simulate the validation logic from CompartimentService
            var existingCompartment = _compartmentList.FirstOrDefault(c => 
                c.Number == newCompartmentNumber && c.IdTank == _selectedTank.IdTank);
            var existingCompartmentsCount = _compartmentList.Count(c => c.IdTank == _selectedTank.IdTank);

            // Assert
            Assert.That(existingCompartment, Is.Null, "Should not find compartment number 2 for tank 50");
            Assert.That(existingCompartmentsCount, Is.EqualTo(1), "Should have exactly 1 compartment for tank 50");
            Assert.That(existingCompartmentsCount, Is.LessThan(_selectedTank.Compartment), "Tank 50 should have space for more compartments");
        }
    }
}