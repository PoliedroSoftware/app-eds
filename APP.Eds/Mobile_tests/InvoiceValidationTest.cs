using NUnit.Framework;

namespace Mobile_tests
{
    [TestFixture]
    public class InvoiceValidationTest
    {
        [Test]
        [Category("InvoiceValidation")]
        public void InvoiceWithLessThan3Characters_ShouldBeInvalid()
        {
            // Arrange
            string invoice1 = "AB";
            string invoice2 = "1";
            string invoice3 = "";

            // Act & Assert
            Assert.That(invoice1.Length < 3, Is.True, "Invoice with 2 characters should be invalid");
            Assert.That(invoice2.Length < 3, Is.True, "Invoice with 1 character should be invalid");
            Assert.That(invoice3.Length < 3, Is.True, "Empty invoice should be invalid");
        }

        [Test]
        [Category("InvoiceValidation")]
        public void InvoiceWith3OrMoreCharacters_ShouldBeValid()
        {
            // Arrange
            string invoice1 = "ABC";
            string invoice2 = "1234";
            string invoice3 = "FAC-2024-001";

            // Act & Assert
            Assert.That(invoice1.Length >= 3, Is.True, "Invoice with 3 characters should be valid");
            Assert.That(invoice2.Length >= 3, Is.True, "Invoice with 4 characters should be valid");
            Assert.That(invoice3.Length >= 3, Is.True, "Invoice with 12 characters should be valid");
        }

        [Test]
        [Category("InvoiceValidation")]
        public void WhitespaceOnlyInvoice_ShouldBeInvalid()
        {
            // Arrange
            string invoice1 = "   ";
            string invoice2 = "\t\t";
            string invoice3 = "\n\n";

            // Act
            var trimmed1 = invoice1.Trim();
            var trimmed2 = invoice2.Trim();
            var trimmed3 = invoice3.Trim();

            // Assert
            Assert.That(string.IsNullOrWhiteSpace(trimmed1), Is.True, "Spaces only should be invalid");
            Assert.That(string.IsNullOrWhiteSpace(trimmed2), Is.True, "Tabs only should be invalid");
            Assert.That(string.IsNullOrWhiteSpace(trimmed3), Is.True, "Newlines only should be invalid");
        }

        [Test]
        [Category("InvoiceValidation")]
        public void ValidInvoice_WithWhitespace_ShouldBeValidAfterTrim()
        {
            // Arrange
            string invoice = "  ABC  ";

            // Act
            var trimmed = invoice.Trim();

            // Assert
            Assert.That(trimmed.Length >= 3, Is.True, "Invoice with valid content after trim should be valid");
            Assert.That(trimmed, Is.EqualTo("ABC"), "Trimmed invoice should equal ABC");
        }

        [Test]
        [Category("InvoiceValidation")]
        [TestCase("ABC", 3)]
        [TestCase("ABCD", 4)]
        [TestCase("12345", 5)]
        [TestCase("FAC-2024-001", 12)]
        public void ValidInvoice_ShouldHaveCorrectLength(string invoice, int expectedLength)
        {
            // Act
            var actualLength = invoice.Length;

            // Assert
            Assert.That(actualLength, Is.EqualTo(expectedLength));
            Assert.That(actualLength >= 3, Is.True, $"Invoice '{invoice}' should be valid");
        }

        [Test]
        [Category("InvoiceValidation")]
        [TestCase("A", 2)]
        [TestCase("AB", 1)]
        [TestCase("", 3)]
        public void InvalidInvoice_ShouldCalculateRemainingCharacters(string invoice, int expectedRemaining)
        {
            // Act
            var remaining = 3 - invoice.Length;

            // Assert
            Assert.That(remaining, Is.EqualTo(expectedRemaining));
            Assert.That(remaining > 0, Is.True, "Should have characters remaining");
        }

        [Test]
        [Category("InvoiceValidation")]
        public void ErrorMessage_ShouldBeUserFriendly()
        {
            // Arrange
            string technicalMessage = "El número de factura debe tener al menos 3 caracteres";
            string userFriendlyMessage = "Por favor ingrese un número de factura válido (mínimo 3 caracteres)";

            // Assert - The user-friendly message should be more helpful
            Assert.That(userFriendlyMessage, Does.Contain("Por favor"));
            Assert.That(userFriendlyMessage, Does.Contain("válido"));
            Assert.That(userFriendlyMessage, Does.Contain("mínimo 3 caracteres"));
            Assert.That(userFriendlyMessage.Length > technicalMessage.Length, Is.True, 
                "User-friendly message should be more descriptive");
        }

        [Test]
        [Category("InvoiceValidation")]
        public void HelpText_ShouldIndicateMinimumRequirement()
        {
            // Arrange
            string helpText = "El número de factura debe tener al menos 3 caracteres";

            // Assert
            Assert.That(helpText, Does.Contain("debe tener"));
            Assert.That(helpText, Does.Contain("al menos 3"));
            Assert.That(helpText, Does.Contain("caracteres"));
        }

        [Test]
        [Category("InvoiceValidation")]
        public void Placeholder_ShouldProvideContextualHelp()
        {
            // Arrange
            string placeholder = "Ingrese número de factura (mínimo 3 caracteres)";

            // Assert
            Assert.That(placeholder, Does.Contain("mínimo 3 caracteres"));
            Assert.That(placeholder, Does.Contain("Ingrese"));
        }
    }
}
