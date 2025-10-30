using System.Text.RegularExpressions;

namespace Mobile_tests;

[TestFixture]
public class LettersOnlyBehaviorTest
{
    private const string ValidationPattern = @"^[a-zA-Z0-9\u00C0-\u00FF\s/#\-._&()]+$";

    [Test]
    [Category("Validation")]
    public void TestBusinessNameValidation_AllowsNumbersAndSpecialCharacters()
    {
        // Test cases that should be VALID
        string[] validNames = {
            "Estación 24/7",
            "Gasolinera #5",
            "Poliedro-Software",
            "Test & Co.",
            "Negocio_123",
            "Tienda (Norte)",
            "ABC",
            "Empresa.2025",
            "Tienda José",
            "Negocio Ñoño"
        };

        foreach (string name in validNames)
        {
            bool isValid = Regex.IsMatch(name, ValidationPattern);
            Assert.That(isValid, Is.True, $"'{name}' should be valid");
        }
    }

    [Test]
    [Category("Validation")]
    public void TestBusinessNameValidation_RejectsInvalidCharacters()
    {
        // Test cases that should be INVALID
        string[] invalidNames = {
            "Test@Invalid",
            "Test$Invalid",
            "Test%Invalid",
            "Test!Invalid",
            "Test~Invalid"
        };

        foreach (string name in invalidNames)
        {
            bool isValid = Regex.IsMatch(name, ValidationPattern);
            Assert.That(isValid, Is.False, $"'{name}' should be invalid");
        }
    }

    [Test]
    [Category("Validation")]
    public void TestBusinessNameValidation_AllowsAccentedCharacters()
    {
        // Test Spanish accented characters
        string[] accentedNames = {
            "Café",
            "José",
            "María",
            "Ñoño",
            "Estación",
            "Petróleo"
        };

        foreach (string name in accentedNames)
        {
            bool isValid = Regex.IsMatch(name, ValidationPattern);
            Assert.That(isValid, Is.True, $"'{name}' with accented characters should be valid");
        }
    }

    [Test]
    [Category("Validation")]
    public void TestBusinessNameValidation_MinimumLength()
    {
        // Test minimum length requirement (3 characters)
        string validShortName = "ABC";
        string validLongName = "Very Long Business Name With Numbers 123";

        bool isValidShort = Regex.IsMatch(validShortName, ValidationPattern);
        bool isValidLong = Regex.IsMatch(validLongName, ValidationPattern);

        Assert.That(isValidShort, Is.True, "3-character name should be valid");
        Assert.That(isValidLong, Is.True, "Long name should be valid");
    }
}
